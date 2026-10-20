using TMPro;
using UnityEngine;

public class DroneSpeedometerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody droneRigidbody;
    [SerializeField] private Transform droneTransform;

    [Header("Texts")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text altitudeText;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private bool showKmh = false;

    private float _timer;

    // 속도계 UI 갱신 주기 처리
    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer < updateInterval)
        {
            return;
        }

        _timer = 0f;
        UpdateSpeedometer();
    }

    // 드론 속도와 고도 표시 갱신
    private void UpdateSpeedometer()
    {
        if (droneRigidbody == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = droneRigidbody.linearVelocity;
#else
        Vector3 velocity = droneRigidbody.velocity;
#endif

        float speed = velocity.magnitude;
        float displaySpeed = showKmh ? speed * 3.6f : speed;
        string speedUnit = showKmh ? "km/h" : "m/s";

        if (speedText != null)
        {
            speedText.text = $"SPD  {displaySpeed:0.0} {speedUnit}";
        }

        if (altitudeText != null && droneTransform != null)
        {
            altitudeText.text = $"ALT  {droneTransform.position.y:0.0} m";
        }
    }
}