using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CommandHandlerEvent : UnityEvent<string[], Player>
{

}

[CreateAssetMenu(fileName = "New Command", menuName = "Command")]
public class Command : ScriptableObject
{
    [SerializeField] public string command_name;
    [SerializeField] public CommandHandlerEvent handler;
}
