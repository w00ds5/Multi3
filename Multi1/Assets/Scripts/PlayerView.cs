using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections; 

public class PlayerView : NetworkBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    public override void OnNetworkSpawn()
    {
        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged += OnHpChanged;

        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);
    }

    public override void OnNetworkDespawn()
    {
        _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged -= OnHpChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (_nicknameText != null)
            _nicknameText.text = newValue.ToString();
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {newValue}";
    }
}