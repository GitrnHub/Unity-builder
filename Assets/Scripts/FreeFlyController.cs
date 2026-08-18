using UnityEngine;

public sealed class FreeFlyController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float fastMultiplier = 3f;
    public float lookSpeed = 0.18f;

    private Vector3 lastMousePosition;

    private void Start()
    {
        transform.position = new Vector3(0f, 2.2f, -12f);
        transform.rotation = Quaternion.Euler(8f, 0f, 0f);
    }

    private void Update()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) input += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) input += Vector3.back;
        if (Input.GetKey(KeyCode.A)) input += Vector3.left;
        if (Input.GetKey(KeyCode.D)) input += Vector3.right;
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)) input += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q)) input += Vector3.down;

        Vector3 horizontal = transform.right * input.x + transform.forward * input.z;
        Vector3 movement = horizontal + Vector3.up * input.y;
        if (movement.sqrMagnitude > 1f) movement.Normalize();
        transform.position += movement * speed * Time.deltaTime;

        float yaw = 0f;
        float pitch = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) yaw -= 75f * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow)) yaw += 75f * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) pitch -= 75f * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) pitch += 75f * Time.deltaTime;
        transform.Rotate(pitch, yaw, 0f, Space.Self);
        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(euler.x, euler.y, 0f);

        if (Input.GetMouseButtonDown(1))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 now = Input.mousePosition;
            Vector3 delta = now - lastMousePosition;
            lastMousePosition = now;

            Vector3 angles = transform.eulerAngles;
            float x = angles.x;
            if (x > 180f) x -= 360f;
            x = Mathf.Clamp(x - delta.y * lookSpeed, -85f, 85f);
            float y = angles.y + delta.x * lookSpeed;
            transform.rotation = Quaternion.Euler(x, y, 0f);
        }
    }
}
