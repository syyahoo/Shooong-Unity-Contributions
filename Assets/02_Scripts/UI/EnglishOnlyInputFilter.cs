using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class EnglishOnlyInputFilter : MonoBehaviour
{
    [Header("Allowed Characters")]
    [SerializeField] private bool allowUppercase = true;
    [SerializeField] private bool allowLowercase = true;
    [SerializeField] private bool allowDigits = true;
    [SerializeField] private bool allowSpace = false;
    [SerializeField] private bool allowBasicSymbols = false;

    private TMP_InputField _inputField;
    private bool _isFiltering;

    // 컴포넌트 참조 및 입력 이벤트 연결
    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.onValueChanged.AddListener(FilterInput);
    }

    // 오브젝트 해제 시 입력 이벤트 해제
    private void OnDestroy()
    {
        if (_inputField == null)
        {
            return;
        }

        _inputField.onValueChanged.RemoveListener(FilterInput);
    }

    // 입력 문자열에서 허용되지 않은 문자 제거
    private void FilterInput(string value)
    {
        if (_isFiltering)
        {
            return;
        }

        string filteredText = GetFilteredText(value);

        if (filteredText == value)
        {
            return;
        }

        _isFiltering = true;

        int caretPosition = Mathf.Min(_inputField.caretPosition, filteredText.Length);

        _inputField.text = filteredText;
        _inputField.caretPosition = caretPosition;
        _inputField.stringPosition = caretPosition;

        _isFiltering = false;
    }

    // 허용 문자만 남긴 문자열 반환
    private string GetFilteredText(string value)
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];

            if (IsAllowedCharacter(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    // 문자 허용 여부 확인
    private bool IsAllowedCharacter(char character)
    {
        if (allowUppercase && character >= 'A' && character <= 'Z')
        {
            return true;
        }

        if (allowLowercase && character >= 'a' && character <= 'z')
        {
            return true;
        }

        if (allowDigits && character >= '0' && character <= '9')
        {
            return true;
        }

        if (allowSpace && character == ' ')
        {
            return true;
        }

        if (allowBasicSymbols && IsBasicSymbol(character))
        {
            return true;
        }

        return false;
    }

    // 기본 특수문자 허용 여부 확인
    private bool IsBasicSymbol(char character)
    {
        return character == '_' ||
               character == '-' ||
               character == '.' ||
               character == '@' ||
               character == '!' ||
               character == '#';
    }
}