using UnityEngine;

public class DroneCameraRigFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraMount;
    [SerializeField] private Transform mainCamera;

    [Header("Position Follow")]
    [SerializeField] private bool alignMainCameraToMount = true;
    [SerializeField] private float positionLerpSpeed = 30f;

    [Header("Rotation Follow")]
    [SerializeField] private bool followYaw = true;
    [SerializeField] private bool followPitchAndRoll = false;
    [SerializeField] private float rotationLerpSpeed = 15f;

    [Header("Start Option")]
    [SerializeField] private bool snapOnStart = true;

    // 시작 시 카메라 리그 위치를 즉시 보정
    private void Start()
    {
        if (!snapOnStart)
        {
            return;
        }

        SnapToMount();
    }

    // 카메라 리그를 드론 카메라 마운트에 맞춰 갱신
    private void LateUpdate()
    {
        if (cameraMount == null)
        {
            return;
        }

        FollowMountRotation();
        FollowMountPosition();
    }

    // XR Origin 회전을 드론 기준 방향에 맞춤
    private void FollowMountRotation()
    {
        Quaternion targetRotation = GetTargetRotation();

        float lerpValue = Mathf.Clamp01(rotationLerpSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            lerpValue
        );
    }

    // XR Origin 위치를 카메라 마운트 위치에 맞춤
    private void FollowMountPosition()
    {
        Vector3 targetPosition = GetTargetOriginPosition();

        float lerpValue = Mathf.Clamp01(positionLerpSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            lerpValue
        );
    }

    // 목표 회전값 계산
    private Quaternion GetTargetRotation()
    {
        if (followPitchAndRoll)
        {
            return cameraMount.rotation;
        }

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 mountEuler = cameraMount.rotation.eulerAngles;

        float targetYaw = followYaw ? mountEuler.y : currentEuler.y;

        return Quaternion.Euler(0f, targetYaw, 0f);
    }

    // 목표 XR Origin 위치 계산
    private Vector3 GetTargetOriginPosition()
    {
        if (!alignMainCameraToMount || mainCamera == null)
        {
            return cameraMount.position;
        }

        Vector3 cameraToMountOffset = cameraMount.position - mainCamera.position;

        return transform.position + cameraToMountOffset;
    }

    // 카메라 리그를 카메라 마운트에 즉시 정렬
    [ContextMenu("Snap To Mount")]
    public void SnapToMount()
    {
        if (cameraMount == null)
        {
            return;
        }

        transform.rotation = GetTargetRotation();
        transform.position = GetTargetOriginPosition();
    }
}