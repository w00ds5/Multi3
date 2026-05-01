using Unity.Netcode;
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
    if (!IsOwner) return;

    if (Input.GetKeyDown(KeyCode.C))
    {
        TryAttack();
    }
    }

    private void TryAttack()
    {
    foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
    {
        if (client.PlayerObject == null)
            continue;

        var target = client.PlayerObject.GetComponent<PlayerNetwork>();

        if (target.OwnerClientId == OwnerClientId)
            continue;

        DealDamageServerRpc(target.NetworkObjectId);
        break; 
    }
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId)
    {
    if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out var obj))
        return;

    var targetPlayer = obj.GetComponent<PlayerNetwork>();

    if (targetPlayer == null)
        return;

    if (targetPlayer.OwnerClientId == OwnerClientId)
        return;

    int newHp = Mathf.Max(0, targetPlayer.HP.Value - _damage);
    targetPlayer.HP.Value = newHp;

    Debug.Log($"Damage: {OwnerClientId} -> {targetPlayer.OwnerClientId}, HP: {newHp}");
    }
}