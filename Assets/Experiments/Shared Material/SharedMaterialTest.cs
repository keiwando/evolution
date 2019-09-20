using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedMaterialTest : MonoBehaviour {
    
    void Start() {

        CreateFromPrefab();
        CreateFromTestPrefab();
        CreateManually();
    }

    private void CreateFromPrefab() {

        var material = Resources.Load("Materials/Joint Color") as Material;

        var joint = Joint.CreateFromData(
            new JointData(
                0, 
                new Vector2(0, 0),
                1f
            )
        );

        var renderer = joint.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
    }

    private void CreateFromTestPrefab() {

        var material = Resources.Load("Materials/Joint Color") as Material;

        var obj = Instantiate(Resources.Load("Prefabs/Joint")) as GameObject;

        var renderer = obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
    }

    private void CreateManually() {

        var material = Resources.Load("Materials/Joint Color") as Material;

        var obj = new GameObject();

        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
    }
}
