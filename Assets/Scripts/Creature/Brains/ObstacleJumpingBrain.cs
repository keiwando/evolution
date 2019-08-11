using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution {

	public class ObstacleJumpingBrain : Brain {

		public override int NumberOfInputs => 7;

		private GameObject obstacle;
		
		public override void FixedUpdate () {
			base.FixedUpdate();
			FindObstacleIfNeeded();
		}

		/*Inputs:
		* 
		* - distance from ground
		* - dx velocity
		* - dy velocity
		* - rotational velocity
		* - number of points touching ground
		* - creature rotation
		* - distance from obstacle
		*/
		protected override void UpdateInputs (){
			
			// distance from ground
			Network.Inputs[0] = creature.DistanceFromGround();
			// horizontal velocity
			Vector3 velocity = creature.GetVelocity();
			Network.Inputs[1] = velocity.x;
			// vertical velocity
			Network.Inputs[2] = velocity.y;
			// rotational velocity
			Network.Inputs[3] = creature.GetAngularVelocity().z;
			// number of points touching ground
			Network.Inputs[4] = creature.GetNumberOfPointsTouchingGround();
			// Creature rotation
			Network.Inputs[5] = creature.GetRotation();
			// distance from obstacle
			Network.Inputs[6] = creature.GetDistanceFromObstacle(this.obstacle);
		}

		private void FindObstacleIfNeeded() {
			if (obstacle != null) return;
			int playbackCreatureLayer = LayerMask.NameToLayer("PlaybackCreature");
			int dynamicForegroundLayer = LayerMask.NameToLayer("DynamicForeground");
			int playbackDynamicForegroundLayer = LayerMask.NameToLayer("PlaybackDynamicForeground");
			var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

			foreach (var obstacle in obstacles) {
				bool correctObstacle = false;
				if (!obstacle.activeSelf) continue;
				if (this.gameObject.layer == playbackCreatureLayer) {
					correctObstacle = obstacle.layer == playbackDynamicForegroundLayer;
				} else {
					correctObstacle = obstacle.layer == dynamicForegroundLayer;
				}
				if (correctObstacle) {
					this.obstacle = obstacle;
					break;
				}
			}
		}
	}

}