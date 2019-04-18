using UnityEngine;
using System;

public class CameraFollowController: MonoBehaviour {

    private int watchingIndex = 0;

    [SerializeField]
    private Evolution evolution;
    [SerializeField]
    private CameraFollowScript cameraFollow;

    void Start() {

        evolution.NewBatchDidBegin += delegate() {
            this.watchingIndex = 0;
			RefreshVisibleCreatures();
        };
    }

    public void FocusOnNextCreature() {

        var batch = evolution.CurrentCreatureBatch;
        watchingIndex = (watchingIndex + 1) % batch.Length;
		cameraFollow.toFollow = batch[watchingIndex];

		RefreshVisibleCreatures();
	}

	public void FocusOnPreviousCreature() {

		var batch = evolution.CurrentCreatureBatch;
        watchingIndex = watchingIndex - 1 < 0 ? batch.Length - 1 : watchingIndex - 1;
		cameraFollow.toFollow = batch[watchingIndex];
		
		RefreshVisibleCreatures();
	}

    public void RefreshVisibleCreatures() {

        var batch = evolution.CurrentCreatureBatch;
		if (batch == null) { return; }

        
		var contractionVisibility = PlayerPrefs.GetInt(PlayerPrefsKeys.SHOW_MUSCLE_CONTRACTION, 0) == 1;

		foreach (var creature in batch) {
			creature.RefreshMuscleContractionVisibility(contractionVisibility);
		}

		// Determine if all or only one creature should be visible
		if (evolution.Settings.showOneAtATime) {

			foreach (var creature in batch) {
				creature.SetOnInvisibleLayer();
			}
			batch[watchingIndex].SetOnVisibleLayer();
		
		} else {
			foreach (var creature in batch) {
				creature.SetOnVisibleLayer();
			}
		}
	}
}