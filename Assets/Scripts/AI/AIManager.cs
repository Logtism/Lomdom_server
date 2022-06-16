using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;


public enum AIType
{
    Civilian = 1,
    DrivingCivilian,
    Enemy,
    MissionCivilian,
    MissionEnemy,
}

public enum AIMode
{
    waypoint = 1,
    patrol,
}

public enum AiId : int
{
    test_civ = 0,
    Assassination_target,
}

public class AIManager : MonoBehaviour
{
    private static AIManager _singleton;
    public static AIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(AIManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] private uint MaxAmountOfAi;

    [SerializeField] private AIInfo[] Ais = new AIInfo[] { };

    // Used for if you want to spawn a random of any of the follow types.
    private List<int> Civilian = new List<int>();
    private List<int> DrivingCivilian = new List<int>();
    private List<int> Enemy = new List<int>();

    private int LastID = 0;
    private Dictionary<int, AI> SpawnedAIs = new Dictionary<int, AI>();
    private Dictionary<int, AI> SpawnedCivs = new Dictionary<int, AI>();

    private void Start()
    {
        for (int i = 0; i <= Ais.Length - 1; ++i)
        {
            if (Ais[i].Type == AIType.Civilian)
            {
                Civilian.Add(i);
            }
            else if (Ais[i].Type == AIType.DrivingCivilian)
            {
                DrivingCivilian.Add(i);
            }
            else if (Ais[i].Type == AIType.Enemy)
            {
                Enemy.Add(i);
            }
        }
    }

    public AI GetAI(int id)
    {
        return SpawnedAIs[id];
    }

    public (int? id, bool spawned) SpawnWaypointAI(AiId aiId, Vector3 SpawnPoint, Vector3 Waypoint, bool force)
    {
        if (MaxAmountOfAi > SpawnedAIs.Count || force)
        {
            AI ai = Instantiate(Ais[(int)aiId].prefab, SpawnPoint, Quaternion.identity).GetComponent<AI>();
            ai.SetInfoWaypoint(LastID, Ais[(int)aiId], Waypoint);

            SpawnedAIs[LastID] = ai;
            if (ai.info.Type == AIType.Civilian || ai.info.Type == AIType.DrivingCivilian)
            {
                SpawnedCivs[LastID] = ai;
            }

            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.spawn_waypoint_ai);
            message.AddInt((int)aiId);
            message.AddInt(LastID);
            message.AddVector3(SpawnPoint);
            message.AddVector3(Waypoint);
            NetworkManager.Singleton.Server.SendToAll(message);

            if (MaxAmountOfAi <= SpawnedAIs.Count && SpawnedCivs.Count > 0)
            {
                foreach (var item in SpawnedCivs)
                {
                    KillAI(item.Key);
                    break;
                }
            }
            LastID++;
            return (LastID - 1, true);
        }
        return (null, false);
    }

    public (int? id, bool spawned) SpawnPatrol(AiId aiId, Vector3 SpawnPoint, List<Vector3> Points, bool force)
    {
        if (MaxAmountOfAi > SpawnedAIs.Count || force)
        {
            AI ai = Instantiate(Ais[(int)aiId].prefab, SpawnPoint, Quaternion.identity).GetComponent<AI>();
            ai.SetInfoPatrol(LastID, Ais[(int)aiId], Points);

            SpawnedAIs[LastID] = ai;
            if (ai.info.Type == AIType.Civilian || ai.info.Type == AIType.DrivingCivilian)
            {
                SpawnedCivs[LastID] = ai;
            }

            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.spawn_waypoint_ai);
            message.AddInt((int)aiId);
            message.AddInt(LastID);
            message.AddVector3(SpawnPoint);
            message.AddVector3(Points[0]);
            NetworkManager.Singleton.Server.SendToAll(message);

            if (MaxAmountOfAi <= SpawnedAIs.Count && SpawnedCivs.Count > 0)
            {
                foreach (var item in SpawnedCivs)
                {
                    KillAI(item.Key);
                    break;
                }
            }
            LastID++;
            return (LastID - 1, true);
        }
        return (null, false);
    }
    public void KillAI(int id)
    {
        AI ai;
        SpawnedAIs.TryGetValue(id, out ai);
        if (ai)
        {
            Destroy(ai.gameObject);
            SpawnedAIs.Remove(id);
            if (ai.info.Type == AIType.Civilian || ai.info.Type == AIType.DrivingCivilian)
            {
                SpawnedCivs.Remove(id);
            }
            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.kill_ai);
            message.AddInt(id);
            NetworkManager.Singleton.Server.SendToAll(message);
        }
    }

    private void Update()
    {
        foreach(var item in SpawnedAIs)
        {
            Message message = Message.Create(MessageSendMode.unreliable, Messages.STC.sync_ai_pos);
            message.AddInt(item.Key);
            message.AddUInt(NetworkManager.Singleton.CurrentTick);
            message.AddVector3(item.Value.gameObject.transform.position);
            NetworkManager.Singleton.Server.SendToAll(message);
        }
    }
}
