using System;

[Serializable]
public class OrderCreateResponse
{
    public string statusCode;
    public string message;
}

[Serializable]
public class OrderListResponse
{
    public string statusCode;
    public string message;
    public OrderSummary[] data;
}

[Serializable]
public class OrderSummary
{
    public int orderId;
    public int flightId;
    public string rating;
    public string score;
    public string totalFlightTime;
    public string averageTiltTime;
    public string orderStatus;
    public int remainWaypointCnt;
    public bool isCrashed;

    // 응답에서 사용할 비행 ID 반환
    public int GetFlightId()
    {
        if (flightId > 0)
        {
            return flightId;
        }

        return orderId;
    }
}
