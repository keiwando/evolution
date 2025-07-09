using System.IO;
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
        /// Solvers are small physics engine objectives which determine a number of 
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

        public bool AutoSyncTransforms { get; set; } = false;

        #region Encode & Decode

        public void Encode(BinaryWriter writer) {
            long lengthOffset = writer.Seek(0, SeekOrigin.Current);
            writer.WriteDummyBlockLength();

            ushort flags = 0;
            writer.Write(flags);
            writer.Write(this.Gravity);
            writer.Write(this.BounceThreshold);
            writer.Write(this.SleepThreshold);
            writer.Write(this.DefaultContactOffset);
            writer.Write(this.DefaultSolverIterations);
            writer.Write(this.DefaultSolverVelocityIterations);
            writer.Write(this.QueriesHitBackfaces);
            writer.Write(this.QueriesHitTriggers);
            writer.Write(this.AutoSyncTransforms);

            writer.WriteBlockLengthToOffset(lengthOffset);
        } 

        public static ScenePhysicsConfiguration Decode(BinaryReader reader) {

            uint dataLength = reader.ReadUInt32();
            long expectedEndByte = reader.BaseStream.Position + dataLength;

            ushort flags = reader.ReadUInt16();
            float gravity = reader.ReadSingle();
            float bounceThreshold = reader.ReadSingle();
            float sleepThreshold = reader.ReadSingle();
            float defaultContactOffset = reader.ReadSingle();
            int defaultSolverIterations = reader.ReadInt32();
            int defaultSolverVelocityIterations = reader.ReadInt32();
            bool queriesHitBackfaces = reader.ReadBoolean();
            bool queriesHitTriggers = reader.ReadBoolean();
            bool autoSyncTransforms = reader.ReadBoolean();

            reader.BaseStream.Seek(expectedEndByte, SeekOrigin.Begin);
            
            return new ScenePhysicsConfiguration {
                Gravity = gravity,
                BounceThreshold = bounceThreshold,
                SleepThreshold = sleepThreshold,
                DefaultContactOffset = defaultContactOffset,
                DefaultSolverIterations = defaultSolverIterations,
                DefaultSolverVelocityIterations = defaultSolverVelocityIterations,
                QueriesHitBackfaces = queriesHitBackfaces,
                QueriesHitTriggers = queriesHitTriggers,
                AutoSyncTransforms = autoSyncTransforms
            };
        }

        private static class CodingKey {
            public const string Gravity = "gravity";
            public const string BounceThreshold = "bounceThreshold";
            public const string SleepThreshold = "sleepThreshold";
            public const string DefaultContactOffset = "defaultContactOffset";
            public const string DefaultSolverIterations = "defaultSolverIterations";
            public const string DefaultSolverVelocityIterations = "defaultSolverVelocityIterations";
            public const string QueriesHitBackfaces = "queriesHitBackfaces";
            public const string QueriesHitTriggers = "queriesHitTriggers";
            public const string AutoSyncTransforms = "autoSyncTransforms";
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
            json[CodingKey.AutoSyncTransforms] = this.AutoSyncTransforms;
            return json;
        }

        public static ScenePhysicsConfiguration Decode(JObject json) {

            var autoSyncTransforms = json.ContainsKey(CodingKey.AutoSyncTransforms) ? json[CodingKey.AutoSyncTransforms].ToBool() : false;

            return new ScenePhysicsConfiguration {
                Gravity = json[CodingKey.Gravity].ToFloat(),
                BounceThreshold = json[CodingKey.BounceThreshold].ToFloat(),
                SleepThreshold = json[CodingKey.SleepThreshold].ToFloat(),
                DefaultContactOffset = json[CodingKey.DefaultContactOffset].ToFloat(),
                DefaultSolverIterations = json[CodingKey.DefaultSolverIterations].ToInt(),
                DefaultSolverVelocityIterations = json[CodingKey.DefaultSolverVelocityIterations].ToInt(),
                QueriesHitBackfaces = json[CodingKey.QueriesHitBackfaces].ToBool(),
                QueriesHitTriggers = json[CodingKey.QueriesHitTriggers].ToBool(),
                AutoSyncTransforms = autoSyncTransforms
            };
        }

        #endregion

        public static ScenePhysicsConfiguration Legacy = new ScenePhysicsConfiguration() {
            Gravity = -50f,
            BounceThreshold = 2f,
            SleepThreshold = 0.005f,
            DefaultContactOffset = 0.01f,
            DefaultSolverIterations = 7,
            DefaultSolverVelocityIterations = 10,
            QueriesHitBackfaces = false,
            QueriesHitTriggers = true,
            AutoSyncTransforms = true
        };
    }
}