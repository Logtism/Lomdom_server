using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public enum Type : ushort
    {
        Pistol = 1,
        Rifle,
        Sniper_Rifle,
    }

    public string name;
    public string description;
    public int damage_head;
    public int damage_body;
    public Type type;

    public Weapon(string name, string description, int damage_head, int damage_body, Type type)
    {
        this.name = name;
        this.description = description;
        this.damage_head = damage_head;
        this.damage_body = damage_body;
        this.type = type;
    }
}


public class WeaponManager : MonoBehaviour
{
    public Dictionary<int, Weapon> Weapons { get; private set; } = new Dictionary<int, Weapon>()
    {
        { 1, new Weapon("Glock-18", "A useful concealed weapon at close quarter combat. in the streets of south London", 50, 25, Weapon.Type.Pistol) },
        { 2, new Weapon("M16", "is an all-rounder American made weapon that is suited to any environment that it faces", 85, 30, Weapon.Type.Rifle) },
        { 3, new Weapon("AWP", "A British Made Sniper rifle that is extremely when used by a skilful operator", 100, 55, Weapon.Type.Sniper_Rifle) },
    };
}
