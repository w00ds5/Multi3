using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuRoot; 

    public static string PlayerNickname { get; private set; } = "Player";

    private void Start()
    {

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }


    public void StartAsHost()
    {
        SaveNickname();
        NetworkManager.Singleton.StartHost();
    }

    public void StartAsClient()
    {
        SaveNickname();
        NetworkManager.Singleton.StartClient();
    }


    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            HideMenu();
        }
    }

    private void HideMenu()
    {
        if (_menuRoot != null)
        {
            _menuRoot.SetActive(false);
        }
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }
}