using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    //[SerializeField] private Healthbar healthbar;
    [SerializeField] private Transform player;
    [SerializeField] private EnemyDataSO data;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private bool movingRight = true;
    private bool isInRange;
    private bool isAttacking;
    private bool isMoving;
    private bool playerAlive;
    private bool isDead;
    private bool isHitted;
    private int health;
    private float movementX;
    private float hitStunDuration = 0.3f;


    private AudioManager audioManager;
    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Start()
    {
        health = data.maxHealth;
        playerAlive = true;
        isDead = false;
        isHitted = false;

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2((movingRight ? 1 : -1) * data.speed, rb.velocity.y);
        RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundLayer);

        if (groundInfo.collider == false)
        {
            Flip();
        }
    }
    private void Update()
    {
        if (playerAlive && !isDead && !isHitted)
        {
            Movement();
        }

        animator.SetBool("isMoving", isMoving && !isHitted);
    }

    // ================= DAMAGE =================

    public void TakingDamage(int damageAmount)
    {
        if (isDead) return;

        Debug.Log("Zombie recibió daño: " + damageAmount);

        health -= damageAmount;
        Debug.Log("Vida zombie: " + health);

        animator.SetTrigger("hitted");
        StartCoroutine(HitStun());

        if (health <= 0)
        {
            isDead = true;
            Die();
        }
    }

    private IEnumerator HitStun()
    {
        isHitted = true;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(hitStunDuration);

        isHitted = false;
    }

    // ================= MOVEMENT =================

    private void Movement()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < data.detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1);

            movementX = direction.x;
            isMoving = true;
        }
        else
        {
            movementX = 0;
            isMoving = false;
        }

        rb.velocity = new Vector2(movementX * data.speed, rb.velocity.y);
    }

    // ================= ATTACK =================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Flip();
        }
        int playerLayer = LayerMask.NameToLayer("Player");

        if (collision.gameObject.layer == playerLayer)
        {
            float distance = Vector2.Distance(transform.position, collision.transform.position);
            bool isInRange = distance <= data.attackRange;

            if (isInRange)
            {
                FacePlayer(collision.transform);

                isAttacking = true;
                animator.SetBool("isInRange", isInRange);
                animator.SetBool("isAttacking", isAttacking);

                audioManager.PlaySFX(audioManager.ZombieAttackSfx);

                PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();

                Vector2 directionDamage = new Vector2(transform.position.x, 0);
                playerScript.TakingDamage(directionDamage, data.damageAmount);

                playerAlive = !playerScript.isDead;

                if (!playerAlive)
                {
                    isMoving = false;
                }
            }
        }
    }

    private void FacePlayer(Transform player)
    {
        if (player == null) return;

        if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void EndAttack()
    {
        isAttacking = false;
        isInRange = false;
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isInRange", isInRange);
    }

    // ================= DEATH =================

    private void Die()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isDead", true);
        Destroy(gameObject, 0.5f);
    }
}