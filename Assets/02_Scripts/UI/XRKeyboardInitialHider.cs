using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

public class XRKeyboardInitialHider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRKeyboard keyboard;

    // 키보드 초기화 후 기본 상태를 숨김으로 설정
    private IEnumerator Start()
    {
        yield return null;

        if (keyboard == null)
        {
            Debug.LogWarning("[XRKeyboardInitialHider] Keyboard is missing.");
            yield break;
        }

        keyboard.Close(false);
    }
}