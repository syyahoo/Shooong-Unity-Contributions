using UnityEngine;

[DefaultExecutionOrder(1000)]
public class DroneMinimumSpeedAssist : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody droneRigidbody;
    [SerializeField] private Transform tiltInputTransform;
    [SerializeField] private Transform directionReference;

    [Header("Tilt Input")]
    [SerializeField] private float maxPitchAngle = 28f;
    [SerializeField] private float maxRollAngle = 24f;
    [SerializeField] private float tiltDeadZone = 0.08f;

    [Header("Speed")]
    [SerializeField] private float minSpeedAtInput = 0f;
    [SerializeField] private float maxSpeedAtFullTilt = 18f;

    [Header("Response")]
    [SerializeField] private float accelerationLerp = 12f;
    [SerializeField] private float brakeLerp = 16f;

    [Header("Vertical")]
    [SerializeField] private bool keepVerticalVelocity = true;

    // 컴포넌트 참조 자동 연결
    private void Reset()
    {
        droneRigidbody = GetComponent<Rigidbody>();
        tiltInputTransform = transform;
        directionReference = transform;
    }

    // 누락된 참조 보정
    private void Awake()
    {
        if (droneRigidbody == null)
        {
            droneRigidbody = GetComponent<Rigidbody>();
        }

        if (tiltInputTransform == null)
        {
            tiltInputTransform = transform;
        }

        if (directionReference == null)
        {
            directionReference = transform;
        }
    }

    // 기울기 기반 수평 속도 직접 제어
    private void FixedUpdate()
    {
        if (droneRigidbody == null || tiltInputTransform == null || directionReference == null)
        {
            return;
        }

        Vector2 tiltInput = ReadTiltInput();
        float inputPower = Mathf.Clamp01(tiltInput.magnitude);

        Vector3 currentVelocity = GetVelocity();

        if (inputPower <= tiltDeadZone)
        {
            BrakeHorizontalVelocity(currentVelocity);
            return;
        }

        Vector3 moveDirection = GetMoveDirection(tiltInput);

        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            BrakeHorizontalVelocity(currentVelocity);
            return;
        }

        float normalizedInput = Mathf.InverseLerp(tiltDeadZone, 1f, inputPower);
        float targetSpeed = Mathf.Lerp(minSpeedAtInput, maxSpeedAtFullTilt, normalizedInput);

        Vector3 targetHorizontalVelocity = moveDirection.normalized * targetSpeed;

        Vector3 currentHorizontalVelocity = new Vector3(
            currentVelocity.x,
            0f,
            currentVelocity.z);

        Vector3 adjustedHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            targetHorizontalVelocity,
            accelerationLerp * Time.fixedDeltaTime);

        float verticalVelocity = keepVerticalVelocity ? currentVelocity.y : 0f;

        SetVelocity(new Vector3(
            adjustedHorizontalVelocity.x,
            verticalVelocity,
            adjustedHorizontalVelocity.z));
    }

    // 드론 기울기 입력값 계산
    private Vector2 ReadTiltInput()
    {
        Vector3 localEuler = tiltInputTransform.localEulerAngles;

        float pitch = NormalizeAngle(localEuler.x);
        float roll = NormalizeAngle(localEuler.z);

        float forwardInput = Mathf.Clamp(pitch / maxPitchAngle, -1f, 1f);
        float sideInput = Mathf.Clamp(-roll / maxRollAngle, -1f, 1f);

        return new Vector2(sideInput, forwardInput);
    }

    // 이동 방향 계산
    private Vector3 GetMoveDirection(Vector2 input)
    {
        Vector3 forward = directionReference.forward;
        Vector3 right = directionReference.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return forward * input.y + right * input.x;
    }

    // 입력이 없을 때 수평 속도 감속
    private void BrakeHorizontalVelocity(Vector3 currentVelocity)
    {
        Vector3 currentHorizontalVelocity = new Vector3(
            currentVelocity.x,
            0f,
            currentVelocity.z);

        Vector3 adjustedHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            Vector3.zero,
            brakeLerp * Time.fixedDeltaTime);

        float verticalVelocity = keepVerticalVelocity ? currentVelocity.y : 0f;

        SetVelocity(new Vector3(
            adjustedHorizontalVelocity.x,
            verticalVelocity,
            adjustedHorizontalVelocity.z));
    }

    // 각도 정규화
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    // Rigidbody 속도 반환
    private Vector3 GetVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return droneRigidbody.linearVelocity;
#else
        return droneRigidbody.velocity;
#endif
    }

    // Rigidbody 속도 설정
    private void SetVelocity(Vector3 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        droneRigidbody.linearVelocity = velocity;
#else
        droneRigidbody.velocity = velocity;
#endif
    }
}