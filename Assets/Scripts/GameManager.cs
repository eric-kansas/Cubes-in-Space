using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class GameManager : MonoBehaviour {

    private SmartFox smartFox;
    private Lobby lobby;
    private Room currentRoom;
    private bool running = false;

    public float WorldSpace = 50f;
    public int numberOfCubes = 10;
    public GameObject GrandCube;
    private List<GameObject> cubeList;

    private string clientName;
    public string ClientName
    {
        get { return clientName; }
    }

    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start () {

        SetupTheFox();
        
        cubeList = new List<GameObject>();
				
        if (GrandCube)
            BuildCubes();
        else
            Debug.LogError("No GrandCube prefab assigned");
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        if (!running) return;
        smartFox.ProcessEvents();
    }

    private void SetupTheFox()
    {
        bool debug = true;
        running = true;
        if (SmartFoxConnection.IsInitialized)
        {
            smartFox = SmartFoxConnection.Connection;
        }
        else
        {
            smartFox = new SmartFox(debug);
        }

        currentRoom = smartFox.LastJoinedRoom;
        clientName = smartFox.MySelf.Name;

        // set up arrays of colors and spawn positions for various players


        // create my avatar


        //start sending transform data
        //NetworkTransformSender tfSender = myAvatar.GetComponent<NetworkTransformSender>();
        //tfSender.StartSendTransform();

        SetupListeners();
    }

    void SetupListeners()
    {
        // listen for an smartfox events 
        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
        smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);
        smartFox.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER_ERROR, OnSpectatorToPlayerError);
        smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
        smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate);
        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
    }

    // when user enters room, we send our transform so they can update us
    public void OnUserEnterRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Debug.Log("user entered room " + user.Name);
        //NetworkTransformSender sender = myAvatar.GetComponent<NetworkTransformSender> ();
        //sender.SendTransformOnRequest ();
    }

    private void OnUserLeaveRoom(BaseEvent evt)
    {
        //remove this user from our world and update our data structures
        User user = (User)evt.Params["user"];

    }


    public void OnUserCountChange(BaseEvent evt)
    {
        //Debug.Log("OnUserCountChange (from game room) ");
    }

    // When connection is lost we load the login scene
    private void OnConnectionLost(BaseEvent evt)
    {
        UnsubscribeDelegates();
        Screen.lockCursor = false;
        Screen.showCursor = true;
        Application.LoadLevel("The Lobby");
    }
    private void OnSpectatorToPlayer(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
    }

    private void OnSpectatorToPlayerError(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];

    }

    private void UnsubscribeDelegates()
    {
        smartFox.RemoveAllEventListeners();
    }
    void OnApplicationQuit()
    {
        UnsubscribeDelegates();
    }

    public void OnUserVariablesUpdate(BaseEvent evt)
    {
        //List<UserVariable> changedVars = (List<UserVariable>)evt.Params["changedVars"];
        //User user = (User)evt.Params["user"];
    }

    public void OnRoomVariablesUpdate(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        ArrayList changedVars = (ArrayList)evt.Params["changedVars"];
    }

    //only message received currently is transform - refactor when this changes
    private void OnObjectMessageReceived(BaseEvent evt)
    {

        User sender = (User)evt.Params["sender"];

        /*if (!others.ContainsKey (sender.Name)){
            Debug.Log ("making " + sender.Name);
            MakeCharacter (sender);
        }
        ISFSObject obj = (SFSObject)evt.Params["message"];
        NetworkTransform ntransform = NetworkTransform.FromSFSObject (obj);
        NetworkTransformReceiver rec = others[sender.Name].GetComponent<NetworkTransformReceiver> ();
        rec.ReceiveTransform (ntransform);
         */
    }

    public void SendTransform(NetworkTransform ntransform)
    {
        ISFSObject data = new SFSObject();
        //ntransform.ToSFSObject(data);
        //smartFox.Send(new ObjectMessageRequest(data));
    }

    private void BuildCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            Vector3 randPos = new Vector3(
                                          -WorldSpace + (Random.value * (WorldSpace*2f)),
                                          -WorldSpace + (Random.value * (WorldSpace*2f)),
                                          -WorldSpace + (Random.value * (WorldSpace*2f))
                                          );
			//Instantiate(GrandCube, randPos, Quaternion.identity);
			
			//add the newly created cube to the cubeList
			cubeList.Add((GameObject) Instantiate(GrandCube, randPos, Quaternion.identity));
        }
    }
	
	
}
