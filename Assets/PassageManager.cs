using UnityEngine;

public class PassageManager : MonoBehaviour
{
    public float switchInterval = 10f;
    public PassageDirection currentDirection = PassageDirection.LeftToRight;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= switchInterval)
        {
            timer = 0f;
            ToggleDirection();
        }
    }

    void ToggleDirection()
    {
        currentDirection = (currentDirection == PassageDirection.LeftToRight)
            ? PassageDirection.RightToLeft
            : PassageDirection.LeftToRight;

        Debug.Log(" ��ǰ����ͨ�з���" + currentDirection);
    }

    public bool IsDirectionAllowed(PassageDirection incoming)
    {
        return currentDirection == incoming;
    }
}
