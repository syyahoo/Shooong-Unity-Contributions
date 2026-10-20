using UnityEngine;

public class DroneMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRDroneInputReader inputReader;
    [SerializeField] private Rigidbody droneRigidbody;

    [Header("Thrust")]
    [SerializeField] private float hoverThrustMultiplier = 1.0f;
    [SerializeField] private float throttlePower = 9f;
    [SerializeField] private float maxVerticalSpeed = 5f;
    [SerializeField] private float throttleDeadZone = 0.05f;

    [Header("Altitude Hold")]
    [SerializeField] private bool useAltitudeHold = true;
    [SerializeField] private float altitudeHoldStrength = 12f;
    [SerializeField] private float altitudeVelocityDamping = 6f;
    [SerializeField] private float maxAltitudeHoldForce = 12f;

    [Header("Tilt Control")]
    [SerializeField] private float maxPitchAngle = 22f;
    [SerializeField] private float maxRollAngle = 22f;
    [SerializeField] private float tiltResponsiveness = 8f;
    
    [Header("Horizontal Assist")]
    [SerializeField] private bool useHorizontalAssist = true;
    [SerializeField] private float forwardAssistAcceleration = 4f;
    [SerializeField] private float sideAssistAcceleration = 2.5f;
    [SerializeField] private float assistDeadZone = 0.05f;

    [Header("Yaw Control")]
    [SerializeField] private float yawTorquePower = 4f;
    [SerializeField] private float maxYawAngularSpeed = 90f;

    [Header("Stabilization")]
    [SerializeField] private float uprightStabilization = 10f;
    [SerializeField] private float angularDamping = 4f;
    [SerializeField] private float horizontalDamping = 0.6f;

    [Header("Safety Limit")]
    [SerializeField] private float maxHorizontalSpeed = 8f;
    [SerializeField] private float maxTotalSpeed = 10f;
    
    [Header("Hover Assist")]
    [SerializeField] private float hoverHorizontalDamping = 4f;
    [SerializeField] private float hoverAngularDamping = 10f;
    [SerializeField] private float hoverAltitudeHoldStrength = 20f;
    [SerializeField] private float hoverAltitudeVelocityDamping = 12f;
    
    private float _targetPitch;
    private float _targetRoll;
    private float _currentPitch;
    private float _currentRoll;

    private float _targetAltitude;
    private bool _altitudeHoldInitialized;
    private bool _wasThrottleInputActive;
    
    private bool _isHoverActive;
    private bool _wasHoverActive;

    // 컴포넌트 참조 자동 연결
    private void Reset()
    {
        droneRigidbody = GetComponent<Rigidbody>();
    }

    // Rigidbody 기본 물리 설정
    private void Awake()
    {
        if (droneRigidbody == null)
        {
            droneRigidbody = GetComponent<Rigidbody>();
        }

        if (droneRigidbody == null)
        {
            Debug.LogWarning("[DroneMovement] Rigidbody is missing.");
            return;
        }

        droneRigidbody.useGravity = true;
        droneRigidbody.isKinematic = false;
        droneRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        droneRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        InitializeAltitudeHold();
    }

    // 물리 프레임에서 드론 물리 조종 처리
    private void FixedUpdate()
    {
        if (droneRigidbody == null)
        {
            return;
        }

        DroneInputData input = inputReader != null
            ? inputReader.CurrentInput
            : new DroneInputData(Vector2.zero, 0f, 0f, false);

        _isHoverActive = input.IsControlling && input.HoverInput;

        UpdateTargetTilt(input);
        ApplyThrust(input);
        ApplyHorizontalAssist(input);
        ApplyYaw(input);
        ApplyTiltStabilization();
        ApplyVelocityDamping();
        ClampVelocity();
        ClampAngularVelocity();
    }

    // 입력 방향에 따라 수평 이동 보조 힘 적용
    private void ApplyHorizontalAssist(DroneInputData input)
    {
        if (!useHorizontalAssist)
        {
            return;
        }

        if (!input.IsControlling || _isHoverActive)
        {
            return;
        }

        Vector2 moveInput = input.MoveInput;

        if (moveInput.sqrMagnitude < assistDeadZone * assistDeadZone)
        {
            return;
        }

        Vector3 forwardAssist = transform.forward * moveInput.y * forwardAssistAcceleration;
        Vector3 sideAssist = transform.right * moveInput.x * sideAssistAcceleration;

        Vector3 assistAcceleration = forwardAssist + sideAssist;

        droneRigidbody.AddForce(assistAcceleration, ForceMode.Acceleration);
    }
    
    // 호버 시작 시 현재 고도를 목표 고도로 저장
    private void UpdateHoverState()
    {
        if (_isHoverActive && !_wasHoverActive)
        {
            _targetAltitude = droneRigidbody.position.y;
        }

        _wasHoverActive = _isHoverActive;
    }
    
    // 시작 고도를 목표 고도로 저장
    private void InitializeAltitudeHold()
    {
        if (droneRigidbody == null)
        {
            return;
        }

        _targetAltitude = droneRigidbody.position.y;
        _altitudeHoldInitialized = true;
    }

    // 입력값을 목표 Pitch/Roll 각도로 변환
    private void UpdateTargetTilt(DroneInputData input)
    {
        if (!input.IsControlling || _isHoverActive)
        {
            _targetPitch = 0f;
            _targetRoll = 0f;
        }
        else
        {
            _targetPitch = input.MoveInput.y * maxPitchAngle;
            _targetRoll = -input.MoveInput.x * maxRollAngle;
        }

        float lerpValue = Mathf.Clamp01(tiltResponsiveness * Time.fixedDeltaTime);

        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, lerpValue);
        _currentRoll = Mathf.Lerp(_currentRoll, _targetRoll, lerpValue);
    }

    // 드론의 로컬 위쪽 방향으로 추력 적용
    private void ApplyThrust(DroneInputData input)
    {
        if (!_altitudeHoldInitialized)
        {
            InitializeAltitudeHold();
        }

        bool hasThrottleInput = input.IsControlling &&
                                Mathf.Abs(input.ThrottleInput) > throttleDeadZone;

        float gravityForce = droneRigidbody.mass * Mathf.Abs(Physics.gravity.y);
        float baseHoverForce = gravityForce * hoverThrustMultiplier;

        float verticalAssistForce = 0f;

        if (hasThrottleInput)
        {
            verticalAssistForce = input.ThrottleInput * throttlePower;

            _targetAltitude = droneRigidbody.position.y;
            _wasThrottleInputActive = true;
        }
        else if (useAltitudeHold)
        {
            if (_wasThrottleInputActive)
            {
                _targetAltitude = droneRigidbody.position.y;
                _wasThrottleInputActive = false;
            }

            verticalAssistForce = CalculateAltitudeHoldForce();
        }

        float finalVerticalForce = baseHoverForce + verticalAssistForce;
        float compensatedThrust = CompensateThrustForTilt(finalVerticalForce);

        droneRigidbody.AddForce(transform.up * compensatedThrust, ForceMode.Force);
    }

    // 현재 고도를 목표 고도에 맞추는 보정 힘 계산
    private float CalculateAltitudeHoldForce()
    {
        float altitudeError = _targetAltitude - droneRigidbody.position.y;
        float verticalVelocity = droneRigidbody.linearVelocity.y;

        float holdStrength = _isHoverActive
            ? hoverAltitudeHoldStrength
            : altitudeHoldStrength;

        float velocityDamping = _isHoverActive
            ? hoverAltitudeVelocityDamping
            : altitudeVelocityDamping;

        float holdForce =
            altitudeError * holdStrength -
            verticalVelocity * velocityDamping;

        return Mathf.Clamp(
            holdForce,
            -maxAltitudeHoldForce,
            maxAltitudeHoldForce
        );
    }

    // 드론이 기울었을 때 수직 추력 손실 보정
    private float CompensateThrustForTilt(float verticalForce)
    {
        float upDot = Vector3.Dot(transform.up, Vector3.up);
        upDot = Mathf.Clamp(upDot, 0.35f, 1f);

        return verticalForce / upDot;
    }

    // 조이스틱 좌우 입력으로 Yaw 토크 적용
    private void ApplyYaw(DroneInputData input)
    {
        if (!input.IsControlling || _isHoverActive)
        {
            return;
        }

        Vector3 localAngularVelocity = transform.InverseTransformDirection(droneRigidbody.angularVelocity);
        float currentYawSpeed = localAngularVelocity.y * Mathf.Rad2Deg;

        if (Mathf.Abs(currentYawSpeed) > maxYawAngularSpeed)
        {
            return;
        }

        Vector3 yawTorque = transform.up * input.YawInput * yawTorquePower;

        droneRigidbody.AddTorque(yawTorque, ForceMode.Acceleration);
    }

    // 목표 Pitch/Roll 자세로 안정화 토크 적용
    private void ApplyTiltStabilization()
    {
        Quaternion currentYaw = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        Quaternion targetRotation =
            currentYaw *
            Quaternion.Euler(_currentPitch, 0f, _currentRoll);

        Quaternion rotationError = targetRotation * Quaternion.Inverse(transform.rotation);

        rotationError.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
        {
            angle -= 360f;
        }

        if (Mathf.Abs(angle) < 0.01f || axis == Vector3.zero)
        {
            return;
        }

        Vector3 stabilizationTorque =
            axis.normalized * angle * Mathf.Deg2Rad * uprightStabilization;

        droneRigidbody.AddTorque(stabilizationTorque, ForceMode.Acceleration);

        float currentAngularDamping = _isHoverActive
            ? hoverAngularDamping
            : angularDamping;

        Vector3 dampingTorque = -droneRigidbody.angularVelocity * currentAngularDamping;

        droneRigidbody.AddTorque(dampingTorque, ForceMode.Acceleration);
    }

    // 수평 이동 감쇠 처리
    private void ApplyVelocityDamping()
    {
        Vector3 horizontalVelocity = new Vector3(
            droneRigidbody.linearVelocity.x,
            0f,
            droneRigidbody.linearVelocity.z
        );

        float currentHorizontalDamping = _isHoverActive
            ? hoverHorizontalDamping
            : horizontalDamping;

        Vector3 dampingForce = -horizontalVelocity * currentHorizontalDamping;

        droneRigidbody.AddForce(dampingForce, ForceMode.Acceleration);
    }

    // 최대 이동 속도 제한
    private void ClampVelocity()
    {
        Vector3 velocity = droneRigidbody.linearVelocity;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

        if (horizontalVelocity.magnitude > maxHorizontalSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;
        }

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        velocity.y = Mathf.Clamp(
            velocity.y,
            -maxVerticalSpeed,
            maxVerticalSpeed
        );

        if (velocity.magnitude > maxTotalSpeed)
        {
            velocity = velocity.normalized * maxTotalSpeed;
        }

        droneRigidbody.linearVelocity = velocity;
    }

    // 최대 각속도 제한
    private void ClampAngularVelocity()
    {
        Vector3 angularVelocity = droneRigidbody.angularVelocity;

        float maxAngularVelocity = maxYawAngularSpeed * Mathf.Deg2Rad;

        angularVelocity.y = Mathf.Clamp(
            angularVelocity.y,
            -maxAngularVelocity,
            maxAngularVelocity
        );

        droneRigidbody.angularVelocity = angularVelocity;
    }
}