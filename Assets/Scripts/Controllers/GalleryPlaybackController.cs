using Keiwando.Evolution.Scenes;
using UnityEngine;

namespace Keiwando.Evolution {

  public class GalleryPlaybackController : MonoBehaviour {

    public CreatureRecordingPlayer recordingPlayer;

    public Creature creature;
    public PhysicsScene physicsScene;

    [SerializeField]
    private TrackedCamera trackedCamera;

    private bool paused = true;

    void Start() {
      Physics.simulationMode = SimulationMode.Script;
    }

    public void Setup(Creature creature, CreatureRecordingPlayer player) {
      this.creature = creature;
      this.recordingPlayer = player;

      creature.recordingPlayer = recordingPlayer;
      creature.recordingPlayer.beginPlayback();

      trackedCamera.Target = creature;
    }

    public void Play() {
      // TODO: Implement resume functionality after pausing (currently the recordingPlayer only stores the initial time)
      paused = false;
      
      creature.SetOnBestCreatureLayer();

			creature.Alive = false;
			creature.gameObject.SetActive(false);
			creature.Alive = true;
			creature.gameObject.SetActive(true);
    }

    void FixedUpdate() {
			if (!paused && physicsScene != null && physicsScene.IsValid()) {
				physicsScene.Simulate(Time.fixedDeltaTime);
			}
    }
  }
}