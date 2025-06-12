using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class CrowdController : MonoBehaviour
{
    [Header(" Χ������ ")]
    public FenceManager fenceManager;
    public float switchInterval = 15f;    // ��ʱ�����л����������ԭ�й��ܣ�
    private int currentGroup = 0;
    private Coroutine switchCoroutine;

    [Header(" �ֶ�/�Զ�ģʽ ")]
    public TextMeshProUGUI fenceStatusText; // UI ��ʾ�ı�
    private bool fencesEnabled = true;      // Χ����ǰ��/��״̬
    private bool fenceSystemEnabled = true; // �Ƿ�����ʱ�����л�

    [Header(" �������ܶ��Զ��� ")]
    public BoxCollider[] monitoringZones;    // ������ IsTrigger �ļ������
    public float densityCheckInterval = 1f;  // ��ü��һ��
    public float upperDensityThreshold = 0.1f;  // ���ڴ��ܶ� (��/�O) ��Χ��
    public float lowerDensityThreshold = 0.05f; // ���ڴ��ܶ� ��Χ��
    private Coroutine densityCoroutine;      // �ܶȼ��Э��

    [Header(" ��ѡ NavMesh ���� ")]
    public NavMeshSurface navSurface;

    private void Start()
    {
        MoveAllFencesToStart();
        // ����ԭ�еĶ�ʱ�����л�Э��
        switchCoroutine = StartCoroutine(SwitchCrowdFlow());
        // �����������ܶȼ��Э��
        densityCoroutine = StartCoroutine(CheckDensityLoop());
        // ��ʼ�� UI ��ʾ
        UpdateFenceStatusUI();
    }

    // ԭ�У���ʱ�����л�
    IEnumerator SwitchCrowdFlow()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer >= switchInterval)
            {
                if (fenceSystemEnabled)  // ֻ������ʱ��ִ�з����л�
                {
                    ApplyFenceGroup(currentGroup);
                    currentGroup = (currentGroup + 1) % 3;
                }
                timer = 0f;
            }
            yield return null;
        }
    }

    public void ApplyFenceGroup(int groupIndex)
    {
        switch (groupIndex)
        {
            case 0:
                fenceManager.OpenFence(new List<string> { "F1-2", "F2-3", "F3-3", "F4-1" });
                break;
            case 1:
                fenceManager.OpenFence(new List<string> { "F1-3", "F2-1", "F3-1", "F4-2" });
                break;
            case 2:
                fenceManager.OpenFence(new List<string> { "F1-1", "F1-3", "F2-1", "F2-2", "F3-2", "F3-3", "F4-1", "F4-3" });
                break;
        }
        if (navSurface != null)
            navSurface.UpdateNavMesh(navSurface.navMeshData);
    }

    // ԭ�У��ֶ��л�
    public void ManualSwitch()
    {
        currentGroup = (currentGroup + 1) % 3;
        ApplyFenceGroup(currentGroup);
        Debug.Log($"�ֶ��л����� {currentGroup}");
    }

    // ԭ�У����������ʱ���
    public void SetSwitchInterval(float value)
    {
        switchInterval = value;
        Debug.Log($"���������л������ {switchInterval} ��");
    }
    public static bool FencesEnabled = true;

    // ԭ�У��ֶ���һ��������Χ�����������Ƿ����ö�ʱ����
    public void ToggleFenceSystem()
    {
        fencesEnabled = !fencesEnabled;
        fenceSystemEnabled = fencesEnabled;       // ͬ�����ƶ�ʱ�߼��Ƿ���Ч
        CrowdController.FencesEnabled = fencesEnabled; //  ֪ͨȫ��״̬
        fenceManager.SetAllFences(fencesEnabled); // һ���Կ�/������Χ��
        if (navSurface != null) navSurface.UpdateNavMesh(navSurface.navMeshData);

        // �����·�Ŀ�ĵ�
        foreach (var a in FindObjectsOfType<NavMeshAgent>())
            a.SetDestination(a.destination);
        UpdateFenceStatusUI();
        Debug.Log($"Χ��ϵͳ�� {(fencesEnabled ? "����" : "�ر�")}");
    }

    // ���� UI �ı�
    private void UpdateFenceStatusUI()
    {
        if (fenceStatusText != null)
            fenceStatusText.text = fencesEnabled
                ? "��ǰ״̬�� ��Χ��"
                : "��ǰ״̬�� ��Χ��";
    }
    private bool autoDensityEnabled = true;
    private bool hasFencesArrived = false;
    // ���� �������������ܶȼ��Э�� ���� 
    IEnumerator CheckDensityLoop()
    {
        yield return new WaitForSeconds(densityCheckInterval);

        while (true)
        {
            if (autoDensityEnabled)
            {
                var persons = GameObject.FindGameObjectsWithTag("Player");
                bool anyHigh = false;   // ��һ�����������
                bool allLow = true;    // ȫ�����򶼵�������

                // ���� ��ÿ��������ֱ����ܶ� ���� 
                foreach (var zone in monitoringZones)
                {
                    // ���������x �� z ��С��ˣ�
                    float area = zone.bounds.size.x * zone.bounds.size.z;

                    // ͳ�����ڱ���������
                    int inZone = 0;
                    foreach (var p in persons)
                        if (zone.bounds.Contains(p.transform.position))
                            inZone++;

                    float zoneDensity = area > 0f ? inZone / area : 0f;
                    Debug.Log($"[Density] Zone={zone.name}, count={inZone}, area={area:F1}, density={zoneDensity:F3}");

                    if (zoneDensity >= upperDensityThreshold)
                        anyHigh = true;      // ֻҪ��һ����������ֵ
                    if (zoneDensity > lowerDensityThreshold)
                        allLow = false;      // ֻҪ��һ�����������ޣ��Ͳ���ȫ���ر�
                }

                // ���� �Զ���/���߼� ���� 
                if (anyHigh && !fencesEnabled)
                {
                    // ��һ����ӵ�� �� ��Χ��
                    ToggleFenceSystem();
                    MoveAllFencesToTarget();
                    hasFencesArrived = true;
                    Debug.Log("Χ����ʼ�ƶ���Ŀ��㣡");
                }
                else if (allLow && fencesEnabled)
                {
                    // ȫ������ϡ�� �� ��Χ��
                   // ToggleFenceSystem();
                    MoveAllFencesToStart();
                    hasFencesArrived = false;
                    Debug.Log("Χ����������ʼ�㣡");
                }
            }

            yield return new WaitForSeconds(densityCheckInterval);
        }
    }
    void MoveAllFencesToTarget()
    {
        foreach (var f in fenceManager.movableFences)
        {
            var mover = f.GetComponent<FenceMoverWithNavMesh>();
            if (mover != null)
            {
                mover.StartMoving(); // �ƶ���Ŀ��㣨pos_b��
            }
        }
    }
    void MoveAllFencesToStart()
    {
        foreach (var f in fenceManager.movableFences)
        {
            var mover = f.GetComponent<FenceMoverWithNavMesh>();
            if (mover != null)
            {
                // ǿ������Ϊ��ʼ״̬
                mover.agent.enabled = false;
                mover.obstacle.enabled = true;
                mover.transform.position = mover.pos_a;
                mover.flipped = false;
                mover.isMoving = false;
            }
        }
    }




    public void SetAutoDensityEnabled(bool on)
    {
        autoDensityEnabled = on;
        Debug.Log($"�ܶ��Զ����� {(on ? "����" : "����")}");
    }

}
