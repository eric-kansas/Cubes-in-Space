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

    public Texture2D titleImage;
	public Texture2D titleText;
	public Texture2D lobbyImage;
	//public Texture2D[] tutorialImages = new Texture2D[3];
	
	//keep track of room we're in
	private Room currentActiveRoom;
	public Room CurrentActiveRoom{ get {return currentActiveRoom;} }
				
	private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
	private int roomSelection = -1;	  //For clicking on list box 
	private string[] roomNameStrings; //Names of rooms
	private string[] roomFullStrings; //Names and descriptions
	private int screenW;

    private int maxPlayers = 16;
    private int numTeams = 2;

    private bool createdMyRoom = false;
	
	//Login Menus
	private bool isTransitioning = false;
	private int loginState = 1;
	private int transitionCounter = 0;
	public GUIStyle buttonStyle;
	public GUIStyle labelStyle;
	public GUIStyle labelStyle2;
	public GUIStyle	textStyle;
	
	//lobby styles
	public GUIStyle titleStyle;
	public GUIStyle windowStyle;
	public GUIStyle buttonStyle2;
	
	
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
		if (!isLoggedIn) 
		{
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
	
	
	private void DrawLoginGUI()  {
		//set up a few variables
		float leftDrawPoint = (Screen.width / 2) - (titleText.width / 2) + 15;
		int offset = 30;
		int buttonWidth = titleText.width - (offset * 2);
		int buttonHeight = 50;
		int labelHeight = 30;
		int labelWidth = 150;
		int textWidth = titleText.width - labelWidth - (offset * 2);
		int arrowWidth = 30;
				
		
		//draw the background image
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        GUILayout.Label(titleImage);
		GUILayout.EndArea();
		
		//draw the title text
		GUILayout.BeginArea(new Rect(leftDrawPoint, 50, titleText.width, titleText.height));
		GUILayout.Label(titleText);
		GUILayout.EndArea();
		
		//start an area to draw stuff in
		GUILayout.BeginArea(new Rect(leftDrawPoint + offset, titleImage.height, titleImage.width, titleImage.height * 2));
		
		//figure out what to draw
		switch (loginState)
		{
		case (1): //the startup screen
			GUILayout.BeginVertical();
			if (GUILayout.Button("Play Game", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 2;	            
	        }
			if (GUILayout.Button("How to Play", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 4;	            
	        }
			if (GUILayout.Button("Credits", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 7;	            
	        }
			GUILayout.EndVertical();
			break;
			
		case (2): //the play game button has been clicked
			GUILayout.BeginVertical();
	        GUILayout.BeginHorizontal();
	        GUILayout.Label("Username: ", labelStyle, GUILayout.MaxHeight(labelHeight), GUILayout.MaxWidth(labelWidth));
			GUI.SetNextControlName("userNameField");
	        username = GUILayout.TextField(username, textStyle, GUILayout.MaxHeight(labelHeight), GUILayout.MaxWidth(textWidth));
			GUI.FocusControl("userNameField");
	        GUILayout.EndHorizontal();
			

			if (GUILayout.Button("Login", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)) || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
	        {
	            AddEventListeners();
	            smartFox.Connect(serverName, serverPort);
	        }
			
			if (GUILayout.Button("Server Settings", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 3;
	        }
			
			if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 1;
	        }			
			
	        GUILayout.Label(loginErrorMessage);
	        GUILayout.EndVertical();
			break;
			
		case (3): //the advanced options buttons has been clicked
			GUILayout.BeginHorizontal();
	        GUILayout.Label("Server: ", labelStyle, GUILayout.MaxHeight(labelHeight), GUILayout.MaxWidth(labelWidth));
	        serverName = GUILayout.TextField(serverName, textStyle, GUILayout.MaxHeight(labelHeight), GUILayout.MaxWidth(textWidth));
	        GUILayout.EndHorizontal();
	
	        GUILayout.BeginHorizontal();
	        GUILayout.Label("Port: ", labelStyle, GUILayout.MaxHeight(labelHeight), GUILayout.MaxWidth(labelWidth));
	        serverPort = int.Parse(GUILayout.TextField(serverPort.ToString(), textStyle, GUILayout.MaxHeight(labelHeight), GUILayout.MaxWidth(textWidth)));
	        GUILayout.EndHorizontal();
	        
			if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 2;
	        }
			break;
			
			
		case (4): //the how-to-play button has been clicked
			GUILayout.BeginHorizontal(GUILayout.MaxHeight(200));
			GUILayout.Space(50);
			//talk about ojectives
			GUILayout.BeginVertical(GUILayout.MaxHeight(200), GUILayout.MaxWidth(buttonWidth));
			GUILayout.Label("                Objectives", labelStyle);
			GUILayout.Label("-capture cubes by painting them", labelStyle2);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			//have some navigation buttons
			GUILayout.BeginHorizontal(GUILayout.MaxHeight(buttonHeight));
			GUILayout.Space(arrowWidth);
			if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(buttonWidth - (arrowWidth * 2))))
	        {
	            loginState = 1;
	        }
			if (GUILayout.Button(">", buttonStyle, GUILayout.MaxWidth(arrowWidth)))
			{
				loginState = 5; //	move to the how-to-play: point values menu
			}
			GUILayout.EndHorizontal();
			break;
			
		
			
		case (5): //the how-to-play: point values menu
			//talk about ojectives
			GUILayout.BeginVertical(GUILayout.MaxHeight(200), GUILayout.MaxWidth(buttonWidth));
			GUILayout.Label("                Points and Scoring:", labelStyle);
			GUILayout.Label("10  points are normally awarded for painting a cube face", labelStyle2);
			GUILayout.Label(" 5   additional points are awarded for stealing cube faces", labelStyle2);
			GUILayout.Label("10   points are lost when a cube face is stolen", labelStyle2);
			GUILayout.Label("20  bonus points awarded when all sides of get captured", labelStyle2);
			GUILayout.EndVertical();
			
			//have some navigation buttons
			GUILayout.BeginHorizontal(GUILayout.MaxHeight(buttonHeight));
			if (GUILayout.Button("<", buttonStyle, GUILayout.MaxWidth(arrowWidth)))
			{
				loginState = 4; //	move to the how-to-play: objective menu
			}
			if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(buttonWidth - (arrowWidth * 2))))
	        {
	            loginState = 1;
	        }
			if (GUILayout.Button(">", buttonStyle, GUILayout.MaxWidth(arrowWidth)))
			{
				loginState = 6; //	move to the how-to-play: painting menu
			}
			GUILayout.EndHorizontal();
			break;
			
		case (6): //the how-to-play: painting menu
			GUILayout.BeginHorizontal(GUILayout.MaxHeight(200));
			GUILayout.Space(50);
			//talk about painting
			GUILayout.BeginVertical(GUILayout.MaxHeight(200), GUILayout.MaxWidth(buttonWidth-50));
			GUILayout.Label("               Painting:", labelStyle);
			GUILayout.Label("You have a limited amount of paint", labelStyle2);
			GUILayout.Label("It is shown by the number at the bottom of the screen", labelStyle2);
			GUILayout.Label("Follow the arrow to a locked cube to refill it", labelStyle2);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			//have some navigation buttons
			GUILayout.BeginHorizontal(GUILayout.MaxHeight(buttonHeight));
			if (GUILayout.Button("<", buttonStyle, GUILayout.MaxWidth(arrowWidth)))
			{
				loginState = 5; //	move to the how-to-play: points menu
			}
			if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(buttonWidth - (arrowWidth * 2))))
	        {
	            loginState = 1;
	        }
			GUILayout.EndHorizontal();
			break;
			
		case (7): //the credits button has been clicked
			GUILayout.BeginHorizontal(GUILayout.MaxHeight(200));
			GUILayout.Space(50);
			//add a section about the creators
			GUILayout.BeginVertical(GUILayout.MaxWidth(buttonWidth / 2));
			GUILayout.Label("Developers:", labelStyle);
			GUILayout.Label("-Jon Hughes", labelStyle2);
			GUILayout.Label("-Eric 'Kansas' Heaney", labelStyle2);
			GUILayout.Label("-Jack McDonald", labelStyle2);
			GUILayout.Label("-Kyler Mulherin", labelStyle2);
			GUILayout.Label("-Jacob Smith", labelStyle2);
			GUILayout.EndVertical();
			
			//add a section about the images
			GUILayout.BeginVertical(GUILayout.MaxWidth(buttonWidth / 2));
			GUILayout.Label("Images and Textures by:", labelStyle);
			GUILayout.Label("-Jon Hughes", labelStyle2);
			GUILayout.Label("-supakilla9", labelStyle2);
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			
			
			if (GUILayout.Button("Back", buttonStyle, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(buttonHeight)))
	        {
	            loginState = 1;
	        }
			break;
			
		default:
			loginState = 1;
			break;
		}
		
		GUILayout.EndArea(); //finish drawing
		
	}

	
	private void DrawLobbyGUI(){
		//draw values
		int titleHeight = 100;
		int margin = 20;
		int buttonHeight = 100;
		int buttonTop = Screen.height - buttonHeight;
		int userWidth = 200;
		int userHeight = Screen.height - (titleHeight + buttonHeight + (margin*3));
		int chatTop = titleHeight + (margin * 2);
		int chatLeft = userWidth + (margin * 2);
		int roomWidth = userWidth;
		int chatWidth = Screen.width - (userWidth + roomWidth + (margin * 4));
		int roomLeft = chatLeft + chatWidth + margin;

		
		//draw the background 
		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
		GUILayout.Box(lobbyImage);
		GUILayout.EndArea();
		
		//draw the title
		GUILayout.BeginArea(new Rect(0,margin, Screen.width, titleHeight));
		GUILayout.Label("The Lobby", titleStyle);
		GUILayout.EndArea();
		
		
        DrawUsersGUI(new Rect(margin, chatTop, userWidth, userHeight));
        DrawChatGUI(new Rect(chatLeft, chatTop, chatWidth, userHeight));
        DrawRoomsGUI(new Rect(roomLeft, chatTop, roomWidth, userHeight));
		DrawButtonsGUI(new Rect(margin, buttonTop, Screen.width - (margin * 2), buttonHeight), userWidth, roomWidth);
	}


    private void DrawUsersGUI(Rect screenPos)
    {
        GUILayout.BeginArea(screenPos);
		
		GUILayout.Label("Players In The Lobby", labelStyle);
        GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
		
        GUILayout.BeginVertical();
		
		//the users window
        userScrollPosition = GUILayout.BeginScrollView(userScrollPosition, false, true, GUILayout.Width(screenPos.width));
        GUILayout.BeginVertical();
        List<User> userList = currentActiveRoom.UserList;
        foreach (User user in userList)
        {
            GUILayout.Label(user.Name, labelStyle2);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
		
		
		
        GUILayout.EndVertical();
		
        GUILayout.EndArea();
    }

    private void DrawRoomsGUI(Rect screenPos)
    {
        roomSelection = -1;

        GUILayout.BeginArea(screenPos);
		GUILayout.Label("Join A Game", labelStyle);
		
        GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
        if (smartFox.RoomList.Count >= 1)
        {
            roomScrollPosition = GUILayout.BeginScrollView(roomScrollPosition, GUILayout.Width(screenPos.width));
            roomSelection = GUILayout.SelectionGrid(roomSelection, roomFullStrings, 1, labelStyle2);

            if (roomSelection >= 0 && roomNameStrings[roomSelection] != currentActiveRoom.Name)
            {
                smartFox.Send(new JoinRoomRequest(roomNameStrings[roomSelection], "", CurrentActiveRoom.Id));
            }
            GUILayout.EndScrollView();

        }
        else
        {
            GUILayout.Label("No rooms available to join", labelStyle2);
        }
        GUILayout.EndArea();
      
    }

    private void DrawChatGUI(Rect screenPos)
    {
		int talkWindowHeight = 150;
		int chatWindowHeight = (int) (screenPos.height - talkWindowHeight);

        GUILayout.BeginArea(screenPos);
		
		GUILayout.Label("        Chat with other players", labelStyle);
		
		//chat window
        GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
        GUILayout.BeginVertical();
        chatScrollPosition = GUILayout.BeginScrollView(chatScrollPosition, false, true, GUILayout.Width(screenPos.width), GUILayout.MaxHeight(chatWindowHeight));
        GUILayout.BeginVertical();
        foreach (string message in messages)
        {
            GUILayout.Label(message, labelStyle2);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
	
        GUILayout.BeginVertical();
		//format the input text field
		GUIStyle aTextStyle = new GUIStyle(textStyle);
		aTextStyle.wordWrap = true;
		aTextStyle.alignment = TextAnchor.UpperLeft;
		aTextStyle.padding.left = 15;
		aTextStyle.padding.right = 15;
		aTextStyle.padding.top = 20;
		aTextStyle.padding.bottom = 10;
		aTextStyle.fontSize = 30;
		
		//make the text field
		GUI.SetNextControlName("chatField");
        newMessage = GUILayout.TextField(newMessage, aTextStyle, GUILayout.MaxWidth(screenPos.width), GUILayout.MaxHeight(talkWindowHeight));
		GUI.FocusControl("chatField");
		//send button
        if (GUILayout.Button("Send", buttonStyle, GUILayout.MaxWidth(screenPos.width), GUILayout.MaxHeight(100)) || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
        {
            smartFox.Send(new PublicMessageRequest(newMessage));
            newMessage = "";
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
	
	private void DrawButtonsGUI(Rect screenPos, int userBoxWidth, int gameBoxWidth)
	{
       	GUILayout.BeginArea(screenPos);
		GUILayout.BeginHorizontal();
		// Logout button
     	if (GUILayout.Button("Logout", buttonStyle, GUILayout.MaxWidth(userBoxWidth + 50)))
        {
            smartFox.Send(new LogoutRequest());
        }
		
		GUILayout.Space(screenPos.width - (userBoxWidth + gameBoxWidth + 125));
		
        // Game Room button
		if (GUILayout.Button("Make Game", buttonStyle, GUILayout.MaxWidth(gameBoxWidth + 125)))
        {
	        if (currentActiveRoom.Name == "The Lobby")
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


                //hmmmmm
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
		GUILayout.EndArea();
	}
	
	
	private void SetupRoomList () {
		List<string> rooms = new List<string> ();
		List<string> roomsFull = new List<string> ();
		List<Room> allRooms = smartFox.RoomManager.GetRoomList();
		
		foreach (Room room in allRooms) {
            if (!room.IsGame && room.UserCount > 0 && room.Name != "The Lobby")
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
