using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    public float MoveSpeed = 6f;
    public float Acceleration = 10f;
    public Vector2 MoveDirection;
    public Vector2 LastMoveDirection;
    [SerializeField] private bool canMove = true;

    [Header("Dash")]
    public float DashSpeed = 20f;
    public float DashTime = 0.15f;
    public float DashCooldownTime = 0.25f;
    [SerializeField] private bool canDash = true;

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
    }

    private void Update()
    {
        HandleDash();
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

    IEnumerator Dash(Vector2 dashDir)
    {
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
    }
}
