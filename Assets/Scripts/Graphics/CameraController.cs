using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    // Input Parameters
    private float rotationSpeed = 1f;
    private float zoomSpeed = 1f;
    private float panSpeed = 1f;
    private float rotationAngleSnap = 45f;
    private float followTargetSmoothSpeed = 0.25f;
    private Vector3 followTargetOffset = Vector3.zero;
    private Vector3[] zoomPositions; // Currently hardcoded in Start()

    // Internal States
    private int zoomPosition = 1;
    private bool canZoom = true;
    private bool canRotate = true;
    private bool inFollowMode = true; // Currently doesnt change. Add a way to toggle
    private bool inPanMode = false;     // Currently doesnt change. Add a way to toggle
    private Vector3 targetRotationAngle;
    private Vector3 currentRotationAngle;
    private Vector3 targetZoomPosition;
    private Vector3 currentZoomPosition;
    private Vector3 followTargetVelocity = Vector3.zero;
    private Vector3 followTargetPosition = Vector3.zero;
    private Transform cam;

    void Awake()
    {
        // Singleton Reference
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        // Hardcoded Zoom Positions
        zoomPositions = new Vector3[3];
        zoomPositions[0] = new Vector3(0f, -1.2f, -40f);
        //zoomPositions[0] = new Vector3(0f, -2.4f, -35f);
        zoomPositions[1] = new Vector3(0f, -2.4f, -30f);
        //zoomPositions[1] = new Vector3(0f, -2.4f, -24f);
        zoomPositions[2] = new Vector3(0f, -3.6f, -19f);
        //zoomPositions[2] = new Vector3(0f, -4.8f, -14f);

        targetRotationAngle = transform.eulerAngles;
        currentRotationAngle = transform.eulerAngles;

        cam = transform.GetChild(0).transform;
        targetZoomPosition = zoomPositions[zoomPosition];
        currentZoomPosition = cam.localPosition;
    }

    void Update()
    {
        GetRotationInput();
        GetZoomInput();
        if (inPanMode && !inFollowMode)
        {
            GetPanInput();
        }
        if (inFollowMode)
        {
            FollowTarget();
        }
        LerpRotation();
        LerpZoom();

        // TODO slerp a LookAt target instead of Camera Target, for a slightly more 3d feel even when not rotating. Without slerp it will make you sick.
        cam.LookAt(transform, Vector3.up);
    }

    private void FollowTarget()
    {
        transform.position = Vector3.SmoothDamp(
        transform.position,
        followTargetPosition + followTargetOffset,
        ref followTargetVelocity,
        followTargetSmoothSpeed);
    }

    private void GetRotationInput()
    {
        if (Input.GetAxis("Camera_Rotate") == 0f)
        {
            canRotate = true;
        }
        if ((Input.GetAxis("Camera_Rotate") < 0f && canRotate == true))
        {
            targetRotationAngle = new Vector3(targetRotationAngle.x, targetRotationAngle.y + rotationAngleSnap, targetRotationAngle.z);
            canRotate = false;
        }
        if ((Input.GetAxis("Camera_Rotate") > 0f && canRotate == true))
        {
            targetRotationAngle = new Vector3(targetRotationAngle.x, targetRotationAngle.y - rotationAngleSnap, targetRotationAngle.z);
            canRotate = false;
        }
    }

    private void GetZoomInput()
    {
        if (Input.GetAxis("Camera_Zoom") == 0f && !canZoom)
        {
            canZoom = true;
        }
        else if (Input.GetAxis("Camera_Zoom") < 0f && zoomPosition < zoomPositions.Length - 1 && canZoom)
        {
            zoomPosition += 1;
            targetZoomPosition = zoomPositions[zoomPosition];
            canZoom = false;
        }
        else if (Input.GetAxis("Camera_Zoom") > 0f && zoomPosition > 0 && canZoom)
        {
            zoomPosition -= 1;
            targetZoomPosition = zoomPositions[zoomPosition];
            canZoom = false;
        }
    }

    private void GetPanInput()
    {
        transform.position += MovementVectorCameraConverter.ConvertAndNormalizeInputVector(
                Input.GetAxisRaw("Walk_Vertical_P1"),
                Input.GetAxisRaw("Walk_Horizontal_P1")) * panSpeed / 20;
    }

    private void LerpRotation()
    {
        // TODO: Combine these 2 lerp methods into one method with parameters, maybe move method to a Utility class

        currentRotationAngle = new Vector3(
            Mathf.LerpAngle(currentRotationAngle.x, targetRotationAngle.x, Time.deltaTime * rotationSpeed * 5),
            Mathf.LerpAngle(currentRotationAngle.y, targetRotationAngle.y, Time.deltaTime * rotationSpeed * 5),
            Mathf.LerpAngle(currentRotationAngle.z, targetRotationAngle.z, Time.deltaTime * rotationSpeed * 5));

        transform.eulerAngles = currentRotationAngle;
    }

    private void LerpZoom()
    {
        // TODO: Combine these 2 lerp methods into one method with parameters, maybe move method to a Utility class

        currentZoomPosition = new Vector3(
            Mathf.Lerp(currentZoomPosition.x, targetZoomPosition.x, Time.deltaTime * zoomSpeed * 2),
            Mathf.Lerp(currentZoomPosition.y, targetZoomPosition.y, Time.deltaTime * zoomSpeed * 2),
            Mathf.Lerp(currentZoomPosition.z, targetZoomPosition.z, Time.deltaTime * zoomSpeed * 2));

        cam.localPosition = currentZoomPosition;
    }

    public void SetFollowTargetPosition(Vector3 position)
    {
        followTargetPosition = position;
    }

    public float GetRotation()
    {
        return targetRotationAngle.y;
    }
}
