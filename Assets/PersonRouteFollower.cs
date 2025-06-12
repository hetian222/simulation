using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PersonRouteFollower : MonoBehaviour
{
    [Header("路径与出口")]
    public List<Transform> waypoints;    
    public Transform exitTarget;         

    [Header("当前行进状态")]
    public int routeId;                  
    private int currentIndex = 0;
    private NavMeshAgent agent;

    public void InitRoute(List<Transform> points)
    {
        waypoints = points;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null && waypoints != null && waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[0].position);
            Debug.Log($"[Follower] 设置初始路径点: {waypoints[0].name}");
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.autoRepath = true;
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.stoppingDistance = 0.5f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // 启动后就走向第一个点
        if (waypoints != null && waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
        else if (exitTarget != null)
        {
            agent.SetDestination(exitTarget.position);
        }
    }

    void Update()
    {
        if (agent == null || !agent.enabled || agent.pathPending)
            return;

        // 如果还没到当前目标点，继续等
        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        // 到达当前目标点，前进到下一个点
        currentIndex++;
        if (waypoints != null && currentIndex < waypoints.Count)
        {
            agent.SetDestination(waypoints[currentIndex].position);
            Debug.Log($"[Follower] {routeId}前往第 {currentIndex} 点");
        }
        else if (exitTarget != null)
        {
            // 全部路点走完后前往出口
            agent.SetDestination(exitTarget.position);
            Debug.Log($"[Follower] 路线{routeId}到达终点，前往出口");
        }
    }
}
