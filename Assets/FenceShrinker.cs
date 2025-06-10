using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))] // ����Χ������ Collider
public class FenceShrinker : MonoBehaviour
{
    [Header("����Ŀ��")]
    [Tooltip("��Χ����ԭʼ��С�����ŵ������ֵ��һ����Ϊ Vector3.zero")]
    public Vector3 targetScale = Vector3.zero;

    [Header("�����ٶ�")]
    [Tooltip("��λ��localScale ÿ����ٵ����ֵ������ȱ����ƶ���")]
    public float shrinkSpeed = 1.0f;

    [Header("��С��ɺ��Ƿ���� Obstacle/Collider")]
    public bool disableObstacleOnComplete = true;

    private Vector3 _startScale;
    private bool _isShrinking = false;
    private NavMeshObstacle _obstacle;
    private Collider _collider;

    private void Awake()
    {
        // ��¼��ʼ��С
        _startScale = transform.localScale;

        // ������� NavMeshObstacle��������
        _obstacle = GetComponent<NavMeshObstacle>();
        _collider = GetComponent<Collider>();
    }

    /// <summary>
    /// �ⲿ���ã���ʼִ�С�Χ������С������
    /// </summary>
    public void StartShrinking()
    {
        if (_isShrinking) return;
        // ȷ�� Obstacle/Collider ����С���������� enabling������������� Carving ����̬���� NavMesh
        if (_obstacle != null) _obstacle.enabled = true;
        if (_collider != null) _collider.enabled = true;

        StartCoroutine(ShrinkCoroutine());
    }

    private IEnumerator ShrinkCoroutine()
    {
        _isShrinking = true;

        // ֻҪ��ǰ localScale �� targetScale ���в�࣬�ͼ��� MoveTowards
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            // MoveTowards ���� localScale ƽ���ؽӽ� targetScale
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                targetScale,
                shrinkSpeed * Time.deltaTime
            );
            yield return null;
        }

        // ���ǿ�ƹ��㣨���⸡��в
        transform.localScale = targetScale;

        // ������ɺ����������� Obstacle �� Collider
        if (disableObstacleOnComplete)
        {
            if (_obstacle != null) _obstacle.enabled = false;
            if (_collider != null) _collider.enabled = false;
        }

        _isShrinking = false;
    }

    /// <summary>
    /// �ⲿ���ã���Χ���ָ�����ʼ��С������������ Obstacle/Collider
    /// </summary>
    public void ResetEnlarge()
    {
        StopAllCoroutines();
        transform.localScale = _startScale;

        if (_obstacle != null) _obstacle.enabled = true;
        if (_collider != null) _collider.enabled = true;

        _isShrinking = false;
    }
}
