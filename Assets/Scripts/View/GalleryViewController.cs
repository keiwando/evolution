using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Keiwando;
using Keiwando.UI;

namespace Keiwando.Evolution.UI {

  public class GalleryViewController: MonoBehaviour {

    [SerializeField]
    private CustomGridLayoutGroup grid;

    [SerializeField]
    private GalleryGridCell templateGridCell;

    private GalleryGridCell[] cells;
    private RenderTexture[] renderTextures;
    private Scene?[] scenes;

    [SerializeField]
    private Button closeButton;

    void Start() {

      int numberOfItemsPerPage = grid.ColumnCount * grid.RowCount;
      this.cells = new GalleryGridCell[numberOfItemsPerPage];
      this.renderTextures = new RenderTexture[numberOfItemsPerPage];
      this.scenes = new Scene?[numberOfItemsPerPage];

      for (int i = 0; i < numberOfItemsPerPage; i++) {
        var cell = Instantiate(templateGridCell, grid.transform);
        cell.gameObject.SetActive(true);
        cells[i] = cell;
      }
      templateGridCell.gameObject.SetActive(false);

      StartCoroutine(loadGalleryScenes());

      closeButton.onClick.AddListener(delegate () {
          Hide();
      });
    }

    void OnDestroy() {  
      releaseRenderResources();
    }

    private IEnumerator loadGalleryScenes() {
      for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++) {
        GalleryGridCell cell = cells[cellIndex];
        if (cell == null) {
          continue;
        }
        // Load Scene
        var sceneName = "DummyScene";
        var options = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
        
        var scene = SceneManager.LoadScene(sceneName, options);
        this.scenes[cellIndex] = scene;

        // var scene = SceneManager.GetSceneByName(sceneName);
        var PhysicsScene = scene.GetPhysicsScene();
        // context.Scene = scene;
        // We need to wait before the scene and its GameObjects
        // are fully loaded
        while(!scene.isLoaded) {
            yield return new WaitForEndOfFrame();
        }
        // Find setup script in the new scene
        var prevActiveScene = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        
        SimulationSceneSetup sceneSetup = null;
        var rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++) {
            sceneSetup = rootObjects[i].GetComponent<SimulationSceneSetup>();
            if (sceneSetup != null) break;
        }
        // DEBUG:
        foreach (GameObject rootObject in rootObjects) {
          if (rootObject.name == "Main Camera") {
            Camera mainCamera = rootObject.GetComponent<Camera>();
            RenderTexture renderTexture = renderTextures[cellIndex];
            if (renderTexture == null) {
              RectTransform cellTransform = cell.transform as RectTransform;
              renderTexture = new RenderTexture(width: (int)cellTransform.sizeDelta.x, height: (int)cellTransform.sizeDelta.y, 0);
              renderTextures[cellIndex] = renderTexture;
            }
            mainCamera.targetTexture = renderTexture;
            cell.rawImage.texture = renderTexture;

            // DEBUG:
            mainCamera.backgroundColor = new Color(Random.value, Random.value, Random.value);

            break;
          }
        }

        // Create the structures
        // sceneSetup.BuildScene(sceneDescription, sceneContext);

        SceneManager.SetActiveScene(prevActiveScene);
        yield return new WaitForFixedUpdate();
        SceneManager.SetActiveScene(scene);

        // Spawn Creatures
        // var spawnOptions = new SimulationSpawnConfig(
        //     creatureDesign, 
        //     creatureSpawnCount,
        //     context.PhysicsScene,
        //     sceneContext,
        //     legacyOptions,
        //     sceneDescription
        // );
        // var creatures = sceneSetup.SpawnBatch(spawnOptions);

        SceneManager.SetActiveScene(prevActiveScene);
        // context.Creatures = creatures;
        yield return null;
      }
    }

    private void releaseRenderResources() {

      for (int i = 0; i < renderTextures.Length; i++) {
        if (renderTextures[i] != null) {
          renderTextures[i].Release();
          renderTextures[i] = null;
        }
      }
      for (int i = 0; i < scenes.Length; i++) {
        Scene? scene = scenes[i];
        if (scene.HasValue) {
          SceneManager.UnloadSceneAsync(scene.Value);
          scenes[i] = null;
        }
      }
      foreach (GalleryGridCell cell in cells) {
        cell.rawImage.texture = null;
      }
    }

    public void Refresh() {

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
    }

    private void OnAndroidBack(AndroidBackButtonGestureRecognizer rec) {
        if (InputRegistry.shared.MayHandle(InputType.AndroidBack, this))
            Hide();
    }
  }

}