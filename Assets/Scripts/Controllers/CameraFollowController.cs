using UnityEngine;
using System;
using Keiwando.Evolution.Scenes;

namespace Keiwando.Evolution {

	public class CameraFollowController: MonoBehaviour {

		private int watchingIndex = 0;

		[SerializeField]
		private Evolution evolution;
		[SerializeField]
		private TrackedCamera cameraFollow;

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
			cameraFollow.Target = batch[watchingIndex];
		} 

		public void RefreshVisibleCreatures() {

			float hiddenOpacity = Settings.HiddenCreatureOpacity;

			var batch = evolution.CurrentCreatureBatch;
			if (batch == null || batch.Length == 0) { return; }

			bool showContraction = Settings.ShowMuscleContraction;

			batch[watchingIndex].SetOnVisibleLayer();
			for (int i = 0; i < batch.Length; i++) {
				batch[i].RefreshMuscleContractionVisibility(showContraction);
				if (i == watchingIndex) 
					batch[i].SetOnVisibleLayer();
				else 
					batch[i].SetOnInvisibleLayer();
			}
		}
	}
}