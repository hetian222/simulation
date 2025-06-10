using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SocialForceAgent : MonoBehaviour
{
    public float desiredSpeed = 2.0f;
    public float personalSpace = 1.0f;
    public float forceStrength = 10.0f;
    public float damping = 0.8f;

    private Rigidbody rb;
    private Vector3 desiredDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.isKinematic = false;
        rb.useGravity = false;
        desiredDirection = transform.forward;
    }

    void FixedUpdate()
    {
        // 1. 计算社会力
        Vector3 socialForce = Vector3.zero;
        Collider[] nearby = Physics.OverlapSphere(transform.position, personalSpace);

        foreach (Collider col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("Player"))
            {
                Vector3 away = transform.position - col.transform.position;
                float dist = away.magnitude;
                if (dist > 0f)
                {
                    socialForce += away.normalized * (personalSpace - dist);
                }
            }
        }

        // 2. 计算目标方向力
        Vector3 desiredVelocity = desiredDirection * desiredSpeed;
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = (desiredVelocity - currentVelocity) * damping;

        // 3. 应用总力
        Vector3 totalForce = socialForce * forceStrength + velocityChange;
        rb.AddForce(totalForce);
    }

    public void SetDirection(Vector3 dir)
    {
        desiredDirection = dir.normalized;
    }

    public void SetDestination(Vector3 destination)
    {
        Vector3 dir = destination - transform.position;
        SetDirection(dir);
    }
}
