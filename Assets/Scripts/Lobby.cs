using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Variables;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Logging;


public class Lobby : MonoBehaviour {

	private SmartFox smartFox;
	private string zone = "EMH"; 
	private string serverName = "129.21.29.6";
	private int serverPort = 9933;

	public string username = "";
	private string loginErrorMessage = "";
	private bool isLoggedIn;
	
	private string newMessage = "";
	private ArrayList messages = new ArrayList();
		
	public GUISkin gSkin;
    public Texture2D titleImage;
	
	//keep track of room we're in
	private Room currentActiveRoom;
	public Room CurrentActiveRoom{ get {return currentActiveRoom;} }
				
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int roomSelection = -1;	  //For clicking on list box 
	private string[] roomNameStrings; //Names of rooms
	private string[] roomFullStrings; //Names and descriptions
	private int screenW;

    private int maxPlayers = 16;
    private int numTeams = 8;

    private bool createdMyRoom = false;
	
	void Start()
	{
		Security.PrefetchSocketPolicy(serverName, serverPort); 
		bool debug = true;

		if (SmartFoxConnection.IsInitialized)
		{
			//If we've been here before, the connection has already been initialized. 
			//and we don't want to re-create this scene, therefore destroy the new one
			smartFox = SmartFoxConnection.Connection;
			//Destroy(gameObject);
            
            username = smartFox.MySelf.Name;
            Debug.Log("username: " + username);

            List<Room> allRooms = smartFox.RoomManager.GetRoomList();
		
		    foreach (Room room in allRooms) 
            {
                if(room.Name == username + " - Room")
                {
                    createdMyRoom = true;
                }
            }

            currentActiveRoom = smartFox.LastJoinedRoom;
            PrepareLobby();
		}
		else
		{
            
			//If this is the first time we've been here, keep the Lobby around
			//even when we load another scene, this will remain with all its data
			smartFox = new SmartFox(debug);
			//DontDestroyOnLoad(gameObject);
		}

        AddEventListeners();
		smartFox.AddLogListener(LogLevel.INFO, OnDebugMessage);
		screenW = Screen.width;
	}
	
