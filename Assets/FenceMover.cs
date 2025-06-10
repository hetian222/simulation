using UnityEngine;
using UnityEngine.AI;

public class FenceMover : MonoBehaviour
{
    public Transform startPoint;  
    public Transform targetPoint;
    public float moveSpeed = 5f;

    private bool _isMoving = false;
    private Vector3 _dest;
    private System.Action _onArrive;

    
    public void MoveToTarget(System.Action onArrive = null)
    {
        _dest = targetPoint.position;
        _isMoving = true;
        _onArrive = onArrive;
    }

    
    public void MoveToStart(System.Action onArrive = null)
    {
        _dest = startPoint.position;
        _isMoving = true;
        _onArrive = onArrive;
    }

    void Update()
    {
        if (_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, _dest, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _dest) < 0.01f)
            {
                transform.position = _dest;
                _isMoving = false;
                _onArrive?.Invoke();
            }
        }
    }
}
