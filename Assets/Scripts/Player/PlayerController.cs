using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Healthbar healthbar;
    [SerializeField] private PlayerDataSO data;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] CameraFearZoom cameraFearZoom;

    public event Action OnEmotionChanged;

    AudioManager audioManager;
    Rigidbody2D rb;
    public ParticleSystem jumpEffect;

    bool facingRight = true;
    bool isGrounded;
    bool bisAttacking;
    bool takingDamage;
    public bool attackCondition;
    public bool isDead;

    private bool isSprinting;
    private int health;
    private float currentJumpForce;
    private float currentSpeed;

    public List<Emotion> emotions;
    public enum EmotionType
    {
        Calm,
        Stressed,
        Fear,
        Pain,
        Tired
    }
    enum State
    {
        PistolIdle, // PistolIdle
        PistolRun, // PistolAimingRunning
        PistolJump, // PistolJumping
        PistolFall, // PistolFalling
        PistolShoot, //PistolShoot
        PistolHit, //PistolHit
        PistolDead, // PistolDead
        PistolCrouchIdle, // PistolCrouchAiming
        PistolCrouchShoot // PistolCrouchShooting
    }

    State state;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        bool condition = !bisAttacking && isGrounded;
        attackCondition = condition;
        emotions = new List<Emotion>
            {
                new Emotion(EmotionType.Calm, false),
                new Emotion(EmotionType.Stressed, false),
                new Emotion(EmotionType.Fear, false),
                new Emotion(EmotionType.Pain, false),
                new Emotion(EmotionType.Tired, false)
            };
    }

    void Start()
    {
        isDead = false;
        takingDamage = false;

        Time.timeScale = 1f;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = true;

        currentJumpForce = data.maxJumpForce;
        currentSpeed = data.velocity;
        health = data.maxHealth;

        ChangeState(State.PistolIdle);
    }

    void Update()
    {
        CheckGround();
        StateMachine();
    }

    // ================= FSM =================

    void StateMachine()
    {
        switch (state)
        {
            case State.PistolIdle:
                Idle();
                break;
            case State.PistolRun:
                Movement();
                break;
            case State.PistolJump:
                Jumping();
                break;
            case State.PistolFall:
                Falling();
                break;
            case State.PistolShoot:
                PistolShooting();
                break;
            case State.PistolHit:
                PistolHit();
                break;
            case State.PistolDead:
                Dead();
                break;
        }
    }

    void ChangeState(State newState)
    {
        if (state == newState) return;

        state = newState;

        switch (state)
        {
            case State.PistolIdle:
                animator.Play("PistolIdle");
                break;
            case State.PistolRun:
                animator.Play("PistolAimingRunning");
                break;
            case State.PistolJump:
                animator.Play("PistolJumping");
                break;
            case State.PistolFall:
                animator.Play("PistolFalling");
                break;
            case State.PistolShoot:
                animator.Play("PistolShoot");
                break;
            case State.PistolHit:
                animator.Play("PistolHit");
                break;
            case State.PistolDead:
                animator.Play("PistolDead");
                break;
        }
    }

    public void TakingDamage(Vector2 direction, int damageAmount)
    {
        if (!takingDamage)
        {
            takingDamage = true;
            health -= damageAmount;
            healthbar.UpdateHealthBar(data.maxHealth, health);
            
            if (health <= 0)
            {
                audioManager.Stop();
                audioManager.PlaySFX(audioManager.LooseSfx);
                deathPanel.SetActive(true);
                healthbar.gameObject.SetActive(false);
                ChangeState(State.PistolDead);
                isDead = true;
                Time.timeScale = 0;
            }
            if (!isDead)
            {
                GetPain();
                Vector2 rebound = new Vector2(transform.position.x - direction.x, 0.5f).normalized;
                rb.AddForce(rebound * data.reboundForce, ForceMode2D.Impulse);
                takingDamage = false;
            }
        }
    }
    // ================= STATES =================

    private void Idle()
    {
        float x = Input.GetAxisRaw("Horizontal");

        HandleSprint();

        if (x != 0)
            ChangeState(State.PistolRun);

        if (Input.GetKeyDown(data.jumpKey) && isGrounded)
            DoJump();

        if (Input.GetKeyDown(data.attackKey) && isGrounded)
            ChangeState(State.PistolShoot);
    }

    private void Movement()
    {
        float velocityX = Input.GetAxisRaw("Horizontal");

        rb.velocity = new Vector2(velocityX * currentSpeed, rb.velocity.y);

        HandleSprint();

        // Flip the player according to the movement direction
        if (velocityX < 0 && facingRight)
        {
            Flip();
        }
        if (velocityX > 0 && !facingRight)
        {
            Flip();
        }

        if (velocityX == 0)
            ChangeState(State.PistolIdle);

        if (Input.GetKeyDown(data.jumpKey) && isGrounded)
            DoJump();

        if (!isGrounded)
            ChangeState(State.PistolFall);
    }

    private void Jumping()
    {
        jumpEffect.Play();
        if (rb.velocity.y < 0)
            ChangeState(State.PistolFall);
    }
    private void PistolHit()
    {
        if (takingDamage)
        {
            if (isGrounded)
                ChangeState(State.PistolIdle);
            else
                ChangeState(State.PistolFall);
            takingDamage = false;
        }
    }
    private void Dead()
    {
        rb.velocity = Vector2.zero;
        Time.timeScale = 0;
    }
    private void Falling()
    {
        if (isGrounded)
            ChangeState(State.PistolIdle);
    }

    private void PistolShooting()
    {
        attackCondition = true;
    }

    // ================= ACTIONS =================

    private void DoJump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * data.maxJumpForce, ForceMode2D.Impulse);
            ChangeState(State.PistolJump);
        }
    }

    public void EndAttack()
    {
        ChangeState(State.PistolIdle);
    }

    // ================= HELPERS =================

    private void HandleSprint()
    {
        if (Input.GetKey(data.sprintKey) && isGrounded)
        {
            if (!isSprinting)
            {
                if (emotions.First(x => x.EmotionType == EmotionType.Tired).bIsActive || emotions.First(x => x.EmotionType == EmotionType.Pain).bIsActive || emotions.First(x => x.EmotionType == EmotionType.Fear).bIsActive)
                {
                    return; // No sprint if tired
                }
                isSprinting = true;
                currentSpeed = data.runVelocity;
            }
        }
        else if (isSprinting)
        {
            isSprinting = false;
            currentSpeed = data.velocity;
        }
    }
    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, data.lengthRayCast, data.layerMask);
        isGrounded = hit.collider != null;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        transform.Rotate(0f, 180f, 0f);
    }
    ///////////////////// Emotions /////////////////////
    public void UpdateEmotions()
    {
        OnEmotionChanged?.Invoke();
    }
    public void GetCalm()
    {
        Emotion calm = emotions.First(x => x.EmotionType == PlayerController.EmotionType.Calm);
        if (!calm.bIsActive)
        {
        calm.bIsActive = true;
        Debug.Log("Player is calm");
        cameraFearZoom.SetFear(false);
        currentSpeed = data.velocity;
        currentJumpForce = data.maxJumpForce;
        // Aplicar ui de calma, aumentar velocidad de movimiento y salto, y mejorar precisión de disparo
        }
    }
    public void GetStressed()
    {
        Emotion stressed = emotions.First(x => x.EmotionType == PlayerController.EmotionType.Stressed);
        if (!stressed.bIsActive)
        {
            stressed.bIsActive = true;
            Debug.Log("Player is stressed");
        }
    }
    public void GetFear()
    {
        Emotion fear = emotions.First(x => x.EmotionType == PlayerController.EmotionType.Fear);
        if (!fear.bIsActive)
        {
        fear.bIsActive = true;
        cameraFearZoom.SetFear(true);
        Debug.Log("Player is afraid");
        // Aplicar ui de miedo, acortar vision y acortar distancia de bala
        } 
    }
    public void GetPain()
    {
        Emotion pain = emotions.First(x => x.EmotionType == PlayerController.EmotionType.Pain);
        if (!pain.bIsActive)
        {
            pain.bIsActive = true;
            Debug.Log("Player is in pain");
            UpdateEmotions();
            // Aplicar ui de dolor, pantalla roja y distorsionada, y reducir vida
        }
    }
    public void GetTired()
    {
        Emotion tired = emotions.First(x => x.EmotionType == PlayerController.EmotionType.Tired);
        if (!tired.bIsActive)
        {
            tired.bIsActive = true;
            currentSpeed = data.fatigueSpeed;
            Debug.Log("Player is tired");
            // Aplicar ui de cansancio, reducir velocidad de movimiento y salto
        }
    }
    // ================= SOUNDS ================= 
    public void PlayShootSfx()
    {
        audioManager.PlaySFX(audioManager.ShootSfx);
    }
}