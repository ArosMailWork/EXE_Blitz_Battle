using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;

[Serializable]
public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 MoveInputVec;
}
public enum PlayerMovementStates
{
    Idle, Running, Dashing, Thrown, Snared, Charging, WallSticking, Hitted
}
public enum PlayerStates
{
    Invincible, ImmuneKB, NoDefense
}


public class PlayerController : NetworkBehaviour
{
    #region Variables

    #region Stats
    [FoldoutGroup("Stats")]
    public string NickName;
    [FoldoutGroup("Stats")]
    public int Team;
    [FoldoutGroup("Stats")]
    public float DamagePercentage;
    private readonly SyncVar<float> DamagePercentageSync = new SyncVar<float>(new SyncTypeSettings(1f));
    [FoldoutGroup("Stats")]
    public float invincibleFrame = 0.1f;
    [FoldoutGroup("Stats")] 
    public AnimationCurve damageCurve;
    [FoldoutGroup("Stats")]
    public float knockback = 1;
    [FoldoutGroup("Stats")]
    [SerializeField] float thrownTimeScale = 0.01f;
    [FoldoutGroup("Stats")]
    public float accel = 10;
    [FoldoutGroup("Stats")]
    public float maxSpeed = 20f;
    
    [FoldoutGroup("Stats/Jump")]
    public float jumpBoost = 5;
    [FoldoutGroup("Stats/Jump")]
    public float airstrafe = 0.75f;
    [FoldoutGroup("Stats/Jump")]
    [ReadOnly] public float JumpRemain;
    [FoldoutGroup("Stats/Jump")]
    public float AirJump = 1;
    
    [FoldoutGroup("Stats/Dash")]
    public float DashAccel = 100;
    [FoldoutGroup("Stats/Dash")]
    public float DashTime = 0.5f;
    [FoldoutGroup("Stats/Dash")]
    public float DashMaxSpeed = 0.5f;
    [FoldoutGroup("Stats/Dash")]
    public float DashMomentum = 0.3f;
    
    #endregion

    #region Debug and Calculate
    
    [FoldoutGroup("Debug")] 
    public FrameInput _frameInput;
    [FoldoutGroup("Debug/State")]
    public PlayerMovementStates _currentMovementStates;
    [FoldoutGroup("Debug/State")]
    public PlayerStates _currentStates;
    [FoldoutGroup("Debug/State")]
    public bool playerAlive = true, isGrounded;
    [FoldoutGroup("Debug/State/Variables")]
    public float KBValue;
    [FoldoutGroup("Debug/State/Variables")]
    public float thrownTime;
    [FoldoutGroup("Debug/State/Variables")]
    public float defaultMaxSpeed;
    [FoldoutGroup("Debug/State/Variables")]
    [ReadOnly] public float DashTimer = 0;
    [FoldoutGroup("Debug/State/Variables")]
    public Vector2 LastMove;
    
    [FoldoutGroup("Setup/Checker")]
    public float radius = 0.45f;
    [FoldoutGroup("Setup/Checker")]
    public float maxLengthGroundCheck = 1f;

    [FoldoutGroup("Setup")] 
    [CanBeNull] public SkillsHolder skillsHolder;
    [FoldoutGroup("Setup")] 
    public LayerMask groundLayer;
    [FoldoutGroup("Setup")]
    public float extraGravity = 10f;
    [FoldoutGroup("Setup")]
    public float KBDelay = 0.1f;
    [FoldoutGroup("Setup")]
    public float JumpMultiplier = 8f, LowJumpMultiplier = 4f;
    
    //Calculate Only
    RaycastHit hit;
    private Rigidbody rb;
    private PredictionRigidbody predictRB;
    float gravityvalue;
    private bool isOwner;
    
    UnityEvent OnLanding;
    
    #endregion
    
    #endregion

