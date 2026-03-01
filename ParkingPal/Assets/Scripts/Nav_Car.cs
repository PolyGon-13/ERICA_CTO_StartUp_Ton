using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class NavMeshParkingAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public ParkingLotManager parkingManager;
    private NavMeshPath _path;

    public Transform targetSpot; 
    private Transform _robotSpot; 
    private List<Transform> _waypoints = new List<Transform>(); 
    private int _currentWaypointIndex = -1; 
    
    public bool isGuidingFinished = false; 
    private bool _isRobotArrivedAtSpot = false;

    public float arrivalThreshold = 0.5f; // 경유지 도착 판정 거리
    public float finalStopThreshold = 0.1f; // 최종 목적지 정밀 도착 판정
    public float alignSpeed = 1.5f; // 숫자를 낮출수록 더 천천히 회전해 (기존 3.0에서 하향)

    [Header("Trail Settings")]
    public List<Vector3> trailPoints = new List<Vector3>(); 
    public float recordDistance = 1.0f;                    
    private Vector3 _lastRecordedPos;

    void Awake() { _path = new NavMeshPath(); }
    void Start() { Invoke("StartParking", 0.5f); }
    void StartParking() { _lastRecordedPos = transform.position; FindAndGoToSpot(); }

    public Transform GetLastWaypoint() { return _waypoints.Count > 0 ? _waypoints.Last() : null; }

    public void FindAndGoToSpot()
    {
        if (parkingManager == null) return;
        targetSpot = FindBestEmptySpot();

        if (targetSpot != null)
        {
            _waypoints.Clear();
            isGuidingFinished = false;
            _isRobotArrivedAtSpot = false;
            trailPoints.Clear();

            int robotLayer = LayerMask.NameToLayer("Robot_Point");
            foreach (Transform child in targetSpot)
            {
                if (child.name.StartsWith("Entry")) _waypoints.Add(child);
                else if (child.gameObject.layer == robotLayer) _robotSpot = child;
            }

            _waypoints = _waypoints.OrderBy(w => {
                string[] split = w.name.Split('_');
                return split.Length > 1 ? int.Parse(split[1]) : 99;
            }).ToList();

            if (_waypoints.Count > 0)
            {
                _currentWaypointIndex = 0;
                agent.SetDestination(_waypoints[0].position);
                agent.updateRotation = true; 
            }
        }
    }

    void Update()
    {
        if (targetSpot == null) return;

        // 1. 자취 기록
        if (!isGuidingFinished && Vector3.Distance(transform.position, _lastRecordedPos) > recordDistance)
        {
            trailPoints.Add(transform.position);
            _lastRecordedPos = transform.position;
        }

        // 2. 경유지 안내 단계
        if (!isGuidingFinished && _currentWaypointIndex != -1)
        {
            if (!agent.pathPending && agent.remainingDistance <= arrivalThreshold)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex < _waypoints.Count)
                    agent.SetDestination(_waypoints[_currentWaypointIndex].position);
                else
                    GoToRobotWaitingSpot();
            }
        }

        // 3. 로봇 대기소 도착 후 부드러운 정렬 단계
        if (isGuidingFinished && _robotSpot != null)
        {
            // 아직 도착 판정 전이라면 거리를 체크
            if (!_isRobotArrivedAtSpot)
            {
                if (!agent.pathPending && agent.remainingDistance <= finalStopThreshold)
                {
                    _isRobotArrivedAtSpot = true;
                    agent.isStopped = true; // 물리적 이동 즉시 정지
                    agent.velocity = Vector3.zero; // 남은 관성 제거
                    agent.updateRotation = false; // NavMesh의 강제 회전 제어권을 뺏음
                }
            }
            // 완전히 멈춘 후부터 서서히 회전 시작
            else
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    _robotSpot.rotation, 
                    Time.deltaTime * alignSpeed // alignSpeed가 낮을수록 더 고급스럽게 돌아감
                );
            }
        }
    }

    private void GoToRobotWaitingSpot()
    {
        isGuidingFinished = true;
        if (_currentWaypointIndex == -1) _currentWaypointIndex = 0; 
        if (_robotSpot != null) agent.SetDestination(_robotSpot.position);
    }

    private Transform FindBestEmptySpot()
    {
        Transform best = null;
        float minLen = float.MaxValue;
        for (int i = 0; i < parkingManager.parkingSpots.Count; i++)
        {
            if (parkingManager.ledLights[i].color == parkingManager.emptyColor)
            {
                float len = GetPathLength(parkingManager.parkingSpots[i].position);
                if (len < minLen) { minLen = len; best = parkingManager.parkingSpots[i]; }
            }
        }
        return best;
    }

    float GetPathLength(Vector3 target)
    {
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, _path))
        {
            float dist = 0f;
            for (int i = 0; i < _path.corners.Length - 1; i++)
                dist += Vector3.Distance(_path.corners[i], _path.corners[i + 1]);
            return dist;
        }
        return float.MaxValue;
    }
}