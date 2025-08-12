using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public class RollingObstacleSpawnerBehaviour: MonoBehaviour, IResettableStructure {

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
        private List<GameObject> spawnedObstacles = new List<GameObject>();

        private Coroutine coroutine;

        void Start() {
            // This is primarily important for correcty gallery playback rendering since
            // we forward the layer of this object to its dynamically spawned children.
            this.gameObject.layer = Context.GetDynamicForegroundLayer();

            this.obstacleTemplate = Instantiate(Resources.Load(PREFAB_PATH), transform) as GameObject;
            this.obstacleTemplate.SetActive(false);
            this.obstacleTemplate.layer = Context.GetDynamicForegroundLayer();

            coroutine = StartCoroutine(SpawnObstacle());
        }

        private IEnumerator SpawnObstacle() {

            while (true) {

                for (int i = spawnedObstacles.Count - 1; i >= 0; i--) {
                    if (spawnedObstacles[i] == null) {
                        spawnedObstacles.RemoveAt(i);
                    }
                }
                
                var obstacle = Instantiate(obstacleTemplate, transform.position, Quaternion.identity, transform) as GameObject;
                spawnedObstacles.Add(obstacle);
                obstacle.SetActive(true);
                var destroyAfterTime = obstacle.AddComponent<DestroyAfterTime>();
                destroyAfterTime.Lifetime = ObstacleLifetime;
                destroyAfterTime.BeginCountdown();
                var body = obstacle.GetComponent<Rigidbody>();
                body.AddForce(transform.right * BASE_FORCE);

                yield return new WaitForSeconds(SpawnInterval);
            }
        }

        public void Reset() {
            if (this.coroutine != null) {
                StopCoroutine(this.coroutine);
            }
            foreach (GameObject gameObject in spawnedObstacles) {
                if (gameObject != null) {
                    Destroy(gameObject);
                }
            }
            spawnedObstacles.Clear();
            coroutine = StartCoroutine(SpawnObstacle());
        }
    }
}