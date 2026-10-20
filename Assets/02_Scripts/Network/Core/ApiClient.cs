using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AuthTokenStore tokenStore;

    [Header("Debug")]
    [SerializeField] private bool logRequest = true;
    [SerializeField] private bool logResponse = true;

    // 컴포넌트 참조 자동 연결
    private void Reset()
    {
        tokenStore = GetComponent<AuthTokenStore>();
    }

    // JSON POST 요청
    public IEnumerator PostJson(
        string url,
        string jsonBody,
        bool useAuthorization,
        Action<string> onSuccess,
        Action<long, string> onFailed)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody ?? string.Empty);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            SetDefaultHeaders(request, useAuthorization);

            if (logRequest)
            {
                Debug.Log($"[ApiClient] POST {url}\n{jsonBody}");
            }

            yield return request.SendWebRequest();

            HandleResponse(request, onSuccess, onFailed);
        }
    }

    // 빈 Body POST 요청
    public IEnumerator PostEmpty(
        string url,
        bool useAuthorization,
        Action<string> onSuccess,
        Action<long, string> onFailed)
    {
        yield return PostJson(
            url,
            string.Empty,
            useAuthorization,
            onSuccess,
            onFailed
        );
    }

    // JSON GET 요청
    public IEnumerator GetJson(
        string url,
        bool useAuthorization,
        Action<string> onSuccess,
        Action<long, string> onFailed)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            SetDefaultHeaders(request, useAuthorization);

            if (logRequest)
            {
                Debug.Log($"[ApiClient] GET {url}");
            }

            yield return request.SendWebRequest();

            HandleResponse(request, onSuccess, onFailed);
        }
    }

    // 기본 헤더 설정
    private void SetDefaultHeaders(UnityWebRequest request, bool useAuthorization)
    {
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Content-Type", "application/json");

        if (!useAuthorization)
        {
            return;
        }

        if (tokenStore == null || !tokenStore.HasAccessToken)
        {
            Debug.LogWarning("[ApiClient] AccessToken is missing.");
            return;
        }

        request.SetRequestHeader("Authorization", tokenStore.GetAuthorizationHeaderValue());
    }

    // 응답 처리
    private void HandleResponse(
        UnityWebRequest request,
        Action<string> onSuccess,
        Action<long, string> onFailed)
    {
        string responseText = request.downloadHandler != null
            ? request.downloadHandler.text
            : string.Empty;

        bool isSuccess =
            request.result == UnityWebRequest.Result.Success &&
            request.responseCode >= 200 &&
            request.responseCode < 300;

        if (logResponse)
        {
            Debug.Log($"[ApiClient] Response {request.responseCode}\n{responseText}");
        }

        if (isSuccess)
        {
            onSuccess?.Invoke(responseText);
            return;
        }

        Debug.LogWarning($"[ApiClient] Request failed. Code: {request.responseCode}\n{responseText}");
        onFailed?.Invoke(request.responseCode, responseText);
    }
}