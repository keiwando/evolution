using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class HiddenCreatureRenderer: MonoBehaviour {

    private new Camera camera;
    private Camera hiddenCamera;

    [SerializeField] private RenderTexture debugRenderTexture;

    void Start() {
        this.camera = GetComponent<Camera>();
        this.hiddenCamera = Instantiate(camera.gameObject, transform.position, transform.rotation, transform).GetComponent<Camera>();
        hiddenCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
        hiddenCamera.cullingMask = (1 << 11) | (1 << 8);
        hiddenCamera.backgroundColor = Color.clear;
        Destroy(hiddenCamera.GetComponent<HiddenCreatureRenderer>());
        Destroy(hiddenCamera.GetComponent<CameraFollowScript>());
        // SetupRenderPass();
    }

    void Update() {
        CopyCameraProperties();
        // SetupRenderPass();
        hiddenCamera.Render();
        Render();
        camera.Render();
        camera.RemoveAllCommandBuffers();
    }

    private void CopyCameraProperties() {
        hiddenCamera.orthographicSize = camera.orthographicSize;
    }

    private void Render() {

        var commandBuffer = new CommandBuffer();

        // commandBuffer.Blit(hiddenCamera.targetTexture, null as RenderTexture);
        commandBuffer.Blit(hiddenCamera.targetTexture, debugRenderTexture);

        camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
    }

    private void SetupRenderPass() {

        var commandBuffer = new CommandBuffer();

        var targetWidth = camera.targetTexture?.width ?? Screen.width;
        var targetHeight = camera.targetTexture?.height ?? Screen.height;
        var renderTexture = debugRenderTexture ?? new RenderTexture(targetWidth, targetHeight, 0);

        var oldTarget = BuiltinRenderTextureType.CurrentActive;
        

        commandBuffer.Blit(null as RenderTexture, renderTexture);
        commandBuffer.SetRenderTarget(renderTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        
        var allObjects = FindObjectsOfType<GameObject>();

        var hiddenObjects = new List<Renderer>();
        for (int i = 0; i < allObjects.Length; i++) {
            var obj = allObjects[i];
            if (obj.layer == 8 || obj.layer == 11) {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null) {
                    hiddenObjects.Add(renderer);
                }
            }
        }
        // Debug.Log(allObjects.Length);
        // Debug.Log(hiddenObjects.Count);

        for (int i = 0; i < hiddenObjects.Count; i++) {
            var renderer = hiddenObjects[i];
            commandBuffer.DrawRenderer(renderer, renderer.material);
        }

        var quad = GetQuad();
        var shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.SetTexture("_MainTex", renderTexture);
        material.SetColor("_Tint", new Color(1f, 1f, 1f, Settings.HiddenCreatureOpacity));

        // commandBuffer.Blit(oldTarget, renderTexture);
        commandBuffer.Blit(renderTexture, null as RenderTexture);
        

        commandBuffer.SetRenderTarget(oldTarget);
        // commandBuffer.DrawMesh(quad, Matrix4x4.identity, material);

        camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, commandBuffer);
    }

    private static Mesh GetQuad() {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f)
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