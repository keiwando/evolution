using UnityEngine;

public class SimulationBatchSpawner: MonoBehaviour {

    public Creature[] SpawnBatch(CreatureDesign design, int batchSize, Vector3 dropPos) {


        var template = new CreatureBuilder(design).Build();
        template.gameObject.SetActive(true);

        var batch = new Creature[batchSize];        
        for (int i = 0; i < batchSize; i++) {
            batch[i] = Instantiate(template, dropPos, Quaternion.identity);
        }

        template.gameObject.SetActive(false);
        Destroy(this.gameObject);
        return batch;
    }
}