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


    void Start()
    {
        smartFox = SmartFoxConnection.Connection;
        currentActiveRoom = smartFox.LastJoinedRoom;
        if (!currentActiveRoom.IsGame)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("whoops :()");
            Destroy(gameObject);
        }

        smartFox.AddLogListener(LogLevel.INFO, OnDebugMessage);
        screenW = Screen.width;
        AddEventListeners();
    }

    private void AddEventListeners()
    {

        smartFox.RemoveAllEventListeners();

        smartFox.AddEventListener(SFSEvent.LOGOUT, OnLogout);
        smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnJoinRoom);
        smartFox.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
        smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
        smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
        smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
        smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
        smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
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
        if (room.Name == "The Lobby")
            Application.LoadLevel(room.Name);
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

    public void OnUserEnterRoom(BaseEvent evt)
    {
        User user = (User)evt.Params["user"];
        messages.Add(user.Name + " has entered the room.");
        if (user.IsItMe)
        {

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
                    Debug.Log("Game started");
                    String[] nameParts = this.currentActiveRoom.Name.Split('-');
                    Debug.Log(nameParts[0] + " - Game");
                    smartFox.Send(new JoinRoomRequest(nameParts[0] + " - Game", "", CurrentActiveRoom.Id));
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
        GUI.skin = gSkin;


        // ****** Show full interface only in the Lobby ******* //
        if (!currentActiveRoom.IsGame)
        {
            //Debug.Log("DRAWING!!!");
            DrawLobbyGUI();
            DrawRoomsGUI();
            //DrawGameLobbyGUI();
        }
    }

    private void DrawLobbyGUI()
    {
        GUI.Label(new Rect(2, -2, 680, 70), "", "SFSLogo");
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
        if (GUI.Button(new Rect(screenW - 115, 20, 85, 24), "Logout"))
        {
            smartFox.Send(new LogoutRequest());
        }
    }
    private void DrawGameLobbyGUI()
    {
        //before creating the GUI...
        //get and parse the variables set up from the lobby
        List<RoomVariable> lobbyVars = currentActiveRoom.GetVariables();
        SFSObject lobbyGameInfo = (SFSObject)((RoomVariable)lobbyVars[1]).GetSFSObjectValue();
        //FORMAT BY INDEX
        //0 = (bool)        gameStarted
        //1 = (SFSObject)   gameInfo
        //      -(string)   the host username               key: "host"
        //      -(SFSArray) playerIds                       key: "playerIDs"
        //      -(int)      number of Teams                 key: "numTeams"
        //      -(SFSArray) teams                           key: "teams"
        //      -(int)      length of the game in seconds   key: "gameLength"

        //we will need the number of teams, the number of players, and the length of the game
        int numPlayers = currentActiveRoom.MaxUsers;
        int lengthOfGame = lobbyGameInfo.GetInt("gameLength");
        int numTeams = lobbyGameInfo.GetInt("numTeams");

        //draw the GUI

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

                List<RoomVariable> roomVars = new List<RoomVariable>();
                roomVars.Add(new SFSRoomVariable("gameStarted", true));
                //Debug.Log("sending start");
                smartFox.Send(new SetRoomVariablesRequest(roomVars));


                // ****** Create the actual game ******* //
                //let smartfox take care of error if duplicate name
                String[] nameParts = this.currentActiveRoom.Name.Split('-');
                String gameName = nameParts[0] + " - Game";

                RoomSettings settings = new RoomSettings(gameName);
                settings.MaxUsers = (short)currentActiveRoom.MaxUsers; // how many players allowed: 12
                settings.Extension = new RoomExtension(GameManager.ExtName, GameManager.ExtClass);
                settings.IsGame = true;

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
                //get the values from the appropriate fields to populate the gameInfo

                smartFox.Send(new CreateRoomRequest(settings, true));

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
            smartFox.Send(new JoinRoomRequest("The Lobby"));
        }
    }
}
