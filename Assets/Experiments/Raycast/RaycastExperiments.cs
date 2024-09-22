using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Keiwando.Evolution.Scenes;
using Keiwando.Evolution;

public class RaycastExperiments: MonoBehaviour {

    void Awake() {

        Physics.simulationMode = SimulationMode.Script;

        StartCoroutine(TestGroundDistance());
    }

    private IEnumerator TestGroundDistance() {

        var joints = new List<JointData>() {
            new JointData(0, new Vector2(0, 2f), 1f, 0f)
        };

        var context = new SceneController.SimulationSceneLoadContext();

        yield return SceneController.LoadSimulationScene(
            creatureDesign: new CreatureDesign("Unnamed", joints, new List<BoneData>(), new List<MuscleData>()),
            creatureSpawnCount: 1,
            sceneDescription: DefaultSimulationScenes.DefaultSceneForObjective(Objective.Running),
            sceneType: SceneController.SimulationSceneType.Simulation,
            legacyOptions: new LegacySimulationOptions(),
            context: context, 
            sceneContext: null
        );

        Debug.Log("TestGroundDistance");

        int groundLayerMask = 1 << 9;

        RaycastHit hit;

        Debug.DrawRay(this.transform.position, Vector3.down * 20, Color.red, 10f);

		if (context.PhysicsScene.Raycast(this.transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayerMask)) {
			
			if (hit.collider.gameObject.CompareTag("Ground")) {
				Debug.Log(hit.distance);
                yield break;
			}
		}

		Debug.Log("No hit!");

        yield break;
    }
}
