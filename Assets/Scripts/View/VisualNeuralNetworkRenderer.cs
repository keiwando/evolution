using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VisualNeuralNetworkRenderer: MonoBehaviour {

    [SerializeField] private Texture2D nodeTexture;
    [SerializeField] private Texture2D connectionTexture;

    [SerializeField] private RenderTexture targetTexture;

    // void Start() {
    //     var testNetworkSettings = NeuralNetworkSettings.Default;
    //     // testNetworkSettings = new NeuralNetworkSettings(new [] { 20, 10, 2, 13, 5 });
    //     // testNetworkSettings = new NeuralNetworkSettings(new [] { 20, 10, 40, 40, 5 });
    //     // testNetworkSettings = new NeuralNetworkSettings(new [] { 100, 100, 100, 100, 100, 100, 100, 100 });
    //     Render(testNetworkSettings, targetTexture);
    //     // StartCoroutine(RenderDelayed(testNetworkSettings, targetTexture, 3));
    // }

    public IEnumerator RenderDelayed(NeuralNetworkSettings network, RenderTexture renderTexture, float delay) {
        yield return new WaitForSeconds(delay);
        Render(network, renderTexture);
    }

    public void Render(NeuralNetworkSettings network, RenderTexture renderTexture) {

        var nodeShader = Shader.Find("Sprites/Default");
        var nodeMaterial = new Material(nodeShader);
        nodeMaterial.color = Color.white;
        nodeMaterial.enableInstancing = true;
        nodeMaterial.SetTexture("_MainTex", nodeTexture);

        var connectionMaterial = new Material(nodeShader);
        // connectionMaterial.color = new Color(1, 0.647f, 0.647f);
        connectionMaterial.enableInstancing = true;
        connectionMaterial.SetTexture("_MainTex", connectionTexture);

        var quad = GetQuad();

        var projMatrix = Matrix4x4.TRS(Vector3.zero, 
                                       Quaternion.identity, 
                                       new Vector3(2f / renderTexture.width, 2f / renderTexture.height, 1f));
        var nodeMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(100, 100, 1));

        // Create Node Mesh

        var layerSizes = new int[network.NumberOfIntermediateLayers + 2];
        layerSizes[0] = 1;
        layerSizes[layerSizes.Length - 1] = 1;
        for (int i = 1; i < layerSizes.Length - 1; i++) {
            layerSizes[i] = network.NodesPerIntermediateLayer[i - 1];
        }
        var nodesList = new List<Matrix4x4[]>(layerSizes.Length);
        nodesList.Add(new [] { MatrixForNode(0, 0, layerSizes, renderTexture) });

        for (int i = 1; i < layerSizes.Length - 1; i++) {
            int layerSize = layerSizes[i];
            var matricesForLayer = new Matrix4x4[layerSize];

            for (int j = 0; j < layerSize; j++) {
                matricesForLayer[j] = MatrixForNode(i, j, layerSizes, renderTexture);
            }
            nodesList.Add(matricesForLayer);
        }

        nodesList.Add(new [] { MatrixForNode(layerSizes.Length - 1, 0, layerSizes, renderTexture) });

        var allNodes = new Matrix4x4[layerSizes.Sum()];
        int nodeIndex = 0;
        foreach (var layer in nodesList) {
            foreach (var node in layer) {
                allNodes[nodeIndex++] = node;
            }
        }

        // Create Connection Mesh

        int numberOfConnections = 0;
        for (int i = 0; i < nodesList.Count - 1; i++) {
            numberOfConnections += layerSizes[i] * layerSizes[i + 1];
        }

        var connections = new Matrix4x4[numberOfConnections];
        int listIndex = 0;
        for (int i = 0; i < nodesList.Count - 1; i++) {
            // Create connections between layers i and i + 1
            for (int l = 0; l < layerSizes[i]; l++) {
                for (int m = 0; m < layerSizes[i + 1]; m++) {
                    var prevMatrix = nodesList[i][l];
                    var nextMatrix = nodesList[i + 1][m];
                    
                    connections[listIndex++] = MatrixForConnection(prevMatrix, nextMatrix);
                }
            }
        }

        // Setup CommandBuffer

        var commandBuffer = new CommandBuffer();
        commandBuffer.SetViewport(new Rect(0, 0, renderTexture.width, renderTexture.height));
        commandBuffer.SetRenderTarget(new RenderTargetIdentifier(renderTexture));
        commandBuffer.ClearRenderTarget(true, true, Color.clear);

        commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, projMatrix);


        var rnd = new System.Random();
        
        // Perform Fisher-Yates shuffle (https://en.wikipedia.org/wiki/Fisher–Yates_shuffle)
        for (int i = connections.Length - 1; i >= 1; i--) {
            int random = rnd.Next(i);
            var tmp = connections[i];
            connections[i] = connections[random];
            connections[random] = tmp;
        }
        
        for (int i = 0; i < connections.Length; i++) {
            commandBuffer.DrawMesh(quad, connections[i], connectionMaterial);
        }

        for (int i = 0; i < allNodes.Length; i++) {
            commandBuffer.DrawMesh(quad, allNodes[i], nodeMaterial);
        }

        Graphics.ExecuteCommandBuffer(commandBuffer);
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

    private static Matrix4x4 MatrixForNode(int layer, int indexInLayer, int[] layerSizes, RenderTexture target) {

        int largestLayerSize = Math.Max(1, layerSizes.Max());
        int nodeHeight = (target.height - largestLayerSize) / Math.Max(10, largestLayerSize);
        int layerSize = layerSizes[layer];
        int layerCount = layerSizes.Length;
        float topNodeY = 0.5f * ((layerSize - 1) * (nodeHeight + 1));
        // The distance between two nodes in adjacent layers on the x axis
        int dLayerX = (target.width - nodeHeight) / (layerCount - 1);
        // The vertical distance between two adjacent nodes in the same layer
        int dNodeY = 1;
        
        float x = (float)(layer * dLayerX + 0.5 * nodeHeight) - target.width * 0.5f;
        float y = (float)(topNodeY - indexInLayer * (nodeHeight + dNodeY));
        
        return Matrix4x4.TRS(new Vector3(x, y), Quaternion.identity, new Vector3(nodeHeight, nodeHeight, 1f));
    }

    private static Matrix4x4 MatrixForConnection(Matrix4x4 startNodeMat, Matrix4x4 endNodeMat, float heightFactor = 0.3f) {

        var nodeHeight = startNodeMat.m00;
        var startPos = TranslationFromMatrix(startNodeMat);
        var endPos = TranslationFromMatrix(endNodeMat);

        var pos = new Vector3(
            (startPos.x + endPos.x) * 0.5f,
            (startPos.y + endPos.y) * 0.5f,
            (startPos.z + endPos.z) * 0.5f
        );

        var length = Vector3.Distance(startPos, endPos);
        var height = nodeHeight * heightFactor;

        var rotation = (float)Math.Atan2(endPos.y - startPos.y, endPos.x - startPos.x);

        return Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, rotation * 180f / (float)Math.PI), new Vector3(length, height));
    }

    private static Vector3 TranslationFromMatrix(Matrix4x4 mat) {
        return new Vector3(mat.m03, mat.m13, mat.m23);
    }
}