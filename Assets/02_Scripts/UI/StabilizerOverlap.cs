using UnityEngine;

/// <summary>
/// 컨트롤러를 따라가는 Stabilizer_A의 sphere들과
/// 회전 고정된 기준(Reference) Stabilizer의 sphere들이 '겹치면' 그 sphere의 색을 바꾼다.
///
/// 세 배열(movingSpheres / movingRenderers / referenceSpheres)은 반드시 같은 순서로 채운다.
/// 예) index 0 = 위쪽 sphere끼리 짝, index 1 = 오른쪽 sphere끼리 짝 ...
/// </summary>
public class StabilizerOverlap : MonoBehaviour
{
    [Header("Stabilizer_A (컨트롤러를 따라가는 쪽)")]
    public Transform[] movingSpheres;
    public Renderer[]  movingRenderers;   // 색을 바꿀 대상. movingSpheres와 같은 순서.

    [Header("기준 Stabilizer (저투명, 회전 고정)")]
    public Transform[] referenceSpheres;  // movingSpheres와 같은 순서.

    [Header("판정 / 색")]
    [Tooltip("이 거리(m)보다 가까우면 '겹침'으로 판정. 작게 시작해서 조절.")]
    public float alignDistance = 0.02f;
    public Color normalColor  = Color.white;
    public Color alignedColor = Color.green;

    [Tooltip("URP면 \"_BaseColor\", Built-in Unlit이면 \"_Color\".")]
    public string colorProperty = "_BaseColor";

    void LateUpdate()
    {
        int count = Mathf.Min(movingSpheres.Length,
            Mathf.Min(referenceSpheres.Length, movingRenderers.Length));

        for (int i = 0; i < count; i++)
        {
            // 짝이 맞는 sphere끼리의 거리만 잰다. (A_Top <-> Ref_Top 등)
            float distance = Vector3.Distance(movingSpheres[i].position, referenceSpheres[i].position);
            bool aligned = distance <= alignDistance;

            // 겹친 sphere만 색이 바뀐다.
            movingRenderers[i].material.SetColor(colorProperty, aligned ? alignedColor : normalColor);
        }
    }
}