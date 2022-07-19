using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Getpos : MonoBehaviour
{
    public void Handle(string[] args, Player sender)
    {
        if (args.Length >= 1)
        {
            foreach (KeyValuePair<ushort, Player> item in PlayerManager.Singleton.Players)
            {
                if (item.Value.Username == args[0])
                {
                    Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                    reply.AddString($"{item.Value.Username} is at {item.Value.gameObject.transform.position.x} {item.Value.gameObject.transform.position.y} {item.Value.gameObject.transform.position.z}");
                    NetworkManager.Singleton.Server.Send(reply, sender.ClientID);
                    return;
                }
            }
            Message reply_fail = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
            reply_fail.AddString($"Could not find username {args[0]}");
            NetworkManager.Singleton.Server.Send(reply_fail, sender.ClientID);
        }
        else
        {
            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
            reply.AddString($"{sender.Username} is at {sender.gameObject.transform.position.x} {sender.gameObject.transform.position.y} {sender.gameObject.transform.position.z}");
            NetworkManager.Singleton.Server.Send(reply, sender.ClientID);
        }
    }
}
