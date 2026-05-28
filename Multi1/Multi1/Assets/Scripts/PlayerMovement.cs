using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public struct MoveData : IReplicateData
{
    public float Horizontal;
    public float Vertical;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;
    public float VerticalVelocity;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Состояние Предсказания")]
    public bool UsePrediction = true; // Переключатель CSP

    private CharacterController _cc;
    private float _verticalVelocity;
    private PlayerNetwork _playerNetwork;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnStartNetwork()
    {
        base.TimeManager.OnTick += OnTick;
    }

    public override void OnStopNetwork()
    {
        base.TimeManager.OnTick -= OnTick;
    }

    private void OnTick()
    {
        if (UsePrediction)
        {
            if (base.IsOwner)
            {
                Reconciliation(default);
                GatherInput(out MoveData md);
                Replicate(md);
            }
            else if (base.IsServerInitialized)
            {
                Replicate(default);
                Reconciliation(default);
            }
        }
        else
        {
            
            if (base.IsOwner)
            {
                GatherInput(out MoveData md);
                
                SendInputToServerRpc(md);
            }
        }
    }

    private void GatherInput(out MoveData md)
    {
        md = new MoveData();
        if (!_playerNetwork.IsAlive.Value) return;

        md.Horizontal = Input.GetAxisRaw("Horizontal");
        md.Vertical = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayerPhysically(MoveData md)
    {
        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical).normalized * _speed;

        _verticalVelocity += _gravity * (float)base.TimeManager.TickDelta;
        move.y = _verticalVelocity;

        if (_cc != null && _cc.enabled)
        {
            _cc.Move(move * (float)base.TimeManager.TickDelta);

            if (_cc.isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f; 
            }
        }
    }

    [Replicate]
    private void Replicate(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        MovePlayerPhysically(md);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        if (_cc != null) _cc.enabled = false;
        transform.position = rd.Position;
        if (_cc != null) _cc.enabled = true;
        
        _verticalVelocity = rd.VerticalVelocity;
    }

    public void CreateReconcile(out ReconcileData rd)
    {
        rd = new ReconcileData
        {
            Position = transform.position,
            VerticalVelocity = _verticalVelocity
        };
        Reconciliation(rd);
    }


    [ServerRpc]
    private void SendInputToServerRpc(MoveData md)
    {
        MovePlayerPhysically(md);
        SyncPositionObserversRpc(transform.position, _verticalVelocity);
    }

    [ObserversRpc]
    private void SyncPositionObserversRpc(Vector3 pos, float vertVel)
    {
        
        if (UsePrediction) return;
        if (base.IsServerInitialized) return; 

        
        if (_cc != null) _cc.enabled = false;
        transform.position = pos;
        if (_cc != null) _cc.enabled = true;
        _verticalVelocity = vertVel;
    }

    
    private void OnGUI()
    {
        if (!base.IsOwner) return;

        UsePrediction = GUI.Toggle(new Rect(15, 15, 250, 30), UsePrediction, "  Включить CSP (Предсказание)");
    }
}