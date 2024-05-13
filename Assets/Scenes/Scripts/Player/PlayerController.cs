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

[Serializable]
public struct FrameInput
{
    public bool JumpDown;
    public bool JumpHeld;
    public Vector2 MoveInputVec;
}
public enum PlayerMovementStates
{
    Idle, Running, Dashing, Thrown, Snared, Charging
}
public enum PlayerStates
{
    Invincible, ImmuneKB, NoDefense
}


public class PlayerController : MonoBehaviour
{
    #region Variables

    #region Stats
    [FoldoutGroup("Stats")]
    public float DamagePercentage;
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
    [FoldoutGroup("Debug/State")]
    public float thrownTime;
    [FoldoutGroup("Debug/State")]
    public float defaultMaxSpeed;
    
    [FoldoutGroup("Setup/Checker")]
    public float radius = 0.45f;
    [FoldoutGroup("Setup/Checker")]
    public float maxLengthGroundCheck = 1f;
    [FoldoutGroup("Setup")]
    public float extraGravity = 10f;
    [FoldoutGroup("Setup")]
    public float JumpMultiplier = 8f, LowJumpMultiplier = 4f;
    
    //Calculate Only
    RaycastHit hit;
    private Rigidbody rb;
    float gravityvalue;
    
    UnityEvent OnLanding;
    
    #endregion
    
    #endregion

    #region Settings
    public void Awake()
    {
        defaultMaxSpeed = maxSpeed;
        DamagePercentage = 0;
        rb = GetComponent<Rigidbody>();
        
        OnLanding = new UnityEvent();
        OnLanding.AddListener(OnLandingHandler);
    }

    #endregion

    #region Unity Method

    private void Update()
    {
        ExtraGravity2();
        GroundChecker();
        LimitSpeed();
    }
    private void FixedUpdate()
    {
        Movement();
        ThrownTimer();
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
        rb.AddForce(DashDir * DashAccel, ForceMode.VelocityChange);
        rb.useGravity = false;
        
        
        await UniTask.Delay(TimeSpan.FromSeconds(DashTime));
        
        Debug.Log("Dash done");
        _currentMovementStates = PlayerMovementStates.Idle;
        _currentStates = PlayerStates.NoDefense;
    
        maxSpeed = defaultMaxSpeed;
        rb.useGravity = true;
        rb.velocity = DashDir * (playerVelBuffer.magnitude * DashMomentum);
    }
    public async UniTaskVoid Blink()
    {
        _currentMovementStates = PlayerMovementStates.Charging;
        
    }
    void Glide()
    {
        
    }
    
    #endregion

    #region Basic Movements
    
    void Movement()
    {
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
            rb.AddForce(moveDir * (accel * 10f * airstrafe * Time.fixedDeltaTime), ForceMode.VelocityChange);
        else
            rb.AddForce(moveDir * (accel * 10f * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }
    void Jump()
    {
        if(JumpRemain <= 0) return;
        
        ApplyJump();
        JumpRemain--;
    }
    
    public void ApplyJump()
    {
        float jumpForce;
        if (Mathf.Abs(rb.velocity.x) != 0) jumpForce = jumpBoost * Mathf.Sqrt(2);
        else jumpForce = jumpBoost;
        
        float currentYvel = rb.velocity.y >= 0 ? 0 : Mathf.Abs(rb.velocity.y);
        float yForce = currentYvel + jumpForce * 100;
            
        Vector3 JumpVec = new Vector3(0, yForce, 0f);
        rb.AddForce(JumpVec, ForceMode.Acceleration);
    }

    #endregion
    
    #region Physics

    void ExtraGravity()
    {
        if(_currentMovementStates != PlayerMovementStates.Thrown)
            
        //Extra Gravity
        if (!isGrounded && _currentMovementStates != PlayerMovementStates.Dashing)
        {
            gravityvalue += Time.fixedDeltaTime;
            rb.AddForce(Vector3.down * (extraGravity * gravityvalue), ForceMode.Acceleration);
        }
        else
        {
            gravityvalue = 1;
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
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
        isGrounded = Physics.BoxCast(transform.position, new Vector3(radius, 0.2f, radius), Vector3.down, out hit, Quaternion.identity, maxLengthGroundCheck);
        if (isGrounded) JumpRemain = AirJump;
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
        thrownTime += damage * DamagePercentage * thrownTimeScale;
    }
    public void DealDamage(Vector2 hitDirect, float damage)
    {
        if(_currentStates == PlayerStates.Invincible) return;
            
        DamagePercentage += damage;
        Knockback(hitDirect);
    }
    
    void Knockback(Vector2 hitDirect) //gen hitDirect by the position of player to the enemy (maybe will adjust for more details but whatever)
    {
        rb.AddForce(hitDirect * knockback * DamagePercentage, ForceMode.Acceleration);
    } 

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.down * maxLengthGroundCheck, new Vector3(radius * 2, 0.1f, radius * 2));
    }
}
