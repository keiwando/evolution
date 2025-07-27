using System.IO;
using Keiwando.JSON;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {
	
	public class StepSpawner : BaseStructure {

		public const string ENCODING_ID = "evolution::structure::stepspawner";

		public StepSpawner(Transform transform): base(transform) {}

    public override StructureType GetStructureType() {
      return StructureType.StepSpawner;
    }

		public void Encode(BinaryWriter writer) {
			this.Transform.Encode(writer);
			ushort flags = 0;
			writer.Write(flags);
		}

		public static StepSpawner Decode(BinaryReader reader) {
			var transform = Transform.Decode(reader);
			ushort flags = reader.ReadUInt16();
			return new StepSpawner(transform);
		}

    public override string GetEncodingKey() {
      return ENCODING_ID;
    }

		public static StepSpawner Decode(JObject json) {
			var transform = BaseStructure.DecodeTransform(json);
			return new StepSpawner(transform);
		}

		public override IStructureBuilder GetBuilder() {
			return new StepSpawnerBuilder(this);
		}

		public class StepSpawnerBuilder : BaseStructureBuilder<StepSpawner> {

      protected override string prefabPath => "Prefabs/Structures/StepSpawner";
      protected override CollisionLayer collisionLayer => CollisionLayer.StaticForeground;

			public StepSpawnerBuilder(StepSpawner spawner): base(spawner) {}

			public override GameObject Build(ISceneContext context) {
				var spawner = base.Build(context).GetComponent<StepSpawnerBehaviour>();
				spawner.spawnPosition = Vector3.zero; 
				spawner.stepRotation = -16f;
				spawner.stepSize = new Vector3(3f, 3f, 30f);
				spawner.numberOfSteps = 4000;
				return spawner.gameObject;
			}
		}
	}
}


