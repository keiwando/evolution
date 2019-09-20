using UnityEngine;
using System.Collections;

namespace Keiwando {

    public static class DelayExtensions {

        public static void Delay(this MonoBehaviour self, float seconds, System.Action action) {
            self.StartCoroutine(Delayed(seconds, action));
        }

        private static IEnumerator Delayed(float seconds, System.Action action) {
            yield return new WaitForSeconds(seconds);
            action();
        }
    }
}