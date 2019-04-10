using System;
using UnityEngine;

[Serializable]
public struct JointData {

    public readonly int id;

    public readonly Vector2 position;
    public readonly float weight;

    public JointData(int id, Vector2 position, float weight) {
        this.id = id;
        this.position = position;
        this.weight = weight;
    }
}