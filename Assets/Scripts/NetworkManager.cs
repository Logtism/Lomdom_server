using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Settings
{
    public ushort Port;
    public ushort MaxClientCount;
    public string Api_url;
    public string ServerID;
    public string ServerToken;
}


public class Messages
{
    // Server to client id's
    public enum STC : ushort
    {
        auth_fail = 1,
        auth_malformed,
        auth_success,
        create_player,
        testing_r,
        testing_ur
    }

    // Client to sever id's
    public enum CTS : ushort
    {
        auth_attempt = 1,
        openworldloaded,
    }
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    public Server Server { get; private set; }
    public Settings Settings { get; private set; }

    private void Start()
    {
        // Setting up riptide logging
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        // Loading settings from 'config.json' file
        Settings = JsonUtility.FromJson<Settings>(File.ReadAllText(Application.dataPath + "/config.json"));
        // Create the server object
        Server = new Server();
        Server.ClientConnected += PlayerConnected;
        Server.ClientDisconnected += PlayerDisconnected;
        // Starting the server
        Server.Start(Settings.Port, Settings.MaxClientCount);
        // Setting the server's status so that players can join
        SetServerStatus(true);
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void SetServerStatus(bool status)
    {
        (int status_code, JObject json_content) r = Request.Post($"{Settings.Api_url}/status/", $"'id': {Settings.ServerID}, 'token': '{Settings.ServerToken}', 'status': {status.ToString().ToLower()}");
        if (r.status_code != 200)
        {
            Debug.Log(r.status_code);
            Debug.Log(r.json_content);
            Application.Quit();
        }
    }

    private void UpdatePlayerCount(bool join)
    {
        bool leave = false;
        if (join == true)
        {
            leave = false;
        }
        else
        {
            leave = true;
        }
        // Need the .ToString.ToLower because if not when going form bool to string it will in capitalized that is not valid json
        (int status_code, JObject json_content) r = Request.Post($"{Settings.Api_url}/update_player_count/", $"'id': {Settings.ServerID}, 'token': '{Settings.ServerToken}', 'player_join': {join.ToString().ToLower()}, 'player_leave': {leave.ToString().ToLower()}");
        if (r.status_code != 200)
        {
            Debug.Log(r.status_code);
            Debug.Log(r.json_content);
        }
    }

    private void PlayerConnected(object sender, ServerClientConnectedEventArgs e)
    {
        UpdatePlayerCount(true);
    }

    private void PlayerDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        UpdatePlayerCount(false);
        PlayerManager.Singleton.RemovePlayer(e.Id);
    }

    private void OnApplicationQuit()
    {
        // Set the server status to offline so that players will noot be sent to join it
        SetServerStatus(false);
        // Stopping the game server
        Server.Stop();
    }
}
