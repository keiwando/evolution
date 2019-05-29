using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class Muscle : BodyComponent {

	public struct Defaults {
		public static float MaxForce = 1500f;
	}

	private const string MATERIAL_PATH = "Materials/MuscleMaterial2";
	private const string BLUE_MATERIAL_PATH = "Materials/MuscleMaterialBlue";
	private const string INVISIBLE_MATERIAL_PATH = "Materials/MuscleMaterialInvisible";

	public enum MuscleAction {
		CONTRACT, EXPAND	
	}

	public MuscleData MuscleData { get; set; }

	public MuscleAction muscleAction;

	public MuscleJoint startingJoint;
	public MuscleJoint endingJoint;

	private SpringJoint spring;

	private LineRenderer lineRenderer;

	private Rigidbody _body;
	private Collider _collider;

	private Vector3[] linePoints = new Vector3[2];

	/// <summary>
	/// Specifies whether the muscle should contract and expand or not.
	/// </summary>
	public bool living {
		set {
			_living = value;
			if (_living) {
				ShouldShowContraction = Settings.ShowMuscleContraction;
			}
		}
		get { return _living; }
	}
	private bool _living;

	public bool ShouldShowContraction { 
		get {
			return shouldShowContraction;
		} 
		set {
			shouldShowContraction = value;
			if (!value) {
				SetRedMaterial();
				SetLineWidth(1f);
			}
		} 
	}
	private bool shouldShowContraction;

	private float LINE_WIDTH = 0.5f;

	//private float CONTRACTION_FACTOR = 0.2f;

	private float SPRING_STRENGTH = 1000; //30000;

	private float MAX_MUSCLE_FORCE = 1500; //5000;

	public float currentForce = 0;

	// MARK: contraction visibility
	private Material redMaterial;
	private Material blueMaterial;
	private Material invisibleMaterial;

	private float minLineWidth = 0.5f;
	private float maxLineWidth = 1.5f;

	private Vector3 resetPosition;
	private Quaternion resetRotation;

	public static Muscle CreateFromData(MuscleData data) {

		Material muscleMaterial = Resources.Load(MATERIAL_PATH) as Material;
		Material blueMaterial = Resources.Load(BLUE_MATERIAL_PATH) as Material;
		Material invisibleMaterial = Resources.Load(INVISIBLE_MATERIAL_PATH) as Material;

		GameObject muscleEmpty = new GameObject();
		muscleEmpty.name = "Muscle";
		muscleEmpty.layer = LayerMask.NameToLayer("Creature");
		var muscle = muscleEmpty.AddComponent<Muscle>();
		muscle.AddLineRenderer();
		muscle.SetMaterial(muscleMaterial);

		muscle.MuscleData = data;

		muscle.redMaterial = muscleMaterial;
		muscle.blueMaterial = blueMaterial;
		muscle.invisibleMaterial = invisibleMaterial;

		return muscle;
	}
		
	public override void Start () {
		base.Start();

		//highlightingShader = Shader.Find("Standard");

		resetPosition = transform.position;
		resetRotation = transform.rotation;
	}

	public void Reset() {
		
		transform.SetPositionAndRotation(resetPosition, resetRotation);	
		currentForce = 0f;
		living = false;
	}

	void Update () {

		UpdateLinePoints();
		UpdateContractionVisibility();
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
		spring.anchor = startingJoint.transform.position;
		spring.connectedAnchor = endingJoint.transform.position;

		spring.connectedBody = endingJoint.GetComponent<Rigidbody>(); // Connect to muscle joint (Default)
		//spring.connectedBody = endingJoint.GetComponentInParent<Rigidbody>(); // Connect directly to bone

		spring.enablePreprocessing = true;
		spring.enableCollision = false;

		// break forces
		//spring.breakForce = 5000;
		//spring.breakTorque = 5000;
	}

	// Try to connect the muscle directly to the bone instead of the muscleJoint
	//public void ConnectToJoints() {

	//	if (startingJoint == null || endingJoint == null) return;

	//	startingJoint.Connect(this);
	//	endingJoint.Connect(this);

	//	// connect the musclejoints with a spring joint
	//	//spring = startingJoint.gameObject.AddComponent<SpringJoint>();
	//	spring = startingJoint.transform.parent.gameObject.AddComponent<SpringJoint>();
	//	spring.spring = SPRING_STRENGTH;
	//	spring.damper = 50;
	//	spring.minDistance = 0;
	//	spring.maxDistance = 0;
	//	//spring.autoConfigureConnectedAnchor = true;
	//	spring.anchor = startingJoint.transform.position;
	//	spring.connectedAnchor = endingJoint.transform.position;

	//	//spring.connectedBody = endingJoint.GetComponent<Rigidbody>(); // Connect to muscle joint (Default)
	//	spring.connectedBody = endingJoint.GetComponentInParent<Rigidbody>(); // Connect directly to bone

	//	spring.enablePreprocessing = true;
	//	spring.enableCollision = false;

	//	// break forces
	//	//spring.breakForce = 5000;
	//	//spring.breakTorque = 5000;
	//}

	/** Set the muscle contraction. O = no contraction/expansion, 1 = fully contracted. */
	public void SetContractionForce(float percent) {

		currentForce = Mathf.Max(0.01f, Mathf.Min(MAX_MUSCLE_FORCE, percent * MAX_MUSCLE_FORCE));
		//Assert.IsFalse(float.IsNaN(currentForce));
	}

	/** Contracts the muscle. */
	public void Contract() {

		if (living) {
			Contract(currentForce);
		}
	}

	public void Contract(float force) {

		var startingPoint = startingJoint.transform.position;
		var endingPoint = endingJoint.transform.position;

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

		var startingPoint = startingJoint.transform.position;
		var endingPoint = endingJoint.transform.position;

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

		//Assert.IsFalse(float.IsNaN(endingForce.x));
		//Assert.IsFalse(float.IsNaN(startingForce.x));

//		startingJoint.GetComponent<FixedJoint>().connectedBody.AddForceAtPosition(startingForce ,startingJoint.position);
//		endingJoint.GetComponent<FixedJoint>().connectedBody.AddForceAtPosition(endingForce, endingJoint.position);
		
		startingJoint.ConnectedBone.AddForceAtPosition(startingForce, startingJoint.transform.position);
		endingJoint.ConnectedBone.AddForceAtPosition(endingForce, endingJoint.transform.position);
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
		//lineRenderer.SetWidth(LINE_WIDTH, LINE_WIDTH); // Deprecated
		lineRenderer.startWidth = LINE_WIDTH;
		lineRenderer.endWidth = LINE_WIDTH;

		lineRenderer.generateLightingData = true;
	}

	public void DeleteAndAddLineRenderer(){
		lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	public void AddCollider() {

		AddColliderToLine();
	}

	private void AddColliderToLine() {

		var startingPoint = startingJoint.transform.position;
		var endingPoint = endingJoint.transform.position;

		BoxCollider col = gameObject.AddComponent<BoxCollider> (); //new GameObject("Collider").
		this._collider = col;

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
		this._body = rBody;
		rBody.isKinematic = true;
	}

	public void RemoveCollider() {
		DestroyImmediate(GetComponent<Rigidbody>());
		DestroyImmediate(GetComponent<BoxCollider>());
		//Destroy(this._body);
		//Destroy(this._collider);
	}

	private void UpdateContractionVisibility() {

		if (_living && !Settings.ShowMuscles) {
			SetInvisibleMaterial();
			return;
		}

		if (!ShouldShowContraction) { return; }

		var alpha = currentForce / MAX_MUSCLE_FORCE;
		//print("Alpha = " + alpha);

		if (muscleAction == MuscleAction.CONTRACT) {
			SetRedMaterial();
		} else if (muscleAction == MuscleAction.EXPAND) {
			SetBlueMaterial();
		}

		SetLineWidth(minLineWidth + alpha * (maxLineWidth - minLineWidth));
	}

	/** Sets the material for the attached lineRenderer. */
	public void SetMaterial(Material mat) {
		lineRenderer.material = mat;
	}

	private void SetRedMaterial() {

		if (redMaterial == null) redMaterial = Resources.Load(MATERIAL_PATH) as Material;
		lineRenderer.material = redMaterial;
	}

	private void SetBlueMaterial() {
		
		if (blueMaterial == null) blueMaterial = Resources.Load(BLUE_MATERIAL_PATH) as Material;
		lineRenderer.material = blueMaterial;
	}

	private void SetInvisibleMaterial() {
		
		if (invisibleMaterial == null) invisibleMaterial = Resources.Load(INVISIBLE_MATERIAL_PATH) as Material;
		lineRenderer.material = invisibleMaterial;
	}

	private void SetLineWidth(float width) {
		//lineRenderer.startWidth = lineRenderer.endWidth = width;
		lineRenderer.widthMultiplier = width;
	}

	/** Points are flattened to 2D. */
	public void SetLinePoints(Vector3 startingP, Vector3 endingP) {

		startingP.z = 0; 
		endingP.z = 0;

		//Assert.IsNotNull(lineRenderer);
		//lineRenderer.SetPositions(new Vector3[]{ startingP, endingP });
		SetLinePoints3D(startingP, endingP);
	}

	public void SetLinePoints3D(Vector3 startingP, Vector3 endingP) {

		//Assert.IsNotNull(lineRenderer);
		linePoints[0] = startingP;
		linePoints[1] = endingP;

		lineRenderer.SetPositions(linePoints);

		//lineRenderer.SetPositions(new Vector3[]{ startingP, endingP });
	}

	public void UpdateLinePoints(){

		if(startingJoint == null || endingJoint == null) return;

		//SetLinePoints(startingPoint, endingPoint);
		SetLinePoints3D(startingJoint.transform.position, endingJoint.transform.position);
	}

	public override void PrepareForEvolution () {
		
		//RemoveCollider();
		living = true;
	}

	// TODO: Remove this
	public override string GetSaveString () {
		
		return string.Format("{0}%{1}%{2}", MuscleData.id, MuscleData.startBoneID, MuscleData.endBoneID);
	}

	/** Deletes the muscle gameobject and the springjoint. */
	public override void Delete() {
		base.Delete();

		//print("Muscle deleted");

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

	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}

	private void OnDestroy()
	{
		Destroy(lineRenderer.material);
	}
}
