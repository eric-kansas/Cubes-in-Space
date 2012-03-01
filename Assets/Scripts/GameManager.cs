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
	private bool tryJoiningRoom = false;

    SFSObject lobbyGameInfo;

      //      -(string)   the host username               key: "host"
                //      -(IntArray) playerIds                       key: "playerIDs"
                //      -(int)      number of Teams                 key: "numTeams"
                //      -(SFSArray) teams                           key: "teams"
                //      -(int)      length of the game in seconds   key: "gameLength"
    string host;
    int numberOfPlayers;
    int numberOfTeams;
    int activeTeams;
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
		Debug.Log("Current Room = " + currentRoom.Name);
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
                if (currentRoom.UserList[i].GetVariable("playerTeam") != null && currentRoom.UserList[i].GetVariable("playerID") != null)
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
		if(!firstTime)
        {
			
			double timePast = TimeManager.Instance.ClientTimeStamp - timeStart;
			double timeLeft = gameLength - timePast;
			timeText.text = "Time Left: " + ((int)timeLeft / 1000).ToString();
			//Debug.Log("Updating Time: " + timeLeft);
			
		    Debug.Log("((float) TimeManager.Instance.ClientTimeStamp): " + TimeManager.Instance.ClientTimeStamp);
			Debug.Log("timeStart: " + timeStart);
			
			if (GameValues.isHost && timeLeft <= 0)
			{
				//leave the game room
				string postGameScreen = host.Trim() + " - Room";
				Debug.Log("Joining the post game screen: " + postGameScreen);
				smartFox.Send(new JoinRoomRequest(postGameScreen, "", currentRoom.Id));
				//Application.LoadLevel("Game Lobby");
				tryJoiningRoom = true;
				Screen.lockCursor = false;
       		 	Screen.showCursor = true;
				
				//pass some room variables that talk about the scores
                //display the score
                
		    }
            string scoreMessage = "";
            //for (int i = 0; i < teamScores.Count; i++)
            for (int i = 0; i < numberOfTeams; i++)
            {
                switch (i)
                {
                    case 0:
                        scoreMessage += "Red: " + teamScores[i].ToString() + "\n";
                        break;
                    case 1:
                        scoreMessage += "Blue: " + teamScores[i].ToString() + "\n";
                        break;
                    case 2:
                        scoreMessage += "Green: " + teamScores[i].ToString() + "\n";
                        break;
                    case 3:
                        scoreMessage += "Purple: " + teamScores[i].ToString() + "\n";
                        break;
                    case 4:
                        scoreMessage += "Yellow: " + teamScores[i].ToString() + "\n";
                        break;
                    case 5:
                        scoreMessage += "Orange: " + teamScores[i].ToString() + "\n";
                        break;
                    case 6:
                        scoreMessage += "Pink: " + teamScores[i].ToString() + "\n";
                        break;
                    case 7:
                        scoreMessage += "Teal: " + teamScores[i].ToString() + "\n";
                        break;
                    default:
                        break;
                }
            }
            scoresText.text = scoreMessage;
		
		
			//scoreMessage += "Team: " + (i + 1) + " " + teamScores[i].ToString() + "\n";
		}
		
		
		
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
            int whichPos = GameValues.playerID;
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
		
    }

    void SetupListeners()
    {
        // listen for an smartfox events 
        smartFox.RemoveAllEventListeners();
		
		smartFox.AddEventListener(SFSEvent.ROOM_JOIN, 			OnJoinRoom);
        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, 	OnUserEnterRoom);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, 		OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, 	OnUserCountChange);
        smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, 	OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER, OnSpectatorToPlayer);
        smartFox.AddEventListener(SFSEvent.SPECTATOR_TO_PLAYER_ERROR, 	OnSpectatorToPlayerError);
        smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, 				OnObjectMessageReceived);
        smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, 		OnUserVariablesUpdate);
        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, 		OnRoomVariablesUpdate);
        smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, 			OnExtensionResponse);
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
		Debug.Log("Syncronizing to time: " + time.ToString());
    }
	
	
	public void OnJoinRoom(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        currentRoom = room;

        Debug.Log("onjoinroom = " + currentRoom.Name);
		
		//unlock the mouse
		Screen.lockCursor = false;
        Screen.showCursor = true;
		
        if (room.Name == "The Lobby")
        {
            //smartFox.RemoveAllEventListeners();
            Debug.Log("onjoinroom = " + smartFox.LastJoinedRoom.Name);
            Application.LoadLevel("The Lobby");
        }
        else if (room.IsGame)
        {
			//this is the game, so it should never actually happen
			Debug.Log("This is the game and should be loaded");
		}
        else
        {
            Debug.Log("GameRoom- OnJoinRoom: joined " + room.Name);
            Application.LoadLevel("Game Lobby");
            Debug.Log("loading Game Lobby");
            //smartFox.Send(new SpectatorToPlayerRequest());
        }
    }
    // when user enters room, we send our transform so they can update us
	
	public void OnJoinRoomError(BaseEvent evt)
    {
        Debug.Log("Error joining room: " + evt.Params["message"]);
        if (tryJoiningRoom)
		{
			string postGameRoomName = host.Trim() + " - Room";
        	smartFox.Send(new JoinRoomRequest(postGameRoomName, "", currentRoom.Id));
			//Debug.Log("Attempting to join room named " + nameParts[0].Trim() + " - Game");
			//tryJoiningRoom = false;
		}
		else
		{
			//smartFox.Send(new JoinRoomRequest("The Lobby", "", CurrentActiveRoom.Id));
		}
	}
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
		Debug.Log("A user left! His name is: " + user.Name);
		Debug.Log("The host's name is: " + host);
		
		//check if it was the host that left
		if (user.Name == host)
		{
			Debug.Log("The user that left was the host");
			//check if because the game was over
			//parse the time text
			int indexOfSpace = timeText.text.IndexOf(": ");
			string theTimeNumber = timeText.text.Substring(indexOfSpace + 1).Trim(); //get rid of leading and trailing spaces
			Debug.Log("Time left in the game: " + theTimeNumber);
			int timeLeft = Int32.Parse(theTimeNumber);
			
			if (timeLeft < 5)
			{
				//if game is over
				//move to game lobby
                Debug.Log("ehrekhrl");
				string postGameRoomName = user.Name.Trim() + " - Room";
				smartFox.Send(new JoinRoomRequest(postGameRoomName, "", currentRoom.Id));
				tryJoiningRoom = true;
				//Application.LoadLevel("Game Lobby");
			}
			else
			{
				//if game ! over, the host must have DC'd
				//return to lobby
				smartFox.Send(new JoinRoomRequest("The Lobby", "", currentRoom.Id));
				//Application.LoadLevel("The Lobby");
			}
		}
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
        currentRoom = null;
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
                    //add the start time to the room variables
                    List<RoomVariable> roomVars = new List<RoomVariable>();
                    gameStartTime = TimeManager.Instance.ClientTimeStamp;

                    SFSRoomVariable startTime = new SFSRoomVariable("startTime", (double)gameStartTime);
                    SFSRoomVariable roomVar = new SFSRoomVariable("gameStarted", true);
                    roomVars.Add(startTime);
                    roomVars.Add(roomVar);
                    timeStart = (float)gameStartTime;
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

        //Debug.Log("Changed Var contains playersJoined? " + changedVars.Contains("playersJoined"));
        //Debug.Log("Changed Var contains cubes in space? " + changedVars.Contains("cubesInSpace"));
        //Debug.Log("Changed var contains game Started? " + changedVars.Contains("gameStarted"));
        //Debug.Log("Changed var contains game game init? " + changedVars.Contains("gameInit"));
		//Debug.Log("Changed var contains game startTime? " + changedVars.Contains("startTime"));

        if (GameValues.isHost)
        {
            if (changedVars.Contains("gameInit"))
            {
                //tell server we have build the world
                List<UserVariable> userVars = new List<UserVariable>();
                userVars.Add(new SFSUserVariable("builtGame", true));
                smartFox.Send(new SetUserVariablesRequest(userVars));
            }
        }
		else// not host
        {
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
			    // Check if the "gameStarted" variable was changed
        if (changedVars.Contains("startTime"))
        {
            timeStart = currentRoom.GetVariable("startTime").GetDoubleValue();
            firstTime = false;    
			Debug.Log("startTime: " + timeStart);
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
