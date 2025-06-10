using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;


public class UIManager : MonoBehaviour
{
    public TMP_InputField heightInput;
    public TMP_InputField shoulderWidthInput;
    public GameObject personPrefab;     // Բ����Ԥ���壬�踽�� NavMeshAgent
    public Transform spawnPoint;        // �������ĵ�
    public Transform[] exitTargets;

    public Transform targetPoint;       // Ŀ��㣬���ｫ�������ƶ�
    public Transform[] targetExit; // �� routeDropdown һһ��Ӧ
    public TMP_InputField speedInput;
    public float defaultSpeed = 5.0f; // Ĭ���ٶ�
    public TMP_InputField countInput;
    public int defaultCount = 1;
    public TMP_Dropdown spawnPointDropdown; //ѡ����������
    public Transform[] spawnPoints; // Ҫ�� Inspector ���ֶ����� A/B/C ��Щ Transform
    public TMP_Dropdown targetPointDropdown;  // ������Ŀ�������
    public Transform[] targetPoints;          // Inspector ��������Ŀ���
    public TextMeshProUGUI spawnedCountText;
    private int totalSpawnedCount = 0;
    //public TMP_Dropdown routeDropdown;//·��ѡ��
    public Toggle[] spawnPointToggles; // ��ѡ Toggle �б�

    public float spawnRangeX = 5f;
    public float spawnRangeZ = 5f;

    public TMP_Dropdown personTypeDropdown; // ��������Ⱥ����ѡ��
    public Color[] routeColors;//·����ɫ
    [HideInInspector]

    public int TotalSpawnedCount => totalSpawnedCount;
    [Header("ͳ�ƣ���ʱ")]
    public TextMeshProUGUI elapsedTimeText;    // �������� Inspector ��һ�� TMP_Text
    private float startTime;
    private bool timingStarted = false;

    void Awake()
    {
        // һ��ʼ�ı�����
        if (elapsedTimeText != null)
            elapsedTimeText.text = "";
    }
    public void QuickCreate()
    {
        int total = int.Parse(countInput.text);
        CreateMixedPeople(total);
    }
    private Dictionary<int, int[]> spawnPointRouteMap = new Dictionary<int, int[]>
    {
        {0, new int[] {0,1,8}},
        {1, new int[] {2,3,9}},
        {2, new int[] {4,5,11}},
        {3, new int[] {6,7,10}}
    };
    private readonly Dictionary<int, int[]> spawnExitMap = new Dictionary<int, int[]>
{
    { 0, new[]{ 1,2,3 } },
    { 1, new[]{ 0,2,3 } },
    { 2, new[]{ 0,1,3 } },
    { 3, new[]{ 0,1,2 } }
};


    public enum PersonType
    {
        Adult,
        Child,
        Elder
    }


    private void GetRandomAttributes(PersonType type, out float height, out float shoulderWidth, out Color color)
    {
        switch (type)
        {
            case PersonType.Child:
                height = Random.Range(1, 1.3f);
                shoulderWidth = Random.Range(0.25f, 0.4f);
                color = Color.cyan;
                break;
            case PersonType.Elder:
                height = Random.Range(1.45f, 1.6f);
                shoulderWidth = Random.Range(0.4f, 0.5f);
                color = new Color(1f, 0.6f, 0.2f);
                break;
            default:
                height = Random.Range(1.65f, 1.8f);
                shoulderWidth = Random.Range(0.45f, 0.6f);
                color = Color.white;
                break;
        }
    }

    public void CreatePerson()
    {
        // 1. �������/���/�ٶ�/����/���ͣ�����ԭ����һ����
        int typeIndex = personTypeDropdown.value;
        PersonType selectedType = (PersonType)typeIndex;
        GetRandomAttributes(selectedType, out float defaultH, out float defaultW, out Color typeColor);

        float height = defaultH;
        if (float.TryParse(heightInput.text, out float h) && h > 0) height = h;

        float shoulder = defaultW;
        if (float.TryParse(shoulderWidthInput.text, out float w) && w > 0) shoulder = w;

        float speed = defaultSpeed;
        if (float.TryParse(speedInput.text, out float s) && s > 0) speed = s;

        int count = defaultCount;
        if (int.TryParse(countInput.text, out int c) && c > 0) count = c;

        // 2. ��ȡ������������ �� Ŀ���
        int si = Mathf.Clamp(spawnPointDropdown.value, 0, spawnPoints.Length - 1);
        Transform sp = spawnPoints[si];

        int ti = Mathf.Clamp(targetPointDropdown.value, 0, targetPoints.Length - 1);
        Transform tp = targetPoints[ti];

        // 3. ѭ������
        for (int i = 0; i < count; i++)
        {
            // 3.1 �� NavMesh �����һ������λ��
            Vector3 raw = sp.position + new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                0, Random.Range(-spawnRangeZ, spawnRangeZ));
            if (!NavMesh.SamplePosition(raw, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            // 3.2 ʵ�������������
            GameObject person = Instantiate(personPrefab, hit.position, Quaternion.identity);
            person.transform.localScale = new Vector3(shoulder, height / 2f, shoulder);
            var rend = person.GetComponent<Renderer>();
            if (rend) rend.material.color = typeColor;
            person.tag = "Player";
            person.AddComponent<PersonStats>();

            // 3.3 ���� NavMeshAgent
            var agent = person.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = speed;
                agent.radius = shoulder * 0.6f + 0.1f;
                agent.stoppingDistance = 0.1f;
                agent.autoRepath = true;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.avoidancePriority = Random.Range(0, 100);
                // **ֱ�Ӹ�Ŀ����´�Ŀ�ĵ�**
                agent.SetDestination(tp.position);
            }


            // 3.4 ͳ�Ʋ�������ʱ
            totalSpawnedCount++;
            if (spawnedCountText != null)
                spawnedCountText.text = $"Number of people: {totalSpawnedCount}";
            RegisterSpawn();
        }
    }



