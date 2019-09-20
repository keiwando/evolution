using UnityEngine;
using System.Collections;

namespace Keiwando.Evolution.Scenes {

    public class DestroyAfterTime: MonoBehaviour {
        
        /// <summary>
        /// The lifetime of the GameObject in seconds.
        /// </summary>
        public float Lifetime { get; set; } = 1f;

        public void BeginCountdown() {
            StartCoroutine(DestroyAfterSeconds(Lifetime));
        }

        private IEnumerator DestroyAfterSeconds(float seconds) {
            yield return new WaitForSeconds(seconds);

            Destroy(this.gameObject);
        }
    }
}