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
    public List<Color> colors = new List<Color>() { Color.red};
    public List<Material> materials;
    private List<Vector3> positions;

    private GameObject myAvatar;
	//private GameObject mainCamera;
    public GameObject avatarPF;
    public GameObject characterPF;
	//public GameObject mainCameraPF;

    List<Vector3> cubePosList = new List<Vector3>();
    List<Vector3> cubeRotList = new List<Vector3>();
	List<int> teamScores = new List<int>();
	
	//gui stuff
	public GUIText scoresText; 	//draw out the current score standings
	public GUIText timeText;	//draw the gameTime
    public int myLatency;

    private string clientName;
    public string ClientName
    {
        get { return clientName; }
    }

    private static GameManager instance;
    private bool worldLoaded = false;
	private bool firstTime = true;

    SFSObject lobbyGameInfo;

      //      -(string)   the host username               key: "host"
                //      -(IntArray) playerIds                       key: "playerIDs"
                //      -(int)      number of Teams                 key: "numTeams"
                //      -(SFSArray) teams                           key: "teams"
                //      -(int)      length of the game in seconds   key: "gameLength"
    string host;
    int numberOfPlayers;
    int numberOfTeams;
    SFSArray teams;
    int gameLength;

    private double gameStartTime;
	private double timeStart;
    private int playerCount;
    private int playerInitCount;
    public bool gameStarted = false;


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

        cubeList = new List<GameObject>();
        otherClients = new Dictionary<string, GameObject>();
        SetupTheFox();

        TimeManager.Instance.Init();

        if (GameValues.isHost)
        {
            BuildCubes();
            BuildCubeLists();
            SendCubesDataToServer(cubePosList, cubeRotList);
        }

        ConfirmJoinedGame();

        //SetupGameWorld();
	
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

        // set up arrays of positions 
        positions = new List<Vector3>();
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnTag");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            positions.Add(spawnPoints[i].transform.position);
        }

        lobbyGameInfo = (SFSObject)currentRoom.GetVariable("gameInfo").GetSFSObjectValue();

        host = lobbyGameInfo.GetUtfString("host");
        numberOfTeams = lobbyGameInfo.GetInt("numTeams");
        numberOfPlayers = currentRoom.GetVariable("numberOfPlayers").GetIntValue();
        teams = (SFSArray)lobbyGameInfo.GetSFSArray("teams");

        gameLength = (int)lobbyGameInfo.GetInt("gameLength") * 1000;

        SetupListeners();

    }

    private void ConfirmJoinedGame()
    {
        //add your player id to the list of joined players
        List<RoomVariable> roomVars = new List<RoomVariable>();
        SFSArray playersJoined = (SFSArray)currentRoom.GetVariable("playersJoined").GetSFSArrayValue();
        Debug.Log("joined players so far: " + playersJoined.Size());
        playersJoined.AddInt(GameValues.playerID);
        SFSRoomVariable roomVar = new SFSRoomVariable("playersJoined", playersJoined);
        roomVars.Add(roomVar);
        smartFox.Send(new SetRoomVariablesRequest(roomVars));

        //register your team 

        List<UserVariable> userVars = new List<UserVariable>();
        userVars.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
        userVars.Add(new SFSUserVariable("playerJoined", true));
        smartFox.Send(new SetUserVariablesRequest(userVars));
    }


    private void SetupGameWorld()
    {
        Debug.Log("SetupGameWorld");
        //build team scores
        for (int i = 0; i < numberOfTeams; i++)
        {
            teamScores.Add(0);
        }

        //Debug.Log("Setting the user variables for playerTeam");


        //draw some gui stuff
        //scoresText = (GUIText)GameObject.Find("GUI Text Scores").GetComponent<GUIText>();
        //timeText = (GUIText)GameObject.Find("GUI Text Time").GetComponent<GUIText>();
        

        //check status of room
        if (!GameValues.isHost && currentRoom.ContainsVariable("cubesInSpace"))
        {
            //make sure that all the cubes in the host room are the same in all other clients
            SFSArray cubes = (SFSArray)currentRoom.GetVariable("cubesInSpace").GetSFSArrayValue();
            cubePosList = new List<Vector3>();
            cubeRotList = new List<Vector3>();
            for (int i = 0; i < cubes.Size(); i++)
            {
                Vector3 pos = new Vector3();
                pos.x = cubes.GetSFSObject(i).GetFloat("x");
                pos.y = cubes.GetSFSObject(i).GetFloat("y");
                pos.z = cubes.GetSFSObject(i).GetFloat("z");

                cubePosList.Add(pos);

                Vector3 rot = new Vector3();
                rot.x = cubes.GetSFSObject(i).GetFloat("rx");
                rot.y = cubes.GetSFSObject(i).GetFloat("ry");
                rot.z = cubes.GetSFSObject(i).GetFloat("rz");

                cubeRotList.Add(rot);
            }
            loadWorld();
        }

        // create my avatar
        MakeCharacter(smartFox.MySelf);

        for (int i = 0; i < currentRoom.UserList.Count; i++)
        {
            if (!currentRoom.UserList[i].IsItMe)
            {
                if (currentRoom.UserList[i].GetVariable("playerTeam") != null)
                {
                    Debug.Log("Player variable exists, making character..");
                    MakeCharacter(currentRoom.UserList[i]);
                }
                else
                {
                    Debug.Log("Not making a character until the player variables exist");
                }
            }
        }

        // Lock the mouse to the center
        Screen.lockCursor = true;

        if (!GameValues.isHost)
        {
            //tell server we have build the world
            List<UserVariable> userVars = new List<UserVariable>();
            userVars.Add(new SFSUserVariable("builtGame", true));
            smartFox.Send(new SetUserVariablesRequest(userVars));
        }
    }
	
	// Update is called once per frame
	void Update () {
		//timeText.material.color = new Color(255, 255, 255);
		//scoresText.material.color = new Color(255, 255, 255);
		//Debug.Log("Current Room: " + currentRoom.ToString());
		
		//display the time
		//client timeStamp - startTime = timePast in milliseconds
		if(!firstTime){
			double timePast = TimeManager.Instance.ClientTimeStamp - timeStart;
			double timeLeft = gameLength - timePast;
			timeText.text = "Time Left: " + ((int)timeLeft / 1000).ToString();

			//Debug.Log("((float) TimeManager.Instance.ClientTimeStamp): " + TimeManager.Instance.ClientTimeStamp);
			//Debug.Log("timeStart: " + timeStart);
			
			if (timeLeft <= 0)
			{
				//leave the game room
				smartFox.Send(new JoinRoomRequest("The Lobby", "", currentRoom.Id));
				//smartFox.Send(new LeaveRoomRequest(currentRoom.Name));
			}
		}
		
		//display the score
		string scoreMessage = "";
		for (int i = 0; i < teamScores.Count; i++)
		{
			
			scoreMessage += "Team: " + (i + 1) + " " + teamScores[i].ToString() + "\n";	
		}
		
		scoresText.text = scoreMessage;
		
        if (Input.GetMouseButtonDown(0))
        {
            if (Screen.lockCursor == false)
            {
                Screen.lockCursor = true;
            }
        }
	}

    void FixedUpdate()
    {
        if (!running) return;
        smartFox.ProcessEvents();
    }

 

    public void TimeSyncRequest()
    {
        ExtensionRequest request = new ExtensionRequest("getTime", new SFSObject(), currentRoom);
        smartFox.Send(request);
    }
	
	public void UpdateTeamScore (int teamLastOwnedBy, int teamOwnedBy)
	{
		if(teamLastOwnedBy != -1)
			teamScores[teamLastOwnedBy]--;
        if (teamOwnedBy != -1)
		    teamScores[teamOwnedBy]++;
	}

    void MakeCharacter(User user)
    {
        //int whichColor = GetColorNumber(user);

        int whichColor = 0;

        GameObject cha;
        if (user.IsItMe)
        {
            whichColor = GameValues.teamNum;
            if (whichColor == -1)
                Debug.Log("-1 fault in color picker");
            //cha = Instantiate(characterPF, positions[whichColor], Quaternion.identity) as GameObject;
            cha = Instantiate(characterPF, positions[whichColor], Quaternion.LookRotation(-positions[whichColor])) as GameObject;
			//Debug.Log("Player looking at: " + cha.transform.forward);
			myAvatar = cha;
			// now that we have the player, give it to the MouseLook script
			Camera.mainCamera.GetComponent<MouseLook>().init(myAvatar.GetComponent<Player>());
			
			//give him a color
			myAvatar.GetComponent<Player>().color = colors[whichColor];
        }
        else
        {
			Debug.Log("Finding the Null object within Make Character function...");
			Debug.Log("-smartfox " + smartFox.ToString());
			Debug.Log("-last joined room " + smartFox.LastJoinedRoom.ToString());
			Debug.Log("-get user by name " +smartFox.LastJoinedRoom.GetUserByName(user.Name).ToString());
			Debug.Log("-get variable " + smartFox.LastJoinedRoom.GetUserByName(user.Name).GetVariable("playerTeam").ToString()); //the player variables seem to be broken
			Debug.Log("-get int value " + smartFox.LastJoinedRoom.GetUserByName(user.Name).GetVariable("playerTeam").GetIntValue().ToString());
			
            whichColor = smartFox.LastJoinedRoom.GetUserByName(user.Name).GetVariable("playerTeam").GetIntValue(); //FIX THIS, THE BUG IS HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			Debug.Log("the user is not me and which color = " + whichColor);
            int whichPos = smartFox.LastJoinedRoom.GetUserByName(user.Name).GetVariable("playerID").GetIntValue();
			Debug.Log("username = " + user.Name + "\tposition index = " + whichPos);
			//Debug.Log("the colorArray length = " + colors.Count);
            cha = Instantiate(avatarPF, positions[whichPos], Quaternion.LookRotation(-positions[whichPos])) as GameObject;
            otherClients.Add(user.Name, cha); //update the dictionary of the other players
			cha.GetComponent<Avatar>().color = colors[whichColor];
            cha.GetComponent<Avatar>().team = whichColor;
            cha.GetComponent<Avatar>().TargetPosition = positions[whichPos];
        }

        Character ch = cha.GetComponent<Character>();
        ch.BodyColor = colors[whichColor];
        ch.IsMe = user.IsItMe;
        ch.SmartFoxUser = user;
		
		//look at the center of the arena and get ready to go
		//ch.transform.LookAt(new Vector3(0,0,0));
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
    }

    private void OnExtensionResponse(BaseEvent evt)
    {
        try
        {
            string cmd = (string)evt.Params["cmd"];
            ISFSObject dt = (SFSObject)evt.Params["params"];
            if (cmd == "time")
            {
                HandleServerTime(dt);
            }
            /*
			if(firstTime){
				//is host
				if(GameValues.isHost){
					//add the start time to the room variables
					List<RoomVariable> st = new List<RoomVariable>();
					gameStartTime = TimeManager.Instance.ClientTimeStamp;
					
					SFSRoomVariable startTime = new SFSRoomVariable("startTime", (double)gameStartTime);
					st.Add(startTime);
					smartFox.Send(new SetRoomVariablesRequest(st));
				
					Debug.Log("Start time of the game: " + gameStartTime);
				 	timeStart = (float)gameStartTime;
					Debug.Log("Start time of the game timeStart: " + timeStart);
				}
				
				firstTime = false;
			}
             */
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
		Debug.Log("Connection lost... Returning to lobby");
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
        ArrayList changedVars = (ArrayList)evt.Params["changedVars"];
        User user = (User)evt.Params["user"];
        Debug.Log("USERVAR: " + changedVars[0]);
        if (GameValues.isHost)
        {
            if (changedVars.Contains("playerJoined")){
                playerCount++;
                Debug.Log("player count: " + playerCount);
                if (playerCount == numberOfPlayers)
                {
                    Debug.Log("GAME INITING");
                    SetupGameWorld();
                    List<RoomVariable> roomVars = new List<RoomVariable>();
                    SFSRoomVariable roomVar = new SFSRoomVariable("gameInit", true);
                    roomVars.Add(roomVar);
                    smartFox.Send(new SetRoomVariablesRequest(roomVars));
                }
            }

            if (changedVars.Contains("builtGame"))
            {
                playerInitCount++;
                Debug.Log("player init count: " + playerInitCount);
                if (playerInitCount == numberOfPlayers)
                {
                    Debug.Log("GAME START");
                    List<RoomVariable> roomVars = new List<RoomVariable>();
                    SFSRoomVariable roomVar = new SFSRoomVariable("gameStarted", true);
                    roomVars.Add(roomVar);
                    smartFox.Send(new SetRoomVariablesRequest(roomVars));
                    Debug.Log("after teh request sent");
                }
            }
        }
		//make a character
		//MakeCharacter(user);
    }
    public void OnRoomVariablesUpdate(BaseEvent evt)
    {
        Debug.Log("ROOM VARS have been updated");
        Room room = (Room)evt.Params["room"];
        ArrayList changedVars = (ArrayList)evt.Params["changedVars"];

        Debug.Log("Changed Var contains playersJoined? " + changedVars.Contains("playersJoined"));
        Debug.Log("Changed Var contains cubes in space? " + changedVars.Contains("cubesInSpace"));
        Debug.Log("Changed var contains game Started? " + changedVars.Contains("gameStarted"));
        Debug.Log("Changed var contains game game init? " + changedVars.Contains("gameInit"));
		Debug.Log("Changed var contains game startTime? " + changedVars.Contains("startTime"));

        if (GameValues.isHost)
        {
            if (changedVars.Contains("gameInit"))
            {
                //tell server we have build the world
                List<UserVariable> userVars = new List<UserVariable>();
                userVars.Add(new SFSUserVariable("builtGame", true));
                smartFox.Send(new SetUserVariablesRequest(userVars));
            }
        }else// not host
        {
            // Check if the "gameStarted" variable was changed
            if (changedVars.Contains("startTime"))
            {
                    timeStart = currentRoom.GetVariable("startTime").GetDoubleValue();
                    Debug.Log("startTime: " + timeStart);
            }

            if (changedVars.Contains("gameInit"))
            {
                SetupGameWorld();
            }
        }

        //both host and client
        if (changedVars.Contains("gameStarted"))
        {
            Debug.Log("starting the game");
            StartGame();
        }
    }

    private void StartGame()
    {
        gameStarted = true;
        myAvatar.GetComponent<Player>().gameStarted = true;
    }


    private void loadWorld()
    {
        if(!worldLoaded){
            cubeList = new List<GameObject>();
            for (int i = 0; i < cubePosList.Count; i++)
            {
                GameObject holderCube = (GameObject)Instantiate(GrandCube, cubePosList[i], Quaternion.Euler(cubeRotList[i]));
                holderCube.GetComponent<Cube>().id = i;
                cubeList.Add(holderCube);
            }
            worldLoaded = true;
            Debug.Log("Cubelist:  " + cubeList.Count);
        }
    }

    //only message received currently is transform - refactor when this changes
    private void OnObjectMessageReceived(BaseEvent evt)
    {
        User sender = (User)evt.Params["sender"];
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
        data.GetSFSObject("launchMessage");

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
                                          -WorldSpace + (UnityEngine.Random.value * (WorldSpace * 2f)),
                                          -WorldSpace + (UnityEngine.Random.value * (WorldSpace * 2f)),
                                          -WorldSpace + (UnityEngine.Random.value * (WorldSpace * 2f))
                                          );
			
			//add the newly created cube to the cubeList
            GameObject holderCube = (GameObject)Instantiate(GrandCube, randPos, Quaternion.identity);
            holderCube.GetComponent<Cube>().id = i;
            cubeList.Add(holderCube);

        }
		//make sure everyone knows where those cubes are
    }

    private void BuildCubeLists()
    {
        Debug.Log("here in building: " + cubeList.Count);
        for (int i = 0; i < cubeList.Count; i++)
        {
            cubePosList.Add(cubeList[i].transform.position);
            cubeRotList.Add(cubeList[i].transform.rotation.eulerAngles);
        }
    }

    private void SendCubesDataToServer(List<Vector3> cubePosList, List<Vector3> cubeRotList)
    {
        List<RoomVariable> roomVars = new List<RoomVariable>();
        SFSArray array = new SFSArray();
        SFSObject sfsObject;

        for (int i = 0; i < cubeList.Count; i++)
        {
            sfsObject = new SFSObject();
            sfsObject.PutInt("id", i);
            int[] sides= {-1,-1,-1,-1,-1,-1};
            sfsObject.PutIntArray("sides", sides);
            sfsObject.PutFloat("x",cubePosList[i].x);
            sfsObject.PutFloat("y", cubePosList[i].y);
            sfsObject.PutFloat("z", cubePosList[i].z);
            sfsObject.PutFloat("rx", cubeRotList[i].x);
            sfsObject.PutFloat("ry", cubeRotList[i].y);
            sfsObject.PutFloat("rz", cubeRotList[i].z);
            array.AddSFSObject(sfsObject);
        }

        Debug.Log("sending room");
        roomVars.Add(new SFSRoomVariable("cubesInSpace", array));
        smartFox.Send(new SetRoomVariablesRequest(roomVars));
    }

    public GameObject GetSide(int cubeID, int sideID)
    {
        Debug.Log("IDS:  " + cubeID + ", " + sideID);
        Debug.Log("Cubelist:  " + cubeList.Count);
        return cubeList[cubeID].GetComponent<Cube>().Sides[sideID];
    }

}
