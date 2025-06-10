using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class FenceManager : MonoBehaviour
{
    public List<GameObject> movableFences;
    public List<GameObject> fixedFences;

    public void OpenFence(List<string> fenceNames)
    {
        foreach (GameObject fence in movableFences)
        {
            bool shouldOpen = fenceNames.Contains(fence.name);
            // ���Ҫ���򿪡�����ʧ�����͵��� StartShrinking������� ResetEnlarge �ָ�ԭʼ��С
            var shrinker = fence.GetComponent<FenceShrinker>();
            if (shrinker != null)
            {
                if (shouldOpen)
                {
                    // ��������С
                    shrinker.StartShrinking();
                }
                else
                {
                    // ���֮ǰ�Ѿ���С��0��Ҫ�ָ�
                    shrinker.ResetEnlarge();
                }
            }
            else
            {
                // �����Χ��û�й� FenceShrinker�����˵����߼�ֱ�� SetActive
                fence.SetActive(!shouldOpen);
                NavMeshObstacle obstacle = fence.GetComponent<NavMeshObstacle>();
                if (obstacle != null)
                    obstacle.enabled = !shouldOpen;
            }
        }
    }
    //��������Χ��
    /// <summary>
    /// һ����ȫ��Χ������/�أ������� movableFences �� fixedFences
    /// Ҫô����ȫ�ɼ����ָ�ԭʼ��С����Ҫô����ȫ���أ�ֱ����С����ã�
    /// </summary>
    public void SetAllFences(bool enabled)
    {
        foreach (GameObject f in movableFences)
        {
            var shrinker = f.GetComponent<FenceShrinker>();
            if (shrinker != null)
            {
                if (enabled)
                {
                    shrinker.ResetEnlarge();
                }
                else
                {
                    shrinker.StartShrinking();
                }
            }
            else
            {
                // ���˵����߼�
                f.SetActive(enabled);
            }
        }
        foreach (GameObject f in fixedFences)
        {
            var shrinker = f.GetComponent<FenceShrinker>();
            if (shrinker != null)
            {
                if (enabled)
                    shrinker.ResetEnlarge();
                else
                    shrinker.StartShrinking();
            }
            else
            {
                f.SetActive(enabled);
            }
        }
    }

    //���ĳ��Χ���Ƿ���
    // ����Ҫ�Ķ� IsFenceOpen����ֻ�Ƕ� activeSelf��������������á�activeSelf����ͬ����ʾ���Ƿ���ȫ��ʾ����
    // ������ FenceShrinker ������ɺ��ֶ� f.SetActive(false)������һ����˵������ scale=0 �ҽ��� Collider/Obstacle ��
    // �Ӿ����Ѿ��������ˡ�
    public bool IsFenceOpen(string fenceName)
    {
        foreach (GameObject fence in movableFences)
        {
            if (fence.name == fenceName)
            {
                return fence.transform.localScale == Vector3.zero;
            }
        }
        return false;
    }
}