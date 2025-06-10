using UnityEngine;
using UnityEngine.AI;

public class DirectionGate : MonoBehaviour
{
    private PassageManager manager;

    void Start()
    {
        manager = FindObjectOfType<PassageManager>();
    }

    private void OnTriggerStay(Collider other)
    {
        var tag = other.GetComponent<NavAgentDirectionTag>();
        var agent = other.GetComponent<NavMeshAgent>();

        if (manager != null && tag != null && agent != null)
        {
            bool allow = manager.IsDirectionAllowed(tag.agentDirection);

            agent.isStopped = !allow; //  ×èÖ¹»ò»Ö¸´Ç°½ø
        }
    }
}
