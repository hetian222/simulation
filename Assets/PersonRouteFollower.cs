using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PersonRouteFollower : MonoBehaviour
{
    [Header("·�������")]
    public List<Transform> waypoints;    // �� UIManager �����·���б�
    public Transform exitTarget;         // �� UIManager ����ġ����ճ��ڡ���

    [Header("��ǰ�н�״̬")]
    public int routeId;                  // UIManager �� SpawnOne ʱ��ֵ
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
                Debug.LogError("δ�ҵ� FenceManager��");
            }
        }
        agent = GetComponent<NavMeshAgent>();
        fenceManager = FindObjectOfType<FenceManager>();

        // **1. �ر��Զ��ع滮**
        agent.autoRepath = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
    }


    // ·�������� �� Χ������ӳ��
    private Dictionary<(int pathId, int pointIndex), string> fenceMapping = new Dictionary<(int, int), string>()
{
    { (0, 2), "F1-1" },  // ·��0�ĵ�2����
    { (1, 2), "F1-1" },  // ·��1�ĵ�2����
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
            Debug.Log($"[Follower] ���ó�ʼ·����: {waypoints[0].name}");
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

        // �״���Ŀ�꣺���ݵ�ǰ�Ƿ���Χ����������ȥ·��[0] ����ֱ��ȥ����
        if (CrowdController.FencesEnabled && waypoints != null && waypoints.Count > 0)
            agent.SetDestination(waypoints[0].position);
        else
            agent.SetDestination(exitTarget.position);
    }
    void Update()
    {
        if (agent == null || !agent.enabled || agent.pathPending)
            return;

        // ���� **�����ǰ����ΪΧ��ͣ�µģ����ȼ��Χ����û�д�** ���� 
        if (agent.isStopped)
        {
            var key = (routeId, currentIndex);
            // ֻ������һ·��ȷʵҪ��Χ����ʱ��ż��
            if (fenceMapping.TryGetValue(key, out string fenceName))
            {
                if (fenceManager.IsFenceOpen(fenceName))
                {
                    // Χ�����ˣ��ָ�����
                    agent.isStopped = false;
                    agent.SetDestination(waypoints[currentIndex].position);
                    Debug.Log($"[Follower] Χ�� {fenceName} �ѿ����ָ����� �� Ŀ��� {currentIndex}");
                }
            }
            return;
        }

        // ���� �����û�ߵ���ǰĿ�꣬������ ���� 
        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        // ���� ���ﵱǰ·�㣬�����һ���費��Ҫ��Χ�� ���� 
        var arriveKey = (routeId, currentIndex);
        if (fenceMapping.TryGetValue(arriveKey, out string waitFence))
        {
            if (!fenceManager.IsFenceOpen(waitFence))
            {
                // Χ�����ţ���ͣ������
                agent.isStopped = true;
                Debug.Log($"[Follower] ����� {currentIndex} �㣬��Ҫ��Χ�� {waitFence} ������ͣ");
                return;
            }
            // ����Ѿ����ˣ���ֱ�� fall-through ȥ��һ����
        }

        // ���� �����õ�Χ����/Χ���ѿ� ���� ����ǰ����һ���� ���� 
        currentIndex++;
        if (currentIndex < waypoints.Count)
        {
            agent.SetDestination(waypoints[currentIndex].position);
            Debug.Log($"[Follower] ·��{routeId}ǰ���� {currentIndex} ��");
        }
        else
        {
            // ȫ��·������֮����ȥ����
            agent.SetDestination(exitTarget.position);
            Debug.Log($"[Follower] ·��{routeId}�����յ㣬ǰ������");
        }
    }

}