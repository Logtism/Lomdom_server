using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Assassination : MonoBehaviour
{
    private static Assassination _singleton;
    public static Assassination Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(Assassination)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] private AiId Target;
    private int TargetAIId;

    private void Start()
    {
        (int? id, bool spawned) ai = AIManager.Singleton.SpawnPatrol(
            Target,
            SpawnPoints.Singleton.GetRandomSpawnPoint(SpawnPoints.Singleton.AssassinationSpawnPoints),
            SpawnPoints.Singleton.GetRandomPatrolRoute(SpawnPoints.Singleton.AssassinationPatrolRoutes),
            true
        );
        if (ai.id != null)
        {
            TargetAIId = (int)ai.id;
            KillEvent killEvent = new KillEvent();
            killEvent.AddListener(AIKillEventHandler);
            AIManager.Singleton.GetAI((int)ai.id).killEvent = killEvent;
        }
    }

    private void AIKillEventHandler()
    {
        AIManager.Singleton.KillAI(TargetAIId);
        MissionManager.Singleton.EndMission();
        Destroy(gameObject);
    }
}
