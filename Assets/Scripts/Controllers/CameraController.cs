using System;
using UnityEngine;

public class CameraController : MonoBehaviour {

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
				position = GetPinchCenter(Input.touches[0].position, Input.touches[1].position);
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

        if (ZoomEnabled && Input.mouseScrollDelta.y != 0) {
            ZoomCamera(Input.mouseScrollDelta.y);
        }
    }

    private void MoveCamera(Vector3 distance) {

        if (!MovementEnabled) { return; }

		distance.z = 0;
		var position = camera.transform.position + distance;
		position = ClampPosition(position);

		camera.transform.position = position;
	}

    private void ZoomCamera(float delta) {

        if (!ZoomEnabled) { return; }

        var size = camera.orthographicSize;
        camera.orthographicSize = Math.Max(minZoom, Math.Min(minZoom + zoomLength, size - delta));

        ClampPosition();
    }

    private void ClampPosition() {
        camera.transform.position = ClampPosition(camera.transform.position);
    }

    private Vector3 ClampPosition(Vector3 pos) {
        //var currentZoomMult = 0.5f * camera.orthographicSize / initialZoom;
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