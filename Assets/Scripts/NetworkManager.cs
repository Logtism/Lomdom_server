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
        sync_tick = 1,

        auth_fail,
        auth_malformed,
        auth_success,

        create_player,
        remove_player,

        playermove,

        update_health,
        update_money,

        kill_player,
        respawn_player,

        damage_ai,
        sync_ai_pos,
        spawn_waypoint_ai,
        spawn_patrol_ai,
        kill_ai,

        weapon_switch,
        weapon_reload,

        mission_started,
        mission_ended,
        mission_already_active,

        playerEnter_missionTrigger,
        playerExit_missionTrigger,

        playerEnter_RobberyTrigger,
        playerExit_RobberyTrigger,

        waypointUpdate,

        spawn_vehicle,
        despawn_vehicle,

        can_enter_vehicle,
        cannot_enter_vehicle,

        entered_vehicle_driver,
        entered_vehicle_passenger,
        left_vehicle,

        vehicle_move,

        send_chat_msg,
    }

    // Client to sever id's
    public enum CTS : ushort
    {
        auth_attempt = 1,
        openworldloaded,
        local_player_created,

        inputs,

        attack,
        weapon_switch,
        weapon_reload,

        start_mission,

        completeRobbery_input,

        playerClick_Respawn,

        enter_vehicle,
        leave_vehicle,

        send_chat_msg,
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
    public uint CurrentTick { get; private set; } = 0;
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

        if (CurrentTick % 200 == 0)
        {
            SendSync();
        }

        CurrentTick++;
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

    private void SendSync()
    {
        Message message = Message.Create(MessageSendMode.unreliable, Messages.STC.sync_tick);
        message.AddUInt(CurrentTick);
        Server.SendToAll(message);
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