	private void AddEventListeners() {
		
		smartFox.RemoveAllEventListeners();
		
		smartFox.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.LOGIN, OnLogin);
		smartFox.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
		smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
		smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
		smartFox.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
        smartFox.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
		smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
	}
	
	void FixedUpdate() {
		//this is necessary to have any smartfox action!
		smartFox.ProcessEvents();
	}
	
	private void UnregisterSFSSceneCallbacks() {
		smartFox.RemoveAllEventListeners();
	}
	
	public void OnConnection(BaseEvent evt) {
		bool success = (bool)evt.Params["success"];
		string error = (string)evt.Params["errorMessage"];
		
		Debug.Log("On Connection callback got: " + success + " (error? : <" + error + ">)");

		if (success) {
			SmartFoxConnection.Connection = smartFox;

			Debug.Log("Sending login request");
			smartFox.Send(new LoginRequest(username, "", zone));

		}
	}

	public void OnConnectionLost(BaseEvent evt) {
		Debug.Log("OnConnectionLost");
		isLoggedIn = false;
		UnregisterSFSSceneCallbacks();
		currentActiveRoom = null;
		roomSelection = -1;	
		Application.LoadLevel("The Lobby");
	}

	// Various SFS callbacks
	public void OnLogin(BaseEvent evt) {
		try
		{
			if (evt.Params.ContainsKey("success") && !(bool)evt.Params["success"]) {
				loginErrorMessage = (string)evt.Params["errorMessage"];
				Debug.Log("Login error: "+loginErrorMessage);
			}
			else {
				Debug.Log("Logged in successfully");
				PrepareLobby();	
			}
		}
		catch (Exception ex) {
			Debug.Log("Exception handling login request: "+ex.Message+" "+ex.StackTrace);
		}
	}

	public void OnLoginError(BaseEvent evt) {
		Debug.Log("Login error: "+(string)evt.Params["errorMessage"]);
	}
	
	void OnLogout(BaseEvent evt) {
		Debug.Log("OnLogout");
		isLoggedIn = false;
		currentActiveRoom = null;
		smartFox.Disconnect();
	}
	
	public void OnDebugMessage(BaseEvent evt) {
		string message = (string)evt.Params["message"];
		Debug.Log("[SFS DEBUG] " + message);
	}
	
	
	public void OnRoomAdded(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];
		SetupRoomList();
		//Debug.Log("Room added: "+room.Name);
	}

    public void OnRoomRemoved(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];
		SetupRoomList();
		//Debug.Log("Room added: "+room.Name);
	}

    
	public void OnRoomCreationError(BaseEvent evt)
	{
		Debug.Log("Error creating room: " + evt.Params["message"]);
	}
	
	public void OnJoinRoom(BaseEvent evt)
	{
		Room room = (Room)evt.Params["room"];
        Debug.Log("here on join");
		currentActiveRoom = room;

        if (room.Name == "The Lobby")
        {
        //    Debug.Log("\t-Loading up the lobby");
            Application.LoadLevel(room.Name);
        }
        else
        {// going to game lobby
         //   Check if this user is the host
            if (username.Equals(room.GetVariable("gameInfo").GetSFSObjectValue().GetUtfString("host")))
            {
         //       Debug.Log("\t-YES IM THE HOST WOOT!!");
                GameValues.isHost = true;
            }
            else
            {
                GameValues.isHost = false;
            }

            Application.LoadLevel("Game Lobby");
        }

        //Debug.Log("\t-end OnJoinRoom funtion");
	}

    private int GetPlayerID(User user)
    {
        int playerID = -1;

        Debug.Log("GetPlayerID function: Is the user me? " + user.IsItMe);

        if (user.IsItMe)
        {

            //assign a new color
            //first get a copy of available numbers, which is a room variable
            //Debug.Log("\t-Getting gameInfo");
            SFSObject gameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();
            //Debug.Log("\t-Getting the ids that are left");
            SFSArray idsLeft = (SFSArray)gameInfo.GetSFSArray("playerIDs");
            //Debug.Log("ya :" + idsLeft.Size());
            //int ran = UnityEngine.Random.Range(0, idsLeft.Size() - 1);
            playerID = idsLeft.GetInt(0);

            Debug.Log("here: " + playerID);
            //update room variable 
            idsLeft.RemoveElementAt(0);
            //send back to store on server
            List<RoomVariable> rData = new List<RoomVariable>();
            gameInfo.PutSFSArray("playerIDs", idsLeft);
            rData.Add(new SFSRoomVariable("gameInfo", gameInfo));
            smartFox.Send(new SetRoomVariablesRequest(rData));

            //store my own color on server as user data
            List<UserVariable> uData = new List<UserVariable>();
            uData.Add(new SFSUserVariable("playerID", playerID));
            smartFox.Send(new SetUserVariablesRequest(uData));

        }
        else
        {
            try
            {
                playerID = (int)user.GetVariable("playerIDs").GetIntValue();
            }
            catch (Exception ex)
            {
                Debug.Log("error in else of playerIDs " + ex.ToString());
            }
        }
        return playerID;
    }
	
	public void OnUserEnterRoom(BaseEvent evt) {
		User user = (User)evt.Params["user"];
			messages.Add( user.Name + " has entered the room.");
	}

	private void OnUserLeaveRoom(BaseEvent evt) {
		User user = (User)evt.Params["user"];
		if(user.Name!=username){
			messages.Add( user.Name + " has left the room.");
		}	
	}

	public void OnUserCountChange(BaseEvent evt) {
		Room room = (Room)evt.Params["room"];
		if (room.IsGame ) {
			SetupRoomList();
		}
	}
	
	void OnPublicMessage(BaseEvent evt) {
		try {
			string message = (string)evt.Params["message"];
			User sender = (User)evt.Params["sender"];
			messages.Add(sender.Name +": "+ message);
			
			chatScrollPosition.y = Mathf.Infinity;
			Debug.Log("User " + sender.Name + " said: " + message); 
		}
		catch (Exception ex) {
			Debug.Log("Exception handling public message: "+ex.Message+ex.StackTrace);
		}
	}
	
	
	//PrepareLobby is called from OnLogin, the callback for login
	//so we can be assured that login was successful
	public void PrepareLobby() {
		Debug.Log("Setting up the lobby");
		SetupRoomList();
		isLoggedIn = true;
	}
	
	
	void OnGUI() {
		if (smartFox == null) return;
		screenW = Screen.width;

		// Login
		if (!isLoggedIn) {
			DrawLoginGUI();
		}
		else if (currentActiveRoom != null) 
		{
			// ****** Show full interface only in the Lobby ******* //
			if(currentActiveRoom.Name == "The Lobby")
			{
                SetupRoomList();
				DrawLobbyGUI();
			}
		}
	}
	
	
	private void DrawLoginGUI(){
        GUILayout.BeginArea(new Rect((Screen.width / 2) - (titleImage.width / 2), Screen.height / 2 - (titleImage.height), titleImage.width, titleImage.height * 2));
        GUILayout.BeginVertical();

        //GUI.Box(new Rect(0, 0 , titleImage.width, titleImage.height * 2), "area is here");
        GUILayout.Label(titleImage);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Username: ", GUILayout.MaxWidth(100));
        username = GUILayout.TextField(username, 25, GUILayout.MaxWidth(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Server: ", GUILayout.MaxWidth(100));
        serverName = GUILayout.TextField(serverName, 25, GUILayout.MaxWidth(200));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Port: ", GUILayout.MaxWidth(100));
        serverPort = int.Parse(GUILayout.TextField(serverPort.ToString(), 4, GUILayout.MaxWidth(200)));
        GUILayout.EndHorizontal();

        GUILayout.Label(loginErrorMessage);

        if (GUILayout.Button("Login", GUILayout.MaxWidth(100)) || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
        {
            AddEventListeners();
            smartFox.Connect(serverName, serverPort);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
	}
			
	private void DrawLobbyGUI(){
        DrawUsersGUI(new Rect(10, 10, 180, 300));
        DrawChatGUI(new Rect(Screen.width - 620, 400, 600, 180));
        DrawRoomsGUI(new Rect(200, 10, 600, 300));
	}


    private void DrawUsersGUI(Rect screenPos)
    {

        GUILayout.BeginArea(screenPos);
        GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
        GUILayout.BeginVertical();
        GUILayout.Label("Users");
        userScrollPosition = GUILayout.BeginScrollView(userScrollPosition, false, true, GUILayout.Width(screenPos.width));
        GUILayout.BeginVertical();
        List<User> userList = currentActiveRoom.UserList;
        foreach (User user in userList)
        {
            GUILayout.Label(user.Name);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        // Logout button
        if (GUILayout.Button("Logout"))
        {
            smartFox.Send(new LogoutRequest());
        }
        // Game Room button
        if (currentActiveRoom.Name == "The Lobby")
        {
            if (GUILayout.Button("Make Game"))
            {
				Debug.Log("Make Game Button clicked");


                if (createdMyRoom)
                {
                    smartFox.Send(new JoinRoomRequest(username + " - Room", "", CurrentActiveRoom.Id));
                    return;
                }

                // ****** Create new room ******* //
                int gameLength = 120; //time in seconds

                //let smartfox take care of error if duplicate name
                RoomSettings settings = new RoomSettings(username + " - Room");
                // how many players allowed
                settings.MaxUsers = (short)maxPlayers;
                //settings.GroupId = "create";
                //settings.IsGame = true;

                List<RoomVariable> roomVariables = new List<RoomVariable>();
                //roomVariables.Add(new SFSRoomVariable("host", username));
                roomVariables.Add(new SFSRoomVariable("gameStarted", false));   //a game started bool                

                SFSObject gameInfo = new SFSObject();
                gameInfo.PutUtfString("host", username);                        //the host
                SFSArray playerIDs = new SFSArray(); //[maxPlayers];

                gameInfo.PutSFSArray("playerIDs", playerIDs);                   //the player IDs
                gameInfo.PutInt("numTeams", numTeams);                          //the number of teams

                SFSArray teams = new SFSArray();								//ASSIGN WHICH PLAYERS GO ON WHICH TEAMS
                int[] teamPlayerIndices;										// numTeams = 8, maxPlayers = 16
                for (int i = 0; i < numTeams; i++)								// i = 0, j = 0, j = 1, index = 0, index = 8
                {																// i = 1, j = 0, j = 1, index = 1, index = 9
                    teamPlayerIndices = new int[maxPlayers / numTeams];			// i = 2, j = 0, j = 1, index = 2, index = 10
                    for (int j = 0; j < maxPlayers / numTeams; j++)				// i = 3, j = 0, j = 1, index = 3, index = 11
                    { 															// ...
						int index = i + (j * numTeams);
                    	teamPlayerIndices[j] = index;							// i = 7, j = 0, j = 1, index = 7, index = 15
                    }
					
                    teams.AddIntArray(teamPlayerIndices);
                }
                gameInfo.PutSFSArray("teams", teams);                           //an array of possible values to be grabbed
                gameInfo.PutInt("gameLength", gameLength);                      //the length of the game

                roomVariables.Add(new SFSRoomVariable("gameInfo", gameInfo));


                settings.Variables = roomVariables;
                smartFox.Send(new CreateRoomRequest(settings, true, CurrentActiveRoom));           // Contains: maxUsers, and roomVariables
                createdMyRoom = true;
                Debug.Log("new room " + username + " - Room ");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawRoomsGUI(Rect screenPos)
    {
        roomSelection = -1;

        GUILayout.BeginArea(screenPos);
        GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
        GUILayout.Label("Rooms");
        if (smartFox.RoomList.Count >= 1)
        {
            roomScrollPosition = GUILayout.BeginScrollView(roomScrollPosition, GUILayout.Width(screenPos.width));
            roomSelection = GUILayout.SelectionGrid(roomSelection, roomFullStrings, 1);

            if (roomSelection >= 0 && roomNameStrings[roomSelection] != currentActiveRoom.Name)
            {
                smartFox.Send(new JoinRoomRequest(roomNameStrings[roomSelection], "", CurrentActiveRoom.Id));
            }
            GUILayout.EndScrollView();

        }
        else
        {
            GUILayout.Label("No rooms available to join");
        }
        GUILayout.EndArea();


        GUI.Box(new Rect(screenW - 200, 260, 180, 130), "Room List");
        GUILayout.BeginArea(new Rect(screenW - 190, 290, 180, 150));
        if (smartFox.RoomList.Count >= 1)
        {
            roomScrollPosition = GUILayout.BeginScrollView(roomScrollPosition, GUILayout.Width(180), GUILayout.Height(130));
            roomSelection = GUILayout.SelectionGrid(roomSelection, roomFullStrings, 1);

            if (roomSelection >= 0 && roomNameStrings[roomSelection] != currentActiveRoom.Name)
            {
                smartFox.Send(new JoinRoomRequest(roomNameStrings[roomSelection], "", CurrentActiveRoom.Id));
            }
            GUILayout.EndScrollView();

        }
        else
        {
            GUILayout.Label("No rooms available to join");
        }
        GUILayout.EndArea();
    }

    private void DrawChatGUI(Rect screenPos)
    {
        GUILayout.BeginArea(screenPos);

        GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
        GUILayout.BeginVertical();
        chatScrollPosition = GUILayout.BeginScrollView(chatScrollPosition, false, true, GUILayout.Width(screenPos.width));
        GUILayout.BeginVertical();
        foreach (string message in messages)
        {
            GUILayout.Label(message);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Send", GUILayout.MinWidth(50), GUILayout.MaxWidth(100)) || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
        {
            smartFox.Send(new PublicMessageRequest(newMessage));
            newMessage = "";
        }
        newMessage = GUILayout.TextField(newMessage, 420);


        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
	
	
	
	
	private void SetupRoomList () {
		List<string> rooms = new List<string> ();
		List<string> roomsFull = new List<string> ();
		List<Room> allRooms = smartFox.RoomManager.GetRoomList();
		
		foreach (Room room in allRooms) {
            if (!room.IsGame && room.UserCount > 0)
            {
                rooms.Add(room.Name);
                roomsFull.Add(room.Name + " (" + room.UserCount + "/" + room.MaxUsers + ")");
            }
                       
		}
		
		roomNameStrings = rooms.ToArray();
		roomFullStrings = roomsFull.ToArray();

        if (smartFox.LastJoinedRoom == null)
            smartFox.Send(new JoinRoomRequest("The Lobby"));// "", CurrentActiveRoom.Id));

	}
}
