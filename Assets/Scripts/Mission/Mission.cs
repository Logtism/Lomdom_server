using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New mission", menuName = "Mission")]
public class Mission : ScriptableObject
{
    public string MissionName;
    [SerializeField] public MissionStart MissionStartFunction = new MissionStart();
}
