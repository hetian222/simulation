using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))] // 假设围栏上有 Collider
public class FenceShrinker : MonoBehaviour
{
    [Header("缩放目标")]
    [Tooltip("将围栏从原始大小逐渐缩放到这个数值。一般设为 Vector3.zero")]
    public Vector3 targetScale = Vector3.zero;

    [Header("缩放速度")]
    [Tooltip("单位：localScale 每秒减少的最大值（各轴等比例移动）")]
    public float shrinkSpeed = 1.0f;

    [Header("缩小完成后是否禁用 Obstacle/Collider")]
    public bool disableObstacleOnComplete = true;

    private Vector3 _startScale;
    private bool _isShrinking = false;
    private NavMeshObstacle _obstacle;
    private Collider _collider;

    private void Awake()
    {
        // 记录初始大小
        _startScale = transform.localScale;

        // 如果存在 NavMeshObstacle，缓存它
        _obstacle = GetComponent<NavMeshObstacle>();
        _collider = GetComponent<Collider>();
    }

    /// <summary>
    /// 外部调用：开始执行“围栏逐渐缩小”流程
    /// </summary>
    public void StartShrinking()
    {
        if (_isShrinking) return;
        // 确保 Obstacle/Collider 在缩小过程中仍在 enabling，方便如果你用 Carving 来动态更新 NavMesh
        if (_obstacle != null) _obstacle.enabled = true;
        if (_collider != null) _collider.enabled = true;

        StartCoroutine(ShrinkCoroutine());
    }

    private IEnumerator ShrinkCoroutine()
    {
        _isShrinking = true;

        // 只要当前 localScale 与 targetScale 还有差距，就继续 MoveTowards
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            // MoveTowards 会让 localScale 平滑地接近 targetScale
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                targetScale,
                shrinkSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 最后强制归零（避免浮点残差）
        transform.localScale = targetScale;

        // 缩放完成后根据需求禁用 Obstacle 或 Collider
        if (disableObstacleOnComplete)
        {
            if (_obstacle != null) _obstacle.enabled = false;
            if (_collider != null) _collider.enabled = false;
        }

        _isShrinking = false;
    }

    /// <summary>
    /// 外部调用：将围栏恢复到初始大小，并重新启用 Obstacle/Collider
    /// </summary>
    public void ResetEnlarge()
    {
        StopAllCoroutines();
        transform.localScale = _startScale;

        if (_obstacle != null) _obstacle.enabled = true;
        if (_collider != null) _collider.enabled = true;

        _isShrinking = false;
    }
}
