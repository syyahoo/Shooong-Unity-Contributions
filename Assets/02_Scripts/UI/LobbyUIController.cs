using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIController : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private AuthService authService;
    [SerializeField] private OrderService orderService;
    [SerializeField] private DronePoseBatchSender poseBatchSender;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Buttons")]
    [SerializeField] private Button signUpButton;
    [SerializeField] private Button loginAndStartButton;

    [Header("Labels")]
    [SerializeField] private TMP_Text signUpButtonLabel;
    [SerializeField] private TMP_Text statusText;

    [Header("Scene")]
    [SerializeField] private string flightSceneName = "Flight";

    [Header("Sign Up Mode")]
    [SerializeField] private bool hideNameFieldInLoginMode = true;

    [Header("Fallback")]
    [SerializeField] private bool forceStartOnLoginTimeout = true;
    [SerializeField] private float loginFallbackDelay = 3f;
    [SerializeField] private int fallbackFlightId = 1;

    private bool _isSignUpMode;
    private bool _isProcessingStart;
    private bool _isProcessingSignUp;
    private Coroutine _loginFallbackCoroutine;

    // 초기화
    private void Awake()
    {
        BindButtons();
        BindNetworkEvents();
        SetLoginMode();
    }

    // 이벤트 해제
    private void OnDestroy()
    {
        UnbindButtons();
        UnbindNetworkEvents();

        if (_loginFallbackCoroutine != null)
        {
            StopCoroutine(_loginFallbackCoroutine);
            _loginFallbackCoroutine = null;
        }
    }

    // 버튼 이벤트 연결
    private void BindButtons()
    {
        if (signUpButton != null)
        {
            signUpButton.onClick.AddListener(OnClickSignUpButton);
        }

        if (loginAndStartButton != null)
        {
            loginAndStartButton.onClick.AddListener(OnClickLoginAndStart);
        }
    }

    // 버튼 이벤트 해제
    private void UnbindButtons()
    {
        if (signUpButton != null)
        {
            signUpButton.onClick.RemoveListener(OnClickSignUpButton);
        }

        if (loginAndStartButton != null)
        {
            loginAndStartButton.onClick.RemoveListener(OnClickLoginAndStart);
        }
    }

    // 네트워크 이벤트 연결
    private void BindNetworkEvents()
    {
        if (authService != null)
        {
            authService.OnSignUpSucceeded += HandleSignUpSucceeded;
            authService.OnLoginSucceeded += HandleLoginSucceeded;
        }

        if (orderService != null)
        {
            orderService.OnFlightIdUpdated += HandleFlightIdUpdated;
        }
    }

    // 네트워크 이벤트 해제
    private void UnbindNetworkEvents()
    {
        if (authService != null)
        {
            authService.OnSignUpSucceeded -= HandleSignUpSucceeded;
            authService.OnLoginSucceeded -= HandleLoginSucceeded;
        }

        if (orderService != null)
        {
            orderService.OnFlightIdUpdated -= HandleFlightIdUpdated;
        }
    }

    // 회원가입 버튼 처리
    private void OnClickSignUpButton()
    {
        if (_isProcessingSignUp)
        {
            return;
        }

        if (!_isSignUpMode)
        {
            SetSignUpMode();
            return;
        }

        RequestSignUp();
    }

    // 회원가입 요청
    private void RequestSignUp()
    {
        if (authService == null)
        {
            SetStatus("회원가입 실패");
            return;
        }

        string id = GetInputText(idInputField);
        string password = GetInputText(passwordInputField);
        string name = GetInputText(nameInputField);

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
        {
            SetStatus("입력 필요");
            return;
        }

        _isProcessingSignUp = true;

        SetButtonInteractable(signUpButton, false);
        SetButtonInteractable(loginAndStartButton, false);
        SetStatus("회원가입 중");

        authService.SignUp(id, password, name);
    }

    // 로그인 후 비행 시작 버튼 처리
    private void OnClickLoginAndStart()
    {
        if (_isProcessingStart || _isSignUpMode)
        {
            return;
        }

        string id = GetInputText(idInputField);
        string password = GetInputText(passwordInputField);

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            SetStatus("입력 필요");
            return;
        }

        _isProcessingStart = true;

        SetButtonInteractable(signUpButton, false);
        SetButtonInteractable(loginAndStartButton, false);
        SetStatus("로그인 중");

        StartLoginFallbackTimer();

        if (authService == null)
        {
            ForceStartFlight();
            return;
        }

        authService.Login(id, password);
    }

    // 회원가입 성공 처리
    private void HandleSignUpSucceeded()
    {
        _isProcessingSignUp = false;

        SetStatus("회원가입 성공");
        SetLoginMode();
    }

    // 로그인 성공 처리
    private void HandleLoginSucceeded()
    {
        if (!_isProcessingStart)
        {
            return;
        }

        StopLoginFallbackTimer();

        SetStatus("비행 준비 중");

        if (orderService == null)
        {
            ForceStartFlight();
            return;
        }

        orderService.CreateOrder();

        if (forceStartOnLoginTimeout)
        {
            _loginFallbackCoroutine = StartCoroutine(OrderFallbackRoutine());
        }
    }

    // 비행 ID 갱신 처리
    private void HandleFlightIdUpdated(int flightId)
    {
        if (!_isProcessingStart)
        {
            return;
        }

        StopLoginFallbackTimer();

        SetStatus("비행 시작");
        SceneManager.LoadScene(flightSceneName);
    }

    // 로그인 실패 대비 타이머 시작
    private void StartLoginFallbackTimer()
    {
        StopLoginFallbackTimer();

        if (!forceStartOnLoginTimeout)
        {
            return;
        }

        _loginFallbackCoroutine = StartCoroutine(LoginFallbackRoutine());
    }

    // 로그인 실패 대비 타이머 정지
    private void StopLoginFallbackTimer()
    {
        if (_loginFallbackCoroutine == null)
        {
            return;
        }

        StopCoroutine(_loginFallbackCoroutine);
        _loginFallbackCoroutine = null;
    }

    // 로그인 응답 지연 시 강제 시작
    private IEnumerator LoginFallbackRoutine()
    {
        yield return new WaitForSeconds(loginFallbackDelay);

        if (!_isProcessingStart)
        {
            yield break;
        }

        ForceStartFlight();
    }

    // 비행 준비 응답 지연 시 강제 시작
    private IEnumerator OrderFallbackRoutine()
    {
        yield return new WaitForSeconds(loginFallbackDelay);

        if (!_isProcessingStart)
        {
            yield break;
        }

        ForceStartFlight();
    }

    // 강제 비행 시작
    private void ForceStartFlight()
    {
        if (!_isProcessingStart)
        {
            return;
        }

        StopLoginFallbackTimer();

        if (poseBatchSender != null)
        {
            poseBatchSender.SetFlightId(fallbackFlightId);
        }

        SetStatus("비행 시작");
        SceneManager.LoadScene(flightSceneName);
    }

    // 로그인 모드 설정
    private void SetLoginMode()
    {
        _isSignUpMode = false;
        _isProcessingSignUp = false;

        SetNameFieldActive(false);

        if (signUpButtonLabel != null)
        {
            signUpButtonLabel.text = "Sign Up";
        }

        SetButtonInteractable(signUpButton, true);
        SetButtonInteractable(loginAndStartButton, true);

        SetStatus("로그인 대기");
    }

    // 회원가입 모드 설정
    private void SetSignUpMode()
    {
        _isSignUpMode = true;

        SetNameFieldActive(true);

        if (signUpButtonLabel != null)
        {
            signUpButtonLabel.text = "계정 생성";
        }

        SetButtonInteractable(signUpButton, true);
        SetButtonInteractable(loginAndStartButton, false);

        SetStatus("회원가입 대기");
    }

    // 이름 입력창 표시 설정
    private void SetNameFieldActive(bool isActive)
    {
        if (nameInputField == null)
        {
            return;
        }

        if (hideNameFieldInLoginMode)
        {
            nameInputField.gameObject.SetActive(isActive);
            return;
        }

        nameInputField.interactable = isActive;
    }

    // 버튼 활성화 설정
    private void SetButtonInteractable(Button button, bool isInteractable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = isInteractable;
    }

    // 입력값 반환
    private string GetInputText(TMP_InputField inputField)
    {
        if (inputField == null)
        {
            return string.Empty;
        }

        return inputField.text.Trim();
    }

    // 상태 텍스트 설정
    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log($"[LobbyUIController] {message}");
    }
}