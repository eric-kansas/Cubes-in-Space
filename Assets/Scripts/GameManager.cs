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

        //check status of room
        Debug.Log("Status of room: " + currentRoom.ContainsVariable("cubesInSpace"));
        if (currentRoom.ContainsVariable("cubesInSpace"))
        {
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

    }

    public void TimeSyncRequest()
    {
        ExtensionRequest request = new ExtensionRequest("getTime", new SFSObject(), currentRoom);
        smartFox.Send(request);
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
			// now that we have the player, give it to the MouseLook script
			Camera.mainCamera.GetComponent<MouseLook>().init(myAvatar);
			myAvatar.GetComponent<Player>().color = colors[whichColor];
        }
        else
        {
            cha = Instantiate(avatarPF, positions[whichColor], Quaternion.identity) as GameObject;
            otherClients.Add(user.Name, cha);
			cha.GetComponent<Avatar>().color = colors[whichColor];
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
