using FishNet.Object;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;

    private void Awake()
    {
        if (_playerNetwork == null)
            _playerNetwork = GetComponent<PlayerNetwork>();
    }

    private void Update()
    {
        if (!base.IsOwner) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        foreach (var client in base.ServerManager.Clients.Values)
        {
            var playerObj = client.FirstObject;
            if (playerObj == null) continue;

            var target = playerObj.GetComponent<PlayerNetwork>();
            if (target == null || target.OwnerId == base.OwnerId)
                continue;

            DealDamageServerRpc(target);
            break; 
        }
    }

    [ServerRpc]
    private void DealDamageServerRpc(PlayerNetwork targetPlayer)
    {
        if (targetPlayer == null) return;
        if (targetPlayer.OwnerId == base.OwnerId) return;

        int newHp = Mathf.Max(0, targetPlayer.HP.Value - _damage);
        targetPlayer.HP.Value = newHp;

        Debug.Log($"Damage: {base.OwnerId} -> {targetPlayer.OwnerId}, HP: {newHp}");
    }
}