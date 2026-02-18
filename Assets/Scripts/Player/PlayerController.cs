using UnityEngine;

public class PlayerController : MonoBehaviour
{                    
    [SerializeField] private PlayerDataSO data;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject deathPanel;

    AudioManager audioManager;
    Rigidbody2D rb;

    bool facingRight = true;
    bool isGrounded;
    bool bisAttacking;
    bool takingDamage;
    public bool attackCondition;
    public bool isDead;

    private int health;

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
    }

    void Start()
    {
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
            //healthbar.UpdateHealthBar(data.maxHealth, health);
            if (health <= 0)
            {
                audioManager.Stop();
                //audioManager.PlaySFX(audioManager.LooseSfx);
                deathPanel.SetActive(true);
                ChangeState(State.PistolDead);
                isDead = true;
                Time.timeScale = 0;
            }
            if (!isDead)
            {
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

        rb.velocity = new Vector2(velocityX * data.velocity, rb.velocity.y);

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
        ChangeState(State.PistolShoot);
        attackCondition = true;
    }

    // ================= ACTIONS =================

    private void DoJump()
    {
        if(isGrounded)
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
    public void GetStressed()
    {
        Debug.Log("Player is stressed");
        // Aplicar ui de stress
    }
    public void GetFear()
    {
        Debug.Log("Player is afraid");
        // Aplicar ui de miedo, acortar vision y acortar distancia de bala
    }
    public void GetPain()
    {
        Debug.Log("Player is in pain");
        // Aplicar ui de dolor, pantalla roja y distorsionada, y reducir vida
    }
    public void GetTired()
    {
        Debug.Log("Player is tired");
        // Aplicar ui de cansancio, reducir velocidad de movimiento y salto
    }
    public void GetCalm()
    {
        Debug.Log("Player is calm");
        // Aplicar ui de calma, aumentar velocidad de movimiento y salto, y mejorar precisión de disparo
    }

    // ================= SOUNDS ================= 

    public void PlayShootSfx()
    {
        audioManager.PlaySFX(audioManager.ShootSfx);
    }
}