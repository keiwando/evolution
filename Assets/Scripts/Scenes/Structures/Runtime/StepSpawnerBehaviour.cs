using System.Collections.Generic;
using UnityEngine;

namespace Keiwando.Evolution.Scenes {

  public class StepSpawnerBehaviour : MonoBehaviour {

    private static Material material;
		
    public Vector3 spawnPosition;
    public float stepRotation;
    public Vector3 stepSize;
		public int numberOfSteps = 4000;

		Vector3[] baseStepCornerPositions = new Vector3[8];

		// The quads that make up each face of a step
		// We only need the top, left and back faces for our mesh.
    // (the "back" faces here face our orthographic camera)
		private static int[][] faceIndices = new int[][] {
			new int[] { 7, 6, 5, 4 }, // Top
			new int[] { 4, 5, 1, 0 }, // Back
			new int[] { 7, 4, 0, 3 }  // Left
		};
		private static Vector3[] faceNormals = new Vector3[] {
			Vector3.up,
      Vector3.back,
			Vector3.left
		};

		private Vector3 spawnDistance;

		void Start() {

      if (StepSpawnerBehaviour.material == null) {
        StepSpawnerBehaviour.material = Resources.Load("Materials/Crumpled Paper") as Material;
      }

			var stepDistance = stepSize.x / 2;
			spawnDistance = new Vector3(stepDistance, Mathf.Sin(Mathf.PI / 2) * stepDistance, 0);

			SpawnSteps();
		}

		private void SpawnSteps() {

			Vector3 half = 0.5f * stepSize;

			this.baseStepCornerPositions = new Vector3[] {
					new Vector3(-half.x, -half.y, -half.z),
					new Vector3( half.x, -half.y, -half.z),
					new Vector3( half.x, -half.y,  half.z),
					new Vector3(-half.x, -half.y,  half.z),
					new Vector3(-half.x,  half.y, -half.z),
					new Vector3( half.x,  half.y, -half.z),
					new Vector3( half.x,  half.y,  half.z),
					new Vector3(-half.x,  half.y,  half.z),
			};
      Quaternion stepRotation = Quaternion.Euler(0, 0, this.stepRotation);
			for (int i = 0; i < baseStepCornerPositions.Length; i++) {
				this.baseStepCornerPositions[i] = stepRotation * this.baseStepCornerPositions[i];
			}

			spawnPosition -= spawnDistance * (numberOfSteps / 2);

			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();

			int vertOffset = 0;

			for (int i = 0; i < numberOfSteps; i++) {
				spawnPosition += spawnDistance;
				AddStepToMesh(vertices, triangles, normals, uvs, ref vertOffset, spawnPosition);
			}

			Mesh mesh = new Mesh();
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetNormals(normals);
			mesh.SetUVs(0, uvs);

			MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
			MeshCollider meshCollider = this.gameObject.AddComponent<MeshCollider>();
      MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
      meshRenderer.sharedMaterial = StepSpawnerBehaviour.material;

			meshFilter.mesh = mesh;
			meshCollider.sharedMesh = mesh;
		}

		void AddStepToMesh(
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector3> normals,
			List<Vector2> uvs,
			ref int vertOffset,
			Vector3 position) {


			for (int i = 0; i < faceIndices.Length; i++) {
				int[] indices = faceIndices[i];
				Vector3 normal = faceNormals[i];

				// Add 4 vertices per face (no shared vertices for easy normals/UVs)
        Vector3 v0 = baseStepCornerPositions[indices[0]] + position;
        Vector3 v1 = baseStepCornerPositions[indices[1]] + position;
        Vector3 v2 = baseStepCornerPositions[indices[2]] + position;
        Vector3 v3 = baseStepCornerPositions[indices[3]] + position;
				vertices.Add(v0);
				vertices.Add(v1);
				vertices.Add(v2);
				vertices.Add(v3);

				int i0 = vertOffset;
				int i1 = vertOffset + 1;
				int i2 = vertOffset + 2;
				int i3 = vertOffset + 3;

				triangles.Add(i0);
				triangles.Add(i1);
				triangles.Add(i2);
				triangles.Add(i2);
				triangles.Add(i3);
				triangles.Add(i0);

				// Normals and UVs
				for (int normalIdx = 0; normalIdx < 4; normalIdx++) {
					normals.Add(normal);
				}
				uvs.Add(new Vector2(v0.x, v0.y));
				uvs.Add(new Vector2(v1.x, v1.y));
				uvs.Add(new Vector2(v2.x, v2.y));
				uvs.Add(new Vector2(v3.x, v3.y));

				vertOffset += 4;
			}
		}
  }
}