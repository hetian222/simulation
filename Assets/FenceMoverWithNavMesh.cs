using UnityEngine;
using UnityEngine.AI;

public class FenceMoverWithNavMesh : MonoBehaviour
{
    public NavMeshAgent agent;
    public NavMeshObstacle obstacle;

    public Vector3 pos_a;
    public Vector3 pos_b;
    private Vector3 targetPos;

    public float stopThreshold = 0.5f;
    public bool flipped = false;
    public bool isMoving = false;

    void Start()
    {
        Debug.Log("[Fence] FenceMoverWithNavMesh started.");

        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();

        if (agent == null || obstacle == null)
        {
            Debug.LogError("NavMeshAgent 또는 NavMeshObstacle 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        agent.updateRotation = true; // 회전 제어 안 할 경우 false로
        agent.updateUpAxis = false;

        agent.Warp(pos_a);
        transform.position = pos_a;

        obstacle.carving = true;
        obstacle.enabled = true;

        StartMoving();
    }

    void Update()
    {
        if (!isMoving || agent == null || !agent.isOnNavMesh)
            return;

        if (!agent.pathPending && agent.remainingDistance <= stopThreshold)
        {
            StopMovement();
        }
    }

    void StopMovement()
    {
        isMoving = false;
        agent.isStopped = true;
        agent.enabled = false;       //  에이전트 꺼줌 (필수)
        obstacle.enabled = true;     //  장애물 다시 켬
        transform.rotation = Quaternion.Euler(0, 0f, 0);
        Debug.Log("[Fence] 도착 및 정지. 장애물 활성화.");
    }

    public void StartMoving()
    {
        if (agent == null) return;

        flipped = !flipped;
        targetPos = flipped ? pos_b : pos_a;

        obstacle.enabled = false;
        agent.enabled = true;            // 이동 전 에이전트 다시 켜줌
        agent.Warp(transform.position);  // 보정
        agent.SetDestination(targetPos);
        agent.isStopped = false;
        isMoving = true;

        Debug.Log($"[Fence] 이동 시작: {(flipped ? "A → B" : "B → A")}");
    }
}