using UnityEngine;

public class WaitZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            Debug.Log($"[{other.name}] �� {gameObject.name} �ȴ���������");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var agent = other.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = false;
            Debug.Log($"[{other.name}] �뿪 {gameObject.name} ����ǰ��");
        }
    }
}
