using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class RobberyManager : MonoBehaviour
{
    public bool robberyOpen = true;
    public bool playerCanDoRobbery;
    [SerializeField] private float robberyCooldownTime;

    private void Awake()
    {
        if (this.tag == "RobberyManager")
        {
            playerCanDoRobbery = false;
            robberyOpen = true;
        }
        else
        {
            playerCanDoRobbery = false;
            robberyOpen = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.tag == "RobberyManager" && robberyOpen)
        {
            if (Robbery.Singleton.robberyPhase == 1 && other.GetComponent<Player>().ClientID == Robbery.Singleton.clientID)
            {
                ushort clientID = other.GetComponent<Player>().ClientID;
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.playerEnter_RobberyTrigger);
                NetworkManager.Singleton.Server.Send(message, clientID);
            }
        }

        if (this.tag == "RobberyManager_EndRobbery" && robberyOpen)
        {
            if (Robbery.Singleton.robberyPhase == 2 && other.GetComponent<Player>().ClientID == Robbery.Singleton.clientID)
            {
                Robbery.Singleton.completeRobbery();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (this.tag == "RobberyManager" && robberyOpen)
        {
            if (Robbery.Singleton.robberyPhase == 1 && other.GetComponent<Player>().ClientID == Robbery.Singleton.clientID)
            {
                ushort clientID = other.GetComponent<Player>().ClientID;
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.playerExit_RobberyTrigger);
                NetworkManager.Singleton.Server.Send(message, clientID);
            }
        }
    }

    public IEnumerator runRobberyCooldown()
    {
        robberyOpen = false;
        yield return new WaitForSeconds(robberyCooldownTime);
        robberyOpen = true;
    }
}
