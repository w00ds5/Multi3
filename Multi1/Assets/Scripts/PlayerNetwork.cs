using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    // Синхронизируем здоровье
    public NetworkVariable<int> HP = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Синхронизируем состояние "Жив/Мертв"
    public NetworkVariable<bool> IsAlive = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<Unity.Collections.FixedString32Bytes> Nickname = new NetworkVariable<Unity.Collections.FixedString32Bytes>(
    "Player",
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner // Или Server, как было у тебя в Практике 1
    );

    [Header("Настройки Респавна")]
    
    [SerializeField] private GameObject _visualModel;  // Объект с моделькой игрока (чтобы скрывать при смерти)

    public override void OnNetworkSpawn()
    {
        // Подписываемся на изменения переменных
        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;
    }

    public override void OnNetworkDespawn()
    {
        // Отписываемся, чтобы избежать ошибок при удалении объекта
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnHpChanged(int prev, int next)
    {
        // Только СЕРВЕР решает, что игрок умер и запускает корутину
        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value == true)
        {
            IsAlive.Value = false; // Игрок мертв
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (spawnPoints.Length > 0)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            
            // ВМЕСТО ПРЯМОГО ПЕРЕМЕЩЕНИЯ ВЫЗЫВАЕМ ClientRpc
            TeleportPlayerClientRpc(spawnPoints[idx].transform.position);
        }
        else
        {
            Debug.LogWarning("Точки респавна не найдены!");
        }

        // Воскрешаем
        HP.Value = 100;
        IsAlive.Value = true;
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        // Этот метод срабатывает у ВСЕХ клиентов (и у хоста).
        // Скрываем или показываем визуальную модельку в зависимости от статуса
        if (_visualModel != null)
        {
            _visualModel.SetActive(next); // next — это новое значение IsAlive (true или false)
        }
    }

    // Этот метод вызывается Сервером, но выполняется на Клиентах
    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 newPosition)
    {
        // Нам нужно, чтобы только сам Владелец передвинул себя, 
        // иначе будет конфликт позиций
        if (!IsOwner) return;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        transform.position = newPosition;
        
        if (cc != null) cc.enabled = true;
    }
}