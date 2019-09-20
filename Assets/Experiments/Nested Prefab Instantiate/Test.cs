using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NestedPlatformInstantiate {

    public class Test: MonoBehaviour {

        [SerializeField] private GameObject parent;
        [SerializeField] private GameObject prefabChild;

        void Start() {


            // Delete components on the prefab instance
            // Destroy(prefabChild.GetComponent<Rigidbody>());
            // Destroy(prefabChild.GetComponent<BoxCollider>());
            DestroyImmediate(prefabChild.GetComponent<Rigidbody>());
            DestroyImmediate(prefabChild.GetComponent<BoxCollider>());

            // Duplicate the parent including children
            Instantiate(parent, new Vector3(2, 0, 0), Quaternion.identity);

            // Check state of copied prefab instances
            // Are the deleted components there again? 
            // (They shouldn't be)

        }
    }

}
