using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float MoveSpeed = 5f;
    public float Acceleration = 15f;
    public Vector2 MoveDirection;
    public Vector2 LastMoveDirection;
    [SerializeField] private float currentSpeed;
    [SerializeField] private bool canMove = true;

    [Header("Dash")]
    public float DashSpeed = 20f;
    public float DashTime = 0.15f;
    public float DashCooldownTime = 0.25f;
    [SerializeField] private bool canDash = true;

    [Header("Attack")]
    public float AttackDamage = 1f;

    [Header("Components")]
    [SerializeField] private bool _hasAnimator;
    [SerializeField] private Animator _animator;
    [SerializeField] private InputController _input;
    [SerializeField] private GhostEffect _ghostEffect;
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Animation Hash IDs")]
    private int _speedHash;
    private int _speedXHash;
    private int _speedYHash;
    private int _dashHash;
    private int _attackHash;

    private void Start()
    {
        _hasAnimator = TryGetComponent<Animator>(out _animator);
        _input = GetComponent<InputController>();
        _ghostEffect = GetComponent<GhostEffect>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _ghostEffect.enabled = false;

        AssignAnimationHashes();
    }

    private void AssignAnimationHashes()
    {
        _speedHash = Animator.StringToHash("Speed");
        _speedXHash = Animator.StringToHash("SpeedX");
        _speedYHash = Animator.StringToHash("SpeedY");
        _dashHash = Animator.StringToHash("Dash");
        _attackHash = Animator.StringToHash("Attack");
    }

    private void Update()
    {
        HandleDash();
        HandleAttack();
        Move();

        HandleFlipX();
    }

    private void HandleDash()
    {
        if (!canDash)
            return;

        if (_input.dash)
        {
            // recalculate dash direction
            Vector2 dashDir = LastMoveDirection;

            StartCoroutine(DisableMovement(DashTime));
            StartCoroutine(Dash(dashDir));
        }
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(Attack());
        }
    }

    private void Move()
    {
        if (!canMove)
            return;

        // store the last move direction if player does move
        if (_input.move.x != 0 || _input.move.y != 0)
            LastMoveDirection = MoveDirection;

        MoveDirection = _input.move;

        // move char by rigidbody
        _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, MoveDirection * MoveSpeed, Acceleration * Time.deltaTime);
        currentSpeed = _rigidbody.velocity.magnitude;

        // animations
        if (_hasAnimator)
        {
            _animator.SetFloat(_speedHash, currentSpeed);
            _animator.SetFloat(_speedXHash, LastMoveDirection.x);
            _animator.SetFloat(_speedYHash, LastMoveDirection.y);
        }
    }

    private void HandleFlipX()
    {
        // handle player animation facing direction
        if (_input.move.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (_input.move.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    IEnumerator Attack()
    {
        // debug
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float currentTime = stateInfo.normalizedTime * stateInfo.length;
        Debug.Log($"Current animation length: {currentTime}");

        // start attack
        _animator.SetBool(_attackHash, true);
        canDash = false;
        canMove = false;
        _rigidbody.velocity = Vector2.zero;

        yield return new WaitForSeconds(currentTime);

        _animator.SetBool(_attackHash, false);
        canDash = true;
        canMove = true;
    }

    IEnumerator AttackBuffer()
    {
        // bool continueAttack = false;
        // bool continueDash = false;
        // if(_input.attack)
        // {
        //     continueAttack = true;
        //     continueDash = false;
        // }
        // else if(_input.dash)
        // {
        //     continueAttack = false;
        //     continueDash = true;
        // }
        yield return null;
    }

    IEnumerator Dash(Vector2 dashDir)
    {
        // animations
        if (_hasAnimator)
        {
            _animator.SetTrigger(_dashHash);
        }

        // start ghost effect
        _ghostEffect.Play(DashTime);

        // start dash
        canDash = false;
        _rigidbody.velocity = dashDir.normalized * DashSpeed;

        yield return new WaitForSeconds(DashTime);

        _rigidbody.velocity = Vector2.zero;

        // dash cooldown
        yield return new WaitForSeconds(DashCooldownTime);
        canDash = true;

        StartCoroutine(DisableMovement(0.5f));
    }
}
