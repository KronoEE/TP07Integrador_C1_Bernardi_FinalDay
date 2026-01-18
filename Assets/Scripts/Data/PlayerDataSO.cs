using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/Player")]

public class PlayerDataSO : ScriptableObject
{
    [Header("---------- Layers ----------")]
    public LayerMask layerMask;
    [Header("---------- Key Bindings ----------")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode attackKey = KeyCode.E;
    [Header("---------- Jump Settings ----------")]
    public float reboundForce = 10f;
    public float lengthRayCast = 1f;
    public float jumpBoostTime = 5f;
    public float jumpBoostForce = 2f;
    public int maxJumpForce = 10;
    [Header("---------- Player Settings ----------")]
    public float velocity = 6f;
    public int maxHealth = 3;

}
