using UnityEngine;
using TMPro;
using System.Collections;

public class ExitTrigger : MonoBehaviour
{
    public float fadeDuration = 1f;
    private static int evacuatedCount = 0;
    public TextMeshProUGUI exitCountText;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var stats = other.GetComponent<PersonStats>();
        if (stats != null && stats.hasEscaped) return;
        if (stats != null) stats.hasEscaped = true;

        StartCoroutine(FadeAndDestroy(other.gameObject));
    }

    private IEnumerator FadeAndDestroy(GameObject obj)
    {
        var rend = obj.GetComponent<Renderer>();
        var mat = rend.material;
        Color c0 = mat.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            mat.color = Color.Lerp(c0, new Color(c0.r, c0.g, c0.b, 0f), t / fadeDuration);
            yield return null;
        }
        Destroy(obj);

        //���� 1) ���ظ�������ɢ���� ����//
        evacuatedCount++;
        if (exitCountText != null)
            exitCountText.text = $"{evacuatedCount} people have been evacuated";

        //���� 2) ֪ͨ UIManager ����//
        var ui = FindObjectOfType<UIManager>();
        // ע�⣺UIManager ����Ҫ��һ�� public ���� TotalSpawnedCount ���ܱ���ͨ��
        ui.RegisterEvacuation(evacuatedCount, ui.TotalSpawnedCount);
    }
}

