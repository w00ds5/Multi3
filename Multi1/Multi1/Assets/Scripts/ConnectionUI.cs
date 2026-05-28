using TMPro;
using FishNet;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private GameObject _menuRoot; 

    public static string PlayerNickname { get; private set; } = "Player";

    private void Start()
    {
        if (InstanceFinder.NetworkManager != null)
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }
    }

    private void OnDestroy()
    {
        if (InstanceFinder.NetworkManager != null)
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        }
    }

    public void StartAsHost()
    {
        SaveNickname();
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
    }

    public void StartAsClient()
    {
        SaveNickname();
        InstanceFinder.ClientManager.StartConnection();
    }

    private void OnClientConnectionState(FishNet.Transporting.ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == FishNet.Transporting.LocalConnectionState.Started)
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