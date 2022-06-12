using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RiptideNetworking;

[System.Serializable]
public class MissionStart : UnityEvent { }
[System.Serializable]
public class MissionEnd : UnityEvent { }

public class MissionManager : MonoBehaviour
{
    private static MissionManager _singleton;
    public static MissionManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(MissionManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] private Mission[] Missions;
    private bool MissionActive;

    public bool StartMission(int mission_id)
    {   
        if (!MissionActive)
        {
            MissionActive = true;
            Missions[mission_id].MissionStartFunction.Invoke();
            return true;
        }
        else
        {
            return false;
        }
    }

    [MessageHandler((ushort)Messages.CTS.start_mission)]
    private static void StartMission(ushort fromClientID, Message message)
    {
        int mission_id = message.GetInt();
        if (Singleton.StartMission(mission_id))
        {
            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.mission_started);
            reply.AddInt(mission_id);
            NetworkManager.Singleton.Server.SendToAll(reply);
        }
        else
        {
            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.mission_already_active);
            NetworkManager.Singleton.Server.Send(reply, fromClientID);
        }

    }
}