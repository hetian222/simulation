using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class FenceManager : MonoBehaviour
{
    public List<GameObject> movableFences;
    public List<GameObject> fixedFences;

    public void OpenFence(List<string> fenceNames)
    {
        foreach (GameObject fence in movableFences)
        {
            bool shouldOpen = fenceNames.Contains(fence.name);
            // 如果要“打开”（消失），就调用 StartShrinking；否则就 ResetEnlarge 恢复原始大小
            var shrinker = fence.GetComponent<FenceShrinker>();
            if (shrinker != null)
            {
                if (shouldOpen)
                {
                    // 触发逐渐缩小
                    shrinker.StartShrinking();
                }
                else
                {
                    // 如果之前已经缩小到0，要恢复
                    shrinker.ResetEnlarge();
                }
            }
            else
            {
                // 如果该围栏没有挂 FenceShrinker，回退到旧逻辑直接 SetActive
                fence.SetActive(!shouldOpen);
                NavMeshObstacle obstacle = fence.GetComponent<NavMeshObstacle>();
                if (obstacle != null)
                    obstacle.enabled = !shouldOpen;
            }
        }
    }
    //控制所有围栏
    /// <summary>
    /// 一键“全部围栏”开/关：把所有 movableFences 和 fixedFences
    /// 要么都完全可见（恢复原始大小），要么都完全隐藏（直接缩小或禁用）
    /// </summary>
    public void SetAllFences(bool enabled)
    {
        foreach (GameObject f in movableFences)
        {
            var shrinker = f.GetComponent<FenceShrinker>();
            if (shrinker != null)
            {
                if (enabled)
                {
                    shrinker.ResetEnlarge();
                }
                else
                {
                    shrinker.StartShrinking();
                }
            }
            else
            {
                // 回退到旧逻辑
                f.SetActive(enabled);
            }
        }
        foreach (GameObject f in fixedFences)
        {
            var shrinker = f.GetComponent<FenceShrinker>();
            if (shrinker != null)
            {
                if (enabled)
                    shrinker.ResetEnlarge();
                else
                    shrinker.StartShrinking();
            }
            else
            {
                f.SetActive(enabled);
            }
        }
    }

    //检查某个围栏是否开启
    // 不需要改动 IsFenceOpen（它只是读 activeSelf），但如果你想让“activeSelf”能同步表示“是否完全显示”，
    // 可以在 FenceShrinker 缩放完成后手动 f.SetActive(false)。不过一般来说，缩到 scale=0 且禁用 Collider/Obstacle 后，
    // 视觉上已经看不见了。
    public bool IsFenceOpen(string fenceName)
    {
        foreach (GameObject fence in movableFences)
        {
            if (fence.name == fenceName)
            {
                return fence.transform.localScale == Vector3.zero;
            }
        }
        return false;
    }
}