using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class MissionMenuTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {   
        if (other.tag == "Player")
        {
            ushort clientID = other.GetComponent<Player>().ClientID;
            
            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.playerEnter_missionTrigger);
            NetworkManager.Singleton.Server.Send(message, clientID);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            ushort clientID = other.GetComponent<Player>().ClientID;

            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.playerExit_missionTrigger);
            NetworkManager.Singleton.Server.Send(message, clientID);
        }
    }
}
