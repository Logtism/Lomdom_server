using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;


public class KillEvent : UnityEvent { }

public class AI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;

    public int id { get; private set; }
    public AIInfo info { get; private set; }
    public AIMode ai_mode { get; private set; }

    private bool GoingToPoint = false;
    private Vector3 waypoint;

    private int destPoint = 0;
    private List<Vector3> PatrolPoints = new List<Vector3>();

    private int health = 100;

    public KillEvent killEvent;
    public void SetInfoWaypoint(int id, AIInfo info, Vector3 waypoint)
    {
        ai_mode = AIMode.waypoint;
        this.id = id;
        this.info = info;
        gameObject.name = $"{this.info.name}:{this.id}";
        this.waypoint = waypoint;
        GoToNextPoint();
    }

    public void SetInfoPatrol(int id, AIInfo info, List<Vector3> PatrolPoints)
    {
        ai_mode = AIMode.patrol;
        this.id = id;
        this.info = info;
        gameObject.name = $"{this.info.name}:{this.id}";
        this.PatrolPoints = PatrolPoints;
        GoToNextPoint();
    }

    public void GoToNextPoint()
    {
        if (ai_mode == AIMode.waypoint)
        {
            if (!GoingToPoint)
            {
                agent.destination = waypoint;
                GoingToPoint = true;
            }
            else
            {
                AIManager.Singleton.KillAI(id);
            }
        }
        else if (ai_mode == AIMode.patrol)
        {
            agent.destination = PatrolPoints[destPoint];
            destPoint = (destPoint + 1) % PatrolPoints.Count;
        }
    }

    private void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.25f)
            GoToNextPoint();
    }

    public void Damage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            if (killEvent != null)
            {
                killEvent.Invoke();
            }
            AIManager.Singleton.KillAI(id);
        }
    }
}
