using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PersonRouteFollower : MonoBehaviour
{
    [Header("路径与出口")]
    public List<Transform> waypoints;    // 由 UIManager 传入的路点列表
    public Transform exitTarget;         // 由 UIManager 传入的“最终出口”点

    [Header("当前行进状态")]
    public int routeId;                  // UIManager 在 SpawnOne 时赋值
    private int currentIndex = 0;
    private NavMeshAgent agent;
    private FenceManager fenceManager;


    private void Awake()
    {
        if (fenceManager == null)
        {
            fenceManager = FindObjectOfType<FenceManager>();
            if (fenceManager == null)
            {
                Debug.LogError("未找到 FenceManager！");
            }
        }
        agent = GetComponent<NavMeshAgent>();
        fenceManager = FindObjectOfType<FenceManager>();

        // **1. 关闭自动重规划**
        agent.autoRepath = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
    }


    // 路径点索引 → 围栏名字映射
    private Dictionary<(int pathId, int pointIndex), string> fenceMapping = new Dictionary<(int, int), string>()
{
    { (0, 2), "F1-1" },  // 路径0的第2个点
    { (1, 2), "F1-1" },  // 路径1的第2个点
    { (8, 1), "F1-2" },  //
    { (4, 3), "F1-3" },
    { (6, 3), "F1-3" },
    { (9, 2), "F1-3" },
    { (5, 3), "F2-1" },
    { (7, 3), "F2-1" },
    { (8, 2), "F2-1" },
    { (9, 1), "F2-2" },
    { (2, 2), "F2-3" },
    { (3, 2), "F2-3" },
    { (4, 2), "F3-1" },
    { (5, 2), "F3-1" },
    { (11, 1), "F3-2" },
    { (2, 4), "F3-3" },
    { (0, 4), "F3-3" },
    { (10, 2), "F3-3" },
    { (1, 4), "F4-1" },
    { (3, 4), "F4-1" },
    { (11, 2), "F4-1" },
    { (6, 2), "F4-2" },
    { (7, 2), "F4-2" },
    { (10, 1), "F4-3" },


};


    public void InitRoute(List<Transform> points)
    {
        waypoints = points;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.stoppingDistance = 0.2f;
            agent.autoBraking = true;
        }

        if (waypoints != null && waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[0].position);
            Debug.Log($"[Follower] 设置初始路径点: {waypoints[0].name}");
        }
    }
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        fenceManager = FindObjectOfType<FenceManager>();

        agent.autoRepath = true;
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.stoppingDistance = 0.5f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // 首次下目标：根据当前是否有围栏，决定是去路点[0] 还是直接去出口
        if (CrowdController.FencesEnabled && waypoints != null && waypoints.Count > 0)
            agent.SetDestination(waypoints[0].position);
        else
            agent.SetDestination(exitTarget.position);
    }
    void Update()
    {
        if (agent == null || !agent.enabled || agent.pathPending)
            return;

        // —— **如果当前是因为围栏停下的，就先检测围栏有没有打开** —— 
        if (agent.isStopped)
        {
            var key = (routeId, currentIndex);
            // 只有在这一路点确实要等围栏的时候才检测
            if (fenceMapping.TryGetValue(key, out string fenceName))
            {
                if (fenceManager.IsFenceOpen(fenceName))
                {
                    // 围栏开了，恢复行走
                    agent.isStopped = false;
                    agent.SetDestination(waypoints[currentIndex].position);
                    Debug.Log($"[Follower] 围栏 {fenceName} 已开，恢复行走 → 目标点 {currentIndex}");
                }
            }
            return;
        }

        // —— 如果还没走到当前目标，继续等 —— 
        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        // —— 到达当前路点，检查这一点需不需要等围栏 —— 
        var arriveKey = (routeId, currentIndex);
        if (fenceMapping.TryGetValue(arriveKey, out string waitFence))
        {
            if (!fenceManager.IsFenceOpen(waitFence))
            {
                // 围栏关着，就停下来等
                agent.isStopped = true;
                Debug.Log($"[Follower] 到达第 {currentIndex} 点，需要等围栏 {waitFence} 开，暂停");
                return;
            }
            // 如果已经开了，就直接 fall-through 去下一个点
        }

        // —— 都不用等围栏了/围栏已开 —— 继续前往下一个点 —— 
        currentIndex++;
        if (currentIndex < waypoints.Count)
        {
            agent.SetDestination(waypoints[currentIndex].position);
            Debug.Log($"[Follower] 路线{routeId}前往第 {currentIndex} 点");
        }
        else
        {
            // 全部路点走完之后，再去出口
            agent.SetDestination(exitTarget.position);
            Debug.Log($"[Follower] 路线{routeId}到达终点，前往出口");
        }
    }

}