using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New mission", menuName = "Mission")]
public class Mission : ScriptableObject
{
    [SerializeField] public string MissionName;
    [SerializeField] public float Reward;
    [SerializeField] public GameObject MissionPrefab;
}
