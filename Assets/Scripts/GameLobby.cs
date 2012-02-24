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

    private Vector2 roomScrollPosition, userScrollPosition, chatScrollPosition;
    private int roomSelection = -1;	  //For clicking on list box 
    private string[] roomNameStrings; //Names of rooms
    private string[] roomFullStrings; //Names and descriptions
    private int screenW;

    private SFSArray currentIDs = new SFSArray();

    SFSObject lobbyGameInfo;
    string host;
    int numberOfPlayers;
    int numberOfTeams;
    SFSArray teams;
    int gameLength;
    int maxPlayers;

    void Start()
    {
        smartFox = SmartFoxConnection.Connection;
        currentActiveRoom = smartFox.LastJoinedRoom;
        Debug.Log("last joined room: " + currentActiveRoom);
        smartFox.AddLogListener(LogLevel.INFO, OnDebugMessage);
        screenW = Screen.width;
        AddEventListeners();
        username = smartFox.MySelf.Name;
        if (GameValues.isHost)
        {
            currentIDs.AddInt(0);
            GameValues.playerID = 0;
            GameValues.teamNum = 0;

            List<UserVariable> uData = new List<UserVariable>();
            uData.Add(new SFSUserVariable("playerID", GameValues.playerID));
            uData.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
            smartFox.Send(new SetUserVariablesRequest(uData));
        }


        lobbyGameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();

        maxPlayers = currentActiveRoom.MaxUsers;
        host = lobbyGameInfo.GetUtfString("host");
        numberOfTeams = lobbyGameInfo.GetInt("numTeams");
        teams = (SFSArray)lobbyGameInfo.GetSFSArray("teams");

        gameLength = (int)lobbyGameInfo.GetInt("gameLength") * 1000;
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
        String[] nameParts = this.currentActiveRoom.Name.Split('-');
        smartFox.Send(new JoinRoomRequest(nameParts[0] + " - Game", "", CurrentActiveRoom.Id));
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
        if(GameValues.isHost)
        {
            RemovePlayerID(user.GetVariable("playerID").GetIntValue());
        }
    }

    private void OnObjectMessageReceived(BaseEvent evt)
    {
        User sender = (User)evt.Params["sender"];
        ISFSObject data = (SFSObject)evt.Params["message"];

        Debug.Log("here in object message the sender is: " + sender.Name);
        Debug.Log("here name: " + data.GetUtfString("name"));
        Debug.Log("here id: " + data.GetInt("id"));
        Debug.Log("username: " + username);
        //deal with messages from other players
        //case 1: A player has launched somewhere
        if (data.ContainsKey("name"))
        {
            if (data.GetUtfString("name") == username)
            {

                //get player id
                GameValues.playerID = data.GetInt("id");
                     
                Debug.Log("Player ID: " + GameValues.playerID);

                //setup teams
                int[] teamPlayerIndices;										// numTeams = 8, maxPlayers = 16
                for (int i = 0; i < numberOfTeams; i++)								// i = 0, j = 0, j = 1, index = 0, index = 8
                {																// i = 1, j = 0, j = 1, index = 1, index = 9
                    teamPlayerIndices = new int[maxPlayers / numberOfTeams];			// i = 2, j = 0, j = 1, index = 2, index = 10
                    for (int j = 0; j < maxPlayers / numberOfTeams; j++)				
                    {
                        int index = i + (j * numberOfTeams);
                        if (index == GameValues.playerID)
                        {
                            GameValues.teamNum = i;
                            break;
                        }
                    }
                }
                Debug.Log("Player is joining team #" + GameValues.teamNum);


                //store player id and team as user data
                List<UserVariable> uData = new List<UserVariable>();
                uData.Add(new SFSUserVariable("playerID", GameValues.playerID));
                uData.Add(new SFSUserVariable("playerTeam", GameValues.teamNum));
                smartFox.Send(new SetUserVariablesRequest(uData));
            }
        }


    }
	

    public void OnRoomVariablesUpdate(BaseEvent evt)
    {
        //Debug.Log("ROOM VARS");
        Room room = (Room)evt.Params["room"];
        ArrayList changedVars = (ArrayList)evt.Params["changedVars"];

        if (!GameValues.isHost)
        {
            // Check if the "gameStarted" variable was changed
            if (changedVars.Contains("gameStarted"))
            {
                if (room.GetVariable("gameStarted").GetBoolValue() == true)
                {
                    Debug.Log("Game started in room vars");
                    String[] nameParts = this.currentActiveRoom.Name.Split('-');
                    smartFox.Send(new JoinRoomRequest(nameParts[0] + " - Game", "", CurrentActiveRoom.Id));
                    Debug.Log(nameParts[0] + " - Game");
                }
                else
                {
                    Debug.Log("Game stopped");
                }
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
                DrawLobbyGUI();
                DrawRoomsGUI();
                //DrawGameLobbyGUI();
            }
        }
    }

    private void DrawLobbyGUI()
    {
        DrawUsersGUI();
        DrawChatGUI();

        // Send message
        newMessage = GUI.TextField(new Rect(10, 480, 370, 20), newMessage, 50);
        if (GUI.Button(new Rect(390, 478, 90, 24), "Send") || (Event.current.type == EventType.keyDown && Event.current.character == '\n'))
        {
            smartFox.Send(new PublicMessageRequest(newMessage));
            newMessage = "";
        }
        // Logout button
        if (GUI.Button(new Rect(screenW - 115, 20, 85, 24), "Leave Game"))
        {
            //RemovePlayerID(GameValues.playerID);
            
            smartFox.Send(new JoinRoomRequest("The Lobby", "", CurrentActiveRoom.Id));
   
        }
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
        smartFox.Send(new ObjectMessageRequest(data));
            
    }

    private void RemovePlayerID(int id)
    {
        SFSObject gameInfo = (SFSObject)currentActiveRoom.GetVariable("gameInfo").GetSFSObjectValue();
        //Debug.Log("\t-Getting the ids that are left");
        SFSArray idsLeft = (SFSArray)gameInfo.GetSFSArray("playerIDs");
        //Debug.Log("ya :" + idsLeft.Size());
        //int ran = UnityEngine.Random.Range(0, idsLeft.Size() - 1);
        //update room variable 
        SFSArray returnInts = new SFSArray();
        returnInts.AddInt(GameValues.playerID);
        Debug.Log("here in stuff: " + returnInts.GetInt(0));
        for (int i = 0; i < idsLeft.Size(); i++)
        {
            returnInts.AddInt(idsLeft.GetInt(i));
            Debug.Log("here in stuff: " + returnInts.GetInt(i + 1));
        }

        //send back to store on server
        List<RoomVariable> rData = new List<RoomVariable>();
        gameInfo.PutSFSArray("playerIDs", returnInts);
        rData.Add(new SFSRoomVariable("gameInfo", gameInfo));
        smartFox.Send(new SetRoomVariablesRequest(rData));
    }

    private void DrawUsersGUI()
    {
        GUI.Box(new Rect(screenW - 200, 80, 180, 170), "Users");
        GUILayout.BeginArea(new Rect(screenW - 190, 110, 150, 160));
        userScrollPosition = GUILayout.BeginScrollView(userScrollPosition, GUILayout.Width(150), GUILayout.Height(150));
        GUILayout.BeginVertical();

        List<User> userList = currentActiveRoom.UserList;
        foreach (User user in userList)
        {
            GUILayout.Label(user.Name);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawRoomsGUI()
    {
        GUILayout.BeginArea(new Rect(screenW - 190, 290, 180, 150));

        if (GameValues.isHost)
        {
            //start game button event listener
            if (GUI.Button(new Rect(80, 110, 85, 24), "Start Game"))
            {

                // ****** Create the actual game ******* //
                String[] nameParts = this.currentActiveRoom.Name.Split('-');
                String gameName = nameParts[0] + " - Game";

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
            if (GUI.Button(new Rect(80, 110, 85, 24), "Waiting..."))
            {
            }
        }
        GUILayout.EndArea();
    }

    private void DrawChatGUI()
    {
        GUI.Box(new Rect(10, 80, 470, 390), "Chat");

        GUILayout.BeginArea(new Rect(20, 110, 450, 350));
        chatScrollPosition = GUILayout.BeginScrollView(chatScrollPosition, GUILayout.Width(450), GUILayout.Height(350));
        GUILayout.BeginVertical();
        foreach (string message in messages)
        {
            //this displays text from messages arraylist in the chat window
            GUILayout.Label(message);
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
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
