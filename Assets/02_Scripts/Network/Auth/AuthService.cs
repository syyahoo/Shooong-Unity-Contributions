using System;
using UnityEngine;

public class AuthService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ApiConfigSO apiConfig;
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private AuthTokenStore tokenStore;

    [Header("Test Account")]
    [SerializeField] private string testId = "";
    [SerializeField] private string testPassword = "";
    [SerializeField] private string testName = "";

    public event Action OnLoginSucceeded;
    public event Action OnSignUpSucceeded;

    // 컴포넌트 참조 자동 연결
    private void Reset()
    {
        apiClient = GetComponent<ApiClient>();
        tokenStore = GetComponent<AuthTokenStore>();
    }

    // 테스트 회원가입 요청
    [ContextMenu("Test Sign Up")]
    public void TestSignUp()
    {
        SignUp(testId, testPassword, testName);
    }

    // 테스트 로그인 요청
    [ContextMenu("Test Login")]
    public void TestLogin()
    {
        Login(testId, testPassword);
    }

    // 회원가입 요청
    public void SignUp(string id, string password, string name)
    {
        if (!CanRequest())
        {
            return;
        }

        SignUpRequest request = new SignUpRequest(id, password, name);
        string json = JsonUtility.ToJson(request);

        StartCoroutine(apiClient.PostJson(
            apiConfig.SignUpUrl,
            json,
            false,
            HandleSignUpSuccess,
            HandleRequestFailed
        ));
    }

    // 로그인 요청
    public void Login(string userId, string password)
    {
        if (!CanRequest())
        {
            return;
        }

        LoginRequest request = new LoginRequest(userId, password);
        string json = JsonUtility.ToJson(request);

        StartCoroutine(apiClient.PostJson(
            apiConfig.LoginUrl,
            json,
            false,
            HandleLoginSuccess,
            HandleRequestFailed
        ));
    }

    // 요청 가능 상태 확인
    private bool CanRequest()
    {
        if (apiConfig == null)
        {
            Debug.LogWarning("[AuthService] ApiConfig is missing.");
            return false;
        }

        if (apiClient == null)
        {
            Debug.LogWarning("[AuthService] ApiClient is missing.");
            return false;
        }

        if (tokenStore == null)
        {
            Debug.LogWarning("[AuthService] AuthTokenStore is missing.");
            return false;
        }

        return true;
    }

    // 회원가입 성공 처리
    private void HandleSignUpSuccess(string responseText)
    {
        SimpleApiResponse response = JsonUtility.FromJson<SimpleApiResponse>(responseText);

        string message = response != null ? response.message : responseText;
        Debug.Log($"[AuthService] Sign up success. {message}");

        OnSignUpSucceeded?.Invoke();
    }

    // 로그인 성공 처리
    private void HandleLoginSuccess(string responseText)
    {
        LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);

        if (response == null || response.data == null || string.IsNullOrEmpty(response.data.accessToken))
        {
            Debug.LogWarning("[AuthService] Login response token is missing.");
            return;
        }

        tokenStore.Save(response.data);

        Debug.Log($"[AuthService] Login success. {response.message}");

        OnLoginSucceeded?.Invoke();
    }

    // 요청 실패 처리
    private void HandleRequestFailed(long statusCode, string responseText)
    {
        Debug.LogWarning($"[AuthService] Request failed. Code: {statusCode}\n{responseText}");
    }
}