using UnityEngine;
using UnityEngine.AI;

public class follow : MonoBehaviour
{
    public NavMeshAgent agent;
    public NavMeshParkingAI leader; 
    public float stopDistanceToLeader = 3.0f; 
    public float alignSpeed = 3.0f;
    
    private enum FollowState { FollowingTrail, HeadingToLastEntry, FinalParking }
    private FollowState _currentState = FollowState.FollowingTrail;

    void Update()
    {
        if (leader == null) return;

        switch (_currentState)
        {
            case FollowState.FollowingTrail:
                HandleTrailFollowing();
                break;

            case FollowState.HeadingToLastEntry:
                HandleLastEntryMovement();
                break;

            case FollowState.FinalParking:
                HandleFinalParking();
                break;
        }

        // 리더와의 거리 유지 (최종 주차 단계가 아닐 때만 적용)
        if (_currentState != FollowState.FinalParking)
        {
            float distToLeader = Vector3.Distance(transform.position, leader.transform.position);
            // 리더와 너무 가까우면 멈추고, 아니면 움직임
            agent.isStopped = (distToLeader < stopDistanceToLeader);
        }
    }

    private void HandleTrailFollowing()
    {
        if (leader.trailPoints.Count > 0)
        {
            agent.SetDestination(leader.trailPoints[0]);
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                leader.trailPoints.RemoveAt(0);
        }
        else if (leader.isGuidingFinished)
        {
            _currentState = FollowState.HeadingToLastEntry;
            Transform lastEntry = leader.GetLastWaypoint();
            if (lastEntry != null) agent.SetDestination(lastEntry.position);
            else StartFinalParking(); // 경유지가 없으면 바로 최종 주차
        }
    }

    private void HandleLastEntryMovement()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
            StartFinalParking();
    }

    private void StartFinalParking()
    {
        _currentState = FollowState.FinalParking;
        agent.isStopped = false; // 멈춤 상태 해제
        agent.SetDestination(leader.targetSpot.position);
        agent.updateRotation = false; // 직접 정렬을 위해 끔
        agent.stoppingDistance = 0.05f; // 목적지에 딱 붙도록 설정
    }

    private void HandleFinalParking()
    {
        // 목적지를 향해 이동하면서 정렬 수행
        transform.rotation = Quaternion.Slerp(transform.rotation, leader.targetSpot.rotation, Time.deltaTime * alignSpeed);
        
        // 도착 완료 시 에이전트 비활성화 등 추가 로직 가능
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("주차가 완료되었습니다.");
            // 필요한 경우 여기서 agent.enabled = false; 처리
        }
    }
}