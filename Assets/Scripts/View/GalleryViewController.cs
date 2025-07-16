using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Keiwando;
using Keiwando.UI;
using Keiwando.Evolution.Scenes;
using Keiwando.NFSO;

namespace Keiwando.Evolution.UI {

  public class GalleryViewController: MonoBehaviour, ISimulationVisibilityOptionsViewDelegate {

    [SerializeField] private CustomGridLayoutGroup grid;
    [SerializeField] private GalleryGridCell templateGridCell;
    [SerializeField] private GameObject fullscreenView;
    [SerializeField] private RawImage fullscreenRawImage;
    [SerializeField] private Button fullscreenCloseButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_Text pageNumberLabel;
    [SerializeField] private Button closeButton;
    [SerializeField] private SimulationVisibilityOptionsView visibilityOptionsView;
    [SerializeField] private Button showStatsButton;
    [SerializeField] private TMP_Text statsLabel;
    [SerializeField] private Button fullscreenMoreButton;
    [SerializeField] private Button galleryMoreButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private Button importButton;
		[SerializeField] private UIFade successfulImportIndicator;
		[SerializeField] private UIFade failedImportIndicator;
    [SerializeField] private Button fullscreenPrevButton;
    [SerializeField] private Button fullscreenNextButton;
    [SerializeField] private DeleteConfirmationDialog deleteConfirmation;
    [SerializeField] private GameObject emptyGalleryInstructions;
    
    private TMP_Text showStatsButtonLabel;

    private GalleryGridCell[] cells;
    private RenderTexture[] renderTextures;
    private Scene?[] scenes;
    private Camera[] cameras;
    private Creature[] creatures;
    struct PerObjectData {
      public int layer;
      public GameObject gameObject;
    }
    // Note: This only contains objects that were active at the time the scene finished loading.
    private List<PerObjectData>[] allObjectsPerScene;

    private int currentPageIndex = 0;
    private int numberOfItemsOnCurrentPage = 0;
    private int sceneLoadingInitiatedForPageIndex = -1;
    private int fullscreenRenderTextureIndex = -1;
    private int? fullscreenSceneIndex = null;
    private Coroutine sceneLoadingCoroutine;

    private int hiddenLayer;

    private CreatureGalleryManager galleryManager = new CreatureGalleryManager();
    private SupportedFileType[] supportedFileTypesForImportAndExport = {
      CustomEvolutionFileType.evolutiongallery
    };

    private bool initialized = false;

