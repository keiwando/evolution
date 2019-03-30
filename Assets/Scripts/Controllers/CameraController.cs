using System;
using UnityEngine;

public class CameraController : MonoBehaviour {

    /// <summary>
    /// Specifies whether the camera is currently being zoomed or moved.
    /// </summary>
    public bool IsAdjustingCamera {
        get { return isAdjustingCamera; }
    }
    private bool isAdjustingCamera = false;

    [SerializeField]
    private bool zoomEnabled = true;

    private bool movementEnabled {
        get { return !lockedHorizontalMovement && !lockedVerticalMovement; }
    }

    [SerializeField]
    private bool lockedHorizontalMovement = false;
    
    [SerializeField]
    private bool lockedVerticalMovement = false;

    /// <summary>
    /// The camera to be controlled
    /// </summary>
    [SerializeField]
    new private Camera camera;

    [SerializeField]
    private float bottomMovementPadding = -1f;
    [SerializeField]
    private float topMovementPadding = -1f;
    [SerializeField]
    private float leftMovementPadding = -1f;
    [SerializeField]
    private float rightMovementPadding = -1f;

    /// <summary>
    /// The amount by which the camera can be zoomed in.
    /// </summary>
    [SerializeField]
    private float zoomInLength = 10;

    /// <summary>
    /// The amount by which the camera can be zoomed out
    /// </summary>
    [SerializeField]
    private float zoomOutLength = 10;

    /// <summary>
    /// The difference between the lowest and highest zoom level (orthographicSize).
    /// </summary>
    private float zoomLength {
        get { return zoomInLength + zoomOutLength; }
    }

    /// <summary>
    /// The minimum allowed value of the camera's orthographic size
    /// </summary>
	private float minZoom;

    /// <summary>
    /// The anchor around which zooming is performed relative to the camera bounds
    /// </summary>
    public Vector2 ZoomAnchor {
        get { return zoomAnchor; }
        set { zoomAnchor = value; }
    }
    [SerializeField]
    private Vector2 zoomAnchor = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Specifies whether the camera movement will be limited by the movementBounds rect.
    /// </summary>
    public bool MovementBoundsEnabled = true;
    
    /// <summary>
    /// The bounds in world coordinates, inside of which the camera can be moved around.
    /// </summary>
    /// <remarks>
    /// The visible bounds of the camera cannot be moved outside of these movementBounds.
    /// </remarks>
    private Rect movementBounds = new Rect();

    // MARK: - Gestures

    private Vector3 lastTouchPos = Vector3.zero;
	private bool firstMovementTouch = true;
    private float lastPinchDist = 0f;


    void Start() {

        var cameraPos = camera.transform.position;
        var size = GetOrthographicSize();
        var halfWidth = size.x * 0.5f;
        var halfHeight = size.y * 0.5f;

        float minX = leftMovementPadding >= 0 ? cameraPos.x - halfWidth - leftMovementPadding : float.MinValue;
        float maxX = rightMovementPadding >= 0 ? cameraPos.x + halfWidth + rightMovementPadding : float.MaxValue;
        float minY = bottomMovementPadding >= 0 ? cameraPos.y - halfHeight - bottomMovementPadding : float.MinValue;
        float maxY = topMovementPadding >= 0 ? cameraPos.y + halfHeight + topMovementPadding : float.MaxValue;
        movementBounds.min = new Vector2(minX, minY);
        movementBounds.max = new Vector2(maxX, maxY);

        var initialZoom = camera.orthographicSize;
		minZoom = initialZoom - zoomInLength;
    }

    void Update() {

        if (!zoomEnabled && !movementEnabled) { return; }

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
            if (movementEnabled) {
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
        
        if (zoomEnabled && Input.mouseScrollDelta.y != 0) {
            ZoomCameraFromScroll(Input.mouseScrollDelta.y);
        }
    }

    private void MoveCamera(Vector3 distance) {

        if (!movementEnabled) { return; }

        isAdjustingCamera = true;
		distance.z = 0;
		var position = camera.transform.position + distance;
		position = ClampPosition(position);

        var oldPos = camera.transform.position;
        if (lockedHorizontalMovement) {
            position.x = oldPos.x;
        }
        if (lockedVerticalMovement) {
            position.y = oldPos.y;
        }

		camera.transform.position = position;
	}

    private void ZoomCameraFromScroll(float delta) {

        SetZoom(camera.orthographicSize - delta);
    }

    private void ZoomCameraFromPinch(float percent) {

        SetZoom(camera.orthographicSize / Math.Max(0.0000001f, percent));
    }

    private void SetZoom(float newZoom) {
        
        if (!zoomEnabled) { return; } 

        var visibleSize = GetOrthographicSize();

        isAdjustingCamera = true;
        var size = camera.orthographicSize;
        var newSize = Math.Max(minZoom, Math.Min(minZoom + zoomLength, newZoom));
        camera.orthographicSize = newSize;

        var dSize = newSize / size;
        var dPercent = dSize - 1f;
        
        // Readjust the center position based on the zoom anchor
        var anchorAdjustX = (zoomAnchor.x - 0.5f) * visibleSize.x;
        var anchorAdjustY = (zoomAnchor.y - 0.5f) * visibleSize.y;

        var pos = camera.transform.position;
        pos.x += anchorAdjustX;
        pos.y += anchorAdjustY;
        camera.transform.position = pos;

        ClampPosition();
    }

    private void ClampPosition() {
        camera.transform.position = ClampPosition(camera.transform.position);
    }

    private Vector3 ClampPosition(Vector3 pos) {

        if (!MovementBoundsEnabled) return pos;

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

    private Vector2 GetOrthographicSize() {

        var halfHeight = camera.orthographicSize;
        var halfWidth = camera.aspect * halfHeight;
        return new Vector2(halfWidth * 2, halfHeight * 2);
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