    // �� UIManager �����棬������������֮�󼴿�
    public void CreateMixedPeople(int total)
    {
        int adults = Mathf.RoundToInt(total * 0.92f);
        int elders = Mathf.RoundToInt(total * 0.03f);
        int children = total - adults - elders;

        List<PersonType> allTypes = new List<PersonType>();
        allTypes.AddRange(Enumerable.Repeat(PersonType.Adult, adults));
        allTypes.AddRange(Enumerable.Repeat(PersonType.Elder, elders));
        allTypes.AddRange(Enumerable.Repeat(PersonType.Child, children));
        Shuffle(allTypes); // ����Ҫʵ��һ�� Shuffle() ����

        for (int i = 0; i < total; i++)
        {
            SpawnOne(allTypes[i]); // �㻹��Ҫʵ�� SpawnOne ����
        }
    }

    // ��������б�
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    // ����һ���ˣ������ͷ���������Ŀ���
    // ����һ���ˣ������ͷ���������Ŀ��㣬������·����ɫ
    void SpawnOne(PersonType type)
    {
        // ���� 1. ���ѡ������ & ��Ӧ��ѡ·�� ���� 
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawnPoint = spawnPoints[spawnIndex];
        int[] possibleRoutes = spawnPointRouteMap[spawnIndex];
        int randomRouteIndex = possibleRoutes[Random.Range(0, possibleRoutes.Length)];

        // ���� 2. ���ѡȡ���� ���� 
        //    ������һ���ֵ� spawnExitMap��key=spawnIndex, value=��ȥ�� exitTargets ��������
        int[] possibleExits = spawnExitMap[spawnIndex];
        int exitChoice = possibleExits[Random.Range(0, possibleExits.Length)];
        Transform chosenExit = exitTargets[exitChoice];


        // ���� 3. �õ�·���б��������ټӳ��ڣ� ���� 
        var routeManager = FindObjectOfType<RouteManager>();
        var selectedRoute = new List<Transform>(routeManager.GetRouteByIndex(randomRouteIndex));

        // ���� 4. ������� & ����λ�� ���� 
        GetRandomAttributes(type, out float height, out float shoulderWidth, out Color color);
        Vector3 rawPos = selectedSpawnPoint.position + new Vector3(
            Random.Range(-spawnRangeX, spawnRangeX), 0, Random.Range(-spawnRangeZ, spawnRangeZ)
        );
        if (!NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            Debug.LogWarning("�Ҳ����Ϸ����ɵ㣡");
            return;
        }

        // ���� 5. ʵ���� & �������� ���� 
        var person = Instantiate(personPrefab, hit.position, Quaternion.identity);
        person.tag = "Player";
        person.transform.localScale = new Vector3(shoulderWidth, height / 2f, shoulderWidth);
        var agent = person.GetComponent<NavMeshAgent>();
        agent.speed = float.TryParse(speedInput.text, out var us) && us > 0 ? us : defaultSpeed;
        agent.radius = shoulderWidth * 1.2f + 0.1f;
        agent.stoppingDistance = 0.1f;
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // ���� 6. ���ظ���ű� & ��ֵ exitTarget ���� 
        var follower = person.AddComponent<PersonRouteFollower>();
        follower.routeId = randomRouteIndex;
        follower.InitRoute(selectedRoute);
        follower.exitTarget = chosenExit;          // �� ���︳��ͬ�ĳ���

        // ���� 7. ����ͬ·����ɫ����ѡ�� ���� 
        if (routeColors != null && randomRouteIndex < routeColors.Length)
        {
            var rend = person.GetComponent<Renderer>();
            if (rend != null) rend.material.color = routeColors[randomRouteIndex];
        }

        // ���� 8. ͳ���������� ���� 
        totalSpawnedCount++;
        spawnedCountText.text = $"Number of people: {totalSpawnedCount}";
        FindObjectOfType<UIManager>().RegisterSpawn();

    }
    public void RegisterSpawn()
    {
        if (!timingStarted)
        {
            timingStarted = true;
            startTime = Time.time;
        }
    }
    public void RegisterEvacuation(int evacuatedCount, int totalSpawnedCount)
    {
        if (!timingStarted) return;

        if (evacuatedCount >= totalSpawnedCount)
        {
            float elapsed = Time.time - startTime;
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            if (elapsedTimeText != null)
                elapsedTimeText.text = $"��{minutes:D2}:{seconds:D2}";
            timingStarted = false;
        }
    }



}