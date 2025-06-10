using UnityEngine;

/// <summary>
/// 鼠标右键拖拽，摄像机绕 target 旋转；
/// 鼠标左键拖拽，摄像机平移；
/// 滚轮缩放距离  
/// 挂在 Main Camera 上，Inspector 里把 target 拖到你想围绕的物体（如场景中央的一个空物体）
/// </summary>
public class MouseOrbitCamera : MonoBehaviour
{
    [Header("环绕目标")]
    public Transform target;            // 摄像机绕它转
    public float distance = 10f;        // 初始距离

    [Header("旋转速度")]
    public float xSpeed = 120f;
    public float ySpeed = 120f;

    [Header("旋转角度范围")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("缩放范围")]
    public float distanceMin = 5f;
    public float distanceMax = 20f;

    [Header("平移速度")]
    public float panSpeed = 0.5f;       // 左键拖拽时的平移灵敏度

    float x = 0f;
    float y = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 右键：环绕旋转
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // 2. 左键：平移
        if (Input.GetMouseButton(0))
        {
            // 计算沿相机本地 XY 平面平移
            float dx = -Input.GetAxis("Mouse X") * panSpeed;
            float dy = -Input.GetAxis("Mouse Y") * panSpeed;
            // 这里用 transform.right、transform.up 来平移
            Vector3 move = transform.right * dx + transform.up * dy;
            target.position += move;
        }

        // 3. 滚轮：缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * 5f, distanceMin, distanceMax);

        // 4. 应用计算结果
        Quaternion rot = Quaternion.Euler(y, x, 0);
        Vector3 pos = rot * new Vector3(0f, 0f, -distance) + target.position;

        transform.rotation = rot;
        transform.position = pos;
    }
}
