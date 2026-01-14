using UnityEngine;

public class DifferentialWheelController : MonoBehaviour
{
    [Header("Wheels (0: Left, 1: Right)")]
    public Transform[] wheels; 
    public float wheelRadius = 0.3f; 
    public float maxRollSpeed = 500f;
    public float lerpSpeed = 5f;

    [Header("Turn Indicators (Renderer)")]
    public Renderer leftPillar;  // 왼쪽 기둥의 Renderer
    public Renderer rightPillar; // 오른쪽 기둥의 Renderer
    public Color normalColor = Color.white;
    public Color turnColor = Color.yellow;

    private Vector3 _lastPosition;
    private float _lastRotationY;
    private float _leftCumulativeAngle = 0f;
    private float _rightCumulativeAngle = 0f;

    void Start()
    {
        _lastPosition = transform.position;
        _lastRotationY = transform.eulerAngles.y;

        // 시작할 때 기둥 색상을 기본색으로 초기화해
        if (leftPillar) leftPillar.material.color = normalColor;
        if (rightPillar) rightPillar.material.color = normalColor;
    }

    void Update()
    {
        HandleDifferentialDrive();
        HandleIndicators(); // 기둥 색상 제어 함수
        ApplyWheelRotations();

        _lastPosition = transform.position;
        _lastRotationY = transform.eulerAngles.y;
    }

    void HandleDifferentialDrive()
    {
        Vector3 displacement = transform.position - _lastPosition;
        float distance = displacement.magnitude;
        float direction = Vector3.Dot(displacement.normalized, transform.forward);
        float moveRoll = (direction >= 0 ? 1 : -1) * (distance / wheelRadius) * Mathf.Rad2Deg;

        float currentRotationY = transform.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(_lastRotationY, currentRotationY);
        
        float turnRoll = deltaYaw * 2.0f; 

        _leftCumulativeAngle += (moveRoll - turnRoll);
        _rightCumulativeAngle += (moveRoll + turnRoll);
    }

    // --- 기둥 색상 변경 로직 설명 ---
    void HandleIndicators()
    {
        // 기둥 오브젝트가 연결 안 되어 있으면 에러 방지를 위해 리턴해
        if (leftPillar == null || rightPillar == null) return;

        float currentRotationY = transform.eulerAngles.y;
        // 현재 프레임과 이전 프레임의 각도 차이를 계산해 (회전 방향 감지)
        float deltaYaw = Mathf.DeltaAngle(_lastRotationY, currentRotationY);

        // deltaYaw가 0보다 작으면 왼쪽으로 회전 중이라는 뜻이야
        if (deltaYaw < -0.05f) 
        {
            leftPillar.material.color = turnColor;   // 왼쪽은 노란색
            rightPillar.material.color = normalColor; // 오른쪽은 하얀색
        }
        // deltaYaw가 0보다 크면 오른쪽으로 회전 중이라는 뜻이야
        else if (deltaYaw > 0.05f) 
        {
            leftPillar.material.color = normalColor;
            rightPillar.material.color = turnColor;  // 오른쪽만 노란색
        }
        // 회전 각도 변화가 거의 없으면(직진/정지) 둘 다 하얀색으로 돌려놔
        else 
        {
            leftPillar.material.color = normalColor;
            rightPillar.material.color = normalColor;
        }
    }

    void ApplyWheelRotations()
    {
        if (wheels.Length < 2) return;
        if (wheels[0] != null) wheels[0].localRotation = Quaternion.Euler(_leftCumulativeAngle, 0f, 0f);
        if (wheels[1] != null) wheels[1].localRotation = Quaternion.Euler(_rightCumulativeAngle, 0f, 0f);
    }
}