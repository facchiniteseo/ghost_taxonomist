using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float controllerSensitivity = 200f;
    public float verticalLookLimit = 85f;

    [Header("Zoom")]
    public float normalFOV = 60f;
    public float zoomFOV = 30f;
    public float zoomSpeed = 8f;
    public float zoomSensitivityMultiplier = 0.4f;

    [Header("Interazione")]
    public float interactDistance = 5f;
    public LayerMask interactableLayer;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private Camera cam;

    private bool isZooming = false; // <-- NUOVO

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = playerCamera.GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleZoom();
        HandleInteraction();
    }

    void HandleLook()
    {
        float currentControllerSensitivity = isZooming ? controllerSensitivity * zoomSensitivityMultiplier : controllerSensitivity;
        float currentMouseSensitivity = isZooming ? mouseSensitivity * zoomSensitivityMultiplier : mouseSensitivity;

        float mouseX = Input.GetAxis("Mouse X") * currentMouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * currentMouseSensitivity * Time.deltaTime;
        float stickX = Input.GetAxis("RightStickHorizontal") * currentControllerSensitivity * Time.deltaTime;
        float stickY = -Input.GetAxis("RightStickVertical") * currentControllerSensitivity * Time.deltaTime;

        float lookX = mouseX + stickX;
        float lookY = mouseY + stickY;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleZoom()
    {
        float targetFOV = isZooming ? zoomFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }

    void HandleInteraction()
    {
        bool interactPressed = Input.GetKeyDown(KeyCode.Mouse0) || (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);
        if (!interactPressed) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer, QueryTriggerInteraction.Collide))
        {
            hit.collider.SendMessage("OnClicked", SendMessageOptions.DontRequireReceiver);
        }
    }

    // 🔥 ZOOM AUTOMATICO TRIGGER

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZOOM"))
        {
            isZooming = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZOOM"))
        {
            isZooming = false;
        }
    }
}