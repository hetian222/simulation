using UnityEngine;

/// <summary>
/// ����Ҽ���ק��������� target ��ת��
/// ��������ק�������ƽ�ƣ�
/// �������ž���  
/// ���� Main Camera �ϣ�Inspector ��� target �ϵ�����Χ�Ƶ����壨�糡�������һ�������壩
/// </summary>
public class MouseOrbitCamera : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target;            // ���������ת
    public float distance = 10f;        // ��ʼ����

    [Header("��ת�ٶ�")]
    public float xSpeed = 120f;
    public float ySpeed = 120f;

    [Header("��ת�Ƕȷ�Χ")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    [Header("���ŷ�Χ")]
    public float distanceMin = 5f;
    public float distanceMax = 20f;

    [Header("ƽ���ٶ�")]
    public float panSpeed = 0.5f;       // �����קʱ��ƽ��������

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

        // 1. �Ҽ���������ת
        if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // 2. �����ƽ��
        if (Input.GetMouseButton(0))
        {
            // ������������� XY ƽ��ƽ��
            float dx = -Input.GetAxis("Mouse X") * panSpeed;
            float dy = -Input.GetAxis("Mouse Y") * panSpeed;
            // ������ transform.right��transform.up ��ƽ��
            Vector3 move = transform.right * dx + transform.up * dy;
            target.position += move;
        }

        // 3. ���֣�����
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * 5f, distanceMin, distanceMax);

        // 4. Ӧ�ü�����
        Quaternion rot = Quaternion.Euler(y, x, 0);
        Vector3 pos = rot * new Vector3(0f, 0f, -distance) + target.position;

        transform.rotation = rot;
        transform.position = pos;
    }
}
