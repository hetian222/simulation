using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class CrowdController : MonoBehaviour
{
    [Header(" 围栏管理 ")]
    public FenceManager fenceManager;
    public float switchInterval = 10f;    // 定时分批切换间隔（保留原有功能）
    private int currentGroup = 0;
    private Coroutine switchCoroutine;

    [Header(" 手动/自动模式 ")]
    public TextMeshProUGUI fenceStatusText; // UI 提示文本
    private bool fencesEnabled = true;      // 围栏当前开/关状态
    private bool fenceSystemEnabled = true; // 是否允许定时分批切换

    [Header(" 多区域密度自动化 ")]
    public BoxCollider[] monitoringZones;    // 拖入多个 IsTrigger 的监控区域
    public float densityCheckInterval = 1f;  // 多久检测一次
    public float upperDensityThreshold = 0.1f;  // 高于此密度 (人/㎡) 开围栏
    public float lowerDensityThreshold = 0.05f; // 低于此密度 关围栏
    private Coroutine densityCoroutine;      // 密度检测协程

    [Header(" 可选 NavMesh 更新 ")]
    public NavMeshSurface navSurface;

    private void Start()
    {
        
        // 启动原有的定时分批切换协程
        switchCoroutine = StartCoroutine(SwitchCrowdFlow());
        // 启动多区域密度检测协程
        densityCoroutine = StartCoroutine(CheckDensityLoop());
        // 初始化 UI 提示
        UpdateFenceStatusUI();
    }

    // 原有：定时分批切换
    IEnumerator SwitchCrowdFlow()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= switchInterval)
            {
                if (fenceSystemEnabled)  // 只有允许时才执行分批切换
                {
                    ApplyFenceGroup(currentGroup);
                    currentGroup = (currentGroup + 1) % 3;
                }
                timer = 0f;
            }
            yield return null;
        }
    }

    public void ApplyFenceGroup(int groupIndex)
    {
        switch (groupIndex)
        {
            case 0:
                fenceManager.OpenFence(new List<string> { "F1-2", "F2-3", "F3-3", "F4-1" });
                break;
            case 1:
                fenceManager.OpenFence(new List<string> { "F1-3", "F2-1", "F3-1", "F4-2" });
                break;
            case 2:
                fenceManager.OpenFence(new List<string> { "F1-1", "F1-3", "F2-1", "F2-2", "F3-2", "F3-3", "F4-1", "F4-3" });
                break;
        }
        if (navSurface != null)
            navSurface.UpdateNavMesh(navSurface.navMeshData);
    }

    // 原有：手动切换
    public void ManualSwitch()
    {
        currentGroup = (currentGroup + 1) % 3;
        ApplyFenceGroup(currentGroup);
        Debug.Log($"手动切换到组 {currentGroup}");
    }

    // 原有：滑块调整定时间隔
    public void SetSwitchInterval(float value)
    {
        switchInterval = value;
        Debug.Log($"调整分批切换间隔到 {switchInterval} 秒");
    }
    public static bool FencesEnabled = true;

    // 原有：手动“一键”开关围栏，并控制是否启用定时分批
    //public void ToggleFenceSystem()
    //{
    //    fencesEnabled = !fencesEnabled;
    //    fenceSystemEnabled = fencesEnabled;       // 同步控制定时逻辑是否生效
    //    CrowdController.FencesEnabled = fencesEnabled; //  通知全局状态
    //    fenceManager.SetAllFences(fencesEnabled); // 一次性开/关所有围栏
    //    if (navSurface != null) navSurface.UpdateNavMesh(navSurface.navMeshData);

    //    // 重新下发目的地
    //    foreach (var a in FindObjectsOfType<NavMeshAgent>())
    //        a.SetDestination(a.destination);
    //    UpdateFenceStatusUI();
    //    Debug.Log($"围栏系统已 {(fencesEnabled ? "开启" : "关闭")}");
    //}

    //// 更新 UI 文本
    //private void UpdateFenceStatusUI()
    //{
    //    if (fenceStatusText != null)
    //        fenceStatusText.text = fencesEnabled
    //            ? "当前状态： 有围栏"
    //            : "当前状态： 无围栏";
    //}
    private bool autoDensityEnabled = true;
    private bool hasFencesArrived = false;
    // // —— 新增：多区域密度检测协程 —— 
    // --- 手动/自动整体开关 ---
    public void ToggleFenceSystem()
    {
        fencesEnabled = !fencesEnabled;
        SetFenceSystemEnabled(fencesEnabled);
        Debug.Log($"围栏系统已 {(fencesEnabled ? "开启" : "关闭")}");
    }

    void SetFenceSystemEnabled(bool enabled)
    {
        fenceSystemEnabled = enabled;
        fenceManager.SetAllFences(enabled); // 全部显示/隐藏
        if (navSurface != null)
            navSurface.UpdateNavMesh(navSurface.navMeshData);

        foreach (var a in FindObjectsOfType<NavMeshAgent>())
            a.SetDestination(a.destination);

        UpdateFenceStatusUI();
    }

    // --- 状态UI ---
    void UpdateFenceStatusUI()
    {
        if (fenceStatusText != null)
            fenceStatusText.text = fencesEnabled
                ? "当前状态：有围栏"
                : "当前状态：无围栏";
    }

    // ------------------ 密度自动控制 + 移动 ------------------
    IEnumerator CheckDensityLoop()
    {
        yield return new WaitForSeconds(densityCheckInterval);

        while (true)
        {
            if (autoDensityEnabled)
            {
                var persons = GameObject.FindGameObjectsWithTag("Player");
                bool anyHigh = false;
                bool allLow = true;

                foreach (var zone in monitoringZones)
                {
                    float area = zone.bounds.size.x * zone.bounds.size.z;
                    int inZone = 0;
                    foreach (var p in persons)
                        if (zone.bounds.Contains(p.transform.position))
                            inZone++;
                    float zoneDensity = area > 0f ? inZone / area : 0f;
                    Debug.Log($"[Density] Zone={zone.name}, count={inZone}, area={area:F1}, density={zoneDensity:F3}");

                    if (zoneDensity >= upperDensityThreshold)
                        anyHigh = true;
                    if (zoneDensity > lowerDensityThreshold)
                        allLow = false;
                }

                // 1. 密度高：围栏移动到目标点并开启三组切换
                if (anyHigh && !hasFencesArrived)
                {
                    MoveAllFencesToTarget(() => {
                        SetFenceSystemEnabled(true);
                        hasFencesArrived = true;
                        Debug.Log("围栏全部到达目标点，开启分组切换！");
                    });
                }
                // 2. 密度低：围栏关闭，移回起始点并隐藏
                else if (allLow && hasFencesArrived)
                {
                    SetFenceSystemEnabled(false);
                    MoveAllFencesToStart();
                    hasFencesArrived = false;
                    Debug.Log("密度低，围栏全部收回起始点并隐藏！");
                }
            }
            yield return new WaitForSeconds(densityCheckInterval);
        }
    }

    void MoveAllFencesToTarget(System.Action onAllArrived = null)
    {
        var fences = fenceManager.movableFences;
        int finished = 0;
        if (fences.Count == 0)
        {
            onAllArrived?.Invoke();
            return;
        }
        foreach (var f in fences)
        {
            var mover = f.GetComponent<FenceMover>();
            var shrinker = f.GetComponent<FenceShrinker>();
            if (mover)
            {
                // 先恢复显示
                if (shrinker) shrinker.ResetEnlarge();
                mover.MoveToTarget(() => {
                    finished++;
                    if (finished == fences.Count)
                        onAllArrived?.Invoke();
                });
            }
            else
            {
                // 无 mover 直接回调
                finished++;
                if (finished == fences.Count)
                    onAllArrived?.Invoke();
            }
        }
    }

    void MoveAllFencesToStart()
    {
        var fences = fenceManager.movableFences;
        foreach (var f in fences)
        {
            var mover = f.GetComponent<FenceMover>();
            var shrinker = f.GetComponent<FenceShrinker>();
            if (mover)
            {
                mover.MoveToStart(() => {
                    if (shrinker) shrinker.StartShrinking();
                });
            }
            else
            {
                if (shrinker) shrinker.StartShrinking();
            }
        }
    }

    public void SetAutoDensityEnabled(bool on)
    {
        autoDensityEnabled = on;
        Debug.Log($"密度自动化已 {(on ? "启用" : "禁用")}");
    }
}

