using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RiptideNetworking;

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
    public bool MissionActive;
    public Player StartedBy;

    public bool StartMission(int mission_id)
    {   
        if (!MissionActive)
        {
            MissionActive = true;
            Instantiate(Missions[mission_id].MissionPrefab);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void EndMission()
    {
        MissionActive = false;
        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.mission_ended);
        NetworkManager.Singleton.Server.SendToAll(message);
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
            Singleton.StartedBy = PlayerManager.Singleton.Players[fromClientID];
        }
        else
        {
            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.mission_already_active);
            NetworkManager.Singleton.Server.Send(reply, fromClientID);
        }

    }

    public void sendWaypoint(Vector3 waypointPosition, ushort clientID)
    {
        Message message = Message.Create(MessageSendMode.unreliable, Messages.STC.waypointUpdate);
        message.AddVector3(waypointPosition);
        NetworkManager.Singleton.Server.Send(message, clientID);
    }
}
