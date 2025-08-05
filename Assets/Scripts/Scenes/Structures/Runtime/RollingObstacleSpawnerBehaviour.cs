using System.Collections;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class RollingObstacleSpawnerBehaviour: MonoBehaviour {

        private const string PREFAB_PATH = "Prefabs/Structures/ObstacleBall";
        private const float BASE_FORCE = 5000f;
        private const float BASE_EQUIVALENT_SPEED = 5f;

        /// <summary>
        /// The duration in seconds between two consecutively spawned obstacles.
        /// </summary>
        public float SpawnInterval { get; set; } = 5f;

        /// <summary>
        /// The duration in seconds after which a spawned obstacle is destroyed.
        /// </summary>
        public float ObstacleLifetime { get; set; } = 5f;

        /// <summary>
        /// Controls the initial force that is applied to the rolling obstacle after 
        /// it has been spawned.
        /// </summary>
        public float ForceMultiplier { get; set; } = 1f;

        public ISceneContext Context;

        private GameObject obstacleTemplate;

        void Start() {
            // This is primarily important for correcty gallery playback rendering since
            // we forward the layer of this object to its dynamically spawned children.
            this.gameObject.layer = Context.GetDynamicForegroundLayer();

            this.obstacleTemplate = Instantiate(Resources.Load(PREFAB_PATH), transform) as GameObject;
            this.obstacleTemplate.SetActive(false);
            this.obstacleTemplate.layer = Context.GetDynamicForegroundLayer();

            StartCoroutine(SpawnObstacle());
        }

        private IEnumerator SpawnObstacle() {

            while (true) {
                
                var obstacle = Instantiate(obstacleTemplate, transform.position, Quaternion.identity, transform) as GameObject;
                obstacle.SetActive(true);
                var destroyAfterTime = obstacle.AddComponent<DestroyAfterTime>();
                destroyAfterTime.Lifetime = ObstacleLifetime;
                destroyAfterTime.BeginCountdown();
                var body = obstacle.GetComponent<Rigidbody>();
                body.AddForce(transform.right * BASE_FORCE);

                yield return new WaitForSeconds(SpawnInterval);
            }
        }
    }
}