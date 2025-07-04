using UnityEngine;

class AutoRotate: MonoBehaviour {

    [SerializeField] private float rotationSpeed = 10f;

    void Update() {
        // Rotate the object around its local Y axis at the specified speed
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}