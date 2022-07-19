using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RiptideNetworking;

public class Chat : MonoBehaviour
{
    private static Chat _singleton;
    public static Chat Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(Chat)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] private Command[] commands;

    private Command GetCommand(string command_name)
    {
        foreach (Command command in Singleton.commands)
        {
            if ("/" + command.command_name == command_name)
            {
                return command;
            }
        }
        return null;
    }

    [MessageHandler((ushort)Messages.CTS.send_chat_msg)]
    private static void SendChatMsg(ushort fromClientID, Message message)
    {
        string msg_text = message.GetString();
        if (msg_text.Length > 0)
        {
            if (msg_text[0] == '/')
            {
                Player player = PlayerManager.Singleton.Players[fromClientID];
                if (player.is_admin)
                {
                    if (msg_text.Split(' ').Length > 1)
                    {
                        List<string> args = new List<string>();
                        string[] args_with_command = msg_text.Split(' ');
                        for (int i = 1; i < args_with_command.Length; i++)
                        {
                            args.Add(args_with_command[i]);
                        }

                        Command command = Singleton.GetCommand(msg_text.Split(' ')[0]);
                        if (command != null)
                        {
                            Debug.Log(args);
                            command.handler.Invoke(args.ToArray(), player);
                        }
                        else
                        {
                            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                            reply.AddString("Could not find command.");
                            NetworkManager.Singleton.Server.Send(reply, fromClientID);
                        }
                    }
                    else
                    {
                        Command command = Singleton.GetCommand(msg_text);
                        if (command != null)
                        {
                            command.handler.Invoke(new string[0], PlayerManager.Singleton.Players[fromClientID]);
                        }
                        else
                        {
                            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                            reply.AddString("Could not find command.");
                            NetworkManager.Singleton.Server.Send(reply, fromClientID);
                        }
                    }
                }
                else
                {
                    Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                    reply.AddString("You do not have permission to do that.");
                    NetworkManager.Singleton.Server.Send(reply, fromClientID);
                }
            }

            else
            {
                Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                reply.AddString(PlayerManager.Singleton.Players[fromClientID].Username + ": " + msg_text);
                NetworkManager.Singleton.Server.SendToAll(reply);
            }
        }
    }
}
