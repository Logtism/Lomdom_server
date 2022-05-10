using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class AuthManager : MonoBehaviour
{
    private static AuthManager _singleton;
    public static AuthManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(AuthManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [MessageHandler((ushort)Messages.CTS.auth_attempt)]
    private static void AuthAttempt(ushort fromClientID, Message message)
    {
        // Getting the username and password
        string[] msg_strings = message.GetStrings();

        string username = msg_strings[0];
        string auth_token = msg_strings[1];
        // Checking if the token is valid
        (int status_code, JObject json_content) r = Request.Post(
            NetworkManager.Singleton.Settings.Api_url + "/check_auth_token/",
            $"'id': {NetworkManager.Singleton.Settings.ServerID}, 'token': '{NetworkManager.Singleton.Settings.ServerToken}', 'username': '{username}', 'auth_token': '{auth_token}'"
        );
        if (r.status_code == 200)
        {
            if ((bool)r.json_content["valid_token"] == true)
            {
                // Tell the client authenticated and to load into the main scene
                Message msg = Message.Create(MessageSendMode.reliable, Messages.STC.auth_success);
                NetworkManager.Singleton.Server.Send(msg, fromClientID);

                Debug.Log($"Player {fromClientID}:{username} authenticated");
                // Create the player
                PlayerManager.Singleton.CreatePlayer(fromClientID, username);
            }
            else
            {
                Message msg = Message.Create(MessageSendMode.reliable, Messages.STC.auth_fail);
                NetworkManager.Singleton.Server.Send(msg, fromClientID);
            }

        }
        else if (r.status_code == 404)
        {
            Message msg = Message.Create(MessageSendMode.reliable, Messages.STC.auth_fail);
            NetworkManager.Singleton.Server.Send(msg, fromClientID);
        }
        else
        {
            Message msg = Message.Create(MessageSendMode.reliable, Messages.STC.auth_malformed);
            NetworkManager.Singleton.Server.Send(msg, fromClientID);
        }
    }
}
