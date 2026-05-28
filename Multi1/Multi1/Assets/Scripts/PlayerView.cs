using TMPro;
using FishNet.Object;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    public override void OnStartNetwork()
    {
        _playerNetwork.OnHpChangedEvent += OnHpChanged;
        _playerNetwork.OnNicknameChangedEvent += OnNicknameChanged;

        OnNicknameChanged(_playerNetwork.Nickname.Value);
        OnHpChanged(_playerNetwork.HP.Value);
    }

    public override void OnStopNetwork()
    {
        _playerNetwork.OnHpChangedEvent -= OnHpChanged;
        _playerNetwork.OnNicknameChangedEvent -= OnNicknameChanged;
    }

    private void OnNicknameChanged(string newValue)
    {
        if (_nicknameText != null)
            _nicknameText.text = newValue;
    }

    private void OnHpChanged(int newValue)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {newValue}";
    }
}