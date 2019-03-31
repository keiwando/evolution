using UnityEngine;
using System;

public class DistanceMarker: MonoBehaviour {

    public TextMesh Text {
        get { return text; }
    } 
    [SerializeField]
    private TextMesh text;
}