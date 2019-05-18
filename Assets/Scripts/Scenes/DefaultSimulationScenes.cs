using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public static class DefaultSimulationScenes {

        public static readonly SimulationScene RunningScene = CreateRunningScene();
        public static readonly SimulationScene JumpingScene = CreateJumpingScene();
        public static readonly SimulationScene ObstacleJumpScene = CreateObstacleJumpScene();
        public static readonly SimulationScene ClimbingScene = CreateClimbingScene();

        public static SimulationScene DefaultSceneForTask(EvolutionTask task) {
            switch (task) {
            case EvolutionTask.Running: return RunningScene;
            case EvolutionTask.Jumping: return JumpingScene;
            case EvolutionTask.ObstacleJump: return ObstacleJumpScene;
            case EvolutionTask.Climbing: return ClimbingScene;
            default: throw new System.ArgumentException("Invalid task!");
            }
        }

        private static SimulationScene CreateRunningScene() {
            
            var groundPos = new Vector3(0.476771f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000f, 9.56f, 29.8f);
            var groundTransform = new Transform(groundPos, groundScale);
            var ground = new Ground(groundTransform);

            var distanceMarkerSpawner = new DistanceMarkerSpawner(
                new Transform(new Vector3(-0.45f, 1.63f, 0))
            );

            // var spawnPoint = new Vector3(0.476771f, -4.299f, -2.5224161f);

            return new SimulationScene() {
                Structures = new IStructure[] { ground, distanceMarkerSpawner }
            };
        }

        private static SimulationScene CreateJumpingScene() {
            // TODO: Set distanceMarkerSpawner to vertical
            return CreateRunningScene();
        }

        private static SimulationScene CreateObstacleJumpScene() {

            var groundPos = new Vector3(0.476771f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000f, 9.56f, 29.8f);
            var groundTransform = new Transform(groundPos, groundScale);
            var ground = new Ground(groundTransform);

            var rightWallPos = new Vector3(40f, -4.8f, -2.61f);
            var leftWallPos = new Vector3(-34.9f, -4.8f, -2.61f);
            var rightWallScale = new Vector3(10000f, 35.78f, 29.8f);
            var leftWallScale = new Vector3(10000f, 22.12f, 29.8f);
            var rightWall = new Wall(new Transform(rightWallPos, rightWallScale));
            var leftWall = new Wall(new Transform(leftWallPos, leftWallScale));

            var obstacleSpawnerPos = new Vector3(31.1f, 4.41f, 0f);
            var obstacleSpawner = new RollingObstacleSpawner(new Transform(obstacleSpawnerPos, 180f));

            // var spawnPoint = new Vector3(0.476771f, -4.299f, -2.5224161f);

            return new SimulationScene() {
                Structures = new IStructure[] { ground, leftWall, rightWall, obstacleSpawner }
            };
        }

        private static SimulationScene CreateClimbingScene() {

            int stepCount = 4000;
            var structures = new IStructure[stepCount + 1];

            // 205.8f 83.21243f -16.5f
            var groundPos = new Vector3(14.6f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000, 30f, 29.8f);
            var ground = new Ground(new Transform(groundPos, 45f, groundScale));
            structures[0] = ground;
            
            var spawnPosition = new Vector3(0.46f, 0.99243f, -1.8f);
            var stepDistance = 0.5f;
            var spawnDistance = new Vector3(stepDistance, Mathf.Sin(Mathf.PI / 2) * stepDistance, 0);
            var stepScale = new Vector3(3f, 3f, 30f);

            spawnPosition -= spawnDistance * (stepCount / 2);
            for (int i = 0; i < stepCount; i++) {
                spawnPosition += spawnDistance;
                structures[i + 1] = new Stairstep(new Transform(spawnPosition, -16f, stepScale));
            }

            // var spawnPoint = new Vector3(0.476771f, -4.299f, -2.5224161f);

            return new SimulationScene() {
                Structures = structures
            };
        }
    }
}