using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private int damage = 50;
    [SerializeField] private GameObject impactEffect;

    private float lifeSpan = 3f;

    private void Start()
    {
        rb.velocity = transform.right * speed;
        Destroy(gameObject, lifeSpan);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakingDamage(damage);

            GameObject impact = Instantiate(
                impactEffect,
                transform.position,
                Quaternion.identity
            );

            Destroy(impact, 0.5f);
        }
        Destroy(gameObject);
    } 
}