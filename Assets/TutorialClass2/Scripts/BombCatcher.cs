using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BombCatcher : MonoBehaviour
{
    [Header("接球设置")]
    public float catchRadius = 1.5f;  // 接球判定范围
    public float catchCooldown = 0.5f;  // 接球冷却时间
    public KeyCode catchKey = KeyCode.Space;  // 接球按键，可在Inspector中设置
    public KeyCode throwKey = KeyCode.Mouse0;  // 投掷按键，默认左键

    [Header("投掷设置")]
    public float throwForce = 10f;      // 投掷力度
    public float upwardForce = 2f;      // 向上的额外力度
    public Camera playerCamera;          // 玩家的分屏相机引用
    [Header("UI")]
    public Text bombTimerText; // 指向玩家画面中心的倒计时文本（Canvas 应该是该玩家的 Screen Space - Camera）

    [Header("效果设置")]
    public bool showCatchRadius = true;  // 是否显示接球范围（仅在编辑器中）
    public Color radiusColor = new Color(0, 1, 0, 0.3f);  // 接球范围显示颜色

    private bool canCatch = true;  // 是否可以接球
    private PlayerManager playerManager;  // 玩家管理器引用
    private Bomb heldBomb;  // 当前持有的炸弹

    void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        // 初始化计时文本（如果有的话）
        if (bombTimerText != null)
        {
            bombTimerText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 如果玩家死亡，则不处理接球和投掷
        if (playerManager != null && playerManager.isDead)
            return;

        // 当按下接球键且不在冷却中，且没有持有炸弹时
        if (canCatch && heldBomb == null && Input.GetKeyDown(catchKey))
        {
            TryCatchBomb();
        }

        // 当按下投掷键且持有炸弹时
        if (heldBomb != null)
        {
            // 更新计时文本显示
            if (bombTimerText != null)
            {
                // 防止持有的炸弹被销毁但引用仍存在的情况
                if (heldBomb != null)
                {
                    bombTimerText.text = heldBomb.RemainingFuse.ToString("F1");
                    if (!bombTimerText.gameObject.activeSelf) bombTimerText.gameObject.SetActive(true);
                }
                else
                {
                    bombTimerText.gameObject.SetActive(false);
                }
            }

            if (Input.GetKeyDown(throwKey))
            {
                ThrowHeldBomb();
            }
        }
        else
        {
            // 如果没有持有炸弹，确保 UI 隐藏
            if (bombTimerText != null && bombTimerText.gameObject.activeSelf)
                bombTimerText.gameObject.SetActive(false);
        }
    }

    void TryCatchBomb()
    {
        // 在指定范围内搜索所有碰撞体
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, catchRadius);
        
        foreach (var hitCollider in hitColliders)
        {
            // 检查是否是炸弹（假设炸弹有Bomb组件）
            Bomb bomb = hitCollider.GetComponent<Bomb>();
            if (bomb != null && bomb.CanBeCaught())
            {
                // 成功接到炸弹
                CatchBomb(bomb);
                StartCoroutine(CatchCooldown());
                break;
            }
        }
    }

    void CatchBomb(Bomb bomb)
    {
        heldBomb = bomb;  // 保存对炸弹的引用

        // 播放接球动画（如果有的话）
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Catch");
        }

        // 播放接球音效
        SoundManager.Instance.PlayOn(gameObject, "CatchSound", volume: 1f);

        // 通知炸弹被接住
        bomb.OnCaught(this);
    }

    void ThrowHeldBomb()
    {
        if (heldBomb == null) return;

        Vector3 throwDirection;
        if (playerCamera != null)
        {
            // 使用相机的前方向作为投掷方向
            throwDirection = playerCamera.transform.forward;
        }
        else
        {
            // 如果没有相机，使用玩家的前方向
            throwDirection = transform.forward;
        }

        // 添加向上的力
        throwDirection.y += upwardForce;
        throwDirection.Normalize();

        // 投掷炸弹
        heldBomb.ThrowFromHolder(throwDirection * throwForce);
        
        // 清除持有的炸弹引用
        heldBomb = null;

        // 播放投掷动画（如果有的话）
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }
    }

    IEnumerator CatchCooldown()
    {
        canCatch = false;
        yield return new WaitForSeconds(catchCooldown);
        canCatch = true;
    }

    // 在编辑器中显示接球范围（仅用于调试）
    void OnDrawGizmos()
    {
        if (showCatchRadius)
        {
            Gizmos.color = radiusColor;
            Gizmos.DrawWireSphere(transform.position, catchRadius);
        }
    }
}