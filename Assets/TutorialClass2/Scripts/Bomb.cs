using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("炸弹设置")]
    public float explosionRadius = 3f;    // 爆炸范围
    public float explosionForce = 10f;    // 爆炸力度
    public float fuseDuration = 3f;       // 引信持续时间
    public float groundCheckDistance = 0.1f; // 地面检测距离
    
    [Header("传递设置")]
    public float throwForce = 8f;         // 投掷力度
    public float upwardForce = 2f;        // 向上投掷的额外力度

    private bool isHeld = false;          // 是否被玩家持有
    private bool hasExploded = false;     // 是否已经爆炸
    private float fuseTimer;              // 引信计时器
    private Rigidbody rb;
    private BombCatcher currentHolder;    // 当前持有者

    // 对外暴露剩余的引信时间（秒）
    public float RemainingFuse { get { return fuseTimer; } }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuseTimer = fuseDuration;
    }

    void Update()
    {
        if (hasExploded) return;

        // 检查是否在地面上
        bool isGrounded = CheckIfGrounded();
        
        if (isGrounded)
        {
            Explode();
            return;
        }

        // 只有在被持有时才倒计时
        if (isHeld)
        {
            fuseTimer -= Time.deltaTime;
            if (fuseTimer <= 0)
            {
                Explode();
            }
        }
        else if (!isGrounded)
        {
            // 在空中且未被持有时，重置计时器
            fuseTimer = fuseDuration;
        }
    }

    private bool CheckIfGrounded()
    {
        // 向下发射射线检测地面
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f; // 稍微抬高起点以避免自身碰撞体
        bool hitGround = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance);
        return hitGround;
    }

    public bool CanBeCaught()
    {
        return !isHeld && !hasExploded;
    }

    public void OnCaught(BombCatcher catcher)
    {
        isHeld = true;
        currentHolder = catcher;
        
        // 开始倒计时
        // 注意：这里不重置计时器，因为我们想保持当前的计时状态
        
        // 播放被接住的音效
        SoundManager.Instance.PlayOn(gameObject, "BombCatch");

        // 禁用刚体以防止物理影响
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // 让炸弹跟随持有者
        transform.SetParent(catcher.transform);
        transform.localPosition = Vector3.forward * 0.5f; // 稍微放在玩家前方
    }

    public void ThrowFromHolder(Vector3 direction)
    {
        if (!isHeld) return;

        // 重置持有状态
        isHeld = false;
        currentHolder = null;
        transform.SetParent(null);

        // 重新启用物理
        if (rb != null)
        {
            rb.isKinematic = false;
            
            // 归一化方向并添加向上的力
            Vector3 throwDirection = direction.normalized;
            throwDirection.y += upwardForce;
            
            // 应用投掷力
            rb.velocity = Vector3.zero;  // 清除现有速度
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
        
        // 播放投掷音效
        SoundManager.Instance.PlayOn(gameObject, "BombThrow");
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 如果还被持有，解除父子关系
        if (isHeld)
        {
            transform.SetParent(null);
        }

        // 播放爆炸音效
        SoundManager.Instance.PlayOn(gameObject, "BombExplode", volume: 1f, spatial: false);

        // 显示游戏结束画面
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.ShowGameOver();
        }

        // 销毁炸弹对象
        Destroy(gameObject, 0.1f);
    }

    // 在编辑器中显示爆炸范围和地面检测范围（仅用于调试）
    void OnDrawGizmos()
    {
        // 显示爆炸范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        // 显示地面检测范围
        Gizmos.color = Color.yellow;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
    }
}