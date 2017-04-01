using UnityEngine;

[ExecuteInEditMode]
public class ReCalcCubeTexture : MonoBehaviour
{
	private Vector3 _currentScale;

	private void Start()
	{
		Calculate();
	}

	private void Update()
	{
		Calculate();
	}

	public void Calculate()
	{
		if (_currentScale == transform.localScale) return;
		if (CheckForDefaultSize()) return;
		_currentScale = transform.localScale;
		var mesh = GetMesh();
		mesh.uv = SetupUvMap(mesh.uv);
		mesh.name = "Cube Instance";
		if (GetComponent<Renderer>().sharedMaterial.mainTexture.wrapMode != TextureWrapMode.Repeat)
		{
			GetComponent<Renderer>().sharedMaterial.mainTexture.wrapMode = TextureWrapMode.Repeat;
		}
	}

	private Mesh GetMesh()
	{
		Mesh mesh;
		#if UNITY_EDITOR
		var meshFilter = GetComponent<MeshFilter>();
		var meshCopy = Instantiate(meshFilter.sharedMesh);
		mesh = meshFilter.mesh = meshCopy;
		#else
		mesh = GetComponent<MeshFilter>().mesh
		#endif
		return mesh;
	}

	private Vector2[] SetupUvMap(Vector2[] meshUVs)
	{
		var width = _currentScale.x;
		var depth = _currentScale.z;
		var height = _currentScale.y;
		//Front
		meshUVs[2] = new Vector2(0, height);
		meshUVs[3] = new Vector2(width, height);
		meshUVs[0] = new Vector2(0, 0);
		meshUVs[1] = new Vector2(width, 0);
		//Back
		meshUVs[7] = new Vector2(0, 0);
		meshUVs[6] = new Vector2(width, 0);
		meshUVs[11] = new Vector2(0, height);
		meshUVs[10] = new Vector2(width, height);
		//Left
		meshUVs[19] = new Vector2(depth, 0);
		meshUVs[17] = new Vector2(0, height);
		meshUVs[16] = new Vector2(0, 0);
		meshUVs[18] = new Vector2(depth, height);
		//Right
		meshUVs[23] = new Vector2(depth, 0);
		meshUVs[21] = new Vector2(0, height);
		meshUVs[20] = new Vector2(0, 0);
		meshUVs[22] = new Vector2(depth, height);
		//Top
		meshUVs[4] = new Vector2(width, 0);
		meshUVs[5] = new Vector2(0, 0);
		meshUVs[8] = new Vector2(width, depth);
		meshUVs[9] = new Vector2(0, depth);
		//Bottom
		meshUVs[13] = new Vector2(width, 0);
		meshUVs[14] = new Vector2(0, 0);
		meshUVs[12] = new Vector2(width, depth);
		meshUVs[15] = new Vector2(0, depth);
		return meshUVs;
	}

	private bool CheckForDefaultSize()
	{
		if (_currentScale != Vector3.one) return false;
		var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		DestroyImmediate(GetComponent<MeshFilter>());
		gameObject.AddComponent<MeshFilter>();
		GetComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;
		DestroyImmediate(cube);
		return true;
	}
}