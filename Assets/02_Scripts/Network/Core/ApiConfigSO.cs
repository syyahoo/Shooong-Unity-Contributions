using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "Shooong/Network/Api Config")]
public class ApiConfigSO : ScriptableObject
{
    [Header("Server")]
    [SerializeField] private string baseUrl = "https://example.invalid";

    [Header("Auth Endpoints")]
    [SerializeField] private string signUpPath = "/api/members";
    [SerializeField] private string loginPath = "/api/auth/login";

    [Header("Order Endpoints")]
    [SerializeField] private string ordersPath = "/api/orders";

    [Header("Telemetry Endpoint")]
    [SerializeField] private string dronePoseBatchPath = "/api/flights/{flightId}/poses";

    public string SignUpUrl => BuildUrl(signUpPath);
    public string LoginUrl => BuildUrl(loginPath);
    public string OrdersUrl => BuildUrl(ordersPath);
    public string OrdersPath => ordersPath;

    // API 경로를 전체 URL로 변환
    public string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return path;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return baseUrl;
        }

        string cleanBaseUrl = baseUrl.TrimEnd('/');
        string cleanPath = path.StartsWith("/") ? path : $"/{path}";

        return $"{cleanBaseUrl}{cleanPath}";
    }

    // 비행 ID가 포함된 드론 위치 전송 URL 생성
    public string BuildDronePoseBatchUrl(int flightId)
    {
        string path = dronePoseBatchPath.Replace("{flightId}", flightId.ToString());
        path = path.Replace("{orderId}", flightId.ToString());

        return BuildUrl(path);
    }
}
