using UnityEngine;
using Keiwando.Evolution.Scenes;

public class SimulationSceneSetup: MonoBehaviour {

    public void BuildScene(SimulationScene scene) {
        SimulationSceneBuilder.Build(scene);
    }

    public Creature[] SpawnBatch(CreatureDesign design, int batchSize, Vector3 dropPos) {


        var template = new CreatureBuilder(design).Build();
        template.RemoveMuscleColliders();
		template.Alive = false;
        template.gameObject.SetActive(true);

        var batch = new Creature[batchSize];        
        for (int i = 0; i < batchSize; i++) {
            batch[i] = Instantiate(template, dropPos, Quaternion.identity);
            batch[i].RefreshLineRenderers();
            // TODO: Connect Obstacle
        }

        template.gameObject.SetActive(false);
        Destroy(this.gameObject);
        return batch;
    }
}