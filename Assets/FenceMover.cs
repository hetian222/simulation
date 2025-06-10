using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform pointA; // 在Inspector中拖入第一个位置
    public Transform pointB; // 在Inspector中拖入第二个位置

    private NavMeshAgent agent;
    private NavMeshSurface surface;
    private bool flip = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 初始化NavMeshSurface（可选，看你需不需要）
        surface = GameObject.Find("NavSurface")?.GetComponent<NavMeshSurface>();

        // 检查pointA是否存在
        if (pointA != null)
        {
            agent.Warp(pointA.position); // 让agent瞬移到A点
            agent.SetDestination(pointB != null ? pointB.position : pointA.position); // 目标是B，如果没B还是A
            flip = false; // 默认从A到B
        }
        else
        {
            Debug.LogWarning("请在Inspector拖入pointA（起点）！");
        }
    }

    void Update()
    {
        if (agent == null) return;

        // 检查NavMesh状态
        if (!agent.isOnNavMesh)
        {
            // 失去寻路时回到pointA
            if (pointA != null)
                agent.Warp(pointA.position);
            return;
        }

        // 到达目标点后切换目标
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            if (flip)
            {
                if (pointB != null)
                    agent.SetDestination(pointB.position);
            }
            else
            {
                if (pointA != null)
                    agent.SetDestination(pointA.position);
            }
            flip = !flip;
        }
    }
}
