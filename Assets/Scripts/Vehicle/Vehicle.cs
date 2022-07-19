using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField] public WheelCollider FrontLeft;
    [SerializeField] public WheelCollider FrontRight;
    [SerializeField] public WheelCollider BackLeft;
    [SerializeField] public WheelCollider BackRight;
    [SerializeField] public Transform campos;

    public uint id;
    public vehicle_data Vehicle_Data;
    private Player Driver;
    private Dictionary<ushort, Player> Occupants = new Dictionary<ushort, Player>();

    public void SetType(vehicle_data vehicle_Data, uint id)
    {
        Vehicle_Data = vehicle_Data;
        this.id = id;
    }

    public (bool driver, bool passenger) Enter(ushort clientid)
    {
        if (Driver == null)
        {
            Driver = PlayerManager.Singleton.Players[clientid];
            return (true, false);
        }

        else
        {
            if (Occupants.Count < Vehicle_Data.MaxOccupants && !Occupants.TryGetValue(clientid, out Player checkplayer))
            {
                Occupants[clientid] = PlayerManager.Singleton.Players[clientid];
                return (false, true);
            }
        }
        return (false, false);
    }

    public void Leave(ushort clientid)
    {
        Player player = PlayerManager.Singleton.Players[clientid];

        if (Driver == player)
        {
            if (Occupants.Count > 0)
            {
                // Getting occupant
                ushort firstkey = 0;
                foreach (ushort key in Occupants.Keys)
                {
                    firstkey = key;
                    break;
                }

                Driver = Occupants[firstkey];
                Occupants.Remove(firstkey);
            }
            else
            {
                Driver = null;
            }
        }
        
        else if(Occupants.TryGetValue(clientid, out Player checkplayer))
        {
            Occupants.Remove(clientid);
        }
    }

    public void Move(bool[] input)
    {

    }
}
