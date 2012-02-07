using UnityEngine;
using System;
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

    private Dictionary<string, GameObject> otherClients;
    private List<Color> colors;
    private List<Vector3> positions;

    private GameObject myAvatar;
    public GameObject avatarPF;
    public GameObject characterPF;

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

        // set up arrays of colors 
        colors = new List<Color>() { new Color(0.6f, 0.3f, 0.1f), Color.red, Color.yellow, new Color(0.5f, 0.1f, 0.7f), Color.blue };

        // set up arrays of positions 
        positions = new List<Vector3>();
        for (int i = 0; i < 5; i++)
        {
            positions.Add(new Vector3(0, 0, i * 15));
        }
        
        otherClients = new Dictionary<string, GameObject>();

        // create my avatar
        MakeCharacter(smartFox.MySelf);

        //start sending transform data
        //NetworkTransformSender tfSender = myAvatar.GetComponent<NetworkTransformSender>();
        //tfSender.StartSendTransform();

        SetupListeners();
    }

    void MakeCharacter(User user)
    {
        int whichColor = GetColorNumber(user);
        if (whichColor == -1)
            Debug.Log("-1 fault in color picker");

        GameObject cha;
        if (user.IsItMe)
        {
            cha = Instantiate(characterPF, positions[whichColor], Quaternion.identity) as GameObject;
            myAvatar = cha;
        }
        else
        {
            cha = Instantiate(avatarPF, positions[whichColor], Quaternion.identity) as GameObject;
            otherClients.Add(user.Name, cha);
        }

        Character ch = cha.GetComponent<Character>();
        ch.BodyColor = colors[whichColor];
        ch.IsMe = user.IsItMe;
        ch.SmartFoxUser = user;
    }

    private int GetColorNumber(User user)
    {
        int colorNum = -1;

        if (user.IsItMe)
        {

            //assign a new color
            //first get a copy of available numbers, which is a room variable
            SFSArray numbers = (SFSArray)currentRoom.GetVariable("colorNums").GetSFSArrayValue();
            int ran = UnityEngine.Random.Range(0, numbers.Size() - 1);
            colorNum = numbers.GetInt(ran);

            //update room variable 
            numbers.RemoveElementAt(ran);
            //send back to store on server
            List<RoomVariable> rData = new List<RoomVariable>();
            rData.Add(new SFSRoomVariable("colorNums", numbers));
            smartFox.Send(new SetRoomVariablesRequest(rData));

            //store my own color on server as user data
            List<UserVariable> uData = new List<UserVariable>();
            uData.Add(new SFSUserVariable("colorIndex", colorNum));
            smartFox.Send(new SetUserVariablesRequest(uData));

        }
        else
        {
            try
            {
                colorNum = (int)user.GetVariable("colorIndex").GetIntValue();
            }
            catch (Exception ex)
            {
                Debug.Log("error in else of getColorNumber " + ex.ToString());
            }
        }
        return colorNum;
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
        //NetworkLaunchMessageSender sender = myAvatar.GetComponent<NetworkLaunchMessageSender>();
        //sender.SendLaunchOnRequest();
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

        //if we don't have this client in our client list add them 
        if (!otherClients.ContainsKey (sender.Name)){
            Debug.Log ("making " + sender.Name);
           // MakeCharacter (sender);
        }

        ISFSObject data = (SFSObject)evt.Params["message"];

        if (data.ContainsKey("launchMessage"))
        {
            LaunchPacket launchMessage = LaunchPacket.FromSFSObject(data);
            NetworkLaunchMessageReceiver rec = otherClients[sender.Name].GetComponent<NetworkLaunchMessageReceiver>();
            rec.ReceiveLaunchData(launchMessage);
        }         
    }

    public void SendLaunchMessage(LaunchPacket launchMessage)
    {
        ISFSObject data = new SFSObject();
        launchMessage.ToSFSObject(data);
        smartFox.Send(new ObjectMessageRequest(data));
    }

    private void BuildCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            Vector3 randPos = new Vector3(
                                          -WorldSpace + (UnityEngine.Random.value * (WorldSpace*2f)),
                                          -WorldSpace + (UnityEngine.Random.value * (WorldSpace * 2f)),
                                          -WorldSpace + (UnityEngine.Random.value * (WorldSpace * 2f))
                                          );
			//Instantiate(GrandCube, randPos, Quaternion.identity);
			
			//add the newly created cube to the cubeList
			cubeList.Add((GameObject) Instantiate(GrandCube, randPos, Quaternion.identity));
        }
    }
	
	
}
