using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class DespawnVehicle : MonoBehaviour
{
    public void Handle(string[] args, Player sender)
    {
        uint vehicleid;
        if (args.Length >= 1)
        {
            try
            {
                vehicleid = uint.Parse(args[0]);
                Vehicle vehicle = VehicleManager.Singleton.vehicles[vehicleid];
            }
            catch (System.FormatException)
            {
                Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                reply.AddString("vehicleid not formated correctly.");
                NetworkManager.Singleton.Server.Send(reply, sender.ClientID);
                return;
            }
            catch (KeyNotFoundException)
            {
                Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                reply.AddString("vehicleid does not exist.");
                NetworkManager.Singleton.Server.Send(reply, sender.ClientID);
                return;
            }
        }
        else
        {
            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
            reply.AddString("No vehicleid supplied.");
            NetworkManager.Singleton.Server.Send(reply, sender.ClientID);
            return;
        }
        VehicleManager.Singleton.DeSpawnVehicle(vehicleid);
        Message success_reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
        success_reply.AddString($"vehicle {vehicleid} has been despawned.");
        NetworkManager.Singleton.Server.Send(success_reply, sender.ClientID);
    }
}
