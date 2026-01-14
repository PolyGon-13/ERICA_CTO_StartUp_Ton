using UnityEngine;

public class CompleteWheelController : MonoBehaviour
{
    public Transform[] wheels;           
    public float maxRollSpeed = 500f;    
    public float maxSteerAngle = 30f;    
    public float lerpSpeed = 5f;         

    public float stopThreshold = 1.0f;
    private float _moveEpsilon = 0.00001f;

    private Vector3 _lastPosition;
    private float _lastRotationY;
    
    private float _currentRollSpeed = 0f;
    private float _targetRollSpeed = 0f;
    private float _cumulativeRollAngle = 0f;

    private float _currentSteerAngle = 0f;
    private float _targetSteerAngle = 0f;

    private float _stopTimer = 0f;

    void Start()
    {
        _lastPosition = transform.position;
        _lastRotationY = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleRolling();   
        HandleSteering();  
        ApplyWheelRotations(); 

        _lastPosition = transform.position;
        _lastRotationY = transform.eulerAngles.y;
    }

    void HandleRolling()
    {
        Vector3 displacement = transform.position - _lastPosition;
        float sqrDistance = displacement.sqrMagnitude;

        if (sqrDistance > _moveEpsilon)
        {
            _stopTimer = 0f;
            float direction = Vector3.Dot(displacement.normalized, transform.forward);
            _targetRollSpeed = (direction > 0) ? maxRollSpeed : -maxRollSpeed;
        }
        else
        {
            _stopTimer += Time.deltaTime;
            if (_stopTimer >= stopThreshold) _targetRollSpeed = 0f;
        }

        _currentRollSpeed = Mathf.Lerp(_currentRollSpeed, _targetRollSpeed, Time.deltaTime * lerpSpeed);
        _cumulativeRollAngle += _currentRollSpeed * Time.deltaTime;
    }

    void HandleSteering()
    {
        float currentRotationY = transform.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(_lastRotationY, currentRotationY);

        if (Mathf.Abs(deltaYaw) > 0.01f)
        {
            _targetSteerAngle = (deltaYaw > 0) ? maxSteerAngle : -maxSteerAngle;
        }
        else
        {
            _targetSteerAngle = 0f;
        }

        _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, _targetSteerAngle, Time.deltaTime * lerpSpeed);
    }

    void ApplyWheelRotations()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null) continue;

            float xRot = _cumulativeRollAngle;
            float yRot = 0f;

            if (i == 0 || i == 2)
            {
                yRot = _currentSteerAngle;
            }

            wheels[i].localRotation = Quaternion.Euler(xRot, yRot, 0f);
        }
    }
}