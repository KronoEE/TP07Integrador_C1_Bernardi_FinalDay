using UnityEngine;

[CreateAssetMenu(fileName = "EnemySettings", menuName = "ScriptableObjects/Enemy")]

public class EnemyDataSO : ScriptableObject
{
    public float detectionRadius = 5f;
    public float loseTargetRadius = 8f;
    public float speed = 2f;
    public float attackRange = 1.5f;
    public int damageAmount = 10;
    public int maxHealth = 100;
}