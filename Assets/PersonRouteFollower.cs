using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PersonRouteFollower : MonoBehaviour
{
    [Header("·�������")]
    public List<Transform> waypoints;    
    public Transform exitTarget;         

    [Header("��ǰ�н�״̬")]
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
            Debug.Log($"[Follower] ���ó�ʼ·����: {waypoints[0].name}");
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

        // ������������һ����
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

        // �����û����ǰĿ��㣬������
        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        // ���ﵱǰĿ��㣬ǰ������һ����
        currentIndex++;
        if (waypoints != null && currentIndex < waypoints.Count)
        {
            agent.SetDestination(waypoints[currentIndex].position);
            Debug.Log($"[Follower] {routeId}ǰ���� {currentIndex} ��");
        }
        else if (exitTarget != null)
        {
            // ȫ��·�������ǰ������
            agent.SetDestination(exitTarget.position);
            Debug.Log($"[Follower] ·��{routeId}�����յ㣬ǰ������");
        }
    }
}
