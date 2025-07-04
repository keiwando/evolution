using UnityEngine;
using UnityEngine.Rendering;
using Keiwando.Evolution;
using Keiwando.Evolution.Scenes;

[RequireComponent(typeof(Camera))]
public class SimulationBackgroundRenderer: MonoBehaviour {

    [SerializeField] private bool renderHiddenCreatures;

    [SerializeField] private Material backgroundGridMaterial;
    [SerializeField] private Material backgroundCreatureMaterial;
    [SerializeField] private Evolution evolution;

    private Color backgroundColor = new Color(0.93f, 0.93f, 0.93f);

    private new Camera camera;
    private Camera hiddenCamera;
    private Mesh quad;
    private CommandBuffer commandBuffer;

    void Start() {
        this.camera = GetComponent<Camera>();
        if (this.renderHiddenCreatures) {
            this.hiddenCamera = Instantiate(camera.gameObject, transform.position, transform.rotation, transform).GetComponent<Camera>();
            hiddenCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
            hiddenCamera.cullingMask = (1 << 11) | (1 << 8);
            hiddenCamera.backgroundColor = Color.clear;
            Destroy(hiddenCamera.GetComponent<SimulationBackgroundRenderer>());
            Destroy(hiddenCamera.GetComponent<TrackedCamera>());
            
            backgroundCreatureMaterial.SetTexture("_MainTex", hiddenCamera.targetTexture);
        }
        
        commandBuffer = new CommandBuffer();
        camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
        
        quad = CreateQuad();
    }

    void OnDestroy() {
        if (this.hiddenCamera != null && this.hiddenCamera.targetTexture != null) {
            this.hiddenCamera.targetTexture.Release();
        }
    }

    void Update() {
        if (renderHiddenCreatures && hiddenCamera != null) {
            hiddenCamera.orthographicSize = camera.orthographicSize;
            this.backgroundCreatureMaterial.SetFloat("_Opacity", Settings.HiddenCreatureOpacity);
            this.backgroundCreatureMaterial.SetColor("_BackgroundColor", backgroundColor);
        }

        Objective task = evolution.SimulationData.Settings.Objective;
        float gridVisibility = task == Objective.Flying ? Settings.FlyingGridVisibility : Settings.DefaultGridVisibility;
        
        this.backgroundGridMaterial.SetColor("_BackgroundColor", backgroundColor); 
        this.backgroundGridMaterial.SetFloat("_GridVisibility", gridVisibility);
        
        commandBuffer.Clear();

        Matrix4x4 viewMat = camera.worldToCameraMatrix;
        Matrix4x4 projMat = camera.projectionMatrix;
        Vector3 cameraBottomLeftInWorldSpace = camera.ViewportToWorldPoint(new Vector3(0, 0));
        Vector3 cameraTopRightInWorldSpace = camera.ViewportToWorldPoint(new Vector3(1, 1));
        float cameraWidthInWorldSpace = cameraTopRightInWorldSpace.x - cameraBottomLeftInWorldSpace.x;
        float cameraHeightInWorldSpace = cameraTopRightInWorldSpace.y - cameraBottomLeftInWorldSpace.y;
        Matrix4x4 quadScaleMat = Matrix4x4.TRS(
            pos: camera.transform.position, 
            q: Quaternion.identity, 
            s: new Vector3(cameraWidthInWorldSpace, cameraHeightInWorldSpace, 1f)
        );
        commandBuffer.SetViewProjectionMatrices(view: viewMat, proj: projMat);
        commandBuffer.DrawMesh(quad, quadScaleMat, backgroundGridMaterial);

        if (renderHiddenCreatures && backgroundCreatureMaterial != null) {
            commandBuffer.SetViewProjectionMatrices(view: Matrix4x4.Scale(new Vector3(2f, 2f)), proj: Matrix4x4.identity);
            commandBuffer.DrawMesh(quad, Matrix4x4.identity, backgroundCreatureMaterial);
        }
    }

    private static Mesh CreateQuad() {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 10f),
            new Vector3(0.5f, -0.5f, 10f),
            new Vector3(-0.5f, 0.5f, 10f),
            new Vector3(0.5f, 0.5f, 10f)
        };
        mesh.uv = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        return mesh;
    }
}