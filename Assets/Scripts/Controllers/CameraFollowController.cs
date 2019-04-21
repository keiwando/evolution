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

		// TODO: Replace after Compile
		// bool showContraction = Settings.ShowMuscleContraction;
		bool showContraction = true;

		foreach (var creature in batch) {
			creature.RefreshMuscleContractionVisibility(showContraction);
		}

		// TODO: Replace after Compile
		// bool oneAtATime = Settings.ShowOneAtATime;
		bool oneAtAtime = false;

		// Determine if all or only one creature should be visible
		if (oneAtAtime) {

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