using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlightResultUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject resultRoot;

    [Header("Hide On Result")]
    [SerializeField] private GameObject[] hideOnResult;

    [Header("Texts")]
    [SerializeField] private TMP_Text clearTimeText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "Lobby";

    // 버튼 이벤트 연결 및 초기 비활성화
    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartFlight);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitToLobby);
        }

        Hide();
    }

    // 버튼 이벤트 해제
    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartFlight);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitToLobby);
        }
    }

    // 결과창 표시
    public void Show(float clearTimeSeconds)
    {
        HideOtherUI();

        if (resultRoot != null)
        {
            resultRoot.SetActive(true);
        }

        if (clearTimeText != null)
        {
            clearTimeText.text = $"Clear Time : {FormatTime(clearTimeSeconds)}";
        }
    }

    // 결과창 숨김
    public void Hide()
    {
        if (resultRoot != null)
        {
            resultRoot.SetActive(false);
        }
    }

    // 클리어 시 다른 UI 비활성화
    private void HideOtherUI()
    {
        if (hideOnResult == null)
        {
            return;
        }

        foreach (GameObject target in hideOnResult)
        {
            if (target == null)
            {
                continue;
            }

            target.SetActive(false);
        }
    }

    // 현재 Flight 씬 다시 로드
    private void RestartFlight()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // 로비 씬으로 이동
    private void QuitToLobby()
    {
        SceneManager.LoadScene(lobbySceneName);
    }

    // 시간 표시 문자열 생성
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainSeconds = Mathf.FloorToInt(seconds % 60f);
        int milliseconds = Mathf.FloorToInt((seconds * 1000f) % 1000f);

        return $"{minutes:00}:{remainSeconds:00}.{milliseconds:000}";
    }
}