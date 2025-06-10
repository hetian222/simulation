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
    public GameObject personPrefab;     // 圆柱体预制体，需附带 NavMeshAgent
    public Transform spawnPoint;        // 出生中心点
    public Transform[] exitTargets;

    public Transform targetPoint;       // 目标点，人物将朝这里移动
    public Transform[] targetExit; // 和 routeDropdown 一一对应
    public TMP_InputField speedInput;
    public float defaultSpeed = 5.0f; // 默认速度
    public TMP_InputField countInput;
    public int defaultCount = 1;
    public TMP_Dropdown spawnPointDropdown; //选择出生点入口
    public Transform[] spawnPoints; // 要在 Inspector 中手动拖入 A/B/C 这些 Transform
    public TMP_Dropdown targetPointDropdown;  // 新增：目标点下拉
    public Transform[] targetPoints;          // Inspector 拖入所有目标点
    public TextMeshProUGUI spawnedCountText;
    private int totalSpawnedCount = 0;
    //public TMP_Dropdown routeDropdown;//路线选择
    public Toggle[] spawnPointToggles; // 多选 Toggle 列表

    public float spawnRangeX = 5f;
    public float spawnRangeZ = 5f;

    public TMP_Dropdown personTypeDropdown; // 新增：人群类型选择
    public Color[] routeColors;//路线颜色
    [HideInInspector]

    public int TotalSpawnedCount => totalSpawnedCount;
    [Header("统计／计时")]
    public TextMeshProUGUI elapsedTimeText;    // 新增：在 Inspector 挂一个 TMP_Text
    private float startTime;
    private bool timingStarted = false;

    void Awake()
    {
        // 一开始文本留空
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
        // 1. 解析身高/肩宽/速度/数量/类型（和你原来的一样）
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

        // 2. 读取下拉：出生点 和 目标点
        int si = Mathf.Clamp(spawnPointDropdown.value, 0, spawnPoints.Length - 1);
        Transform sp = spawnPoints[si];

        int ti = Mathf.Clamp(targetPointDropdown.value, 0, targetPoints.Length - 1);
        Transform tp = targetPoints[ti];

        // 3. 循环生成
        for (int i = 0; i < count; i++)
        {
            // 3.1 在 NavMesh 上随机一个出生位置
            Vector3 raw = sp.position + new Vector3(
                Random.Range(-spawnRangeX, spawnRangeX),
                0, Random.Range(-spawnRangeZ, spawnRangeZ));
            if (!NavMesh.SamplePosition(raw, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            // 3.2 实例化并设置外观
            GameObject person = Instantiate(personPrefab, hit.position, Quaternion.identity);
            person.transform.localScale = new Vector3(shoulder, height / 2f, shoulder);
            var rend = person.GetComponent<Renderer>();
            if (rend) rend.material.color = typeColor;
            person.tag = "Player";
            person.AddComponent<PersonStats>();

            // 3.3 配置 NavMeshAgent
            var agent = person.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = speed;
                agent.radius = shoulder * 0.6f + 0.1f;
                agent.stoppingDistance = 0.1f;
                agent.autoRepath = true;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.avoidancePriority = Random.Range(0, 100);
                // **直接给目标点下达目的地**
                agent.SetDestination(tp.position);
            }


            // 3.4 统计并启动计时
            totalSpawnedCount++;
            if (spawnedCountText != null)
                spawnedCountText.text = $"Number of people: {totalSpawnedCount}";
            RegisterSpawn();
        }
    }



    // 在 UIManager 类里面，加在其他方法之后即可
    public void CreateMixedPeople(int total)
    {
        int adults = Mathf.RoundToInt(total * 0.92f);
        int elders = Mathf.RoundToInt(total * 0.03f);
        int children = total - adults - elders;

        List<PersonType> allTypes = new List<PersonType>();
        allTypes.AddRange(Enumerable.Repeat(PersonType.Adult, adults));
        allTypes.AddRange(Enumerable.Repeat(PersonType.Elder, elders));
        allTypes.AddRange(Enumerable.Repeat(PersonType.Child, children));
        Shuffle(allTypes); // 你需要实现一个 Shuffle() 函数

        for (int i = 0; i < total; i++)
        {
            SpawnOne(allTypes[i]); // 你还需要实现 SpawnOne 方法
        }
    }

    // 随机打乱列表
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

    // 生成一个人，按类型分配出生点和目标点
    // 生成一个人，按类型分配出生点和目标点，并根据路线着色
    void SpawnOne(PersonType type)
    {
        // —— 1. 随机选出生点 & 对应可选路线 —— 
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawnPoint = spawnPoints[spawnIndex];
        int[] possibleRoutes = spawnPointRouteMap[spawnIndex];
        int randomRouteIndex = possibleRoutes[Random.Range(0, possibleRoutes.Length)];

        // —— 2. 随机选取出口 —— 
        //    这里用一个字典 spawnExitMap，key=spawnIndex, value=可去的 exitTargets 索引数组
        int[] possibleExits = spawnExitMap[spawnIndex];
        int exitChoice = possibleExits[Random.Range(0, possibleExits.Length)];
        Transform chosenExit = exitTargets[exitChoice];


        // —— 3. 拿到路点列表（不往里再加出口） —— 
        var routeManager = FindObjectOfType<RouteManager>();
        var selectedRoute = new List<Transform>(routeManager.GetRouteByIndex(randomRouteIndex));

        // —— 4. 随机属性 & 生成位置 —— 
        GetRandomAttributes(type, out float height, out float shoulderWidth, out Color color);
        Vector3 rawPos = selectedSpawnPoint.position + new Vector3(
            Random.Range(-spawnRangeX, spawnRangeX), 0, Random.Range(-spawnRangeZ, spawnRangeZ)
        );
        if (!NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            Debug.LogWarning("找不到合法生成点！");
            return;
        }

        // —— 5. 实例化 & 基础设置 —— 
        var person = Instantiate(personPrefab, hit.position, Quaternion.identity);
        person.tag = "Player";
        person.transform.localScale = new Vector3(shoulderWidth, height / 2f, shoulderWidth);
        var agent = person.GetComponent<NavMeshAgent>();
        agent.speed = float.TryParse(speedInput.text, out var us) && us > 0 ? us : defaultSpeed;
        agent.radius = shoulderWidth * 1.2f + 0.1f;
        agent.stoppingDistance = 0.1f;
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // —— 6. 挂载跟随脚本 & 赋值 exitTarget —— 
        var follower = person.AddComponent<PersonRouteFollower>();
        follower.routeId = randomRouteIndex;
        follower.InitRoute(selectedRoute);
        follower.exitTarget = chosenExit;          // ← 这里赋不同的出口

        // —— 7. 给不同路线着色（可选） —— 
        if (routeColors != null && randomRouteIndex < routeColors.Length)
        {
            var rend = person.GetComponent<Renderer>();
            if (rend != null) rend.material.color = routeColors[randomRouteIndex];
        }

        // —— 8. 统计生成人数 —— 
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
                elapsedTimeText.text = $"：{minutes:D2}:{seconds:D2}";
            timingStarted = false;
        }
    }



}