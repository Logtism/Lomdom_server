using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Vehicle", menuName = "Vehicle")]
public class vehicle_data : ScriptableObject
{
    [SerializeField] public string vehicle_name;
    [SerializeField] public GameObject Prefab;
    // Occupants do not include driver
    [SerializeField] public ushort MaxOccupants;
    [SerializeField] public bool FourWheelDrive;
    [SerializeField] public float Acceleration;
    [SerializeField] public float BrakingForce;
    [SerializeField] public float MaxTurnAngle;
}
