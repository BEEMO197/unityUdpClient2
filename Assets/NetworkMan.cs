using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using UnityEngine.UIElements;
using System.Drawing;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    // Start is called before the first frame update
    public GameState gameState;
    void Start()
    {
        udp = new UdpClient();

        // 54.90.198.69 - Server Ip
        // localhost - Local Ip

        udp.Connect("localhost", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);

    }

    void OnDestroy(){
        udp.Dispose();
    }


    public enum commands{
        NEW_CLIENT,
        UPDATE,
        LOST_CLIENT
    };
    
    [Serializable]
    public class Message{
        public commands cmd;
    }
    
    [Serializable]
    public class Player{
        [Serializable]
        public struct receivedColor{
            public float R;
            public float G;
            public float B;
        }
        public string id;
        public receivedColor color;
        public bool init = true;
        public GameObject cube = null;
    }

    [Serializable]
    public class NewPlayer{
        public Player newPlayer;
    }

    [Serializable]
    public class DiePlayer
    {
        public Player lostPlayer;
    }

    [Serializable]
    public class GameState{
        public Player[] players;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    public NewPlayer lastestNewPlayer;
    public DiePlayer lastestLostPlayer;

    public List<Player> PlayerList;

    public bool newPlayerSpawned = false;

    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try{
            switch(latestMessage.cmd){
                case commands.NEW_CLIENT:
                    lastestNewPlayer = JsonUtility.FromJson<NewPlayer>(returnData);
                    newPlayerSpawned = true;
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.LOST_CLIENT:
                    lastestLostPlayer = JsonUtility.FromJson<DiePlayer>(returnData);
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }

        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers()
    {
        if (newPlayerSpawned)
        {
            // Debug.Log(lastestNewPlayer.newPlayer.id);
            PlayerList.Add(lastestNewPlayer.newPlayer);
            PlayerList.Last().cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            PlayerList.Last().cube.AddComponent<PlayerCube>();

            newPlayerSpawned = false;
        }
    }

    void UpdatePlayers()
    {
        for(int i = 0; i < lastestGameState.players.Length; i++)
        {
            for (int k = 0; k < PlayerList.Count(); k++)
            {
                if (lastestGameState.players[i].id == PlayerList[k].id)
                {
                    PlayerList[k].color = lastestGameState.players[i].color;
                    PlayerList[k].cube.GetComponent<PlayerCube>().playerRef = PlayerList[k];
                }
            }
        }
        // Inside Player Cube Script
    }

    void DestroyPlayers()
    {
        foreach(Player player in PlayerList)
        {
            if(player.id == lastestLostPlayer.lostPlayer.id)
            {
                PlayerList.Remove(player);
            }
        }
    }
    
    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update()
    {
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}
