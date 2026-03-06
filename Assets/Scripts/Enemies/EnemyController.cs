using Cinemachine;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyDataSO data;

    public Transform groundCheck;
    public LayerMask groundLayer;

    private Transform player;
    private bool isDead;
    private bool isHitted;
    private int health;
    private float hitStunDuration = 0.3f;
    private bool isChasing;

    // 1 = derecha | -1 = izquierda
    private int facingDirection;

    private CinemachineImpulseSource impulseSource;
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
        isDead = false;
        isHitted = false;

        impulseSource = GetComponent<CinemachineImpulseSource>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Detectar dirección inicial según el prefab
        facingDirection = transform.localScale.x >= 0 ? 1 : -1;
    }

    private void FixedUpdate()
    {
        if (isDead || isHitted) return;

        if (player == null)
        {
            Patrol();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        bool wasChasing = isChasing;

        if (distance <= data.detectionRadius)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
        if (wasChasing && !isChasing)
        {
            Flip();
        }

        if (isChasing)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
        animator.SetBool("isMoving", Mathf.Abs(rb.velocity.x) > 0.1f);
    }

    // ================= MOVEMENT =================

    private void Patrol()
    {
        rb.velocity = new Vector2(facingDirection * data.speed, rb.velocity.y);

        RaycastHit2D groundInfo = Physics2D.Raycast
            (
                groundCheck.position,
                Vector2.down,
                1f,
                groundLayer
            );

        if (groundInfo.collider == null)
        {
            Flip();
        }
    }

    private void Chase()
    {
        float directionToPlayer = player.position.x - transform.position.x;

        if (Mathf.Abs(directionToPlayer) < 0.1f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        int moveDir = (int)Mathf.Sign(directionToPlayer);

        rb.velocity = new Vector2(moveDir * data.speed, rb.velocity.y);

        if (moveDir != facingDirection)
            Flip();
    }

    private void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(facingDirection, 1, 1);
    }

    // ================= DAMAGE =================

    public void TakingDamage(int damageAmount)
    {
        if (isDead) return;

        CameraShakeManager.Instance.CameraShake(impulseSource);

        health -= damageAmount;

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

    // ================= ATTACK =================
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            float distance = Vector2.Distance(transform.position, collision.transform.position);
            if (distance <= data.attackRange)
            {
                animator.SetBool("isInRange", true);
                animator.SetBool("isAttacking", true);
                audioManager.PlaySFX(audioManager.ZombieAttackSfx);

                PlayerController playerScript =
                collision.gameObject.GetComponent<PlayerController>();

                Vector2 damageDirection = new Vector2(facingDirection, 0);
                playerScript.TakingDamage(damageDirection, data.damageAmount);
            }
        }
    }

    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool("isInRange", false);
    }

    // ================= DEATH =================

    private void Die()
    {
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", true);

        rb.velocity = Vector2.zero;

        Destroy(gameObject, 0.5f);
    }
}