    void Start() {

      hiddenLayer = LayerMask.NameToLayer("Hidden");

      int numberOfItemsPerPage = grid.ColumnCount * grid.RowCount;
      this.cells = new GalleryGridCell[numberOfItemsPerPage];
      // The last index is the fullscreen texture
      this.renderTextures = new RenderTexture[numberOfItemsPerPage + 1];
      this.fullscreenRenderTextureIndex = this.renderTextures.Length - 1;
      this.scenes = new Scene?[numberOfItemsPerPage];
      this.cameras = new Camera[numberOfItemsPerPage];
      this.creatures = new Creature[numberOfItemsPerPage];
      this.allObjectsPerScene = new List<PerObjectData>[numberOfItemsPerPage];
      for (int i = 0; i < numberOfItemsPerPage; i++) {
        this.allObjectsPerScene[i] = new List<PerObjectData>();
      }

      for (int i = 0; i < numberOfItemsPerPage; i++) {
        var cell = Instantiate(templateGridCell, grid.transform);
        cell.gameObject.SetActive(true);
        cells[i] = cell;

        int cellIndex = i;
        cell.button.onClick.AddListener(delegate () {
          enterFullscreen(sceneIndex: cellIndex);
        });
      }
      templateGridCell.gameObject.SetActive(false);

      closeButton.onClick.AddListener(delegate () {
          Hide();
      });
      fullscreenCloseButton.onClick.AddListener(delegate () {
          exitFullscreen();
      });
      
      visibilityOptionsView.Delegate = this;

      showStatsButtonLabel = showStatsButton.GetComponentInChildren<TMP_Text>();
      showStatsButton.onClick.AddListener(delegate () {
        statsLabel.gameObject.SetActive(!statsLabel.gameObject.activeSelf);
        refreshStatsLabel();
        showStatsButtonLabel.text = statsLabel.gameObject.activeSelf ? "Hide Stats" : "Show Stats";
      });
      statsLabel.gameObject.SetActive(false);

      GameObject galleryMoreMenu = importButton.transform.parent.gameObject;
      GameObject fullscreenMoreMenu = exportButton.transform.parent.gameObject;
      
      galleryMoreButton.onClick.AddListener(delegate () {
        galleryMoreMenu.gameObject.SetActive(!galleryMoreMenu.activeSelf);
      });
      fullscreenMoreButton.onClick.AddListener(delegate () {
        fullscreenMoreMenu.gameObject.SetActive(!fullscreenMoreMenu.activeSelf);
      });

      importButton.onClick.AddListener(delegate () {
        galleryMoreMenu.gameObject.SetActive(false);
        onImportClicked();
      });
      exportButton.onClick.AddListener(delegate () {
        fullscreenMoreMenu.gameObject.SetActive(false);
        onExportClicked();
      });
      deleteButton.onClick.AddListener(delegate () {
        fullscreenMoreMenu.gameObject.SetActive(false);
        onDeleteClicked();
      });

      prevPageButton.onClick.AddListener(delegate () {
        this.currentPageIndex -= 1;
        Refresh();
      });
      nextPageButton.onClick.AddListener(delegate () {
        this.currentPageIndex += 1;
        Refresh();
      });
      fullscreenPrevButton.onClick.AddListener(delegate () {
        if (!this.fullscreenSceneIndex.HasValue) { return; }
        if (this.fullscreenSceneIndex.Value <= 0) { return; }
        int newFullscreenSceneIndex = this.fullscreenSceneIndex.Value - 1;
        exitFullscreen();
        enterFullscreen(newFullscreenSceneIndex);
      });
      fullscreenNextButton.onClick.AddListener(delegate () {
        if (!this.fullscreenSceneIndex.HasValue) { return; }
        if (this.fullscreenSceneIndex.Value >= this.scenes.Length - 1) { return; }
        int newFullscreenSceneIndex = this.fullscreenSceneIndex.Value + 1;
        exitFullscreen();
        enterFullscreen(newFullscreenSceneIndex);
      });

      initialized = true;

      Refresh();
    }

    void OnDestroy() {  
      releaseRenderResources();
    }

    void OnEnable() {
      GameObject galleryMoreMenu = importButton.transform.parent.gameObject;
      GameObject fullscreenMoreMenu = exportButton.transform.parent.gameObject;
      
      galleryMoreMenu.gameObject.SetActive(false);
      fullscreenMoreMenu.gameObject.SetActive(false);
    }

    public void Refresh() {
      if (!initialized) { return; }
      
      int numberOfItemsPerPage = Math.Max(1, grid.ColumnCount * grid.RowCount);
      int totalItemCount = galleryManager.gallery.entries.Count;
      int totalNumberOfPages = Math.Max(1, (totalItemCount + numberOfItemsPerPage - 1) / numberOfItemsPerPage);
      currentPageIndex = Math.Clamp(currentPageIndex, 0, totalNumberOfPages - 1);

      numberOfItemsOnCurrentPage = currentPageIndex == totalNumberOfPages - 1 ? totalItemCount % numberOfItemsPerPage : numberOfItemsPerPage;

      for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++) {
        GalleryGridCell cell = cells[cellIndex];
        if (cellIndex < numberOfItemsOnCurrentPage) {
          cell.gameObject.SetActive(true);
        } else {
          cell.gameObject.SetActive(false);
        }
      }

      prevPageButton.interactable = currentPageIndex > 0;
      nextPageButton.interactable = currentPageIndex < totalNumberOfPages - 1;

