using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public ushort ClientID;
    public string Username;
    public PlayerMove playermove;

    public void SetPlayerInfo(ushort ClientID, string Username)
    {
        this.ClientID = ClientID;
        this.Username = Username;
        gameObject.name = $"Player - {ClientID}:{Username}";
        playermove = gameObject.GetComponent<PlayerMove>();
    }
}
