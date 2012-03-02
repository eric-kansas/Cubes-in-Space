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


public class GameLobby : MonoBehaviour
{

    private SmartFox smartFox;
    public string username = "";
    private string loginErrorMessage = "";

    private string newMessage = "";
    private ArrayList messages = new ArrayList();

    public GUISkin gSkin;

    //keep track of room we're in
    private Room currentActiveRoom;
    public Room CurrentActiveRoom { get { return currentActiveRoom; } }
	private bool tryJoiningRoom;
	
	
    private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
    private int roomSelection = -1;	  //For clicking on list box 
    private string[] roomNameStrings; //Names of rooms
    private string[] roomFullStrings; //Names and descriptions
    private int screenW;

    private SFSArray currentIDs = new SFSArray();
    private int[] currentTeams;

    SFSObject lobbyGameInfo;
    string host;
    int numberOfPlayers;
    int numberOfTeams;
    SFSArray teams;
    int gameLength;
    int maxPlayers;

    private bool showGameList = false;
    private bool showTeamList = false;
    private int gameListEntry = 0;
    private int teamListEntry = 0;
    private GUIContent[] gameLengthList = new GUIContent[4];
    private GUIContent[] teamNumberList = new GUIContent[7];
    private GUIStyle listStyle = new GUIStyle();
    private bool teamPicked = false;
    private bool gamePicked = false;

    private List<Vector2> teamScrollPositions = new List<Vector2>();
    private int playerPerTeam;

    void Start()
    {
        Debug.Log("start game lobby");
        smartFox = SmartFoxConnection.Connection;
        currentActiveRoom = smartFox.LastJoinedRoom;

        smartFox.AddLogListener(LogLevel.INFO, OnDebugMessage);
        screenW = Screen.width;
        AddEventListeners();

        username = smartFox.MySelf.Name;

        lobbyGameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();

        maxPlayers = currentActiveRoom.MaxUsers;
        host = lobbyGameInfo.GetUtfString("host");
        numberOfTeams = lobbyGameInfo.GetInt("numTeams");
        Debug.Log("start number of teams: " + numberOfTeams);
        if (GameValues.isHost)
            currentTeams = new int[8];

        teams = (SFSArray)lobbyGameInfo.GetSFSArray("teams");
        playerPerTeam = numberOfPlayers / numberOfTeams;
        gameLength = (int)lobbyGameInfo.GetInt("gameLength");

        //Make Contents for the popup gameLength list
        gameLengthList[0] = (new GUIContent("30"));
        gameLengthList[1] = (new GUIContent("60"));
        gameLengthList[2] = (new GUIContent("90"));
        gameLengthList[3] = (new GUIContent("120"));

        //Make Contents for the popup number of teams list
        teamNumberList[0] = (new GUIContent("2"));
        teamNumberList[1] = (new GUIContent("3"));
        teamNumberList[2] = (new GUIContent("4"));
        teamNumberList[3] = (new GUIContent("5"));
        teamNumberList[4] = (new GUIContent("6"));
        teamNumberList[5] = (new GUIContent("7"));
        teamNumberList[6] = (new GUIContent("8"));

        //Set team scroll positions to zero
        for(int i = 0; i < 8; i++)
        {
            teamScrollPositions.Add(Vector2.zero);
        }

        //Make a gui style for the lists
        listStyle.normal.textColor = Color.white;
        Texture2D tex = new Texture2D(2, 2);
        Color[] colors = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            colors[i] = Color.white;
        }
        tex.SetPixels(colors);
        tex.Apply();
        listStyle.hover.background = tex;
        listStyle.onHover.background = tex;
        listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

        if (GameValues.isHost)
        {
            currentIDs.AddInt(0);
            GameValues.playerID = 0;
            GameValues.teamNum = 0;
            currentTeams[0]++;
            List<UserVariable> uData = new List<UserVariable>();
            uData.Add(new SFSUserVariable("playerID", GameValues.playerID));
            uData.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
            smartFox.Send(new SetUserVariablesRequest(uData));

            SFSObject gameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();

            //send back to store on server
            List<RoomVariable> rData = new List<RoomVariable>();
            gameInfo.PutSFSArray("playerIDs", currentIDs);
            rData.Add(new SFSRoomVariable("gameInfo", gameInfo));
            smartFox.Send(new SetRoomVariablesRequest(rData));

        }

