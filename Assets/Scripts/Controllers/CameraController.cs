using System;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public bool IsAdjustingCamera {
        get { return isAdjustingCamera; }
    }
    private bool isAdjustingCamera = false;

    public bool ZoomEnabled { 
        get { return zoomEnabled; }
        set { zoomEnabled = value; } 
    }
    [SerializeField]
    private bool zoomEnabled = true;

    public bool MovementEnabled {
        get { return movementEnabled; }
        set { movementEnabled = value; }
    }
    [SerializeField]
    private bool movementEnabled = true;

    [SerializeField]
    new private Camera camera;

    [SerializeField]
    private Vector2 movementBoundsSize;

    [SerializeField]
    private float zoomLength = 20;
	private float minZoom;

    private Rect movementBounds = new Rect();

    private Vector3 lastTouchPos = Vector3.zero;
	private bool firstMovementTouch = true;
    private float lastPinchDist = 0f;

    private float initialZoom;

    void Start() {

        var cameraPos = camera.transform.position;
        var dX = movementBoundsSize.x;
        var dY = movementBoundsSize.y;
        movementBounds.min = new Vector2(cameraPos.x - dX * 0.5f, cameraPos.y - dY * 0.5f);
        movementBounds.max = new Vector2(cameraPos.x + dX * 0.5f, cameraPos.y + dY * 0.5f);

        initialZoom = camera.orthographicSize;
		minZoom = initialZoom - zoomLength / 2;
    }

    void Update() {
        if (!ZoomEnabled && !MovementEnabled) { return; }

        // Middle click or two touches to move the camera
		if ((Input.GetMouseButton(2) && Input.touchCount == 0) 
            || Input.touchCount == 2) {

			var position = Input.mousePosition;

			if (Input.touchCount == 2) {
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);
                
				position = GetPinchCenter(touch1.position, touch2.position);
                var touchDistance = (touch1.position - touch2.position).magnitude * 0.5f;
                
                if (touch1.phase != TouchPhase.Began && 
                    touch2.phase != TouchPhase.Began) {

                    var zoom = touchDistance / lastPinchDist;
                    ZoomCameraFromPinch(zoom);
                }

                lastPinchDist = touchDistance;
			}

			var distance = lastTouchPos - position;
			lastTouchPos = position;

			if (firstMovementTouch) { 
				firstMovementTouch = false;
				return; 
			}
			firstMovementTouch = false;

			// move the camera by the distance
			distance = ScreenToWorldDistance(distance);
            if (MovementEnabled) {
                MoveCamera(distance);
            }

		} else {
			firstMovementTouch = true;
			lastTouchPos = Vector3.zero;
		}

        #if UNITY_IOS || UNITY_ANDROID
        if (isAdjustingCamera && Input.touchCount == 0) {
            isAdjustingCamera = false;
        }
        #else 
        if (Input.GetMouseButtonUp(2)) {
            isAdjustingCamera = false;
        }
        #endif
        

        if (ZoomEnabled && Input.mouseScrollDelta.y != 0) {
            ZoomCameraFromScroll(Input.mouseScrollDelta.y);
        }
    }

    private void MoveCamera(Vector3 distance) {

        if (!MovementEnabled) { return; }

        isAdjustingCamera = true;
		distance.z = 0;
		var position = camera.transform.position + distance;
		position = ClampPosition(position);

		camera.transform.position = position;
	}

    private void ZoomCameraFromScroll(float delta) {

        if (!ZoomEnabled) { return; }

        isAdjustingCamera = true;
        var size = camera.orthographicSize;
        camera.orthographicSize = Math.Max(minZoom, Math.Min(minZoom + zoomLength, 
                                  size - delta));

        ClampPosition();
    }

    private void ZoomCameraFromPinch(float percent) {

        if (!ZoomEnabled) { return; }

        isAdjustingCamera = true;
        var size = camera.orthographicSize;
        camera.orthographicSize = Math.Max(minZoom, Math.Min(minZoom + zoomLength, 
                                  size / percent));

        ClampPosition();
    }

    private void ClampPosition() {
        camera.transform.position = ClampPosition(camera.transform.position);
    }

    private Vector3 ClampPosition(Vector3 pos) {

        var halfHeight = camera.orthographicSize;
        var halfWidth = camera.aspect * halfHeight;

        var minX = movementBounds.min.x + halfWidth;
        var maxX = movementBounds.max.x - halfWidth;
        var minY = movementBounds.min.y + halfHeight;
        var maxY = movementBounds.max.y - halfHeight;

        pos.x = Mathf.Clamp(pos.x, minX, maxX); 
		pos.y = Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }

    private Vector3 GetPinchCenter(Vector2 touch1, Vector2 touch2) {

		var center2D = 0.5f * (touch1 + touch2);
		return new Vector3(center2D.x, center2D.y);
	}

    private Vector3 ScreenToWorldPoint(Vector3 point) {
		return Camera.main.ScreenToWorldPoint(point);
	}

	private Vector3 ScreenToWorldDistance(Vector3 distance) {

		var p1 = ScreenToWorldPoint(new Vector3(0,0,0));
		var p2 = ScreenToWorldPoint(distance);

		return p2 - p1;
	}
}