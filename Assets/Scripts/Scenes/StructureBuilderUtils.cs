using UnityEngine;

namespace Keiwando.Evolution.Scenes {
    
    public static class StructureBuilderUtils {

        public static GameObject Build(string prefabPath, Transform transform) {
            var position = transform.Position;
            var rotation = Quaternion.Euler(0, 0, transform.Rotation);
            var obj = GameObject.Instantiate(Resources.Load(prefabPath), position, rotation) as GameObject;
            obj.transform.localScale = transform.Scale;
            return obj;
        }

        public static T AddGameObjectWithBehaviour<T>(Transform transform) where T: MonoBehaviour {
            
            var obj = new GameObject();
            var objTransform = obj.transform;
            objTransform.position = transform.Position;
            objTransform.rotation = Quaternion.Euler(0, 0, transform.Rotation);
            objTransform.localScale = transform.Scale;
            return obj.AddComponent<T>();
        }
    }
}