      if (this.fullscreenSceneIndex.HasValue) {
        this.fullscreenPrevButton.interactable = this.fullscreenSceneIndex.Value > 0;
        this.fullscreenNextButton.interactable = this.fullscreenSceneIndex.Value < numberOfItemsOnCurrentPage - 1;
      }

      emptyGalleryInstructions.gameObject.SetActive(totalItemCount == 0);
      pageNumberLabel.SetText("Page {0}/{1}", currentPageIndex + 1, totalNumberOfPages);

      bool needsSceneLoading = sceneLoadingInitiatedForPageIndex != currentPageIndex;
      if (needsSceneLoading) {
        unloadScenes();
        if (sceneLoadingCoroutine != null) {
          StopCoroutine(sceneLoadingCoroutine);
        }
        sceneLoadingCoroutine = StartCoroutine(loadGalleryScenes());
        sceneLoadingInitiatedForPageIndex = currentPageIndex;
      }
    }

    private int getGalleryEntryIndexForSceneIndex(int sceneIndex) {
      int numberOfItemsPerPage = Math.Max(1, grid.ColumnCount * grid.RowCount);
      int firstItemIndexOnPage = currentPageIndex * numberOfItemsPerPage;
      int galleryEntryIndex = firstItemIndexOnPage + sceneIndex; 
      return galleryEntryIndex;
    }

    private CreatureGalleryEntry getAndLoadGalleryEntryForSceneIndex(int sceneIndex) {
      int galleryEntryIndex = getGalleryEntryIndexForSceneIndex(sceneIndex);
      galleryManager.loadGalleryEntry(galleryEntryIndex);
      return this.galleryManager.gallery.entries[galleryEntryIndex];
    }

    private CreatureRecording getRecordingForSceneIndex(int sceneIndex) {
      CreatureGalleryEntry galleryEntry = getAndLoadGalleryEntryForSceneIndex(sceneIndex);
      if (galleryEntry.loadedData == null) {
        return null;
      }
      return galleryEntry.loadedData.recording;
    }

