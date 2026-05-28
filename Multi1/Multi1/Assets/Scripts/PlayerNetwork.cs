using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);
    public readonly SyncVar<string> Nickname = new SyncVar<string>("Player");

    [Header("Настройки Респавна")]
    [SerializeField] private GameObject _visualModel; 

    public event System.Action<int> OnHpChangedEvent;
    public event System.Action<string> OnNicknameChangedEvent;

    public override void OnStartNetwork()
    {
        
        HP.OnChange += OnHpChanged;
        IsAlive.OnChange += OnIsAliveChanged;
        Nickname.OnChange += OnNicknameChanged;

        if (base.Owner.IsLocalClient)
        {
            SetNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnStopNetwork()
    {
        
        HP.OnChange -= OnHpChanged;
        IsAlive.OnChange -= OnIsAliveChanged;
        Nickname.OnChange -= OnNicknameChanged;
    }

    [ServerRpc]
    public void SetNicknameServerRpc(string nickname)
    {
        Nickname.Value = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerId}"
            : nickname.Trim();
    }

    private void OnHpChanged(int prev, int next, bool asServer)
    {
        OnHpChangedEvent?.Invoke(next);

        if (asServer)
        {
            if (next <= 0 && IsAlive.Value)
            {
                IsAlive.Value = false;
                StartCoroutine(RespawnRoutine());
            }
        }
    }

    private void OnNicknameChanged(string prev, string next, bool asServer)
    {
        OnNicknameChangedEvent?.Invoke(next);
    }

    private void OnIsAliveChanged(bool prev, bool next, bool asServer)
    {
        if (_visualModel != null)
        {
            _visualModel.SetActive(next);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            TeleportPlayerObserversRpc(spawnPoints[idx].transform.position);
        }
        else
        {
            Debug.LogWarning("Точки респавна не найдены!");
        }

        HP.Value = 100;
        IsAlive.Value = true;
    }

    [ObserversRpc]
    private void TeleportPlayerObserversRpc(Vector3 newPosition)
    {
        if (!base.IsOwner) return;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        transform.position = newPosition;
        
        if (cc != null) cc.enabled = true;
    }
}