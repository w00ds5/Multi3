using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Настройки камеры")]
    [SerializeField] private Vector3 _offset = new(0f, 8f, -6f); // Смещение камеры (настройте под себя)

    private Camera _cam;

    public override void OnNetworkSpawn()
    {
        // Если этот объект принадлежит НЕ нам (это "чужой" игрок)
        if (!IsOwner)
        {
            // Мы полностью отключаем этот скрипт на чужом клоне.
            // Это очень надежный способ гарантировать, что чужой игрок 
            // не перехватит нашу камеру в LateUpdate().
            enabled = false;
            return;
        }

        // Если это наш персонаж, забираем контроль над главной камерой сцены
        _cam = Camera.main;
    }

    // LateUpdate вызывается ПОСЛЕ Update() физики движения.
    // Если двигать камеру в Update(), она будет неприятно дергаться.
    private void LateUpdate()
    {
        if (_cam == null) return;

        // Двигаем камеру в позицию игрока + наше смещение
        _cam.transform.position = transform.position + _offset;
        
        // Поворачиваем камеру, чтобы она всегда смотрела на персонажа
        _cam.transform.LookAt(transform.position);
    }
}