    #region Settings
    public void Awake()
    {
        defaultMaxSpeed = maxSpeed;
        DamagePercentageSync.Value = 0;
        DamagePercentageSync.OnChange += UpdateInspector;
        rb = GetComponent<Rigidbody>();
        
        predictRB = new PredictionRigidbody();
        predictRB.Initialize(rb);
        
        OnLanding = new UnityEvent();
        OnLanding.AddListener(OnLandingHandler);
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        CameraTracking.Instance.AddObj(this.gameObject);

        if (base.IsOwner)
        {
            //Debug.Log("owner");
            isOwner = true;
        }
        else
        {
            //Debug.Log("client disable stuffs");
            if(skillsHolder != null) skillsHolder.enabled = false;   
            
            PlayerInput otherPlayerInput = gameObject.GetComponent<PlayerInput>();
            if(otherPlayerInput != null) otherPlayerInput.enabled = false;
        }
        
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    #endregion

    #region Unity Method

    private void Update()
    {
        ExtraGravity2();
        GroundChecker();
        //LimitSpeed();
    }
    private void FixedUpdate()
    {
        Movement();
        ThrownTimer();
        ApplyDash();
    }

    #endregion

    #region Player Input

    public void MoveInput(InputAction.CallbackContext ctx)
    {
        if(!playerAlive) return;
        
        _frameInput.MoveInputVec = ctx.ReadValue<Vector2>();
    }
    public void JumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _frameInput.JumpHeld = true;
            Jump();
        }

