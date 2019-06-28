using UnityEngine;
using Keiwando.Evolution.Scenes;

public class SimulationSceneSetup: MonoBehaviour {

    public void BuildScene(SimulationScene scene, ISceneContext context) {
        SimulationSceneBuilder.Build(scene, context);
    }

    public Creature[] SpawnBatch(CreatureDesign design, int batchSize, PhysicsScene physicsScene) {

        var template = new CreatureBuilder(design).Build();
        // SetupCreature(template, physicsScene);
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

            var builder = new CreatureBuilder(design);
            batch[i] = builder.Build();
            batch[i].transform.position = spawnPosition;
            // SetupCreature(batch[i], physicsScene);

            // batch[i] = Instantiate(template, spawnPosition, Quaternion.identity);
            batch[i].RefreshLineRenderers();
            batch[i].PhysicsScene = physicsScene;
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