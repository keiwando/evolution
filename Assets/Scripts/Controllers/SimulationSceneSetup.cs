using UnityEngine;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

    public class SimulationSpawnConfig {
        public readonly CreatureDesign Design;
        public readonly int BatchSize;
        public readonly PhysicsScene PhysicsScene;
        public readonly bool IsLegacySimulation;

        public SimulationSpawnConfig(
            CreatureDesign design, int batchSize, 
            PhysicsScene physicsScene, bool IsLegacySimulation
        ) {
            this.Design = design; 
            this.BatchSize = batchSize;
            this.PhysicsScene = physicsScene;
            this.IsLegacySimulation = IsLegacySimulation;
        }
    }

    public class SimulationSceneSetup: MonoBehaviour {

        public void BuildScene(SimulationScene scene, ISceneContext context) {
            SimulationSceneBuilder.Build(scene, context);
        }

        public Creature[] SpawnBatch(SimulationSpawnConfig options) {

            var template = new CreatureBuilder(options.Design).Build();
            // SetupCreature(template, physicsScene);
            template.PhysicsScene = options.PhysicsScene;
            template.RemoveMuscleColliders();
            template.Alive = false;
            template.gameObject.SetActive(true);

            // Determine the spawn position
            // Update safe drop offset
            // Ensures that the creature isn't spawned into the ground
            var lowestY = template.GetLowestPoint().y;
            var safeHeightOffset = lowestY < 0 ? -lowestY + 1f : 0f;

            // Calculate the drop height
            float distanceFromGround = template.DistanceFromGround();
            float padding = 0.5f;
            var spawnPosition = template.transform.position;
            spawnPosition.y -= distanceFromGround - padding;
            spawnPosition.y += safeHeightOffset;

            var batch = new Creature[options.BatchSize];        
            for (int i = 0; i < options.BatchSize; i++) {

                var builder = new CreatureBuilder(options.Design);
                batch[i] = builder.Build();
                batch[i].transform.position = spawnPosition;
                batch[i].usesLegacyRotationCalculation = options.IsLegacySimulation;
                // batch[i].usesLegacyRotationCalculation = true;
                // SetupCreature(batch[i], physicsScene);

                // batch[i] = Instantiate(template, spawnPosition, Quaternion.identity);
                batch[i].RefreshLineRenderers();
                batch[i].PhysicsScene = options.PhysicsScene;
                // TODO: Connect Obstacle
            }

            template.gameObject.SetActive(false);
            Destroy(this.gameObject);
            return batch;
        }

        // private void SetupCreature(Creature creature, PhysicsScene physicsScene) {

        //     creature.PhysicsScene = physicsScene;
        //     creature.RemoveMuscleColliders();
        // 	creature.Alive = false;
        //     creature.gameObject.SetActive(true);
        // }
    }
}

