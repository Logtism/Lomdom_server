using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class VehicleManager : MonoBehaviour
{
    private static VehicleManager _singleton;
    public static VehicleManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(VehicleManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    private uint LastUsedID = 0;
    public vehicle_data[] VehiclesTypes;
    public Dictionary<uint, Vehicle> vehicles = new Dictionary<uint, Vehicle>();

    public Vehicle GetVehicleFromID(uint vehicleid)
    {
        vehicles.TryGetValue(vehicleid, out Vehicle vehicle);
        return vehicle;
    }

    public void CanEnterVehicle(ushort clientid, uint vehicleid)
    {
        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.can_enter_vehicle);
        message.AddUInt(vehicleid);
        NetworkManager.Singleton.Server.Send(message, clientid);
    }

    public void CannotEnterVehicle(ushort clientid, uint vehicleid)
    {
        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.cannot_enter_vehicle);
        message.AddUInt(vehicleid);
        NetworkManager.Singleton.Server.Send(message, clientid);
    }

    public uint SpawnVehicle(int vehicletypeid, Vector3 position, Quaternion rotation)
    {
        vehicle_data vehicletype = VehiclesTypes[vehicletypeid];
        GameObject vehicle = Instantiate(vehicletype.Prefab, position, rotation);
        vehicle.GetComponent<Vehicle>().SetType(vehicletype, LastUsedID);
        vehicles[LastUsedID] = vehicle.GetComponent<Vehicle>();

        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.spawn_vehicle);
        message.AddInt(vehicletypeid);
        message.AddUInt(LastUsedID);
        message.AddVector3(position);
        message.AddQuaternion(rotation);
        NetworkManager.Singleton.Server.SendToAll(message);

        LastUsedID++;
        return LastUsedID - 1;
    }

    public void DeSpawnVehicle(uint vehicletypeid)
    {
        Vehicle vehicle = vehicles[vehicletypeid];
        vehicles.Remove(vehicletypeid);
        Destroy(vehicle.gameObject);
        Message message = Message.Create(MessageSendMode.reliable, Messages.STC.despawn_vehicle);
        message.AddUInt(vehicletypeid);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    [MessageHandler((ushort)Messages.CTS.enter_vehicle)]
    private static void EnterVehicle(ushort fromClientID, Message message)
    {
        Player player = PlayerManager.Singleton.Players[fromClientID];
        player.EnterVehicle();
    }

    [MessageHandler((ushort)Messages.CTS.leave_vehicle)]
    private static void LeaveVehicle(ushort fromClientID, Message message)
    {
        Player player = PlayerManager.Singleton.Players[fromClientID];
        player.LeaveVehicle();
    }
}
