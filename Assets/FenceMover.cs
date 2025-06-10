using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform pointA; // ��Inspector�������һ��λ��
    public Transform pointB; // ��Inspector������ڶ���λ��

    private NavMeshAgent agent;
    private NavMeshSurface surface;
    private bool flip = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // ��ʼ��NavMeshSurface����ѡ�������費��Ҫ��
        surface = GameObject.Find("NavSurface")?.GetComponent<NavMeshSurface>();

        // ���pointA�Ƿ����
        if (pointA != null)
        {
            agent.Warp(pointA.position); // ��agent˲�Ƶ�A��
            agent.SetDestination(pointB != null ? pointB.position : pointA.position); // Ŀ����B�����ûB����A
            flip = false; // Ĭ�ϴ�A��B
        }
        else
        {
            Debug.LogWarning("����Inspector����pointA����㣩��");
        }
    }

    void Update()
    {
        if (agent == null) return;

        // ���NavMesh״̬
        if (!agent.isOnNavMesh)
        {
            // ʧȥѰ·ʱ�ص�pointA
            if (pointA != null)
                agent.Warp(pointA.position);
            return;
        }

        // ����Ŀ�����л�Ŀ��
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
