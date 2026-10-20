using System;
using UnityEngine;

public class AuthTokenStore : MonoBehaviour
{
    private const string AccessTokenKey = "Shooong_AccessToken";
    private const string RefreshTokenKey = "Shooong_RefreshToken";
    private const string TokenTypeKey = "Shooong_TokenType";

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string TokenType { get; private set; } = "Bearer";

    public bool HasAccessToken => !string.IsNullOrEmpty(AccessToken);

    // 저장된 토큰 불러오기
    private void Awake()
    {
        Load();
    }

    // 로그인 토큰 저장
    public void Save(TokenData tokenData)
    {
        if (tokenData == null)
        {
            Debug.LogWarning("[AuthTokenStore] Token data is null.");
            return;
        }

        AccessToken = WithBearerPrefix(tokenData.accessToken);
        RefreshToken = tokenData.refreshToken;
        TokenType = string.IsNullOrEmpty(tokenData.tokenType)
            ? "Bearer"
            : tokenData.tokenType;

        PlayerPrefs.SetString(AccessTokenKey, AccessToken);
        PlayerPrefs.SetString(RefreshTokenKey, RefreshToken);
        PlayerPrefs.SetString(TokenTypeKey, TokenType);
        PlayerPrefs.Save();

        Debug.Log("[AuthTokenStore] Token saved.");
    }

    // 저장된 토큰 불러오기
    public void Load()
    {
        AccessToken = WithBearerPrefix(PlayerPrefs.GetString(AccessTokenKey, string.Empty));
        RefreshToken = PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
        TokenType = PlayerPrefs.GetString(TokenTypeKey, "Bearer");
    }

    // 토큰 제거
    [ContextMenu("Clear Tokens")]
    public void Clear()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        TokenType = "Bearer";

        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.DeleteKey(TokenTypeKey);
        PlayerPrefs.Save();

        Debug.Log("[AuthTokenStore] Token cleared.");
    }

    // Authorization 헤더 값 반환
    public string GetAuthorizationHeaderValue()
    {
        if (!HasAccessToken)
        {
            return string.Empty;
        }

        return HasBearerPrefix(AccessToken)
            ? AccessToken
            : $"{TokenType} {AccessToken}";
    }

    private static string WithBearerPrefix(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return string.Empty;
        }

        string trimmedToken = token.Trim();

        if (HasBearerPrefix(trimmedToken))
        {
            return trimmedToken;
        }

        return $"Bearer {trimmedToken}";
    }

    private static bool HasBearerPrefix(string token)
    {
        return !string.IsNullOrWhiteSpace(token) &&
               token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }
}