        if (ctx.canceled)
        {
            _frameInput.JumpHeld = false;
        }
    }
    
    #endregion
    
    #region Ability

    private Vector3 playerVelBuffer;
    public async UniTaskVoid Dash()
    {
        Vector2 DashDir = _frameInput.MoveInputVec.normalized;
        if (DashDir == Vector2.zero || 
            _currentMovementStates == PlayerMovementStates.Dashing ||
            _currentMovementStates == PlayerMovementStates.Thrown || 
            _currentMovementStates == PlayerMovementStates.Snared) return;
        
        Debug.Log("Dashing !!!!");
        _currentMovementStates = PlayerMovementStates.Dashing;
        _currentStates = PlayerStates.ImmuneKB;
        
        maxSpeed = DashMaxSpeed;
        playerVelBuffer = rb.velocity;
        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        DashTimer = DashTime;
        
        //After
        await UniTask.Delay(TimeSpan.FromSeconds(DashTime + 0.1f));
        
        Debug.Log("Dash done");
        _currentMovementStates = PlayerMovementStates.Idle;
        _currentStates = PlayerStates.NoDefense;
        maxSpeed = defaultMaxSpeed;
        rb.useGravity = true;
        rb.velocity = DashDir * (playerVelBuffer.magnitude * DashMomentum);
    }

    void ApplyDash()
    {
        if (DashTimer <= 0) return;
        
        DashTimer -= Time.deltaTime;
        
        Vector2 DashDir = _frameInput.MoveInputVec.normalized;
        Vector3 dashForce = new Vector3(DashDir.x, DashDir.y * 0.3f, 0f).normalized * DashAccel;
        //rb.AddForceAtPosition(dashForce, transform.position, ForceMode.VelocityChange);
        if(dashForce.x == 0 && DashDir.y != 0)
            rb.velocity = dashForce * 0.5f;
        else
            rb.velocity = dashForce;
    }

    /*
    public async UniTaskVoid Blink()
    {
        _currentMovementStates = PlayerMovementStates.Charging;
        
    }
    */
    
    #endregion

    #region Basic Movements
    
    void Movement()
    {
        if (_frameInput.MoveInputVec != Vector2.zero) LastMove = _frameInput.MoveInputVec;
        if(_currentMovementStates == PlayerMovementStates.Thrown) return;
        if (_frameInput.MoveInputVec == Vector2.zero)
        {
            if(rb.velocity.magnitude <= 0.001f) _currentMovementStates = PlayerMovementStates.Idle;
            return;
        }
        if(_currentMovementStates == PlayerMovementStates.Idle)
            _currentMovementStates = PlayerMovementStates.Running;

        Vector3 moveDir = new Vector3(_frameInput.MoveInputVec.x, 0, 0).normalized;
        if (!isGrounded)
            //rb.AddForce(moveDir * (accel * 10f * airstrafe * Time.fixedDeltaTime), ForceMode.VelocityChange);
            rb.velocity = new Vector3(moveDir.x * (accel * airstrafe), rb.velocity.y, 0);
        else
            rb.velocity = new Vector3(moveDir.x * (accel), rb.velocity.y, 0);
        //rb.AddForce(moveDir * (accel * 10f * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }
    void Jump()
    {
        if(JumpRemain <= 0) return;
        
        ApplyJump();
        JumpRemain--;
    }
    
    public void ApplyJump(float JumpMultiplier = 1, float ExtraHorVel = 0)
    {
        float jumpForce;
        if (Mathf.Abs(rb.velocity.x) != 0) jumpForce = jumpBoost * Mathf.Sqrt(2);
        else jumpForce = jumpBoost;
        
        //float currentYvel = rb.velocity.y >= 0 ? 0 : Mathf.Abs(rb.velocity.y);
        //float yForce = currentYvel + jumpForce * JumpMultiplier;
        float yForce = jumpForce * JumpMultiplier;
            
        Vector3 JumpVec = new Vector3(0, yForce, 0f);
        //rb.AddForce(JumpVec, ForceMode.Acceleration);
        rb.velocity = new Vector3(rb.velocity.x, yForce, 0);
        
        //if(ExtraHorVel <= 0) return;
        //Vector3 extraVel = new Vector3(rb.velocity.x, 0, 0);
        //rb.AddForce(extraVel.normalized * ExtraHorVel, ForceMode.Acceleration);
    }

    #endregion
    
    #region Physics

    //too lazy so I make this, lol
    public void SimpleExternalForce(Vector3 Dir ,float force = 1)
    {
        rb.AddForce(Dir * force, ForceMode.VelocityChange);
    }

    void ExtraGravity2()
    {
        if(_currentMovementStates == PlayerMovementStates.Dashing) return;
        
        if (rb.velocity.y < -0.5f)
            rb.velocity += Vector3.up * (Physics.gravity.y * (JumpMultiplier - 1) * Time.deltaTime);
        else if (rb.velocity.y > -0.5f && _frameInput.JumpHeld)
            rb.velocity += Vector3.up * (Physics.gravity.y * (LowJumpMultiplier - 1) * Time.deltaTime);
    }
    
    void LimitSpeed()
    {
        if (rb.velocity.magnitude > maxSpeed) rb.velocity = rb.velocity.normalized * maxSpeed;
    }
    void GroundChecker()
    {
        isGrounded = Physics.BoxCast(transform.position, new Vector3(radius, 0.2f, radius), Vector3.down, out hit, Quaternion.identity, maxLengthGroundCheck, groundLayer);
        if (isGrounded) ResetJumpCount();
    }

    public void ResetJumpCount(int BonusJump = 0)
    {
        JumpRemain = AirJump + BonusJump;
    }

    #endregion

    #region Events and Timers
    
    void OnLandingHandler()
    {
        // Do something when the player lands
        Debug.Log("Player has landed. Particle here.");
    }

    public void ThrownTimer()
    {
        if (thrownTime > 0)
        {
            thrownTime -= Time.fixedDeltaTime;
            _currentMovementStates = PlayerMovementStates.Thrown;
        }
    }
    
    public void ThrownTimeAdd(float damage)
    {
        thrownTime += damage * DamagePercentageSync.Value * thrownTimeScale;
    }

    [ObserversRpc]
    public void ReceiveDamage(Vector3 hitDirect, float damage)
    {
        Debug.Log("received");
        applyDamage(hitDirect, damage);
    }
    
    public async UniTaskVoid applyDamage(Vector3 hitDirect, float damage)
    {
        if(_currentStates == PlayerStates.Invincible) return;
        
        //DamagePercentage += damage;
        DamagePercentageSync.Value += damage;
        Knockback(hitDirect);
        _currentStates = PlayerStates.Invincible;
        await UniTask.Delay(TimeSpan.FromSeconds(invincibleFrame));
        _currentStates = PlayerStates.NoDefense;
    }
    
    [ObserversRpc(ExcludeOwner = true)]
    public void ReceiveKB(Vector3 hitDirect, float extraKB, float Damage = 0)
    {
        Debug.Log("KB Only");
        Knockback(hitDirect, extraKB, Damage);
    }

    public void setFreeze()
    {
        predictRB.Velocity(Vector3.zero);
    }
    
    public async UniTaskVoid Knockback(Vector3 hitDirect, float extraKBMultiplier = 1, float damage = 0)
    {
        setFreeze();
        await UniTask.Delay(TimeSpan.FromSeconds(KBDelay * 0.5f));
        CamShake.Instance.ActivateShake();
        
        // Evaluate the animation curve at DamagePercentage
        float curveValue = damageCurve.Evaluate(DamagePercentageSync.Value); 
        
        // Calculate the knockback factor using the curve value
        float knockbackFactor = curveValue * knockback;

        ThrownTimeAdd(damage);
        
        if (extraKBMultiplier != 1) knockbackFactor = extraKBMultiplier;
        rb.AddForce(hitDirect * knockbackFactor, ForceMode.VelocityChange);
    }

    #endregion

    #region Sync

    private void UpdateInspector(float prev, float next, bool asServer)
    {
        DamagePercentage = DamagePercentageSync.Value;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.down * maxLengthGroundCheck, new Vector3(radius * 2, 0.1f, radius * 2));
    }
}
