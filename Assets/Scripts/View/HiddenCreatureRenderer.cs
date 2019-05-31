using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class HiddenCreatureRenderer: MonoBehaviour {

    [SerializeField] private RenderTexture debugRenderTexture;

    private new Camera camera;
    private Camera hiddenCamera;

    private Material material;

    void Start() {
        this.camera = GetComponent<Camera>();
        this.hiddenCamera = Instantiate(camera.gameObject, transform.position, transform.rotation, transform).GetComponent<Camera>();
        hiddenCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 0);
        hiddenCamera.cullingMask = (1 << 11) | (1 << 8);
        hiddenCamera.backgroundColor = Color.clear;
        Destroy(hiddenCamera.GetComponent<HiddenCreatureRenderer>());
        Destroy(hiddenCamera.GetComponent<CameraFollowScript>());
        
        SetupRenderPass();
    }

    void Update() {
        CopyCameraProperties();
        UpdateMaterial();
    }

    private void CopyCameraProperties() {
        hiddenCamera.orthographicSize = camera.orthographicSize;
    }

    private void UpdateMaterial() {
        this.material.color = new Color(1f, 1f, 1f, Settings.HiddenCreatureOpacity);
    }

    private void SetupRenderPass() {

        var commandBuffer = new CommandBuffer();

        // commandBuffer.Blit(hiddenCamera.targetTexture, debugRenderTexture);

        var quad = GetQuad();
        var shader = Shader.Find("Sprites/Default");
        var material = new Material(shader);
        material.SetTexture("_MainTex", hiddenCamera.targetTexture);
        material.color = new Color(1f, 1f, 1f, Settings.HiddenCreatureOpacity);
        this.material = material;

        commandBuffer.SetViewProjectionMatrices(Matrix4x4.Scale(new Vector3(2f, 2f)), Matrix4x4.identity);
        commandBuffer.DrawMesh(quad, Matrix4x4.identity, material);

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