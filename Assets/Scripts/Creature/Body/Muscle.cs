using UnityEngine;
using System;
using System.Collections;

public class Muscle : BodyComponent {

	public struct Defaults {
		public static float MaxForce = 1500f;
	}

	private const string MATERIAL_PATH = "Materials/MuscleMaterial2";
	private const string BLUE_MATERIAL_PATH = "Materials/MuscleMaterialBlue";
	private const string INVISIBLE_MATERIAL_PATH = "Materials/MuscleMaterialInvisible";

	private const float LINE_WIDTH = 0.5f;
	private const float SPRING_STRENGTH = 1000;

	public enum MuscleAction {
		CONTRACT, EXPAND	
	}

	public MuscleData MuscleData { get; set; }

	public MuscleAction muscleAction;

	public Bone startingBone;
	public Bone endingBone;

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

		resetPosition = transform.position;
		resetRotation = transform.rotation;
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

		if (startingBone == null || endingBone == null) return;

		startingBone.Connect(this);
		endingBone.Connect(this);

		// connect the musclejoints with a spring joint
		if (startingBone.BoneData.legacy) {
			spring = startingBone.legacyWeightObj.gameObject.AddComponent<SpringJoint>();
		} else {
			spring = startingBone.gameObject.AddComponent<SpringJoint>();
		}
		
		spring.spring = SPRING_STRENGTH;
		spring.damper = 50;
		spring.minDistance = 0;
		spring.maxDistance = 0;

		spring.anchor = startingBone.Center;
		spring.connectedAnchor = endingBone.Center;

		if (endingBone.BoneData.legacy)
			spring.connectedBody = endingBone.legacyWeightObj.GetComponent<Rigidbody>();
		else 
			spring.connectedBody = endingBone.GetComponent<Rigidbody>();