        if (currentActiveRoom.ContainsVariable("lastGameScores"))
        {
            Debug.Log("scores from last game are present, adding to the message window...");
            //write out the last games scores to the chat window
            //format:
            //scores: int array
            //winner: string
            SFSObject lastGameData = (SFSObject)currentActiveRoom.GetVariable("lastGameScores").GetSFSObjectValue();
            int[] scoresArray = lastGameData.GetIntArray("scores");
            string[] teamColors = lastGameData.GetUtfStringArray("teamColors");

            messages.Add("Previous Game's Scores");
            messages.Add("The Winner is... " + lastGameData.GetUtfString("winner").ToString());
            for (int i = 0; i < scoresArray.Length; i++)
            {
                string teamName = teamColors[i] + " Team";
                messages.Add(teamName + "\tscored " + scoresArray[i].ToString() + " points.");
            }
        }


		tryJoiningRoom = false;
    }

    private void AddEventListeners()
    {

        smartFox.RemoveAllEventListeners();

        smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
        smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
        smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
        smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
        smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
        smartFox.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnJoinRoomError);
        
    }

    void FixedUpdate()
    {
        //this is necessary to have any smartfox action!
        smartFox.ProcessEvents();
    }

    private void UnregisterSFSSceneCallbacks()
    {
        smartFox.RemoveAllEventListeners();
    }

    void OnLogout(BaseEvent evt)
    {
        Debug.Log("OnLogout");
        //isLoggedIn = false;
        currentActiveRoom = null;
        smartFox.Disconnect();
    }

    public void OnConnectionLost(BaseEvent evt)
    {
        Debug.Log("OnConnectionLost");
        UnregisterSFSSceneCallbacks();
        currentActiveRoom = null;
        roomSelection = -1;
        Application.LoadLevel("The Lobby");
    }

    public void OnDebugMessage(BaseEvent evt)
    {
        string message = (string)evt.Params["message"];
        Debug.Log("[SFS DEBUG] " + message);
    }

    public void OnRoomCreationError(BaseEvent evt)
    {
        Debug.Log("Error creating room: " + evt.Params["message"]);
    }

    public void OnJoinRoom(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        currentActiveRoom = room;

        Debug.Log("onjoinroom = " + currentActiveRoom.Name);

        if (room.Name == "The Lobby")
        {
            //smartFox.RemoveAllEventListeners();
            Debug.Log("onjoinroom = " + smartFox.LastJoinedRoom.Name);
            Application.LoadLevel(room.Name);
        }
        else if (room.IsGame)
        {
            //Debug.Log("is game!!!!");
            //store my own color on server as user data
            List<UserVariable> uData = new List<UserVariable>();
            uData.Add(new SFSUserVariable("playerID", GameValues.playerID));
            smartFox.Send(new SetUserVariablesRequest(uData));
            Application.LoadLevel("testScene");
        }
        else
        {
            Debug.Log("GameLobby- OnJoinRoom: joined " + room.Name);
            Application.LoadLevel("Game Lobby");
            Debug.Log("loading Game Lobby");
            //smartFox.Send(new SpectatorToPlayerRequest());
        }
    }

    public void OnJoinRoomError(BaseEvent evt)
    {
        Debug.Log("Error joining room: " + evt.Params["message"]);
        if (tryJoiningRoom)
		{
			String[] nameParts = this.currentActiveRoom.Name.Split('-');
        	smartFox.Send(new JoinRoomRequest(nameParts[0].Trim() + " - Game", "", CurrentActiveRoom.Id));
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
        messages.Add(user.Name + " has entered the room.");
        if (GameValues.isHost)
        {
            Debug.Log("host passing host");
            AddPlayerID(user.Name);
        }
    }

    public void OnUserExitRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        //messages.Add(user.Name + " has exit the room.");
        if(GameValues.isHost && user.Name != host)
        {
            RemovePlayer(user.GetVariable("playerID").GetIntValue(), user.GetVariable("playerTeam").GetIntValue());
        }
    }

    private void OnObjectMessageReceived(BaseEvent evt)
    {
        User sender = (User)evt.Params["sender"];
        ISFSObject data = (SFSObject)evt.Params["message"];

        Debug.Log("here ib ob mess");

        if (GameValues.isHost)
        {
            
            if (data.ContainsKey("RequestTeamID"))
            {
                int passedPlayerTeamRequest = data.GetInt("RequestTeamID");

                //add if there is room on team; not invalid index
                if (!(currentTeams[passedPlayerTeamRequest] + 1 > playerPerTeam) || !(passedPlayerTeamRequest < 0) || !(passedPlayerTeamRequest > numberOfTeams - 1))
                {
                    int passedPlayerTeam = data.GetInt("CurrentTeamID");
                    currentTeams[passedPlayerTeamRequest]++;
                    currentTeams[passedPlayerTeam]--;

                    Debug.Log("Left team has: " + passedPlayerTeam);
                    Debug.Log("Joined team has: " + passedPlayerTeamRequest);
                    SFSObject returnData = new SFSObject();
					returnData.PutUtfString("name", data.GetUtfString("name"));
                    returnData.PutBool("teamRequest", true);
                    returnData.PutInt("team", passedPlayerTeamRequest);
                    smartFox.Send(new ObjectMessageRequest(returnData));
                }
            }
        }
        else //client
        {
            if (data.ContainsKey("name"))
            {
                if (data.GetUtfString("name") == username)
                {
                    //store player id and team as user data
                    List<UserVariable> uData = new List<UserVariable>();
                    if (data.ContainsKey("id"))
                    {
                        //get player id
                        GameValues.playerID = data.GetInt("id");
                        Debug.Log("Player ID: " + GameValues.playerID);
                        uData.Add(new SFSUserVariable("playerID", GameValues.playerID));
                    }
  

                    GameValues.teamNum = data.GetInt("team");
                    Debug.Log("Player is joining team #" + GameValues.teamNum);

                    uData.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
                    smartFox.Send(new SetUserVariablesRequest(uData));
                }

            }
        }


        /***/
    }
	

    public void OnRoomVariablesUpdate(BaseEvent evt)
    {
        
        Room room = (Room)evt.Params["room"];
        ArrayList changedVars = (ArrayList)evt.Params["changedVars"];
		//Debug.Log("ROOM VARS: " + changedVars.Contains("gameInfo"));
        if (!GameValues.isHost)
        {
            // Check if the "gameStarted" variable was changed
            if (changedVars.Contains("gameStarted"))
            {
                if (room.GetVariable("gameStarted").GetBoolValue() == true)
                {
                    Debug.Log("Game started in room vars");
                    String[] nameParts = this.currentActiveRoom.Name.Split('-');
                    smartFox.Send(new JoinRoomRequest(nameParts[0].Trim() + " - Game", "", CurrentActiveRoom.Id));
                    Debug.Log(nameParts[0].Trim() + " - Game");
					tryJoiningRoom = true;
                }
                else
                {
                    Debug.Log("Game stopped");
                }
            }
			if (changedVars.Contains("gameInfo"))
			{
				Debug.Log("here in room vars gameinfo");
				numberOfTeams = room.GetVariable("gameInfo").GetSFSObjectValue().GetInt("numTeams");
			}
        }
 
    }

    private void OnUserLeaveRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        if (user.Name != username)
        {
            messages.Add(user.Name + " has left the room.");
        }
    }

    public void OnUserCountChange(BaseEvent evt)
    {
        Room room = (Room)evt.Params["room"];
        if (room.IsGame)
        {
            SetupRoomList();
        }
    }

    void OnPublicMessage(BaseEvent evt)
    {
        try
        {
            string message = (string)evt.Params["message"];
            User sender = (User)evt.Params["sender"];
            messages.Add(sender.Name + ": " + message);

            chatScrollPosition.y = Mathf.Infinity;
            Debug.Log("User " + sender.Name + " said: " + message);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception handling public message: " + ex.Message + ex.StackTrace);
        }
    }


    //PrepareLobby is called from OnLogin, the callback for login
    //so we can be assured that login was successful
    private void PrepareLobby()
    {
        Debug.Log("Setting up the lobby");
        SetupRoomList();
    }


    void OnGUI()
    {
        if (smartFox == null) return;
        screenW = Screen.width;


        if (currentActiveRoom != null)
        {
            // ****** Show full interface only in the Lobby ******* //
            if (!currentActiveRoom.IsGame)
            {
                //Debug.Log("DRAWING!!!");
                DrawUsersGUI(new Rect(10, 10, 180, 300));
                DrawSettingsGUI(new Rect(200, 10, 600, 300));
                DrawChatGUI(new Rect(Screen.width - 620, 400, 600, 180));
                //DrawGameLobbyGUI();
            }
        }
    }

    private void DrawSettingsGUI(Rect screenPos)
    {
        GUILayout.BeginArea(screenPos);
            GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
            //begin elements
            GUILayout.BeginVertical();
                //begin setting bar
				if(GameValues.isHost)
				{
	                GUILayout.BeginHorizontal();
	                    // Number of teams options
	                    GUILayout.Label("Number of teams: ");
	                    if (Popup.List(new Rect(110, 3, 75, 17), ref showTeamList, ref teamListEntry, new GUIContent("Click here"), teamNumberList, listStyle))
	                    {
	                        numberOfTeams = Convert.ToInt32(teamNumberList[teamListEntry].text);
	                        List<RoomVariable> tData = new List<RoomVariable>();
	                        lobbyGameInfo.PutInt("numTeams", numberOfTeams);
							Debug.Log("send num teams change");
	                        tData.Add(new SFSRoomVariable("gameInfo", lobbyGameInfo));
	                        smartFox.Send(new SetRoomVariablesRequest(tData));
	
	                        playerPerTeam = numberOfPlayers / numberOfTeams;
	                        ReallocateTeams();
	                    }
	
	                    // Length of games options
	                    GUILayout.Label("Length of games: ");
	                    if (Popup.List(new Rect(410, 3, 75, 17), ref showGameList, ref gameListEntry, new GUIContent("Click here"), gameLengthList, listStyle))
	                        gamePicked = true;
	                    if (gamePicked)
	                        gameLength = Convert.ToInt32(gameLengthList[gameListEntry].text);
	                        List<RoomVariable> rData = new List<RoomVariable>();
	                        lobbyGameInfo.PutInt("gameLength", gameLength);
	                        rData.Add(new SFSRoomVariable("gameInfo", lobbyGameInfo));
	                        smartFox.Send(new SetRoomVariablesRequest(rData));
	                GUILayout.EndHorizontal();
				}
            //end elements
                GUILayout.BeginVertical();
                if (numberOfTeams < 4)
                {
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < numberOfTeams; i++)
                    {
                        DrawSingleTeamBox(screenPos, i);
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < 4; i++)
                    {
                        DrawSingleTeamBox(screenPos, i);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < (numberOfTeams - 4); i++)
                    {
                        DrawSingleTeamBox(screenPos, i+4);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void ReallocateTeams()
    {
        currentTeams = new int[8];
        /*host stuff*/
        GameValues.teamNum = 0;
        currentTeams[0]++;        
        List<UserVariable> uData = new List<UserVariable>();
        uData.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
        smartFox.Send(new SetUserVariablesRequest(uData));

        foreach (User user in currentActiveRoom.UserList)
        {
            if (!user.IsItMe)
            {
                SFSObject data = new SFSObject();
                data.PutUtfString("name", user.Name);
                int playerTeamIndex = findLowestNumTeam();
                data.PutInt("team", playerTeamIndex);
                if (GameValues.isHost)
                    currentTeams[playerTeamIndex]++;
                smartFox.Send(new ObjectMessageRequest(data));
            }
        }
    }

    private void DrawSingleTeamBox(Rect screenPos, int team)
    {
        GUILayout.BeginVertical();
        //GUILayout.TextArea("ye dude " + team, GUILayout.MinHeight((screenPos.height / 2) - 40));
        teamScrollPositions[team] = GUILayout.BeginScrollView(teamScrollPositions[team], GUILayout.MinHeight((screenPos.height / 2) - 40), GUILayout.MaxWidth(screenPos.width/4));
        GUILayout.BeginVertical();
        List<User> userList = currentActiveRoom.UserList;
        GUILayout.Label("Team:  " + team );
        foreach (User user in userList)
        {
            if (user.GetVariable("playerTeam") != null)
            {
                if (user.GetVariable("playerTeam").GetIntValue() == team)
                {
                    GUILayout.Label("--> " + user.Name);
                }
            }
            
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        if (GUILayout.Button("Join", GUILayout.MaxHeight(17)))
        {
            RequestTeamChange(team);
        }
        GUILayout.EndVertical();
    }

    private void RequestTeamChange(int teamIndex)
    {
        SFSObject data = new SFSObject();
        if (GameValues.isHost)
        {
            currentTeams[teamIndex]++;
            currentTeams[GameValues.teamNum]--;
            GameValues.teamNum = teamIndex;
            List<UserVariable> uData = new List<UserVariable>();
            uData.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
            smartFox.Send(new SetUserVariablesRequest(uData));
            Debug.Log("joined another team:" + GameValues.teamNum);
        }
        else
        {
			data.PutUtfString("name", username);
            data.PutInt("RequestTeamID", teamIndex);
            data.PutInt("CurrentTeamID", GameValues.teamNum);
            smartFox.Send(new ObjectMessageRequest(data));
        }
    }
    //private string ReturnTeam(

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
        if (GUILayout.Button("Leave Game"))
        {
            //RemovePlayerID(GameValues.playerID);
            smartFox.Send(new JoinRoomRequest("The Lobby", "", CurrentActiveRoom.Id));
        }
        DrawRoomsGUI();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawRoomsGUI()
    {
        if (GameValues.isHost)
        {
            //start game button event listener
            if (GUILayout.Button("Start Game"))
            {

                // ****** Create the actual game ******* //
                String[] nameParts = this.currentActiveRoom.Name.Split('-');
                String gameName = nameParts[0].Trim() + " - Game";
                Debug.Log("Host created game named: " + gameName);

                RoomSettings settings = new RoomSettings(gameName);
                settings.MaxUsers = (short)currentActiveRoom.MaxUsers; // how many players allowed: 12
                settings.Extension = new RoomExtension(GameManager.ExtName, GameManager.ExtClass);
                settings.IsGame = true;
                settings.MaxVariables = 15;

                //get the variables set up from the lobby
                List<RoomVariable> lobbyVars = currentActiveRoom.GetVariables();
                SFSObject lobbyGameInfo = (SFSObject)((RoomVariable)lobbyVars[1]).GetSFSObjectValue();
                //lobbyGameInfo.GetSFSArray("
                //FORMAT BY INDEX
                //0 = (bool)        gameStarted
                //1 = (SFSObject)   gameInfo
                //      -(string)   the host username               key: "host"
                //      -(IntArray) playerIds                       key: "playerIDs"
                //      -(int)      number of Teams                 key: "numTeams"
                //      -(SFSArray) teams                           key: "teams"
                //      -(int)      length of the game in seconds   key: "gameLength"

                SFSRoomVariable gameInfo = new SFSRoomVariable("gameInfo", lobbyGameInfo);
                settings.Variables.Add(gameInfo);

                Debug.Log("numberOfPlayers: " + currentActiveRoom.UserCount);
                SFSRoomVariable userCountVar = new SFSRoomVariable("numberOfPlayers", currentActiveRoom.UserCount);
                settings.Variables.Add(userCountVar);

                SFSArray joinedPlayers = new SFSArray();
                SFSRoomVariable joinedVar = new SFSRoomVariable("playersJoined", joinedPlayers);
                settings.Variables.Add(joinedVar);

                if (GameValues.isHost)
                {
                    Debug.Log("user join room and is me and host");
                    //let other players know to switch rooms
                    List<RoomVariable> roomVars = new List<RoomVariable>();
                    roomVars.Add(new SFSRoomVariable("gameStarted", true));
                    smartFox.Send(new SetRoomVariablesRequest(roomVars));
                }

                //get the values from the appropriate fields to populate the gameInfo
                smartFox.Send(new CreateRoomRequest(settings, true, currentActiveRoom));
            }
        }
        else
        {
            //start game button event listener
            if (GUILayout.Button("Waiting..."))
            {
            }
        }
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

    private void AddPlayerID(string user)
    {
        currentIDs.AddInt(currentIDs.Size());

        Debug.Log("player id getting passed as: " + (currentIDs.Size() - 1));
        SFSObject gameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();

        //send back to store on server
        List<RoomVariable> rData = new List<RoomVariable>();
        gameInfo.PutSFSArray("playerIDs", currentIDs);
        rData.Add(new SFSRoomVariable("gameInfo", gameInfo));
        smartFox.Send(new SetRoomVariablesRequest(rData));

        Debug.Log("player name: " + user);
        SFSObject data = new SFSObject();
        data.PutUtfString("name", user);
        data.PutInt("id", currentIDs.Size() - 1);

        int playerTeamIndex = findLowestNumTeam();
        Debug.Log("assigning: " + playerTeamIndex);
        data.PutInt("team", playerTeamIndex);
        if(GameValues.isHost)
            currentTeams[playerTeamIndex]++;
        smartFox.Send(new ObjectMessageRequest(data));

        for (int i = numberOfTeams-1; i >= 0; i--)
        {
            Debug.Log("team " + i + ": " + currentTeams[i]);
        }
        
    }

    private int findLowestNumTeam()
    {
        int lowest = 25;
        int returnIndex = 0;
        for (int i = numberOfTeams-1; i >= 0; i--)
        {
            if (currentTeams[i] <= lowest)
            {
                lowest = currentTeams[i];
                returnIndex= i;
            }
        }
        return returnIndex;
    }

    private void RemovePlayer(int id, int teamId)
    {
        SFSObject gameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();
        SFSArray idsLeft = (SFSArray)gameInfo.GetSFSArray("playerIDs");

        //update room variable 
        SFSArray returnInts = new SFSArray();
        returnInts.AddInt(GameValues.playerID);
        Debug.Log("here in stuff: " + returnInts.GetInt(0));
        for (int i = 0; i < idsLeft.Size(); i++)
        {
            returnInts.AddInt(idsLeft.GetInt(i));
            Debug.Log("here in stuff: " + returnInts.GetInt(i + 1));
        }

        for (int i = 0; i < currentIDs.Size(); i++)
        {
            if (currentIDs.GetInt(i) == id)
            {
                currentIDs.RemoveElementAt(i);
                break;
            }
        }
        
        currentTeams[teamId]--;

        //send back to store on server
        List<RoomVariable> rData = new List<RoomVariable>();
        gameInfo.PutSFSArray("playerIDs", returnInts);
        rData.Add(new SFSRoomVariable("gameInfo", gameInfo));
        smartFox.Send(new SetRoomVariablesRequest(rData));
    }

    private void SetupRoomList()
    {
        List<string> rooms = new List<string>();
        List<string> roomsFull = new List<string>();

        List<Room> allRooms = smartFox.RoomManager.GetRoomList();

        foreach (Room room in allRooms)
        {
            rooms.Add(room.Name);
            roomsFull.Add(room.Name + " (" + room.UserCount + "/" + room.MaxUsers + ")");
        }

        roomNameStrings = rooms.ToArray();
        roomFullStrings = roomsFull.ToArray();

        if (smartFox.LastJoinedRoom == null)
        {
            smartFox.Send(new JoinRoomRequest("The Lobby","",currentActiveRoom.Id));
        }
    }
}