    private Creature getFullscreenPlaybackCreature() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return null;
      }
      return this.creatures[this.fullscreenSceneIndex.Value];
    }

    private IEnumerator loadGalleryScenes() {

      for (int cellIndex = 0; cellIndex < this.numberOfItemsOnCurrentPage; cellIndex++) {
        GalleryGridCell cell = cells[cellIndex];
        if (cell == null) {
          continue;
        }
        CreatureRecording recording = getRecordingForSceneIndex(cellIndex);
        if (recording == null) {
          Debug.LogWarning($"Gallery entry for cell {cellIndex} not loaded!");
          continue;
        }

        var sceneLoadContext = new SceneController.SimulationSceneLoadContext();
        
        yield return SceneController.LoadSimulationScene(
          creatureDesign: recording.creatureDesign,
          creatureSpawnCount: 1,
          sceneDescription: recording.sceneDescription,
          sceneType: SceneController.SimulationSceneType.GalleryPlayback,
          legacyOptions: LegacySimulationOptions.Default,
          context: sceneLoadContext,
          sceneContext: new GalleryPlaybackSceneContext(recording.stats, recording.task)
        );
        var scene = sceneLoadContext.Scene;
        this.scenes[cellIndex] = scene;
        this.creatures[cellIndex] = sceneLoadContext.Creatures[0];

        var prevActiveScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        
        SimulationSceneSetup sceneSetup = null;
        var rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++) {
            sceneSetup = rootObjects[i].GetComponent<SimulationSceneSetup>();
            if (sceneSetup != null) break;
        }
        this.allObjectsPerScene[cellIndex].Clear();
        foreach (GameObject rootObject in rootObjects) {
          // GetComponentsInChildren includes the root object
          foreach (UnityEngine.Transform childTransform in rootObject.GetComponentsInChildren<UnityEngine.Transform>()) {
            allObjectsPerScene[cellIndex].Add(new PerObjectData {
              layer = childTransform.gameObject.layer,
              gameObject = childTransform.gameObject
            });
          }
        }
        foreach (GameObject rootObject in rootObjects) {
          if (rootObject.name == "Main Camera") {
            Camera mainCamera = rootObject.GetComponent<Camera>();
            RenderTexture renderTexture = renderTextures[cellIndex];
            if (renderTexture == null) {
              Canvas canvas = GetComponent<Canvas>();
              RectTransform cellTransform = cell.transform as RectTransform;
              renderTexture = new RenderTexture(
                width: (int)(cellTransform.sizeDelta.x * canvas.transform.localScale.x),
                height: (int)(cellTransform.sizeDelta.y * canvas.transform.localScale.y),
                depth: 0
              );
              renderTextures[cellIndex] = renderTexture;
            }
            mainCamera.targetTexture = renderTexture;
            cell.rawImage.texture = renderTexture;
            
            mainCamera.enabled = false;
            mainCamera.scene = scene;
            this.cameras[cellIndex] = mainCamera;

            SimulationBackgroundRenderer backgroundRenderer = mainCamera.GetComponent<SimulationBackgroundRenderer>();
            backgroundRenderer.task = recording.task;

          } else if (rootObject.name == "GalleryPlaybackController") {
            GalleryPlaybackController playbackController = rootObject.GetComponent<GalleryPlaybackController>();
            CreatureRecordingPlayer player = new CreatureRecordingPlayer(recording: recording.movementData);
            playbackController.Setup(sceneLoadContext.Creatures[0], player);
            playbackController.Play();
          }
        }

        SceneManager.SetActiveScene(prevActiveScene);
        
        yield return null;
      }
    }

    void Update() {
      for (int sceneIndex = 0; sceneIndex < numberOfItemsOnCurrentPage; sceneIndex++) {
        if (this.cameras[sceneIndex] == null) {
          continue;
        }
        List<PerObjectData> allObjectsInScene = allObjectsPerScene[sceneIndex];
        for (int i = 0; i < allObjectsInScene.Count; i++) {
          PerObjectData perObjectData = allObjectsInScene[i];
          if (perObjectData.gameObject != null) {
            if (perObjectData.gameObject.layer != hiddenLayer) {
              perObjectData.layer = perObjectData.gameObject.layer;
            }
            perObjectData.gameObject.layer = hiddenLayer;
            allObjectsInScene[i] = perObjectData;
          }
        }
      }
      for (int sceneIndex = 0; sceneIndex < numberOfItemsOnCurrentPage; sceneIndex++) {
        if (fullscreenSceneIndex.HasValue && sceneIndex != fullscreenSceneIndex.Value){
          continue;
        }
        Camera camera = this.cameras[sceneIndex];
        if (camera == null) {
          continue;
        }
        for (int i = 0; i < allObjectsPerScene[sceneIndex].Count; i++) {
          PerObjectData perObjectData = allObjectsPerScene[sceneIndex][i];
          if (perObjectData.gameObject != null) {
            perObjectData.gameObject.layer = perObjectData.layer;
          }
        }
        camera.Render();
        for (int i = 0; i < allObjectsPerScene[sceneIndex].Count; i++) {
          PerObjectData perObjectData = allObjectsPerScene[sceneIndex][i];
          if (perObjectData.gameObject != null) {
            perObjectData.gameObject.layer = hiddenLayer;
          }
        }
      }
      // Reset all object layers to their original values so that 
      // we don't reset perObjectData.layer to hidden for all objects on the next frame
      for (int sceneIndex = 0; sceneIndex < numberOfItemsOnCurrentPage; sceneIndex++) {
        List<PerObjectData> allObjectsInScene = allObjectsPerScene[sceneIndex];
        foreach (PerObjectData perObjectData in allObjectsInScene) {
          if (perObjectData.gameObject != null) {
            perObjectData.gameObject.layer = perObjectData.layer;
          }
        }
      }
    }

    private void releaseRenderResources() {

      for (int i = 0; i < renderTextures.Length; i++) {
        if (renderTextures[i] != null) {
          renderTextures[i].Release();
          renderTextures[i] = null;
        }
      }
      unloadScenes();
      foreach (GalleryGridCell cell in cells) {
        cell.rawImage.texture = null;
      }
    }

    private void unloadScenes() {
      sceneLoadingInitiatedForPageIndex = -1;
      for (int i = 0; i < scenes.Length; i++) {
        Scene? scene = scenes[i];
        if (scene.HasValue) {
          SceneManager.UnloadSceneAsync(scene.Value);
          scenes[i] = null;
        }
      }
      galleryManager.unloadAllGalleryEntries();
      for (int i = 0; i < cameras.Length; i++) {
        this.cameras[i] = null;
      }
      for (int i = 0; i < creatures.Length; i++) {
        this.creatures[i] = null;
      }
      for (int i = 0; i < allObjectsPerScene.Length; i++) {
        if (this.allObjectsPerScene[i] != null) {
          this.allObjectsPerScene[i].Clear();
        }
      }
    }

    private void enterFullscreen(int sceneIndex) {
      if (this.fullscreenSceneIndex.HasValue) {
        if (this.fullscreenSceneIndex.Value == sceneIndex) {
          return;
        }
        exitFullscreen();
      }

      Camera camera = this.cameras[sceneIndex];
      if (camera == null) {
        return;
      }
      if (this.renderTextures[this.fullscreenRenderTextureIndex] == null) {
        this.renderTextures[this.fullscreenRenderTextureIndex] = new RenderTexture(
          width: Screen.width,
          height: Screen.height,
          depth: 0
        ); 
        this.fullscreenRawImage.texture = this.renderTextures[this.fullscreenRenderTextureIndex];
      }

      camera.targetTexture = this.renderTextures[this.fullscreenRenderTextureIndex];
      fullscreenView.gameObject.SetActive(true);

      this.fullscreenSceneIndex = sceneIndex;

      refreshStatsLabel();
      Refresh();
    }

    private void exitFullscreen() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return;
      }
      int sceneIndex = this.fullscreenSceneIndex.Value;

      Camera camera = this.cameras[sceneIndex];
      if (camera != null) {
        RenderTexture previewRenderTexture = this.renderTextures[sceneIndex];
        camera.targetTexture = previewRenderTexture;
      }
      fullscreenView.gameObject.SetActive(false);
      this.fullscreenSceneIndex = null;
    }

    private void onImportClicked() {
      NativeFileSO.shared.OpenFiles(supportedFileTypesForImportAndExport, 
      delegate (bool filesWereOpened, OpenedFile[] files) {
        if (filesWereOpened) {
          tryImport(files);
        }
      });
    }

    private void tryImport(OpenedFile[] files) {
      // Whether at least one file was successfully imported
      bool successfulImport = false;
      // Whether at least one file failed to be imported
      var failedImport = false;
      string lastImportedRecordingFilename = null;
      foreach (var file in files) {
        using (MemoryStream memoryStream = new MemoryStream(file.Data))
        using (BinaryReader reader = new BinaryReader(memoryStream)) {
          CreatureRecording recording = CreatureRecordingSerializer.DecodeCreatureRecording(reader);
          if (recording != null) {
            lastImportedRecordingFilename = CreatureRecordingSerializer.SaveCreatureRecordingFile(recording);
          } else {
            failedImport = true;
            Debug.LogError($"Failed to parse evolutiongallery file: {file.Name}");
            continue;
          }
          successfulImport = true;
        }
      }
      exitFullscreen();
      unloadScenes();
      galleryManager.shallowLoadGalleryEntries();

      if (lastImportedRecordingFilename != null) {
        int pageNumberOfLastImportedRecording = 0;
        for (int galleryIndex = 0; galleryIndex < galleryManager.gallery.entries.Count; galleryIndex++) {
          if (galleryManager.gallery.entries[galleryIndex].filename == lastImportedRecordingFilename) {
            int cellsPerPage = grid.ColumnCount * grid.RowCount;
            pageNumberOfLastImportedRecording = galleryIndex / cellsPerPage;
            break;
          }
        }
        currentPageIndex = pageNumberOfLastImportedRecording;
      }

      Refresh();

      if (successfulImport) {
        successfulImportIndicator.FadeInOut();
      }
      if (failedImport) {
        failedImportIndicator.FadeInOut(1.8f);
      }
    }

    private void onExportClicked() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return;
      }
      int galleryEntryIndex = getGalleryEntryIndexForSceneIndex(this.fullscreenSceneIndex.Value);
      CreatureGalleryEntry galleryEntry = galleryManager.gallery.entries[galleryEntryIndex];
      string filePath = CreatureRecordingSerializer.PathToCreatureRecordingSave(galleryEntry.filename);
      FileToSave fileToSave = new FileToSave(
        srcPath: filePath,
        fileType: CustomEvolutionFileType.evolutiongallery
      );
      NativeFileSO.shared.SaveFile(fileToSave);
    }

    private void onDeleteClicked() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return;
      }
      int galleryEntryIndex = getGalleryEntryIndexForSceneIndex(this.fullscreenSceneIndex.Value);
      CreatureGalleryEntry galleryEntry = galleryManager.gallery.entries[galleryEntryIndex];

      deleteConfirmation.ConfirmDeletionFor(galleryEntry.filename, delegate(string name) {
        exitFullscreen();
        CreatureRecordingSerializer.DeleteCreatureRecording(galleryEntry.filename);
        unloadScenes();
        galleryManager.shallowLoadGalleryEntries();
        Refresh();
      });
    }

    private void refreshStatsLabel() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return;
      }
      if (!this.showStatsButtonLabel.gameObject.activeSelf) {
        return;
      }

      CreatureRecording recording = getRecordingForSceneIndex(this.fullscreenSceneIndex.Value);

      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine(recording.creatureDesign.Name);
      stringBuilder.AppendLine($"Generation: {recording.generation}");
      stringBuilder.AppendLine($"Task: {ObjectiveUtil.StringRepresentation(recording.task)}\n");

      string statsLabelText = BestCreaturesOverlayView.CreateStatsString(
        stringBuilder,
        recording.stats,
        recording.networkSettings,
        recording.networkInputCount,
        recording.networkOutputCount
      );
      statsLabel.SetText(stringBuilder.ToString());
    }
    
    public void Show() {
        gameObject.SetActive(true);
        InputRegistry.shared.Register(InputType.AndroidBack, this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
        
        galleryManager.shallowLoadGalleryEntries();
        Refresh();
    }

    public void Hide() {
        InputRegistry.shared.Deregister(this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
        gameObject.SetActive(false);

        releaseRenderResources();
        sceneLoadingInitiatedForPageIndex = -1;
    }

    private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
        if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
            Hide();
    }


    #region ISimulationVisibilityOptionsViewDelegate

    public void ShowMuscleContractionDidChange(SimulationVisibilityOptionsView view, bool showMuscleContraction) {
      Creature creature = getFullscreenPlaybackCreature();
      if (creature == null) {
        return;
      }
      creature.RefreshMuscleContractionVisibility(Settings.ShowMuscleContraction);
    }

    public void ShowMusclesDidChange(SimulationVisibilityOptionsView view, bool showMuscles) {
      // Creature creature = getFullscreenPlaybackCreature();
      // if (creature == null) {
      //   return;
      // }
      // creature.Refresh
    }

    public Objective GetCurrentTask(SimulationVisibilityOptionsView view) {
      if (!this.fullscreenSceneIndex.HasValue) {
        return Objective.Running;
      }
      CreatureRecording recording = getRecordingForSceneIndex(this.fullscreenSceneIndex.Value);
      if (recording == null) {
        return Objective.Running;
      }
      return recording.task;
    }

    #endregion
  }

}