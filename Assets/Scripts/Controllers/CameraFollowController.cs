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
			RefreshCameraFocus();
			RefreshVisibleCreatures();
        };
    }

    public void FocusOnNextCreature() {

        var batch = evolution.CurrentCreatureBatch;
        watchingIndex = (watchingIndex + 1) % batch.Length;
		
		RefreshCameraFocus();
		RefreshVisibleCreatures();
	}

	public void FocusOnPreviousCreature() {

		var batch = evolution.CurrentCreatureBatch;
        watchingIndex = watchingIndex - 1 < 0 ? batch.Length - 1 : watchingIndex - 1;
		
		RefreshCameraFocus();
		RefreshVisibleCreatures();
	}

	private void RefreshCameraFocus() {
		var batch = evolution.CurrentCreatureBatch;
		cameraFollow.toFollow = batch[watchingIndex];
	} 

    public void RefreshVisibleCreatures() {

		float hiddenOpacity = Settings.HiddenCreatureOpacity;

        var batch = evolution.CurrentCreatureBatch;
		if (batch == null) { return; }

		bool showContraction = Settings.ShowMuscleContraction;

		foreach (var creature in batch) {
			creature.RefreshMuscleContractionVisibility(showContraction);
		}

		bool oneAtATime = true; //Settings.ShowOneAtATime;

		// Determine if all or only one creature should be visible
		if (oneAtATime) {

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