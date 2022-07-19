using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Player : MonoBehaviour
{
    public ushort ClientID;
    public string Username;

    public PlayerMove playermove;

    private int Health;
    private int MaxHealth = 100;

    public float Balance = 0f;
    public bool is_admin { get; private set; } = false;

    private Weapon ActiveWeapon;
    private float LastShotTimer;
    public uint CurrectAmmo;
    private bool Reloading = false;
    private float ReloadTimer;

    public bool InVehicle = false;
    public Vehicle vehicle;
    public bool vehicle_driver;
    private Vehicle CanEnterVehicle;

    public void SetPlayerInfo(ushort ClientID, string Username, float Balance, bool is_admin)
    {
        this.ClientID = ClientID;
        this.Username = Username;
        this.Balance = Balance;
        this.is_admin = is_admin;
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
                CurrectAmmo = ActiveWeapon.MagCapacity;
            }
        }
    }

    public void GiveMoney(float amount)
    {
        (int status_code, JObject json_content) r = Request.Post(
            $"{NetworkManager.Singleton.Settings.Api_url}/add_money/",
            $"'id': {NetworkManager.Singleton.Settings.ServerID}, 'token': '{NetworkManager.Singleton.Settings.ServerToken}', 'username': '{Username}', 'amount': {amount}"
        );

        if (r.status_code == 200)
        {
            Balance = (float)r.json_content["amount"];
            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.update_money);
            message.AddFloat(Balance);
            NetworkManager.Singleton.Server.Send(message, ClientID);
        }
        else
        {
            Debug.Log(r.status_code);
            Debug.Log(r.json_content);
        }
    }

    public void TakeMoney(float amount)
    {
        GiveMoney(-amount);
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
                    Message message = Message.Create(MessageSendMode.reliable, Messages.STC.update_health);
                    message.AddInt(hit_player.Health);
                    NetworkManager.Singleton.Server.Send(message, hit_player.ClientID);
                }
                else if (hit.collider.gameObject.CompareTag("AI"))
                {
                    // Damage ai
                    AI hit_ai = hit.collider.gameObject.GetComponent<AI>();
                    hit_ai.Damage(ActiveWeapon.Damage);
                }

            }
        }
    }

    public void EnterVehicle()
    {
        if (!InVehicle && CanEnterVehicle)
        {
            (bool driver, bool passenger) EnterVehicle = VehicleManager.Singleton.GetVehicleFromID(CanEnterVehicle.id).Enter(ClientID);
            if (EnterVehicle.driver)
            {
                InVehicle = true;
                vehicle = CanEnterVehicle;
                vehicle_driver = true;
                // Move player cam and change movement to vehicle
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.entered_vehicle_driver);
                message.AddUShort(ClientID);
                message.AddUInt(vehicle.id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
            else if (EnterVehicle.passenger)
            {
                InVehicle = true;
                vehicle = CanEnterVehicle;
                vehicle_driver = false;
                // Move player cam and disable movement
                Message message = Message.Create(MessageSendMode.reliable, Messages.STC.entered_vehicle_passenger);
                message.AddUShort(ClientID);
                message.AddUInt(vehicle.id);
                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }
    }

    public void LeaveVehicle()
    {
        if (InVehicle)
        {
            vehicle.Leave(ClientID);

            InVehicle = false;
            vehicle_driver = false;
            vehicle = null;

            Message message = Message.Create(MessageSendMode.reliable, Messages.STC.left_vehicle);
            message.AddUShort(ClientID);
            NetworkManager.Singleton.Server.SendToAll(message);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("VehicleEnterRange"))
        {
            CanEnterVehicle = other.gameObject.GetComponentInParent<Vehicle>();
            Debug.Log(CanEnterVehicle);
            VehicleManager.Singleton.CanEnterVehicle(ClientID, CanEnterVehicle.id);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("VehicleEnterRange"))
        {
            CanEnterVehicle = null;
            VehicleManager.Singleton.CannotEnterVehicle(ClientID, other.gameObject.GetComponentInParent<Vehicle>().id);
        }
    }
}
