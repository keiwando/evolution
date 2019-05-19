using UnityEngine;
using Keiwando.Evolution.Scenes;

public class SimulationSceneSetup: MonoBehaviour {

    public void BuildScene(SimulationScene scene) {
        SimulationSceneBuilder.Build(scene);
    }

    public Creature[] SpawnBatch(CreatureDesign design, int batchSize, PhysicsScene physicsScene) {

        var template = new CreatureBuilder(design).Build();
        template.PhysicsScene = physicsScene;
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

        var batch = new Creature[batchSize];        
        for (int i = 0; i < batchSize; i++) {
            batch[i] = Instantiate(template, spawnPosition, Quaternion.identity);
            batch[i].RefreshLineRenderers();
            batch[i].PhysicsScene = physicsScene;
            // TODO: Connect Obstacle
        }

        template.gameObject.SetActive(false);
        Destroy(this.gameObject);
        return batch;
    }
}