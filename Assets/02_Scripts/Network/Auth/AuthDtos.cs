using System;

[Serializable]
public class SignUpRequest
{
    public string id;
    public string password;
    public string name;

    // 회원가입 요청 데이터 생성
    public SignUpRequest(string id, string password, string name)
    {
        this.id = id;
        this.password = password;
        this.name = name;
    }
}

[Serializable]
public class LoginRequest
{
    public string userId;
    public string password;

    // 로그인 요청 데이터 생성
    public LoginRequest(string userId, string password)
    {
        this.userId = userId;
        this.password = password;
    }
}

[Serializable]
public class TokenData
{
    public string accessToken;
    public string refreshToken;
    public string tokenType;
}

[Serializable]
public class LoginResponse
{
    public string statusCode;
    public string message;
    public TokenData data;
}

[Serializable]
public class SimpleApiResponse
{
    public string statusCode;
    public string message;
}
