using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Player : MonoBehaviour
{
    public ushort ClientID;
    public string Username;

    public PlayerMove playermove;

    private int Health;
    private int MaxHealth = 100;

    private Weapon ActiveWeapon;
    private float LastShotTimer;
    public uint CurrectAmmo;
    private bool Reloading = false;
    private float ReloadTimer;

    public void SetPlayerInfo(ushort ClientID, string Username)
    {
        this.ClientID = ClientID;
        this.Username = Username;
        gameObject.name = $"Player - {ClientID}:{Username}";
        playermove = gameObject.GetComponent<PlayerMove>();
    }

    private void Start()
    {
        Health = MaxHealth;
    }

    private void Update()
    {
        if (ActiveWeapon && LastShotTimer < ActiveWeapon.RateOfFire)
        {
            LastShotTimer += Time.deltaTime;
        }
        if (ActiveWeapon && Reloading)
        {
            ReloadTimer += Time.deltaTime;
            if (ReloadTimer >= ActiveWeapon.ReloadTime)
            {
                Reloading = false;
                ReloadTimer = 0f;
            }
        }
    }

    public void Damage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            PlayerManager.Singleton.DeSpawnPlayer(ClientID);
        }
    }

    public void SwitchWeapon(int weapon_id)
    {
        if (weapon_id == -1)
        {
            ActiveWeapon = null;
            CurrectAmmo = 0;
        }
        else
        {
            ActiveWeapon = WeaponManger.Singleton.weapons[weapon_id];
            CurrectAmmo = ActiveWeapon.MagCapacity;
        }
    }

    public void Reload()
    {
        if (ActiveWeapon)
        {
            Reloading = true;
        }
    }

    public void Attack(Vector3 rotation)
    {
        if (ActiveWeapon && !Reloading && CurrectAmmo > 0 && LastShotTimer >= ActiveWeapon.RateOfFire)
        {
            CurrectAmmo--;

            RaycastHit hit;
            // Debug.DrawRay(transform.position + rotation, rotation, Color.green, 100f); // Debug
            bool raycast = Physics.Raycast(transform.position + rotation, rotation, out hit, ActiveWeapon.Range, WeaponManger.Singleton.ShootableMask);

            if (raycast)
            {
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    Player hit_player = hit.collider.gameObject.GetComponent<Player>();
                    hit_player.Damage(ActiveWeapon.Damage);
                    // Update the health on the player that was damaged for ui.
                    Message message = Message.Create(MessageSendMode.reliable, Messages.STC.damage_player);
                    message.AddInt(hit_player.Health);
                    NetworkManager.Singleton.Server.Send(message, hit_player.ClientID);
                }
                else if (hit.collider.gameObject.CompareTag("AI"))
                {
                    // Damage ai
                    // Use the hit var to get the ai script on the ai (not added yet) and decrease the health.
                }

            }
        }
    }
}
