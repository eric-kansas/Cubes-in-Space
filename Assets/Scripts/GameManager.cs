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

    public readonly static string ExtName = "sfsFps";
    public readonly static string ExtClass = "dk.fullcontrol.fps.FpsExtension";

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
	//private GameObject mainCamera;
    public GameObject avatarPF;
    public GameObject characterPF;
	//public GameObject mainCameraPF;

    List<Vector3> cubePosList = new List<Vector3>();
	
    public int myLatency;

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

        TimeManager.Instance.Init();
        
        cubeList = new List<GameObject>();
				
        if (!GrandCube)
            Debug.LogError("No GrandCube prefab assigned");
        if(GameValues.isHost)
            BuildCubes();
        
		
		// Lock the mouse to the center
		Screen.lockCursor = true;
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

        smartFox.enableLagMonitor(true);
        currentRoom = smartFox.LastJoinedRoom;
        clientName = smartFox.MySelf.Name;

        // set up arrays of colors 
        colors = new List<Color>() { Color.red, Color.magenta, Color.yellow, Color.green, Color.blue, Color.cyan };
		//make sure this matches the game lobby list
		

        // set up arrays of positions 
        positions = new List<Vector3>();
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnTag");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            positions.Add(spawnPoints[i].transform.position);
        }
        
        otherClients = new Dictionary<string, GameObject>();

        

        //start sending transform data
        //NetworkTransformSender tfSender = myAvatar.GetComponent<NetworkTransformSender>();
        //tfSender.StartSendTransform();

        SetupListeners();

        // create my avatar
        MakeCharacter(smartFox.MySelf);

        //check status of room
        if (currentRoom.ContainsVariable("cubesInSpace"))
        {
			//make sure that all the cubes in the host room are the same in all other clients
            SFSArray cubes = (SFSArray)currentRoom.GetVariable("cubesInSpace").GetSFSArrayValue();
            cubePosList = new List<Vector3>();
            for (int i = 0; i < cubes.Size(); i++)
            {
                Vector3 pos = new Vector3();
                pos.x = cubes.GetSFSObject(i).GetFloat("x");
                pos.y = cubes.GetSFSObject(i).GetFloat("y");
                pos.z = cubes.GetSFSObject(i).GetFloat("z");
                
                cubePosList.Add(pos);
            }
            loadWorld();
        }

        for(int i = 0; i < currentRoom.UserList.Count; i++)
        {
			if (!currentRoom.UserList[i].IsItMe)
			{
            	MakeCharacter(currentRoom.UserList[i]);
			}
        }
        

    }

    public void TimeSyncRequest()
    {
        ExtensionRequest request = new ExtensionRequest("getTime", new SFSObject(), currentRoom);
        smartFox.Send(request);
    }

    void MakeCharacter(User user)
    {
        //int whichColor = GetColorNumber(user);

        int whichColor = 0;

        GameObject cha;
        if (user.IsItMe)
        {
            whichColor = GameValues.colorIndex;
            if (whichColor == -1)
                Debug.Log("-1 fault in color picker");
            //cha = Instantiate(characterPF, positions[whichColor], Quaternion.identity) as GameObject;
            cha = Instantiate(characterPF, positions[whichColor], Quaternion.LookRotation(-positions[whichColor])) as GameObject;
			Debug.Log("Player looking at: " + cha.transform.forward);
			myAvatar = cha;
			// now that we have the player, give it to the MouseLook script
			Camera.mainCamera.GetComponent<MouseLook>().init(myAvatar);
			
			//give him a color
			int numPeopleInRoom = smartFox.LastJoinedRoom.UserList.Count;
        	whichColor = numPeopleInRoom - 1;
			myAvatar.GetComponent<Player>().color = colors[whichColor];
        }
        else
        {
			//not me, so make a character somewhere else
			int numPeopleInRoom = smartFox.LastJoinedRoom.UserList.Count;
        	whichColor = numPeopleInRoom - 1;
			
            Debug.Log("here in thing: " + whichColor);
            cha = Instantiate(avatarPF, positions[whichColor], Quaternion.LookRotation(-positions[whichColor])) as GameObject;
            otherClients.Add(user.Name, cha); //update the dictionary of the other players
			cha.GetComponent<Avatar>().color = colors[whichColor];
        }

        Character ch = cha.GetComponent<Character>();
        ch.BodyColor = colors[whichColor];
        ch.IsMe = user.IsItMe;
        ch.SmartFoxUser = user;
		
		//look at the center of the arena and get ready to go
		//ch.transform.LookAt(new Vector3(0,0,0));
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
        smartFox.RemoveAllEventListeners();

        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
        smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);
        smartFox.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER_ERROR, OnSpectatorToPlayerError);
        smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
        smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate);
        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
        smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
        //smartFox.AddEventListener(SFSEvent.PING_PONG, OnPingPong);
    }

    private void OnExtensionResponse(BaseEvent evt)
    {
        try
        {
           // Debug.Log("extension:");
            string cmd = (string)evt.Params["cmd"];
            ISFSObject dt = (SFSObject)evt.Params["params"];
            if (cmd == "time")
            {
                HandleServerTime(dt);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception handling response: " + e.Message + " >>> " + e.StackTrace);
        }

    }

    private void HandleServerTime(ISFSObject dt)
    {
        //Debug.Log("server time");
        long time = dt.GetLong("t");
        TimeManager.Instance.Synchronize(Convert.ToDouble(time));
    }

    // when user enters room, we send our transform so they can update us
    public void OnUserEnterRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        Debug.Log("user entered room " + user.Name);
        //NetworkLaunchMessageSender sender = myAvatar.GetComponent<NetworkLaunchMessageSender>();
        //sender.SendLaunchOnRequest();
		
		//make a character
		MakeCharacter(user);
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
        Debug.Log("ROOM VARS");
        Room room = (Room)evt.Params["room"];
        ArrayList changedVars = (ArrayList)evt.Params["changedVars"];

        Debug.Log(changedVars.Contains("cubesInSpace"));
        Debug.Log(changedVars.Contains("gameStarted"));
        if (!GameValues.isHost)
        {
            // Check if the "gameStarted" variable was changed
            if (changedVars.Contains("gameStarted"))
            {
                if (room.GetVariable("gameStarted").GetBoolValue() == true)
                {
                    Debug.Log("Game started");
                }
                else
                {
                    Debug.Log("Game stopped");
                }
            }
            // Check if map has been uploaded
            if (changedVars.Contains("cubesInSpace"))
            {
                Debug.Log("ROOM cubesInSpace");
                cubePosList = new List<Vector3>();
                ISFSArray data = room.GetVariable("cubesInSpace").GetSFSArrayValue();
                for (int i = 0; i < data.Size(); i++)
                {
                    Vector3 pos = new Vector3();
                    pos.x = data.GetSFSObject(i).GetFloat("x");
                    pos.y = data.GetSFSObject(i).GetFloat("y");
                    pos.z = data.GetSFSObject(i).GetFloat("z");
                    cubePosList.Add(pos);
                }

                loadWorld();
            }
        }

    }

    private void loadWorld()
    {
        cubeList = new List<GameObject>();
        for (int i = 0; i < cubePosList.Count; i++)
        {
            cubeList.Add((GameObject)Instantiate(GrandCube, cubePosList[i], Quaternion.identity));
        }
    }

    //only message received currently is transform - refactor when this changes
    private void OnObjectMessageReceived(BaseEvent evt)
    {
        User sender = (User)evt.Params["sender"];

        //if we don't have this client in our client list add them 
        /*if (!otherClients.ContainsKey (sender.Name)){
            Debug.Log ("making " + sender.Name);
            MakeCharacter (sender);
        }*/

        ISFSObject data = (SFSObject)evt.Params["message"];
		
		Debug.Log("here in object message the sender is: " + sender.Name);
		
		//deal with messages from other players
		//case 1: A player has launched somewhere
        if (data.ContainsKey("launchMessage")) 
        {
            LaunchPacket launchMessage = LaunchPacket.FromSFSObject(data);
            NetworkLaunchMessageReceiver rec = otherClients[sender.Name].GetComponent<NetworkLaunchMessageReceiver>();
            rec.ReceiveLaunchData(launchMessage);
        }         
		
		//case 2: A player has arrived somewhere
		//nope, the arrival will be calculated on each client
		
		//case 3:
		
    }
	
	//send out a launch message to all other players that you are moving somewhere
    public void SendLaunchMessage(LaunchPacket launchMessage)
    {
		Debug.Log("Sending Launch Message");
        ISFSObject data = new SFSObject();
        data = launchMessage.ToSFSObject(data);
        smartFox.Send(new ObjectMessageRequest(data));
    }

    private void BuildCubes()
    {
		//if you are the first one in the room, make a shit-ton of cubes and scatter them everywhere
        List<Vector3> cubePosList = new List<Vector3>();

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
            cubePosList.Add(randPos);
        }
		//make sure everyone knows where those cubes are
        SendCubesDataToServer(cubePosList);
    }

    private void SendCubesDataToServer(List<Vector3> cubePosList)
    {
        List<RoomVariable> roomVars = new List<RoomVariable>();
        SFSArray array = new SFSArray();
        SFSObject sfsObject;

        for (int i = 0; i < cubePosList.Count; i++)
        {
            sfsObject = new SFSObject();
            sfsObject.PutInt("id", i);
            sfsObject.PutIntArray("sides", new int[6]);
            sfsObject.PutFloat("x",cubePosList[i].x);
            sfsObject.PutFloat("y", cubePosList[i].y);
            sfsObject.PutFloat("z", cubePosList[i].z);
            array.AddSFSObject(sfsObject);
        }

        Debug.Log("sending room");
        roomVars.Add(new SFSRoomVariable("cubesInSpace", array));
        smartFox.Send(new SetRoomVariablesRequest(roomVars));
    }

}
