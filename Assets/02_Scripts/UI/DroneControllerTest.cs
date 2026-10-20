using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 40f;        // 기본 속도 (m/s)
    public float boostMultiplier = 3f;   // Shift 누르면 가속
    [Header("시점")]
    public float lookSensitivity = 2f;
    public float pitchClamp = 85f;

    Rigidbody rb;
    float yaw, pitch;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;             // 중력 끄기 (드론은 떠 있어야 함)
        rb.freezeRotation = true;          // 충돌 시 멋대로 회전 방지
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // 빠르게 날아도 벽 통과 방지

        Vector3 e = transform.eulerAngles;
        yaw = e.y; pitch = e.x;
        Cursor.lockState = CursorLockMode.Locked;   // 마우스 잠금 (시점 조작)
    }

    void Update()
    {
        // 마우스로 시점 회전
        yaw   += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch  = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))       // Esc로 마우스 풀기
            Cursor.lockState = CursorLockMode.None;
    }

    void FixedUpdate()
    {
        // WASD + QE → 드론이 바라보는 방향 기준 이동
        float x = Input.GetAxisRaw("Horizontal");   // A / D
        float z = Input.GetAxisRaw("Vertical");     // W / S
        float y = (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f); // E 위 / Q 아래

        Vector3 dir = transform.right * x + transform.forward * z + transform.up * y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);
        rb.linearVelocity = dir * speed;            // ⚠️ Unity 6는 velocity가 아니라 linearVelocity!
    }

    // ── 충돌 감지 (콜라이더 작동 확인) ──
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"💥 충돌! → {collision.gameObject.name} 에 부딪힘 (위치: {collision.GetContact(0).point})");

        // TODO: 나중에 여기서 게임오버 처리
        // 예) Time.timeScale = 0f;  또는  GameManager.GameOver();
    }
}