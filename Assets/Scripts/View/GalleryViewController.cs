using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Keiwando;
using Keiwando.UI;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution.UI {

  public class GalleryViewController: MonoBehaviour {

    [SerializeField]
    private CustomGridLayoutGroup grid;

    [SerializeField]
    private GalleryGridCell templateGridCell;

    private GalleryGridCell[] cells;
    private RenderTexture[] renderTextures;
    private Scene?[] scenes;
    private Camera[] cameras;
    struct PerObjectData {
      public int layer;
      public GameObject gameObject;
    }
    // Note: This only contains objects that were active at the time the scene finished loading.
    private List<PerObjectData>[] allObjectsPerScene;

    private int currentPageIndex = 0;
    private int numberOfItemsOnCurrentPage = 0;
    private int sceneLoadingInitiatedForPageIndex = -1;
    private Coroutine sceneLoadingCoroutine;

    private int hiddenLayer;

    [SerializeField]
    private Button closeButton;

    private CreatureGalleryManager galleryManager = new CreatureGalleryManager();

    private bool initialized = false;

    void Start() {

      galleryManager.loadGalleryEntries();
      hiddenLayer = LayerMask.NameToLayer("Hidden");

      int numberOfItemsPerPage = grid.ColumnCount * grid.RowCount;
      this.cells = new GalleryGridCell[numberOfItemsPerPage];
      this.renderTextures = new RenderTexture[numberOfItemsPerPage];
      this.scenes = new Scene?[numberOfItemsPerPage];
      this.cameras = new Camera[numberOfItemsPerPage];
      this.allObjectsPerScene = new List<PerObjectData>[numberOfItemsPerPage];
      for (int i = 0; i < numberOfItemsPerPage; i++) {
        this.allObjectsPerScene[i] = new List<PerObjectData>();
      }

      for (int i = 0; i < numberOfItemsPerPage; i++) {
        var cell = Instantiate(templateGridCell, grid.transform);
        cell.gameObject.SetActive(true);
        cells[i] = cell;
      }
      templateGridCell.gameObject.SetActive(false);

      closeButton.onClick.AddListener(delegate () {
          Hide();
      });

      initialized = true;

      Refresh();
    }

    void OnDestroy() {  
      releaseRenderResources();
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

      // TODO: load the gallery entries of the current page

      bool needsSceneLoading = sceneLoadingInitiatedForPageIndex != currentPageIndex;
      if (needsSceneLoading) {
        if (sceneLoadingCoroutine != null) {
          StopCoroutine(sceneLoadingCoroutine);
        }
        sceneLoadingCoroutine = StartCoroutine(loadGalleryScenes());
        sceneLoadingInitiatedForPageIndex = currentPageIndex;
      }
    }

    private IEnumerator loadGalleryScenes() {
      
      int numberOfItemsPerPage = Math.Max(1, grid.ColumnCount * grid.RowCount);
      int firstItemIndexOnPage = currentPageIndex * numberOfItemsPerPage;

      for (int cellIndex = 0; cellIndex < this.numberOfItemsOnCurrentPage; cellIndex++) {
        GalleryGridCell cell = cells[cellIndex];
        if (cell == null) {
          continue;
        }
        LoadedCreatureGalleryEntry galleryEntry = this.galleryManager.gallery.entries[firstItemIndexOnPage + cellIndex].loadedData;
        if (galleryEntry == null) {
          Debug.LogWarning($"Gallery entry for cell {cellIndex} not loaded in time!");
          continue;
        }
        CreatureRecording recording = galleryEntry.recording;

        var sceneLoadContext = new SceneController.SimulationSceneLoadContext();
        
        yield return SceneController.LoadSimulationScene(
          creatureDesign: recording.creatureDesign,
          creatureSpawnCount: 1,
          sceneDescription: recording.sceneDescription,
          sceneType: SceneController.SimulationSceneType.GalleryPlayback,
          legacyOptions: LegacySimulationOptions.Default,
          context: sceneLoadContext,
          sceneContext: new GalleryPlaybackSceneContext()
        );
        var scene = sceneLoadContext.Scene;
        this.scenes[cellIndex] = scene;

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
      for (int i = 0; i < scenes.Length; i++) {
        Scene? scene = scenes[i];
        if (scene.HasValue) {
          SceneManager.UnloadSceneAsync(scene.Value);
          scenes[i] = null;
        }
      }
      for (int i = 0; i < cameras.Length; i++) {
        this.cameras[i] = null;
      }
      for (int i = 0; i < allObjectsPerScene.Length; i++) {
        if (this.allObjectsPerScene[i] != null) {
          this.allObjectsPerScene[i].Clear();
        }
      }
    }
    
    public void Show() {
        gameObject.SetActive(true);
        InputRegistry.shared.Register(InputType.AndroidBack, this);
        GestureRecognizerCollection.shared.GetAndroidBackButtonGestureRecognizer().OnGesture += OnAndroidBack;
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
  }

}