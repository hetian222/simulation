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
    public float switchInterval = 15f;    // 定时分批切换间隔（保留原有功能）
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
        MoveAllFencesToStart();
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
    public void ToggleFenceSystem()
    {
        fencesEnabled = !fencesEnabled;
        fenceSystemEnabled = fencesEnabled;       // 同步控制定时逻辑是否生效
        CrowdController.FencesEnabled = fencesEnabled; //  通知全局状态
        fenceManager.SetAllFences(fencesEnabled); // 一次性开/关所有围栏
        if (navSurface != null) navSurface.UpdateNavMesh(navSurface.navMeshData);

        // 重新下发目的地
        foreach (var a in FindObjectsOfType<NavMeshAgent>())
            a.SetDestination(a.destination);
        UpdateFenceStatusUI();
        Debug.Log($"围栏系统已 {(fencesEnabled ? "开启" : "关闭")}");
    }

    // 更新 UI 文本
    private void UpdateFenceStatusUI()
    {
        if (fenceStatusText != null)
            fenceStatusText.text = fencesEnabled
                ? "当前状态： 有围栏"
                : "当前状态： 无围栏";
    }
    private bool autoDensityEnabled = true;
    private bool hasFencesArrived = false;
    // —— 新增：多区域密度检测协程 —— 
    IEnumerator CheckDensityLoop()
    {
        yield return new WaitForSeconds(densityCheckInterval);

        while (true)
        {
            if (autoDensityEnabled)
            {
                var persons = GameObject.FindGameObjectsWithTag("Player");
                bool anyHigh = false;   // 任一区域高于上限
                bool allLow = true;    // 全部区域都低于下限

                // —— 对每个监控区分别算密度 —— 
                foreach (var zone in monitoringZones)
                {
                    // 区域面积（x 和 z 大小相乘）
                    float area = zone.bounds.size.x * zone.bounds.size.z;

                    // 统计落在本区的人数
                    int inZone = 0;
                    foreach (var p in persons)
                        if (zone.bounds.Contains(p.transform.position))
                            inZone++;

                    float zoneDensity = area > 0f ? inZone / area : 0f;
                    Debug.Log($"[Density] Zone={zone.name}, count={inZone}, area={area:F1}, density={zoneDensity:F3}");

                    if (zoneDensity >= upperDensityThreshold)
                        anyHigh = true;      // 只要有一个区超过阈值
                    if (zoneDensity > lowerDensityThreshold)
                        allLow = false;      // 只要有一个区高于下限，就不能全部关闭
                }

                // —— 自动开/关逻辑 —— 
                if (anyHigh && !fencesEnabled)
                {
                    // 任一区域拥堵 → 开围栏
                    ToggleFenceSystem();
                    MoveAllFencesToTarget();
                    hasFencesArrived = true;
                    Debug.Log("围栏开始移动到目标点！");
                }
                else if (allLow && fencesEnabled)
                {
                    // 全部区域稀疏 → 关围栏
                   // ToggleFenceSystem();
                    MoveAllFencesToStart();
                    hasFencesArrived = false;
                    Debug.Log("围栏回收至起始点！");
                }
            }

            yield return new WaitForSeconds(densityCheckInterval);
        }
    }
    void MoveAllFencesToTarget()
    {
        foreach (var f in fenceManager.movableFences)
        {
            var mover = f.GetComponent<FenceMoverWithNavMesh>();
            if (mover != null)
            {
                mover.StartMoving(); // 移动到目标点（pos_b）
            }
        }
    }
    void MoveAllFencesToStart()
    {
        foreach (var f in fenceManager.movableFences)
        {
            var mover = f.GetComponent<FenceMoverWithNavMesh>();
            if (mover != null)
            {
                // 强制设置为起始状态
                mover.agent.enabled = false;
                mover.obstacle.enabled = true;
                mover.transform.position = mover.pos_a;
                mover.flipped = false;
                mover.isMoving = false;
            }
        }
    }




    public void SetAutoDensityEnabled(bool on)
    {
        autoDensityEnabled = on;
        Debug.Log($"密度自动化已 {(on ? "启用" : "禁用")}");
    }

}
