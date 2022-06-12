using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class WeaponManger : MonoBehaviour
{
    private static WeaponManger _singleton;
    public static WeaponManger Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(WeaponManger)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [SerializeField] public Weapon[] weapons;
    [SerializeField] public LayerMask ShootableMask;

    [MessageHandler((ushort)Messages.CTS.weapon_switch)]
    private static void WeaponSwitch(ushort fromClientID, Message message)
    {
        Player player = PlayerManager.Singleton.Players[fromClientID];
        player.SwitchWeapon(message.GetInt());
    }

    [MessageHandler((ushort)Messages.CTS.weapon_reload)]
    private static void WeaponReload(ushort fromClientID, Message message)
    {
        Player player = PlayerManager.Singleton.Players[fromClientID];
        player.Reload();
    }

    [MessageHandler((ushort)Messages.CTS.attack)]
    private static void Attack(ushort fromClientID, Message message)
    {
        Vector3 rotation = message.GetVector3();
        PlayerManager.Singleton.Players[fromClientID].Attack(rotation);
    }
}
