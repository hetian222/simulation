using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SocialForceAgent))]
public class SocialForceRouteFollower : MonoBehaviour
{
    public List<Transform> waypoints;
    private int currentIndex = 0;
    private SocialForceAgent agent;
    public float reachThreshold = 0.5f;
    public PassageDirection agentDirection;


    void Start()
    {
        agent = GetComponent<SocialForceAgent>();

        if (waypoints != null && waypoints.Count > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        if (agent == null || waypoints == null || currentIndex >= waypoints.Count) return;

        float distance = Vector3.Distance(transform.position, waypoints[currentIndex].position);
        if (distance < reachThreshold)
        {
            currentIndex++;
            if (currentIndex < waypoints.Count)
            {
                agent.SetDestination(waypoints[currentIndex].position);
            }
            else
            {
                Destroy(gameObject); // 到达最后目标后销毁
            }
        }
    }

    public void InitRoute(List<Transform> points)
    {
        waypoints = points;
    }
}
