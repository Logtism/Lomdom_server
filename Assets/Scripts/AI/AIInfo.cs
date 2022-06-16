using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ai", menuName = "AI")]
public class AIInfo : ScriptableObject
{
    [SerializeField] public string AIName;
    [SerializeField] public AIType Type;
    [SerializeField] public GameObject prefab;
}
