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
    [SerializeField] private BoundedCamera editorCamera;
    
    private TMP_Text showStatsButtonLabel;

    private GalleryGridCell[] cells;
    private RenderTexture[] renderTextures;
    private int fullscreenRenderTextureIndex => renderTextures.Length - 1;

    struct PerObjectData {
      public int layer;
      public GameObject gameObject;
      public bool isDynamicObjectSpawner;
    }
    struct LoadedSceneData {
      public Scene scene;
      public Camera camera;
      public PhysicsScene physicsScene;
      public Creature creature;
      public List<PerObjectData> allObjects;
      public bool needsPhysicsSimulation;
      public CreatureRecordingPlayer recordingPlayer;
    }
    private LoadedSceneData?[] loadedScenes;
    /// The unordered set of all scene indices that are requested
    /// to be loaded.
    private HashSet<int> neededSceneIndices = new HashSet<int>();
    /// The ordered list of scenes that get requested to get loaded. This list
    /// may contain duplicates and it may contain scene indices that are
    /// already loaded.
    private List<int> sceneLoadRequests = new List<int>();
    /// The index of the scene that is currently being loaded. Only one scene can be
    /// in the loading process at a time.
    private int? currentlyLoadingSceneIndex;
    private Coroutine sceneLoadingCoroutine;

    /// The current page index in the gallery view. This is irrelevant when in fullscreen.
    private int currentPageIndex = 0;
    private int numberOfItemsOnCurrentPage = 0;
    
    /// The absolute scene index that is currently visible in fullscreen mode.
    private int? fullscreenSceneIndex = null;
    /// The scene index that we want to switch to in fullscreen mode
    /// once it is loaded.
    private int? requestedFullscreenSceneIndex = null;

    private int hiddenLayer;

    private CreatureGalleryManager galleryManager = new CreatureGalleryManager();
    private SupportedFileType[] supportedFileTypesForImportAndExport = {
      CustomEvolutionFileType.evolutiongallery
    };

    void Start() {

      hiddenLayer = LayerMask.NameToLayer("Hidden");

      int numberOfItemsPerPage = grid.ColumnCount * grid.RowCount;
      this.cells = new GalleryGridCell[numberOfItemsPerPage];
      // The last index is the fullscreen texture
      this.renderTextures = new RenderTexture[numberOfItemsPerPage + 1];

      this.loadedScenes = new LoadedSceneData?[1];

      for (int i = 0; i < numberOfItemsPerPage; i++) {
        var cell = Instantiate(templateGridCell, grid.transform);
        cell.gameObject.SetActive(false);
        cells[i] = cell;

        int cellIndex = i;
        cell.button.onClick.AddListener(delegate () {
          int galleryEntryIndex = getGalleryEntryIndexForCellIndex(cellIndex);
          requestNewFullscreenIndexIfPossible(galleryEntryIndex);
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
      });
      nextPageButton.onClick.AddListener(delegate () {
        this.currentPageIndex += 1;
      });
      fullscreenPrevButton.onClick.AddListener(delegate () {
        if (!this.fullscreenSceneIndex.HasValue) { return; }
        requestNewFullscreenIndexIfPossible(this.fullscreenSceneIndex.Value - 1);
      });
      fullscreenNextButton.onClick.AddListener(delegate () {
        if (!this.fullscreenSceneIndex.HasValue) { return; }
        requestNewFullscreenIndexIfPossible(this.fullscreenSceneIndex.Value + 1);
      });
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

    void FixedUpdate() {
      bool needsPhysicsUpdate = false;
      foreach (LoadedSceneData? loadedSceneData in this.loadedScenes) {
        if (loadedSceneData.HasValue && loadedSceneData.Value.needsPhysicsSimulation) {
          needsPhysicsUpdate = true;
          break;
        }
      }
      if (needsPhysicsUpdate) {
        // We have to set the correct layers, otherwise the objects of different scenes will
        // collide with each other…
        setAllObjectsToHiddenAndRememberOldLayersIfNecessary();
        for (int cellIndex = 0; cellIndex < numberOfItemsOnCurrentPage; cellIndex++) {
          int galleryEntryIndex = getGalleryEntryIndexForCellIndex(cellIndex);
          if (this.loadedScenes[galleryEntryIndex] == null) {
            continue;
          }
          LoadedSceneData loadedSceneData = this.loadedScenes[galleryEntryIndex].Value;
          if (!loadedSceneData.needsPhysicsSimulation) {
            continue;
          }
          for (int i = 0; i < loadedSceneData.allObjects.Count; i++) {
            PerObjectData perObjectData = loadedSceneData.allObjects[i];
            if (perObjectData.gameObject != null) {
              perObjectData.gameObject.layer = perObjectData.layer;
              if (perObjectData.isDynamicObjectSpawner) {
                foreach (UnityEngine.Transform childTransform in perObjectData.gameObject.transform) {
                  childTransform.gameObject.layer = perObjectData.layer;
                }
              }
            }
          }

          loadedSceneData.physicsScene.Simulate(Time.fixedDeltaTime);

          for (int i = 0; i < loadedSceneData.allObjects.Count; i++) {
            PerObjectData perObjectData = loadedSceneData.allObjects[i];
            if (perObjectData.gameObject != null) {
              perObjectData.gameObject.layer = hiddenLayer;
              if (perObjectData.isDynamicObjectSpawner) {
                foreach (UnityEngine.Transform childTransform in perObjectData.gameObject.transform) {
                  childTransform.gameObject.layer = hiddenLayer;
                }
              }
            }
          }
        }
      }
    }

    void Update() {

      if (this.requestedFullscreenSceneIndex.HasValue) {
        if (this.loadedScenes[this.requestedFullscreenSceneIndex.Value] != null) {
          this.fullscreenSceneIndex = this.requestedFullscreenSceneIndex.Value;
          this.requestedFullscreenSceneIndex = null;

          refreshStatsLabel();
        }
      }
      if (this.fullscreenSceneIndex.HasValue) {
        currentPageIndex = getPageIndexForGalleryEntry(this.fullscreenSceneIndex.Value);
      }

      this.fullscreenView.gameObject.SetActive(this.fullscreenSceneIndex.HasValue);

      int numberOfItemsPerPage = Math.Max(1, this.cells.Length);
      int totalItemCount = galleryManager.gallery.entries.Count;
      int totalNumberOfPages = Math.Max(1, (totalItemCount + numberOfItemsPerPage - 1) / numberOfItemsPerPage);
      currentPageIndex = Math.Clamp(currentPageIndex, 0, totalNumberOfPages - 1);

      numberOfItemsOnCurrentPage = currentPageIndex == totalNumberOfPages - 1 ? totalItemCount % numberOfItemsPerPage : numberOfItemsPerPage;

      if (loadedScenes.Length != galleryManager.gallery.entries.Count) {
        Array.Resize(ref loadedScenes, galleryManager.gallery.entries.Count);
      }

      // Automatically update the necessary scene requests
      neededSceneIndices.Clear();
      sceneLoadRequests.Clear();
      if (this.fullscreenSceneIndex.HasValue) {
        // In fullscreen mode, we want to keep the current, previous and next scene loaded.
        requestScene(this.fullscreenSceneIndex.Value);
        requestScene(this.fullscreenSceneIndex.Value - 1);
        requestScene(this.fullscreenSceneIndex.Value + 1);
      } else {
        // In gallery mode, we want the current page of scenes to be loded
        for (int i = 0; i < numberOfItemsOnCurrentPage; i++) {
          int galleryEntryIndex = getGalleryEntryIndexForCellIndex(i);
          requestScene(galleryEntryIndex);
        }
      }

      if (this.requestedFullscreenSceneIndex.HasValue) {
        requestScene(this.requestedFullscreenSceneIndex.Value);
      }

      // Unload scenes that are not needed any more
      for (int i = 0; i < loadedScenes.Length; i++) {
        LoadedSceneData? loadedScene = loadedScenes[i];
        if (loadedScene.HasValue && !neededSceneIndices.Contains(i)) {
          unloadScene(i);
        }
      }
      if (this.currentlyLoadingSceneIndex.HasValue && 
          !neededSceneIndices.Contains(this.currentlyLoadingSceneIndex.Value)) {
        StopCoroutine(this.sceneLoadingCoroutine);
        unloadScene(this.currentlyLoadingSceneIndex.Value);
        this.currentlyLoadingSceneIndex = null;
        this.sceneLoadingCoroutine = null;
      }

      // Load requested scene that isn't loaded or being loaded yet
      if (!currentlyLoadingSceneIndex.HasValue) {
        while (sceneLoadRequests.Count > 0) {
          int nextSceneIndexToLoad = sceneLoadRequests[0];
          sceneLoadRequests.RemoveAt(0);
          if (loadedScenes[nextSceneIndexToLoad] == null) {
            currentlyLoadingSceneIndex = nextSceneIndexToLoad;
            sceneLoadingCoroutine = StartCoroutine(loadScene(nextSceneIndexToLoad));
            break;
          }
        }
      }

      for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++) {
        GalleryGridCell cell = cells[cellIndex];
        cell.gameObject.SetActive(cellIndex < numberOfItemsOnCurrentPage);
      }

      prevPageButton.interactable = currentPageIndex > 0;
      nextPageButton.interactable = currentPageIndex < totalNumberOfPages - 1;

      if (this.fullscreenSceneIndex.HasValue) {
        this.fullscreenPrevButton.interactable = this.fullscreenSceneIndex.Value > 0;
        this.fullscreenNextButton.interactable = this.fullscreenSceneIndex + 1 < galleryManager.gallery.entries.Count;
      }
      
      emptyGalleryInstructions.gameObject.SetActive(totalItemCount == 0);
      pageNumberLabel.SetText("Page {0}/{1}", currentPageIndex + 1, totalNumberOfPages);

      // Update the render texture of all cells that need an update
      if (this.fullscreenSceneIndex.HasValue && this.loadedScenes[this.fullscreenSceneIndex.Value].HasValue) {
        if (this.renderTextures[this.fullscreenRenderTextureIndex] == null) {
          this.renderTextures[this.fullscreenRenderTextureIndex] = new RenderTexture(
            width: Screen.width,
            height: Screen.height,
            depth: 16
          ); 
          this.fullscreenRawImage.texture = this.renderTextures[this.fullscreenRenderTextureIndex];
        }
        LoadedSceneData loadedSceneData = this.loadedScenes[this.fullscreenSceneIndex.Value].Value;
        loadedSceneData.camera.targetTexture = this.renderTextures[this.fullscreenRenderTextureIndex];
      } else {
        for (int cellIndex = 0; cellIndex < this.numberOfItemsOnCurrentPage; cellIndex++) {
          int galleryEntryIndex = getGalleryEntryIndexForCellIndex(cellIndex);
          if (!loadedScenes[galleryEntryIndex].HasValue) {
            continue;
          }
          GalleryGridCell cell = cells[cellIndex];
          LoadedSceneData loadedSceneData = loadedScenes[galleryEntryIndex].Value;

          RenderTexture renderTexture = renderTextures[cellIndex];
          if (renderTexture == null) {
            Canvas canvas = GetComponent<Canvas>();
            RectTransform cellTransform = cell.transform as RectTransform;
            renderTexture = new RenderTexture(
              width: (int)(cellTransform.sizeDelta.x * canvas.transform.localScale.x),
              height: (int)(cellTransform.sizeDelta.y * canvas.transform.localScale.y),
              depth: 16
            );
            renderTextures[cellIndex] = renderTexture;
          }
          loadedSceneData.camera.targetTexture = renderTexture;
          if (cell.rawImage.texture != renderTexture) {
            cell.rawImage.texture = renderTexture;
          }
        }
      }

      // Manually render the necessary scenes

      setAllObjectsToHiddenAndRememberOldLayersIfNecessary();
      for (int cellIndex = 0; cellIndex < numberOfItemsOnCurrentPage; cellIndex++) {
        int galleryEntryIndex = getGalleryEntryIndexForCellIndex(cellIndex);
        if (fullscreenSceneIndex.HasValue && galleryEntryIndex != fullscreenSceneIndex.Value){
          continue;
        }
        if (this.loadedScenes[galleryEntryIndex] == null) {
          continue;
        }
        LoadedSceneData loadedSceneData = this.loadedScenes[galleryEntryIndex].Value;
        for (int i = 0; i < loadedSceneData.allObjects.Count; i++) {
          PerObjectData perObjectData = loadedSceneData.allObjects[i];
          if (perObjectData.gameObject != null) {
            perObjectData.gameObject.layer = perObjectData.layer;
            if (perObjectData.isDynamicObjectSpawner) {
              foreach (UnityEngine.Transform childTransform in perObjectData.gameObject.transform) {
                childTransform.gameObject.layer = perObjectData.layer;
              }
            }
          }
        }

        loadedSceneData.camera.Render();

        for (int i = 0; i < loadedSceneData.allObjects.Count; i++) {
          PerObjectData perObjectData = loadedSceneData.allObjects[i];
          if (perObjectData.gameObject != null) {
            perObjectData.gameObject.layer = hiddenLayer;
            if (perObjectData.isDynamicObjectSpawner) {
              foreach (UnityEngine.Transform childTransform in perObjectData.gameObject.transform) {
                childTransform.gameObject.layer = hiddenLayer;
              }
            }
          }
        }
      }
    }

    private void setAllObjectsToHiddenAndRememberOldLayersIfNecessary() {
      foreach (int sceneIndex in neededSceneIndices) {
        if (this.loadedScenes[sceneIndex] == null) {
          continue;
        }
        LoadedSceneData loadedSceneData = this.loadedScenes[sceneIndex].Value;
        for (int i = 0; i < loadedSceneData.allObjects.Count; i++) {
          PerObjectData perObjectData = loadedSceneData.allObjects[i];
          if (perObjectData.gameObject != null) {
            if (perObjectData.gameObject.layer != hiddenLayer) {
              perObjectData.layer = perObjectData.gameObject.layer;
            }
            perObjectData.gameObject.layer = hiddenLayer;
            if (perObjectData.isDynamicObjectSpawner) {
              foreach (UnityEngine.Transform childTransform in perObjectData.gameObject.transform) {
                childTransform.gameObject.layer = hiddenLayer;
              }
            }
            loadedSceneData.allObjects[i] = perObjectData;
          }
        }
      }
    }

    private void requestScene(int sceneIndex) {
      if (!sceneIndexIsValid(sceneIndex)) {
        return;
      }
      if (!neededSceneIndices.Contains(sceneIndex)) {
        neededSceneIndices.Add(sceneIndex);
        sceneLoadRequests.Add(sceneIndex);
      }
    }

    private void requestNewFullscreenIndexIfPossible(int newFullscreenIndex) {
      if (!sceneIndexIsValid(newFullscreenIndex)) {
        return;
      }
      this.requestedFullscreenSceneIndex = newFullscreenIndex;
    }

    private bool sceneIndexIsValid(int sceneIndex) {
      return sceneIndex >= 0 && sceneIndex < loadedScenes.Length;
    }

    private int getGalleryEntryIndexForCellIndex(int cellIndex) {
      int numberOfItemsPerPage = Math.Max(1, this.cells.Length);
      int firstItemIndexOnPage = currentPageIndex * numberOfItemsPerPage;
      int galleryEntryIndex = firstItemIndexOnPage + cellIndex; 
      return galleryEntryIndex;
    }

    private int getPageIndexForGalleryEntry(int galleryEntryIndex) {
      int numberOfItemsPerPage = Math.Max(1, this.cells.Length);
      return galleryEntryIndex / numberOfItemsPerPage;
    }

    private int getCellIndexForGalleryEntry(int galleryEntryIndex, int pageIndex) {
      int numberOfItemsPerPage = Math.Max(1, this.cells.Length);
      return galleryEntryIndex - pageIndex * numberOfItemsPerPage;
    }

    private CreatureGalleryEntry getAndLoadGalleryEntry(int galleryEntryIndex) {
      galleryManager.loadGalleryEntry(galleryEntryIndex);
      return this.galleryManager.gallery.entries[galleryEntryIndex];
    }

    private CreatureRecording getRecording(int galleryEntryIndex) {
      CreatureGalleryEntry galleryEntry = getAndLoadGalleryEntry(galleryEntryIndex);
      if (galleryEntry.loadedData == null) {
        return null;
      }
      return galleryEntry.loadedData.recording;
    }

    private Creature getFullscreenPlaybackCreature() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return null;
      }
      if (this.loadedScenes[this.fullscreenSceneIndex.Value] == null) {
        return null;
      }
      return this.loadedScenes[this.fullscreenSceneIndex.Value].Value.creature;
    }

    private IEnumerator loadScene(int galleryEntryIndex) {
      CreatureRecording recording = getRecording(galleryEntryIndex);
      if (recording == null) {
        Debug.LogWarning($"Gallery entry at absolute index {galleryEntryIndex} not loaded!");
        yield break;
      }

      var sceneLoadContext = new SceneController.SimulationSceneLoadContext();
      sceneLoadContext.DisableAllRenderers = true;
        
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
      LoadedSceneData loadedSceneData = new LoadedSceneData();
      loadedSceneData.scene = scene;
      loadedSceneData.creature = sceneLoadContext.Creatures[0];
      loadedSceneData.physicsScene = sceneLoadContext.PhysicsScene;

      var prevActiveScene = SceneManager.GetActiveScene();
      SceneManager.SetActiveScene(scene);
      
      var rootObjects = scene.GetRootGameObjects();
      loadedSceneData.allObjects = new List<PerObjectData>();
      foreach (GameObject rootObject in rootObjects) {
        // GetComponentsInChildren includes the root object
        foreach (UnityEngine.Transform childTransform in rootObject.GetComponentsInChildren<UnityEngine.Transform>()) {
          bool isDynamicObjectSpawner = false;
          if (recording.task == Objective.ObstacleJump) { 
              RollingObstacleSpawnerBehaviour obstacleSpawner = childTransform.GetComponent<RollingObstacleSpawnerBehaviour>();
              if (obstacleSpawner != null) { 
                isDynamicObjectSpawner = true;
                loadedSceneData.needsPhysicsSimulation = true;
              }
          }
          loadedSceneData.allObjects.Add(new PerObjectData {
            layer = childTransform.gameObject.layer,
            gameObject = childTransform.gameObject,
            isDynamicObjectSpawner = isDynamicObjectSpawner
          });
        }
      }
      foreach (GameObject rootObject in rootObjects) {
        if (rootObject.name == "Main Camera") {
          Camera mainCamera = rootObject.GetComponent<Camera>();
          mainCamera.enabled = false;
          loadedSceneData.camera = mainCamera;

          SimulationBackgroundRenderer backgroundRenderer = mainCamera.GetComponent<SimulationBackgroundRenderer>();
          backgroundRenderer.task = recording.task;

        } else if (rootObject.name == "GalleryPlaybackController") {
          GalleryPlaybackController playbackController = rootObject.GetComponent<GalleryPlaybackController>();
          CreatureRecordingPlayer player = new CreatureRecordingPlayer(recording: recording.movementData);
          loadedSceneData.recordingPlayer = player;
          for (int i = 0; i < loadedSceneData.allObjects.Count; i++) {
            if (loadedSceneData.allObjects[i].isDynamicObjectSpawner) {
              if (player.structuresToReset == null) {
                player.structuresToReset = new List<IResettableStructure>();
              }
              player.structuresToReset.Add(loadedSceneData.allObjects[i].gameObject.GetComponent<IResettableStructure>());
            }
          }
          playbackController.Setup(sceneLoadContext.Creatures[0], player);
          playbackController.Play();
        }
      }

      SceneManager.SetActiveScene(prevActiveScene);

      // The gallery size might have reduced while this scene was loading…
      if (this.loadedScenes.Length > galleryEntryIndex) {
        this.loadedScenes[galleryEntryIndex] = loadedSceneData;
      }

      this.sceneLoadingCoroutine = null;
      this.currentlyLoadingSceneIndex = null;
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
      neededSceneIndices.Clear();
      galleryManager.unloadAllGalleryEntries();
      for (int i = 0; i < loadedScenes.Length; i++) {
        unloadScene(i);
      }
    }

    private void unloadScene(int index) {
      if (index < 0 || index >= loadedScenes.Length || loadedScenes[index] == null) {
        return;
      }
      LoadedSceneData loadedSceneData = loadedScenes[index].Value;
      foreach (PerObjectData obj in loadedSceneData.allObjects) {
        if (obj.gameObject != null) {
          // We must manually disable all objects in this scene before it starts asynchronously
          // unloading, otherwise its objects will show up in the manual camera renders
          // of the loaded scenes (since we won't have a reference to this scene any more
          // in order to manually change the object layers before rendering…)
          obj.gameObject.SetActive(false);
        }
      }
      SceneManager.UnloadSceneAsync(loadedSceneData.scene);
      galleryManager.unloadGalleryEntry(index);
      this.loadedScenes[index] = null;
    }

    private void exitFullscreen() {
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
            int cellsPerPage = this.cells.Length;
            pageNumberOfLastImportedRecording = galleryIndex / cellsPerPage;
            break;
          }
        }
        currentPageIndex = pageNumberOfLastImportedRecording;
      }

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
      CreatureGalleryEntry galleryEntry = galleryManager.gallery.entries[this.fullscreenSceneIndex.Value];
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
      CreatureGalleryEntry galleryEntry = galleryManager.gallery.entries[this.fullscreenSceneIndex.Value];

      deleteConfirmation.ConfirmDeletionFor(galleryEntry.filename, delegate(string name) {
        exitFullscreen();
        CreatureRecordingSerializer.DeleteCreatureRecording(galleryEntry.filename);
        unloadScenes();
        galleryManager.shallowLoadGalleryEntries();
      });
    }

    private void refreshStatsLabel() {
      if (!this.fullscreenSceneIndex.HasValue) {
        return;
      }
      if (!this.showStatsButtonLabel.gameObject.activeSelf) {
        return;
      }

      CreatureRecording recording = getRecording(this.fullscreenSceneIndex.Value);

      var fitnessPercentage = Mathf.Round(recording.stats.fitness * 1000f) / 10f;

      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine(recording.creatureDesign.Name);
      stringBuilder.AppendLine(string.Format("Fitness: {0}%", fitnessPercentage));
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

    #region ISimulationVisibilityOptionsViewDelegate

    public void ShowMuscleContractionDidChange(SimulationVisibilityOptionsView view, bool showMuscleContraction) {
      Creature creature = getFullscreenPlaybackCreature();
      if (creature == null) {
        return;
      }
      creature.RefreshMuscleContractionVisibility(Settings.ShowMuscleContraction);
    }

    public void ShowMusclesDidChange(SimulationVisibilityOptionsView view, bool showMuscles) {}

    public Objective GetCurrentTask(SimulationVisibilityOptionsView view) {
      if (!this.fullscreenSceneIndex.HasValue) {
        return Objective.Running;
      }
      CreatureRecording recording = getRecording(this.fullscreenSceneIndex.Value);
      if (recording == null) {
        return Objective.Running;
      }
      return recording.task;
    }

    #endregion

    public void Show() {
        gameObject.SetActive(true);
        InputRegistry.shared.Register(InputType.AndroidBack, this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;

        editorCamera.InteractiveZoomEnabled = false;
        
        galleryManager.shallowLoadGalleryEntries();
    }

    public void Hide() {
        InputRegistry.shared.Deregister(this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture -= OnAndroidBack;
        gameObject.SetActive(false);

        editorCamera.InteractiveZoomEnabled = true;

        releaseRenderResources();
    }

    private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
        if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
            Hide();
    }
  }

}