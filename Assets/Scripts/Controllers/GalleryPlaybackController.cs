using Keiwando.Evolution.Scenes;
using UnityEngine;

namespace Keiwando.Evolution {

  // TODO: Fix open multi-page gallery, go to second page, enter fullscreen, use arrows to go to different scene -> error about destroyed camera.
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