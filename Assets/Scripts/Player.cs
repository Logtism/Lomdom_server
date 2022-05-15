using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Player : MonoBehaviour
{
    public ushort ClientID;
    public string Username;
    public int health { get; private set; }
    public int maxhealth { get; private set; } = 100;

    public int Weapon;
    public PlayerMove playermove;

    public void SetPlayerInfo(ushort ClientID, string Username)
    {
        this.ClientID = ClientID;
        this.Username = Username;
        gameObject.name = $"Player - {ClientID}:{Username}";
        playermove = gameObject.GetComponent<PlayerMove>();
    }

    public void Damage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        PlayerManager.Singleton.DeSpawnPlayer(ClientID);
        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.player_died);
        message.AddString(Username);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void Respawn()
    {
        PlayerManager.Singleton.ReSpawnPlayer(ClientID);
        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.player_respawn);
        message.AddString(Username);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
