using System;
using System.Collections;
using UnityEngine;

public class OrderService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ApiConfigSO apiConfig;
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private DronePoseBatchSender poseBatchSender;

    [Header("Endpoint")]
    [SerializeField] private string ordersPath = "/api/orders";

    [Header("Fallback")]
    [SerializeField] private bool useFallbackFlightId = true;
    [SerializeField] private int fallbackFlightId = 1;

    public event Action<int> OnFlightIdUpdated;

    public int CurrentFlightId { get; private set; }

    // 테스트용 비행 준비 요청
    [ContextMenu("Test Create Flight")]
    public void TestCreateOrder()
    {
        CreateOrder();
    }

    // 테스트용 비행 목록 요청
    [ContextMenu("Test Get Flights")]
    public void TestGetOrders()
    {
        GetOrders();
    }

    // 비행 준비 요청
    public void CreateOrder()
    {
        if (!CanRequest())
        {
            ApplyFallbackFlightId();
            return;
        }

        StartCoroutine(CreateOrderRoutine());
    }

    // 비행 목록 요청
    public void GetOrders()
    {
        if (!CanRequest())
        {
            ApplyFallbackFlightId();
            return;
        }

        StartCoroutine(GetOrdersRoutine());
    }

    // 비행 준비 요청 처리
    private IEnumerator CreateOrderRoutine()
    {
        string url = apiConfig.BuildUrl(ordersPath);
        string json = "{}";

        bool requestFinished = false;
        bool requestSucceeded = false;

        yield return apiClient.PostJson(
            url,
            json,
            true,
            response =>
            {
                requestFinished = true;
                requestSucceeded = true;

                Debug.Log($"[OrderService] Create flight response: {response}");

                int parsedFlightId = TryFindIntValue(response, "flightId");

                if (parsedFlightId <= 0)
                {
                    parsedFlightId = TryFindIntValue(response, "orderId");
                }

                if (parsedFlightId <= 0)
                {
                    parsedFlightId = fallbackFlightId;
                }

                SetCurrentFlightId(parsedFlightId);
            },
            (statusCode, error) =>
            {
                requestFinished = true;
                requestSucceeded = false;

                Debug.LogWarning($"[OrderService] Create flight failed. Status: {statusCode}, Error: {error}");
            });

        if (!requestFinished || requestSucceeded)
        {
            yield break;
        }

        ApplyFallbackFlightId();
    }

    // 비행 목록 요청 처리
    private IEnumerator GetOrdersRoutine()
    {
        string url = apiConfig.BuildUrl(ordersPath);

        bool requestFinished = false;
        bool requestSucceeded = false;

        yield return apiClient.GetJson(
            url,
            true,
            response =>
            {
                requestFinished = true;
                requestSucceeded = true;

                Debug.Log($"[OrderService] Get flights response: {response}");

                int parsedFlightId = TryFindIntValue(response, "flightId");

                if (parsedFlightId <= 0)
                {
                    parsedFlightId = TryFindIntValue(response, "orderId");
                }

                if (parsedFlightId <= 0)
                {
                    parsedFlightId = fallbackFlightId;
                }

                SetCurrentFlightId(parsedFlightId);
            },
            (statusCode, error) =>
            {
                requestFinished = true;
                requestSucceeded = false;

                Debug.LogWarning($"[OrderService] Get flights failed. Status: {statusCode}, Error: {error}");
            });

        if (!requestFinished || requestSucceeded)
        {
            yield break;
        }

        ApplyFallbackFlightId();
    }

    // 현재 비행 ID 설정
    private void SetCurrentFlightId(int newFlightId)
    {
        CurrentFlightId = newFlightId;

        if (poseBatchSender != null)
        {
            poseBatchSender.SetFlightId(CurrentFlightId);
        }

        Debug.Log($"[OrderService] Current Flight Id: {CurrentFlightId}");

        OnFlightIdUpdated?.Invoke(CurrentFlightId);
    }

    // 서버 실패 시 임시 비행 ID 적용
    private void ApplyFallbackFlightId()
    {
        if (!useFallbackFlightId)
        {
            Debug.LogWarning("[OrderService] Fallback flight id is disabled.");
            return;
        }

        Debug.LogWarning($"[OrderService] Using fallback flight id: {fallbackFlightId}");
        SetCurrentFlightId(fallbackFlightId);
    }

    // 요청 가능 여부 확인
    private bool CanRequest()
    {
        if (apiConfig == null)
        {
            Debug.LogWarning("[OrderService] ApiConfig is missing.");
            return false;
        }

        if (apiClient == null)
        {
            Debug.LogWarning("[OrderService] ApiClient is missing.");
            return false;
        }

        return true;
    }

    // JSON 문자열에서 정수 필드 값 추출
    private int TryFindIntValue(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key))
        {
            return 0;
        }

        string pattern = $"\"{key}\"";

        int keyIndex = json.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);

        if (keyIndex < 0)
        {
            return 0;
        }

        int colonIndex = json.IndexOf(':', keyIndex);

        if (colonIndex < 0)
        {
            return 0;
        }

        int startIndex = colonIndex + 1;

        while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
        {
            startIndex++;
        }

        int endIndex = startIndex;

        while (endIndex < json.Length && char.IsDigit(json[endIndex]))
        {
            endIndex++;
        }

        if (endIndex <= startIndex)
        {
            return 0;
        }

        string numberText = json.Substring(startIndex, endIndex - startIndex);

        if (!int.TryParse(numberText, out int result))
        {
            return 0;
        }

        return result;
    }
}