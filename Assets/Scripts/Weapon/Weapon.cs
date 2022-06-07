using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponType : ushort
{
    Pistol,
    Rifle,
    SniperRifle,
}

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public string WeaponName;
    public WeaponType type;
    public int Damage;
    public float RateOfFire;
    public float ReloadTime;
    public uint MagCapacity;
    public float Range;
}
