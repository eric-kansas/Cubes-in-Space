
using System;
using System.Collections;
using UnityEngine;
using Sfs2X.Entities.Data;

// This class holds transform values and can load and send the data to server
public class LaunchPacket
{
    private string type = "LAUNCH";
    private int cubeID = -1;
    private int sideID = -1;
    private Vector3 launchPosition; // Position as Vector3
    private Vector3 launchDestination; // Position as Vector3
    private double localGameTime; //current game time that event happened
    private double gameTimeETA;
    //private double localLatency; //my average latency to the server at that time 

    public LaunchPacket()
    {

    }

    public LaunchPacket(Vector3 pos, Vector3 destination, double gameTime, double eta, int cubeID, int sideID)
    {
        //side hit
        this.cubeID = cubeID;

        //side hit
        this.sideID = sideID;

        // Launch Position
        this.launchPosition = pos;

        // Launch Destination
        this.launchDestination = destination;

        //Local Game Time
        this.localGameTime = gameTime;

        // local game time of arrival
        this.gameTimeETA = eta;
    }

	// Check if this transform is different from given one with specified accuracy
    public bool IsDifferent(LaunchPacket transform, float accuracy)
    {
        float posDif = Vector3.Distance(this.LaunchPosition, transform.LaunchPosition);
		
		return (posDif>accuracy /*|| angDif > accuracy*/);
	}
		
	// Copies another NetworkTransform to itself
    public void Load(LaunchPacket launchMessage)
    {
        //Message type
        this.type = launchMessage.type;

        //cube hit id
        this.sideID = launchMessage.cubeID;

        //side hit id
        this.sideID = launchMessage.sideID;

        // Launch Position
        this.launchPosition = launchMessage.launchPosition;
       
        // Launch Destination
        this.launchDestination = launchMessage.launchDestination;

        //Local Game Time
        this.localGameTime = launchMessage.localGameTime;

        //Local Game Time
        this.gameTimeETA = launchMessage.gameTimeETA;
	}
	
	// Copy the Unity transform to itself
	public void UpdateTransform(Transform trans) {
        //trans.position = this.launchPosition;
		//trans.localEulerAngles = this.AngleRotation;
	}

    public string ToString()
    {
        string holder = "";

        holder += "type: " + type;
        holder += " Start Pos: " + launchPosition;
        holder += " Destination: " + launchDestination;
        holder += " Game Time: " + localGameTime;
        
        return holder;
    }

    // Stores the transform values to SFSObject to send them to server
    public ISFSObject ToSFSObject(ISFSObject data)
    {
        ISFSObject launchMessage = new SFSObject();

        //Message
        launchMessage.PutUtfString("messageType", type);

        //side hit id
        launchMessage.PutInt("cubeID", cubeID);

        //side hit id
        launchMessage.PutInt("sideID", sideID);

        // Launch Position
        launchMessage.PutFloat("sx", this.launchPosition.x);
        launchMessage.PutFloat("sy", this.launchPosition.y);
        launchMessage.PutFloat("sz", this.launchPosition.z);

        // Launch Destination
        launchMessage.PutFloat("ex", this.launchDestination.x);
        launchMessage.PutFloat("ey", this.launchDestination.y);
        launchMessage.PutFloat("ez", this.launchDestination.z);

        //Local Game Time
        launchMessage.PutDouble("localGameTime", this.localGameTime);

        //Local Game Time
        launchMessage.PutDouble("gameTimeETA", this.gameTimeETA);

        data.PutSFSObject("launchMessage", launchMessage);
		
		return data;
    }
	
	// Creating NetworkTransform from SFS object
    public static LaunchPacket FromSFSObject(ISFSObject data)
    {
        LaunchPacket launchMessage = new LaunchPacket();

        ISFSObject launchData = data.GetSFSObject("launchMessage");
        
        launchMessage.type = launchData.GetUtfString("messageType");

        //cube hit id
        launchMessage.cubeID = launchData.GetInt("cubeID");

        //side hit id
        launchMessage.sideID = launchData.GetInt("sideID");

        //get launch pos
        float sx = launchData.GetFloat("sx");
        float sy = launchData.GetFloat("sy");
        float sz = launchData.GetFloat("sz");

        //set lauch pos in object
        launchMessage.launchPosition = new Vector3(sx, sy, sz);

        //get launch destination
        float ex = launchData.GetFloat("ex");
        float ey = launchData.GetFloat("ey");
        float ez = launchData.GetFloat("ez");

        //set lauch pos in object
        launchMessage.launchDestination = new Vector3(ex, ey, ez);

        //get & set senders local game time
        launchMessage.localGameTime = launchData.GetDouble("localGameTime");

        //get & set senders local game time
        launchMessage.gameTimeETA = launchData.GetDouble("gameTimeETA");

        return launchMessage;
	}
	
	// Creating NetworkTransform from Unity transform
    public static LaunchPacket InstantiatePacket(Vector3 Pos)
    {
        LaunchPacket trans = new LaunchPacket();

       // trans.launchPosition = transform.position;
		//trans.angleRotation = transform.localEulerAngles;
				
		return trans;
	}
	
	// Clone itself
    public static LaunchPacket Clone(LaunchPacket ntransform)
    {
        LaunchPacket trans = new LaunchPacket();
		trans.Load(ntransform);
		return trans;
	}

    //getters and setters
    public string Type
    {
        get
        {
            return type;
        }
    }

    public int CubeID
    {
        get
        {
            return cubeID;
        }
    }

    public int SideID
    {
        get
        {
            return sideID;
        }
    }

    public Vector3 LaunchPosition
    {
        get
        {
            return launchPosition;
        }
    }

    public Vector3 LaunchDestination
    {
        get
        {
            return launchDestination;
        }
    }

    public double LocalGameTime
    {
        get
        {
            return localGameTime;
        }
    }

    public double GameTimeETA
    {
        get
        {
            return gameTimeETA;
        }
    }
}
