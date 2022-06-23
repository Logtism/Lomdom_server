using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Robbery : MonoBehaviour
{
    private static Robbery _singleton;
    public static Robbery Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(Robbery)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    [SerializeField] private GameObject[] robberyManagers;
    private GameObject selectedRobberyManager;
    public int robberyPhase = 0;
    public ushort clientID;

    private void Awake()
    {
        Singleton = this;
        clientID = MissionManager.Singleton.StartedBy.ClientID;
    }

    private void Start()
    {
        robberyManagers = GameObject.FindGameObjectsWithTag("RobberyManager");

        for (int i = 0; i < robberyManagers.Length; i++)
        {
            Debug.Log("Found " + robberyManagers[i].gameObject.name + " Robbery");
        }

        selectRobberyLocation();
        
        Debug.Log(selectedRobberyManager.name);
    }

    private void selectRobberyLocation()
    {
        int index = Random.Range(0, robberyManagers.Length);     
        selectedRobberyManager = robberyManagers[index];

        if (selectedRobberyManager.GetComponent<RobberyManager>().robberyOpen == false)
        {
            Debug.Log("Randomly selected robbery was on cooldown");
            
            for (int i = 0; i < robberyManagers.Length; i++)
            {
                if (robberyManagers[i].GetComponent<RobberyManager>().robberyOpen)
                {
                    selectedRobberyManager = robberyManagers[i];
                    Debug.Log(selectedRobberyManager.name + " is not on cooldown. Starting selected robbery");
                    startRobbery();
                    return;
                }

                else
                    Debug.Log(robberyManagers[i].gameObject.name + " is on cooldown. Checking next robbery");
            }
        }

        if (selectedRobberyManager.GetComponent<RobberyManager>().robberyOpen)
        {
            Debug.Log("Randomly selected robbery is open. Starting selected robbery");
            startRobbery();
            return;
        }

        endMissionError("All robberies are on cooldown. Cannot start mission");
    }

    private void startRobbery()
    {
        if (robberyPhase == 0)
        {
            robberyPhase = 1;
        }
    }

    [MessageHandler((ushort)Messages.CTS.completeRobbery_input)]
    private static void OnPlayerCompleteRobberyInput(Message message)
    {
        Singleton.completePhaseOne();
    }

    private void completePhaseOne()
    {
        if (robberyPhase == 1)
        {
            robberyPhase = 2;
            StartCoroutine(selectedRobberyManager.GetComponent<RobberyManager>().runRobberyCooldown());
        }
    }

    public void completeRobbery()
    {
        robberyPhase = 0;
        MissionManager.Singleton.EndMission();
        Destroy(gameObject);
    }

    public void endMissionError(string errorMsg)
    {
        Debug.Log(errorMsg);

        if(robberyPhase > 0)
        {
            robberyPhase = 0;
        }

        MissionManager.Singleton.EndMission();
        Destroy(gameObject);
    }


}
