using UnityEngine;

public class DroneAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody droneRigidbody;
    [SerializeField] private AudioSource engineAudioSource;

    [Header("Speed Range")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Pitch")]
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.4f;

    [Header("Volume")]
    [SerializeField] private float minVolume = 0.5f;
    [SerializeField] private float maxVolume = 1f;

    [Header("Smoothing")]
    [SerializeField] private float responsiveness = 4f;

    private float _currentPitch;
    private float _currentVolume;

    // 컴포넌트 참조 자동 연결
    private void Reset()
    {
        droneRigidbody = GetComponent<Rigidbody>();
        engineAudioSource = GetComponent<AudioSource>();
    }

    // 사운드 기본 설정 및 재생 시작
    private void Awake()
    {
        if (engineAudioSource == null)
        {
            engineAudioSource = GetComponent<AudioSource>();
        }

        if (engineAudioSource == null)
        {
            Debug.LogWarning("[DroneAudioController] AudioSource is missing.");
            return;
        }

        if (droneRigidbody == null)
        {
            Debug.LogWarning("[DroneAudioController] Rigidbody is missing.");
        }

        engineAudioSource.loop = true;

        _currentPitch = minPitch;
        _currentVolume = minVolume;

        engineAudioSource.pitch = _currentPitch;
        engineAudioSource.volume = _currentVolume;

        if (!engineAudioSource.isPlaying)
        {
            engineAudioSource.Play();
        }
    }

    // 매 프레임 드론 속도에 따라 사운드 피치/볼륨 갱신
    private void Update()
    {
        if (engineAudioSource == null || droneRigidbody == null)
        {
            return;
        }

        float speed = droneRigidbody.linearVelocity.magnitude;

        // 속도를 0~1 비율로 변환 (minSpeed 이하 = 0, maxSpeed 이상 = 1)
        float speedRatio = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

        float targetPitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
        float targetVolume = Mathf.Lerp(minVolume, maxVolume, speedRatio);

        // DroneMovement의 틸트 보간과 동일한 방식으로 부드럽게 따라감
        float lerpValue = Mathf.Clamp01(responsiveness * Time.deltaTime);

        _currentPitch = Mathf.Lerp(_currentPitch, targetPitch, lerpValue);
        _currentVolume = Mathf.Lerp(_currentVolume, targetVolume, lerpValue);

        engineAudioSource.pitch = _currentPitch;
        engineAudioSource.volume = _currentVolume;
    }
}