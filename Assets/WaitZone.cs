using UnityEngine;

public class WaitZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            Debug.Log($"[{other.name}] 在 {gameObject.name} 等待护栏开启");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var agent = other.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = false;
            Debug.Log($"[{other.name}] 离开 {gameObject.name} 继续前进");
        }
    }
}
