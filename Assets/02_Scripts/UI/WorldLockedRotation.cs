using UnityEngine;

/// <summary>
/// 위치는 부모(컨트롤러)를 따라가고, 회전은 '기울기(pitch=X, roll=Z)'만 고정한다.
/// 방향(yaw=Y)은 컨트롤러를 따라가므로 나침반처럼 한 방향에 묶이지 않는다.
/// 수평 '기준(Reference)' Stabilizer에 붙인다.
/// </summary>
public class WorldLockedRotation : MonoBehaviour
{
    [Tooltip("yaw(방향)를 읽어올 컨트롤러. 비워두면 부모 Transform을 사용.")]
    public Transform controller;

    [Tooltip("고정할 기준 기울기. X=pitch, Z=roll. Stabilizer_A의 로컬 Rotation과 동일하게 둔다.")]
    public Vector3 tiltOffsetEuler = new Vector3(115f, 0f, 0f);

    void Start()
    {
        if (controller == null) controller = transform.parent;
    }

    void LateUpdate()
    {
        // 컨트롤러 forward를 수평면에 투영해 '방향(yaw)'만 뽑아낸다.
        Vector3 flatForward = controller.forward;
        flatForward.y = 0f;

        // 거의 수직으로 가리키면 방향이 불안정 -> 직전 회전 유지.
        if (flatForward.sqrMagnitude < 1e-4f) return;

        Quaternion heading = Quaternion.LookRotation(flatForward, Vector3.up);

        // 방향은 손을 따라가고, 기울기(X·Z)는 tiltOffsetEuler로 고정.
        transform.rotation = heading * Quaternion.Euler(tiltOffsetEuler);
    }
}