		spring.enablePreprocessing = true;
		spring.enableCollision = false;
	}

	/// <summary>
	/// Updates the current muscle force to be a percentage of the maximum force.
	/// </summary>
	/// <param name="percent">The percentage of the maximum force.</param>
	public void SetContractionForce(float percent) {

		float maxForce = MuscleData.strength;
		currentForce = Mathf.Max(0.01f, Mathf.Min(maxForce, percent * maxForce));
	}

	public void Contract() {

		if (living) {
			Contract(currentForce);
		}
	}

	public void Contract(float force) {

		var startingPoint = startingBone.Center;
		var endingPoint = endingBone.Center;

		// Apply a force on both connection joints.
		Vector3 midPoint = (startingPoint + endingPoint) / 2;

		Vector3 endingForce = (midPoint - endingPoint).normalized;
		Vector3 startingForce = (midPoint - startingPoint).normalized;

		ApplyForces(force, startingForce, endingForce);
	}

	public void Expand() {

		if (living && MuscleData.canExpand) {
			Expand(currentForce);
		}
	}

	public void Expand(float force) {

		if (!MuscleData.canExpand) return;

		var startingPoint = startingBone.Center;
		var endingPoint = endingBone.Center;

		// Apply a force on both connection joints.
		Vector3 midPoint = (startingPoint + endingPoint) / 2;

		Vector3 endingForce = (endingPoint - midPoint).normalized;
		Vector3 startingForce = (startingPoint - midPoint).normalized;

		ApplyForces(force, startingForce, endingForce);
	} 

	/// <summary>
	/// Applies the starting Force to the startingJoint and endingForce to the endingJoint. 
	/// force specifies the magnitude of the force.
	/// </summary>
	private void ApplyForces(float force, Vector3 startingForce, Vector3 endingForce) {

		Vector3 scaleVector = new Vector3(force, force, force);
		endingForce.Scale(scaleVector);
		startingForce.Scale(scaleVector);

		startingBone.Body.AddForceAtPosition(startingForce, startingBone.Center);
		endingBone.Body.AddForceAtPosition(endingForce, endingBone.Center);
	}

	public void AddLineRenderer(){
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.startWidth = LINE_WIDTH;
		lineRenderer.endWidth = LINE_WIDTH;
		lineRenderer.receiveShadows = false;
		lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineRenderer.allowOcclusionWhenDynamic = false;
		lineRenderer.sortingOrder = -1;

		lineRenderer.generateLightingData = true;
	}

	public void DeleteAndAddLineRenderer(){
		lineRenderer = gameObject.GetComponent<LineRenderer>();
	}

	public void AddCollider() {

		AddColliderToLine();
	}

	private void AddColliderToLine() {

		var startingPoint = startingBone.Center;
		var endingPoint = endingBone.Center;

		BoxCollider col = gameObject.AddComponent<BoxCollider> ();
		this._collider = col;

		// Collider is added as child object of line
		// col.transform.parent = lineRenderer.transform;
		// length of line
		float lineLength = Vector3.Distance (startingPoint, endingPoint); 
		// size of collider is set where X is length of line, Y is width of line, Z will be set as per requirement
		col.size = new Vector3 (lineLength, LINE_WIDTH, 1f); 
		Vector3 midPoint = (startingPoint + endingPoint)/2;
		// setting position of collider object
		col.transform.position = midPoint; 
		col.center = Vector3.zero;
		// Calculate the angle between startPos and endPos
		float angle = (Mathf.Abs (startingPoint.y - endingPoint.y) / Mathf.Abs (startingPoint.x - endingPoint.x));
		if ((startingPoint.y < endingPoint.y && startingPoint.x > endingPoint.x) || (endingPoint.y < startingPoint.y && endingPoint.x > startingPoint.x)) {
			angle *= -1;
		}

		angle = Mathf.Rad2Deg * Mathf.Atan (angle);
		col.transform.eulerAngles = new Vector3(0, 0, angle);

		// Add a rigidbody
		Rigidbody rBody = gameObject.AddComponent<Rigidbody>();
		this._body = rBody;
		rBody.isKinematic = true;
	}

	public void RemoveCollider() {
		DestroyImmediate(GetComponent<Rigidbody>());
		DestroyImmediate(GetComponent<BoxCollider>());
	}

	private void UpdateContractionVisibility() {

		if (!_living) return;

		if (!Settings.ShowMuscles) {
			SetInvisibleMaterial();
			return;
		}

		if (!ShouldShowContraction) { return; }

		var alpha = (float)Math.Min(1f, currentForce / Math.Max(MuscleData.strength, 0.000001));

		if (muscleAction == MuscleAction.CONTRACT) {
			SetRedMaterial();
		} else if (muscleAction == MuscleAction.EXPAND && MuscleData.canExpand) {
			SetBlueMaterial();
		}

		if (!MuscleData.canExpand && muscleAction == MuscleAction.EXPAND) {
			SetLineWidth(minLineWidth);
		} else {
			SetLineWidth(minLineWidth + alpha * (maxLineWidth - minLineWidth));
		}
	}

	/// <summary>
	/// Sets the material for the attached LineRenderer.
	/// </summary>
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
		lineRenderer.widthMultiplier = width;
	}

	/// <summary>
	/// Points are flattened to 2D.
	/// </summary>
	public void SetLinePoints(Vector3 startingP, Vector3 endingP) {

		startingP.z = 0; 
		endingP.z = 0;
		SetLinePoints3D(startingP, endingP);
	}

	public void SetLinePoints3D(Vector3 startingP, Vector3 endingP) {

		linePoints[0] = startingP;
		linePoints[1] = endingP;
		lineRenderer.SetPositions(linePoints);
	}

	public void UpdateLinePoints(){

		if (startingBone == null || endingBone == null) return;

		SetLinePoints3D(startingBone.transform.position, endingBone.transform.position);
	}

	public override void PrepareForEvolution () {
		living = true;
	}

	/// <summary>
	/// Deletes the muscle gameObject and the sprint joint
	/// </summary>
	public override void Delete() {
		base.Delete();

		Destroy(spring);
		startingBone.Disconnect(this);
		endingBone.Disconnect(this);
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

		return (m.startingBone.Equals(startingBone) && m.endingBone.Equals(endingBone)) ||
			(m.startingBone.Equals(endingBone) && m.endingBone.Equals(startingBone));
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
