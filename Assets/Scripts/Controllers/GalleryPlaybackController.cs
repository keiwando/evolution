using Keiwando.Evolution.Scenes;
using UnityEngine;

namespace Keiwando.Evolution {

  public class GalleryPlaybackController : MonoBehaviour {

    public CreatureRecordingPlayer recordingPlayer;
    public Creature creature;

    [SerializeField]
    private TrackedCamera trackedCamera;

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
      creature.SetOnBestCreatureLayer();

			creature.Alive = false;
			creature.gameObject.SetActive(false);
			creature.Alive = true;
			creature.gameObject.SetActive(true);
    }
  }
}