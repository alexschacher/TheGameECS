using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Singleton
    private static CameraController _instance;
    public static CameraController Instance { get { return _instance; } }

    // Input Parameters
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float angleSnap = 45f;
    [SerializeField] private float followTargetSmoothSpeed = 0.25f;
    [SerializeField] private Vector3 followTargetOffset = Vector3.zero;
    private Vector3[] zoomPositions; // Currently hardcoded in Start()

    // Internal States
    private int zoomPosition = 1;
    private bool canZoom = true;
    private bool canRotate = true;
    private bool inFollowMode = true; // Currently doesnt change. Add a way to toggle
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
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Start()
    {
        // Hardcoded Zoom Positions
        zoomPositions = new Vector3[3];
        zoomPositions[0] = new Vector3(0f, -2.4f, -40f); //24f
        zoomPositions[1] = new Vector3(0f, -3.6f, -19f);
        zoomPositions[2] = new Vector3(0f, -4.8f, -14f);

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
        if (!inFollowMode)
        {
            GetPanInput();
        }
        LerpRotation();
        LerpZoom();

        // TODO slerp a LookAt target instead of Camera Target, for a slightly more 3d feel even when not rotating. Without slerp it will make you sick.
        cam.LookAt(transform, Vector3.up);
    }

    void FixedUpdate()
    {
        if (inFollowMode)
        {
            FollowTarget();
        }
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
        if (Input.GetAxis("Horizontal2") == 0f)
        {
            canRotate = true;
        }
        if ((Input.GetAxis("Horizontal2") < 0f && canRotate == true) || Input.GetKeyDown(KeyCode.Q))
        {
            targetRotationAngle = new Vector3(targetRotationAngle.x, targetRotationAngle.y + angleSnap, targetRotationAngle.z);
            canRotate = false;
        }
        if ((Input.GetAxis("Horizontal2") > 0f && canRotate == true) || Input.GetKeyDown(KeyCode.E))
        {
            targetRotationAngle = new Vector3(targetRotationAngle.x, targetRotationAngle.y - angleSnap, targetRotationAngle.z);
            canRotate = false;
        }
    }

    private void GetZoomInput()
    {
        // TODO: Optimize this to better scroll back and forth through a list of positions

        if (Input.GetKeyDown(KeyCode.Z))
        {
            zoomPosition += 1;
            if (zoomPosition >= zoomPositions.Length)
            {
                zoomPosition = 0;
            }
            targetZoomPosition = zoomPositions[zoomPosition];
        }

        if (Input.GetAxis("Vertical2") == 0f && !canZoom)
        {
            canZoom = true;
        }
        else if (Input.GetAxis("Vertical2") < 0f && zoomPosition == 0 && canZoom)
        {
            zoomPosition = 1;
            targetZoomPosition = zoomPositions[1];
            canZoom = false;
        }
        else if (Input.GetAxis("Vertical2") < 0f && zoomPosition == 1 && canZoom)
        {
            zoomPosition = 2;
            targetZoomPosition = zoomPositions[2];
            canZoom = false;
        }
        else if (Input.GetAxis("Vertical2") > 0f && zoomPosition == 2 && canZoom)
        {
            zoomPosition = 1;
            targetZoomPosition = zoomPositions[1];
            canZoom = false;
        }
        else if (Input.GetAxis("Vertical2") > 0f && zoomPosition == 1 && canZoom)
        {
            zoomPosition = 0;
            targetZoomPosition = zoomPositions[0];
            canZoom = false;
        }
    }

    private void GetPanInput()
    {
        transform.position += (MovementVectorCameraConverter.convertMovementVector(
                Input.GetAxisRaw("Walk_Vertical_P1"),
                Input.GetAxisRaw("Walk_Horizontal_P1"))) / 5;
    }

    private void LerpRotation()
    {
        // TODO: Combine these 2 lerp methods into one method with parameters, maybe move method to a Utility class

        currentRotationAngle = new Vector3(
            Mathf.LerpAngle(currentRotationAngle.x, targetRotationAngle.x, Time.deltaTime * rotationSpeed),
            Mathf.LerpAngle(currentRotationAngle.y, targetRotationAngle.y, Time.deltaTime * rotationSpeed),
            Mathf.LerpAngle(currentRotationAngle.z, targetRotationAngle.z, Time.deltaTime * rotationSpeed));

        transform.eulerAngles = currentRotationAngle;
    }

    private void LerpZoom()
    {
        // TODO: Combine these 2 lerp methods into one method with parameters, maybe move method to a Utility class

        currentZoomPosition = new Vector3(
            Mathf.Lerp(currentZoomPosition.x, targetZoomPosition.x, Time.deltaTime * zoomSpeed),
            Mathf.Lerp(currentZoomPosition.y, targetZoomPosition.y, Time.deltaTime * zoomSpeed),
            Mathf.Lerp(currentZoomPosition.z, targetZoomPosition.z, Time.deltaTime * zoomSpeed));

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
