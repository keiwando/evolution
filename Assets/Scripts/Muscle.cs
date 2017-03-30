using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Muscle : BodyComponent {

	private const string MATERIAL_PATH = "Materials/MuscleMaterial";

	public enum MuscleAction {
		CONTRACT, EXPAND	
	}

	public MuscleAction muscleAction;

	public MuscleJoint startingJoint;
	public MuscleJoint endingJoint;

	public Vector3 startingPoint {
		get {
			return startingJoint.position;
		}
	}

	public Vector3 endingPoint {
		get {
			return endingJoint.position;
		}	
	}

	private SpringJoint spring;

	private LineRenderer lineRenderer;

	/// <summary>
	/// Specifies whether the muscle should contract and expand or not.
	/// </summary>
	public bool living;

	private float LINE_WIDTH = 0.5f;

	private float CONTRACTION_FACTOR = 0.2f;

	private float SPRING_STRENGTH = 1000; //30000;

	private float MAX_MUSCLE_FORCE = 1500; //5000;

	public float currentForce = 0;


	public static Muscle Create() {
		ID_COUNTER++;

		Material muscleMaterial = Resources.Load(MATERIAL_PATH) as Material;

		GameObject muscleEmpty = new GameObject();
		muscleEmpty.name = "Muscle";
		var muscle = muscleEmpty.AddComponent<Muscle>();
		muscle.AddLineRenderer();
		muscle.SetMaterial(muscleMaterial);
		muscle.ID = ID_COUNTER;

		return muscle;
	}

	public static Muscle CreateFromString(string data, List<Bone> bones) {

		var parts = data.Split('%');
		var muscleID = int.Parse(parts[0]);
		var startID = int.Parse(parts[1]);
		var endID = int.Parse(parts[2]);

		var muscle = Muscle.Create();
		muscle.ID = muscleID;
		ID_COUNTER = Mathf.Max(ID_COUNTER - 1, muscle.ID);	// ID_COUNTER - 1 because the counter gets increased on Create()

		foreach (var bone in bones) {
			if(bone.ID == startID) {
				muscle.startingJoint = bone.muscleJoint;
			} else if (bone.ID == endID) {
				muscle.endingJoint = bone.muscleJoint;
			}
		}

		muscle.ConnectToJoints();
		muscle.UpdateLinePoints();

		muscle.AddCollider();

		return muscle;
	}

	// Use this for initialization
	public override void Start () {
		base.Start();

		highlightingShader = Shader.Find("Standard");


	}
	
	// Update is called once per frame
	void Update () {

		UpdateLinePoints();
	}

	void FixedUpdate() {
		
		if (muscleAction == MuscleAction.CONTRACT) {
			Contract();
		} else {
			Expand();	
		}
	}

	/// <summary>
	/// Connects the gameobject to the starting end endingJoint
	/// </summary>
	public void ConnectToJoints() {

		if (startingJoint == null || endingJoint == null) return;

		startingJoint.Connect(this);
		endingJoint.Connect(this);

		// connect the musclejoints with a spring joint
		spring = startingJoint.gameObject.AddComponent<SpringJoint>();
		spring.spring = SPRING_STRENGTH;
		spring.damper = 50;
		spring.minDistance = 0;
		spring.maxDistance = 0;
		//spring.autoConfigureConnectedAnchor = true;
		spring.anchor = startingJoint.position;
		spring.connectedAnchor = endingJoint.position;
		spring.connectedBody = endingJoint.GetComponent<Rigidbody>();

		spring.enablePreprocessing = false;
		spring.enableCollision = false;

		// break forces
		//spring.breakForce = 5000;
		//spring.breakTorque = 5000;

	}

	/** Set the muscle contraction. O = no contraction/expansion, 1 = fully contracted. */
	public void SetContractionForce(float percent) {

		currentForce = Mathf.Max(0.01f, Mathf.Min(MAX_MUSCLE_FORCE, percent * MAX_MUSCLE_FORCE));
		Assert.IsFalse(float.IsNaN(currentForce), "Percent: " + percent);
	}

	/** Contracts the muscle. */
	public void Contract() {

		if (living) {

			Contract(currentForce);
		}
	}

	public void Contract(float force) {

		// Apply a force on both connection joints.
		Vector3 midPoint = (startingPoint + endingPoint) / 2;

		Vector3 endingForce = (midPoint - endingPoint).normalized;
		Vector3 startingForce = (midPoint - startingPoint).normalized;

		ApplyForces(force, startingForce, endingForce);
	}

	/** Expands the muscle. */
	public void Expand() {

		if (living) {

			Expand(currentForce);
		}
	}

	public void Expand(float force) {

		// Apply a force on both connection joints.
		Vector3 midPoint = (startingPoint + endingPoint) / 2;

		Vector3 endingForce = (endingPoint - midPoint).normalized;
		Vector3 startingForce = (startingPoint - midPoint).normalized;

		ApplyForces(force, startingForce, endingForce);
	} 

	/** Applies the starting Force to the startingJoint and endingForce to the endingJoint. force specifies the magnitude of the force. */
	private void ApplyForces(float force, Vector3 startingForce, Vector3 endingForce) {

		Vector3 scaleVector = new Vector3(force, force, force);
		endingForce.Scale(scaleVector);
		startingForce.Scale(scaleVector);

		Assert.IsFalse(float.IsNaN(endingForce.x),"force: " + force);
		Assert.IsFalse(float.IsNaN(startingForce.x),"force: " + force);

		Rigidbody rb = endingJoint.GetComponent<Rigidbody>();

		//startingJoint.GetComponent<Rigidbody>().AddForce(startingForce);
		//endingJoint.GetComponent<Rigidbody>().AddForce(endingForce);
		startingJoint.GetComponent<FixedJoint>().connectedBody.AddForceAtPosition(startingForce ,startingJoint.position);
		endingJoint.GetComponent<FixedJoint>().connectedBody.AddForceAtPosition(endingForce, endingJoint.position);
	}

	/// <summary>
	/// Tries to setup 
	/// </summary>
	private void TrySetupFromPrefab() {
		
	}

	private void TestContraction () {
		if (living) {

			Contract(currentForce);
		}
	}

	IEnumerator ExpandAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		Expand();
	}

	IEnumerator ContractAfterTime(float time) {
		yield return new WaitForSeconds(time);

		Contract();

		StartCoroutine(ContractAfterTime(time));
	}

	public void AddLineRenderer(){
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetWidth(LINE_WIDTH, LINE_WIDTH);
	}

	public void DeleteAndAddLineRenderer(){
		lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	public void AddCollider() {

		AddColliderToLine();
	}

	private void AddColliderToLine() {

		BoxCollider col = gameObject.AddComponent<BoxCollider> (); //new GameObject("Collider").

		// Collider is added as child object of line
		col.transform.parent = lineRenderer.transform;
		// length of line
		float lineLength = Vector3.Distance (startingPoint, endingPoint); 
		// size of collider is set where X is length of line, Y is width of line, Z will be set as per requirement
		col.size = new Vector3 (lineLength, LINE_WIDTH, 1f); 
		Vector3 midPoint = (startingPoint + endingPoint)/2;
		// setting position of collider object
		col.transform.position = midPoint; 
		col.center = Vector3.zero;
		// Following lines calculate the angle between startPos and endPos
		float angle = (Mathf.Abs (startingPoint.y - endingPoint.y) / Mathf.Abs (startingPoint.x - endingPoint.x));
		if((startingPoint.y < endingPoint.y && startingPoint.x > endingPoint.x) || (endingPoint.y < startingPoint.y && endingPoint.x > startingPoint.x)) {
			
			angle *= -1;
		}

		angle = Mathf.Rad2Deg * Mathf.Atan (angle);
		col.transform.Rotate (0, 0, angle);

		// Add a rigidbody
		Rigidbody rBody = gameObject.AddComponent<Rigidbody>();
		rBody.isKinematic = true;

		//lineRenderer.material.shader = Shader.Find("Standard");
		//lineRenderer.material
	}

	private void RemoveCollider() {
		Destroy(GetComponent<Rigidbody>());
		Destroy(GetComponent<BoxCollider>());
	}

	/** Sets the material for the attached lineRenderer. */
	public void SetMaterial(Material mat) {
		lineRenderer.material = mat;
	}

	/** Points are flattened to 2D. */
	public void SetLinePoints(Vector3 startingP, Vector3 endingP) {

		startingP.z = 0; 
		endingP.z = 0;

		Assert.IsNotNull(lineRenderer);
		lineRenderer.SetPositions(new Vector3[]{ startingP, endingP });
	}

	public void UpdateLinePoints(){

		if(startingJoint == null || endingJoint == null) return;

		SetLinePoints(startingPoint, endingPoint);
	}

	public override void PrepareForEvolution () {
		
		RemoveCollider();
		living = true;
	}

	public override string GetSaveString () {
		
		return string.Format("{0}%{1}%{2}", ID, startingJoint.ID, endingJoint.ID);
	}

	/** Deletes the muscle gameobject and the springjoint. */
	public override void Delete() {
		base.Delete();

		print("Muscle deleted");

		Destroy(spring);
		startingJoint.Disconnect(this);
		endingJoint.Disconnect(this);
		Destroy(gameObject);
	}

	/// <summary>
	/// Do not use unless you know what you're doing.
	/// </summary>
	public void DeleteWithoutDisconnecting() {

		Destroy(spring);
		Destroy(gameObject);	
	}

	public override bool Equals (object o)
	{
		return base.Equals (o) || Equals(o as Muscle);
	}

	public bool Equals(Muscle m) {

		if (m == null) return false;

		return (m.startingJoint.Equals(startingJoint) && m.endingJoint.Equals(endingJoint)) ||
			(m.startingJoint.Equals(endingJoint) && m.endingJoint.Equals(startingJoint));
	}

	private void OnDestroy()
	{
		Destroy(lineRenderer.material);
	}
}
