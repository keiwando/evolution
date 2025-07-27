using System;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public static class DefaultSimulationScenes {

        private static CameraControlPoint[] flatGroundControlPoints = new [] {
            new CameraControlPoint(0, 0, 0.11f), new CameraControlPoint(1, 0, 0.11f)
        };

        public static SimulationSceneDescription RunningScene {
            get {
                if (_runningScene == null) _runningScene = CreateRunningScene();
                return _runningScene;
            }
        }
        private static SimulationSceneDescription _runningScene;
        public static SimulationSceneDescription JumpingScene {
            get {
                if (_jumpingScene == null) _jumpingScene = CreateJumpingScene();
                return _jumpingScene;
            }
        }
        private static SimulationSceneDescription _jumpingScene;
        
        public static  SimulationSceneDescription ObstacleJumpScene {
            get {
                if (_obstacleJumpScene == null) _obstacleJumpScene = CreateObstacleJumpScene();
                return _obstacleJumpScene;
            }
        }
        private static SimulationSceneDescription _obstacleJumpScene;

        public static SimulationSceneDescription ClimbingScene {
            get {
                if (_climbingScene == null) _climbingScene = CreateClimbingScene();
                return _climbingScene;
            }
        }
        private static SimulationSceneDescription _climbingScene;
        // public static readonly SimulationSceneDescription RunningScene = CreateIncrementalClimbingScene();

        public static SimulationSceneDescription FlyingScene {
            get {
                if (_flyingScene == null) _flyingScene = CreateFlyingScene();
                return _flyingScene;
            }
        }
        private static SimulationSceneDescription _flyingScene;

        public static SimulationSceneDescription DefaultSceneForObjective(Objective objective) {
            switch (objective) {
            case Objective.Running: return RunningScene;
            case Objective.Jumping: return JumpingScene;
            case Objective.ObstacleJump: return ObstacleJumpScene;
            case Objective.Flying: return FlyingScene;
            case Objective.Climbing: return ClimbingScene;
            default: throw new System.ArgumentException("Invalid objective!");
            }
        }

        private static SimulationSceneDescription CreateRunningScene() {
            
            var groundPos = new Vector3(0.476771f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000f, 9.56f, 29.8f);
            var groundTransform = new Transform(groundPos, groundScale);
            var ground = new Ground(groundTransform);

            var distanceMarkerSpawner = new DistanceMarkerSpawner(
                new Transform(new Vector3(-0.45f, 1.63f, 0)),
                5f
            );

            return new SimulationSceneDescription {
                Version = 1,
                Structures = new IStructure[] { ground, distanceMarkerSpawner },
                DropHeight = 0.5f,
                CameraControlPoints = flatGroundControlPoints
            };
        }

        private static SimulationSceneDescription CreateJumpingScene() {
           
            var groundPos = new Vector3(0.476771f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000f, 9.56f, 29.8f);
            var groundTransform = new Transform(groundPos, groundScale);
            var ground = new Ground(groundTransform);

            var distanceMarkerSpawner = new DistanceMarkerSpawner(
                new Transform(new Vector3(-0.45f, 1.63f, 0), 90f), 5, 1, 90f
            );

            return new SimulationSceneDescription {
                Version = 1,
                Structures = new IStructure[] { ground, distanceMarkerSpawner },
                DropHeight = 0.5f,
                CameraControlPoints = flatGroundControlPoints
            };
        }

        private static SimulationSceneDescription CreateFlyingScene() {
           
            var groundPos = new Vector3(0.476771f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000f, 9.56f, 29.8f);
            var groundTransform = new Transform(groundPos, groundScale);
            var ground = new Ground(groundTransform);

            var distanceMarkerSpawner = new DistanceMarkerSpawner(
                new Transform(new Vector3(-0.45f, 1.63f, 0), 90f), 5, 1, 90f
            );

            return new SimulationSceneDescription {
                Version = 1,
                Structures = new IStructure[] { ground, distanceMarkerSpawner },
                DropHeight = 0.5f,
                CameraControlPoints = flatGroundControlPoints
                // CameraControlPoints = new CameraControlPoint[] {}
            };
        }

        private static SimulationSceneDescription CreateObstacleJumpScene() {

            var groundPos = new Vector3(0.476771f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000f, 9.56f, 29.8f);
            var groundTransform = new Transform(groundPos, groundScale);
            var ground = new Ground(groundTransform);

            var rightWallPos = new Vector3(40f, -4.8f, -2.61f);
            var leftWallPos = new Vector3(-41.73f, -4.8f, -2.61f);
            var rightWallScale = new Vector3(10000f, 35.78f, 29.8f);
            var leftWallScale = new Vector3(10000f, 35.78f, 29.8f);
            var rightWall = new Wall(new Transform(rightWallPos, 90f, rightWallScale));
            var leftWall = new Wall(new Transform(leftWallPos, 90f, leftWallScale));

            var obstacleSpawnerPos = new Vector3(31.1f, 4.41f, 0f);
            var obstacleSpawner = new RollingObstacleSpawner(new Transform(obstacleSpawnerPos, 180f));

            return new SimulationSceneDescription {
                Version = 1,
                Structures = new IStructure[] { ground, leftWall, rightWall, obstacleSpawner },
                DropHeight = 0.5f,
                CameraControlPoints = flatGroundControlPoints
            };
        }

        private static SimulationSceneDescription CreateClimbingScene() {

            var structures = new IStructure[3];

            var groundPos = new Vector3(14.6f, -4.8f, -2.61f);
            var groundScale = new Vector3(1000000, 30f, 29.8f);
            var ground = new Ground(new Transform(groundPos, 45f, groundScale));
            structures[0] = ground;

            var distanceMarkerSpawner = new DistanceMarkerSpawner(
                new Transform(new Vector3(-0.45f, 5.5f, 0), 45f),
                5f * Mathf.Sin((float)Math.PI * 0.25f), 
                1f / Mathf.Sin((float)Math.PI * 0.25f)
            );
            structures[1] = distanceMarkerSpawner;
            
            var spawnPosition = new Vector3(0.46f, 0.99243f, -1.8f);
            structures[2] = new StepSpawner(new Transform(pos: spawnPosition));

            return new SimulationSceneDescription {
                Version = 1,
                Structures = structures,
                DropHeight = 1f,
                CameraControlPoints = new [] {
                    new CameraControlPoint(0, 9, 0.5f), new CameraControlPoint(1, 10, 0.5f)
                }
            };
        }
    }
}