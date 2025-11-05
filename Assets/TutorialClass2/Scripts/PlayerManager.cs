using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // 每个玩家独立的管理器，不再使用单例
    public int playerId = 0;

    public int playerHealth = 3;
    public int coin = 0;
    
    public bool isDead = false;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError($"PlayerMovement component missing on {gameObject.name}");
        }
    }

    // 角色受伤方法，传递参数为伤害值
    public void TakeDamage(int damage, Vector3 knockbackDirection)
    {
        if (isDead)
            return;

        playerHealth -= damage;
        playerHealth = Mathf.Max(0, playerHealth);

        // 若存在 PlayerMovement 则触发击退
        playerMovement?.Knockback(knockbackDirection);

        if (playerHealth <= 0)
        {
            Dead();
        }
    }

    // 角色获得金币方法
    public void GetCoin()
    {
        coin++;
        Debug.Log($"{gameObject.name} get a coin! total: {coin}");
    }

    //角色死亡方法
    public void Dead()
    {
        if (isDead) return;

        isDead = true;
        // 调用表现层死亡（动画/禁用输入等）
        playerMovement?.Dead();

        // 可以在这里触发 UI / 游戏管理器 的事件
        SoundManager.Instance.PlayOn(gameObject, "PlayerDeath", volume: 0.8f, loop: false, spatial: false);
        Debug.Log($"{gameObject.name} is dead!");
    }

    //角色胜利方法
    public void Win()
    {
        Debug.Log($"{gameObject.name} win!");
    }
}