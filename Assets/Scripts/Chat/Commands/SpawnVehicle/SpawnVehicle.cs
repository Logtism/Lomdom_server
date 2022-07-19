using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class SpawnVehicle : MonoBehaviour
{
    public void Handle(string[] args, Player sender)
    {
        if (args.Length >= 4)
        {
            int vehicleid;

            try
            {
                vehicleid = int.Parse(args[0]);
                vehicle_data vehicle = VehicleManager.Singleton.VehiclesTypes[vehicleid];
            }
            catch (System.ArgumentNullException)
            {
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                message.AddString("Vehicleid not a int.");
                NetworkManager.Singleton.Server.Send(message, sender.ClientID);
                return;
            }
            catch (System.IndexOutOfRangeException)
            {
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                message.AddString("Vehicleid is not a valid id.");
                NetworkManager.Singleton.Server.Send(message, sender.ClientID);
                return;
            }

            int posx;
            int posy;
            int posz;
            Vector3 pos;

            try
            {
                posx = int.Parse(args[1]);
                posy = int.Parse(args[2]);
                posz = int.Parse(args[3]);
                pos = new Vector3(posx, posy, posz);
            }
            catch (System.ArgumentNullException)
            {
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                message.AddString("Position is not valid.");
                NetworkManager.Singleton.Server.Send(message, sender.ClientID);
                return;
            }

            Quaternion rotation;

            if (args.Length > 4)
            {
                int rotx;
                int roty;
                int rotz;

                try
                {
                    rotx = int.Parse(args[4]);
                    roty = int.Parse(args[5]);
                    rotz = int.Parse(args[6]);
                    rotation = Quaternion.Euler(new Vector3(rotx, roty, rotz));
                }
                catch (System.ArgumentNullException)
                {
                    Message message = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                    message.AddString("Rotation is not valid.");
                    NetworkManager.Singleton.Server.Send(message, sender.ClientID);
                    return;
                }
                catch (System.FormatException)
                {
                    Message message = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
                    message.AddString("formant is invalid");
                    NetworkManager.Singleton.Server.Send(message, sender.ClientID);
                    return;
                }
            }

            else
            {
                rotation = Quaternion.identity;
            }

            uint vehiclesid = VehicleManager.Singleton.SpawnVehicle(vehicleid, pos, rotation);
            Message reply = Message.Create(MessageSendMode.reliable, Messages.STC.send_chat_msg);
            reply.AddString($"Spawned vehicle {VehicleManager.Singleton.VehiclesTypes[vehicleid].vehicle_name} at {posx} {posy} {posz} id:{vehiclesid}");
            NetworkManager.Singleton.Server.Send(reply, sender.ClientID);
        }
    }
}
