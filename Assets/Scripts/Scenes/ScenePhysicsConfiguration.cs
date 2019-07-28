using Keiwando.JSON;

namespace Keiwando.Evolution.Scenes {

    public class ScenePhysicsConfiguration: IJsonConvertible {

        public float Gravity { get; set; } = -50f;

        /// <summary>
        /// Set a velocity value. If two colliding objects have a relative 
        /// velocity below this value, they do not bounce off each other. 
        /// This value also reduces jitter, so it is not recommended to set 
        /// it to a very low value.
        /// </summary>
        public float BounceThreshold { get; set; } = 2f;

        /// <summary>
        /// Set a global energy threshold, below which a non-kinematic 
        /// Rigidbody (that is, one that is not controlled by the physics 
        /// system) may go to sleep. When a Rigidbody is sleeping, it is not 
        /// updated every frame, making it less resource-intensive. If a 
        /// Rigidbody’s kinetic energy divided by its mass is below this 
        /// threshold, it is a candidate for sleeping.
        /// </summary>
        public float SleepThreshold { get; set; } = 0.005f;

        /// <summary>
        /// Set the distance the collision detection system uses to generate 
        /// collision contacts. The value must be positive, and if set too 
        /// close to zero, it can cause jitter. This is set to 0.01 by default. 
        /// Colliders only generate collision contacts if their distance is 
        /// less than the sum of their contact offset values.
        /// </summary>
        public float DefaultContactOffset { get; set; } = 0.01f;

        /// <summary>
        /// Define how many solver processes Unity runs on every physics frame. 
        /// Solvers are small physics engine tasks which determine a number of 
        /// physics interactions, such as the movements of joints or managing 
        /// contact between overlapping Rigidbody components. 
        /// This affects the quality of the solver output and it’s advisable 
        /// to change the property in case non-default Time.fixedDeltaTime is 
        /// used, or the configuration is extra demanding. Typically, it’s used 
        /// to reduce the jitter resulting from joints or contacts.
        /// </summary>
        public int DefaultSolverIterations { get; set; } = 7;

        /// <summary>
        /// Set how many velocity processes a solver performs in each physics 
        /// frame. The more processes the solver performs, the higher the accuracy 
        /// of the resulting exit velocity after a Rigidbody bounce. If you 
        /// experience problems with jointed Rigidbody components or Ragdolls 
        /// moving too much after collisions, try increasing this value.
        /// </summary>
        public int DefaultSolverVelocityIterations { get; set; } = 10;

        /// <summary>
        /// Enable this option if you want physics queries (such as Physics.Raycast) 
        /// to detect hits with the backface triangles of MeshColliders.
        /// </summary>
        public bool QueriesHitBackfaces { get; set; } = false;

        /// <summary>
        /// Enable this option if you want physics hit tests (such as Raycasts, 
        /// SphereCasts and SphereTests) to return a hit when they intersect with 
        /// a Collider marked as a Trigger. Individual raycasts can override this 
        /// behavior. 
        /// </summary>
        public bool QueriesHitTriggers { get; set; } = true;

        #region Encode & Decode

        private static class CodingKey {
            public const string Gravity = "gravity";
            public const string BounceThreshold = "bounceThreshold";
            public const string SleepThreshold = "sleepThreshold";
            public const string DefaultContactOffset = "defaultContactOffset";
            public const string DefaultSolverIterations = "defaultSolverIterations";
            public const string DefaultSolverVelocityIterations = "defaultSolverVelocityIterations";
            public const string QueriesHitBackfaces = "queriesHitBackfaces";
            public const string QueriesHitTriggers = "queriesHitTriggers";
        }

        public JObject Encode() {

            var json = new JObject();
            json[CodingKey.Gravity] = this.Gravity;
            json[CodingKey.BounceThreshold] = this.BounceThreshold;
            json[CodingKey.SleepThreshold] = this.SleepThreshold;
            json[CodingKey.DefaultContactOffset] = this.DefaultContactOffset;
            json[CodingKey.DefaultSolverIterations] = this.DefaultSolverIterations;
            json[CodingKey.DefaultSolverVelocityIterations] = this.DefaultSolverVelocityIterations;
            json[CodingKey.QueriesHitBackfaces] = this.QueriesHitBackfaces;
            json[CodingKey.QueriesHitTriggers] = this.QueriesHitTriggers;
            return json;
        }

        public static ScenePhysicsConfiguration Decode(JObject json) {

            return new ScenePhysicsConfiguration {
                Gravity = json[CodingKey.Gravity].ToFloat(),
                BounceThreshold = json[CodingKey.BounceThreshold].ToFloat(),
                SleepThreshold = json[CodingKey.SleepThreshold].ToFloat(),
                DefaultContactOffset = json[CodingKey.DefaultContactOffset].ToFloat(),
                DefaultSolverIterations = json[CodingKey.DefaultSolverIterations].ToInt(),
                DefaultSolverVelocityIterations = json[CodingKey.DefaultSolverVelocityIterations].ToInt(),
                QueriesHitBackfaces = json[CodingKey.QueriesHitBackfaces].ToBool(),
                QueriesHitTriggers = json[CodingKey.QueriesHitTriggers].ToBool()
            };
        }

        #endregion
    }
}