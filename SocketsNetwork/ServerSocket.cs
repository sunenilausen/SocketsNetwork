﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

public class ServerSocket{

    //The ClientInfo structure holds the required information about every
    //client connected to the server
    struct ClientInfo
    {
        public Socket socket;   //Socket of the client
        public string strName;  //Name by which the user logged into the chat room
    }

    //The collection of all clients logged into the room (an array of type ClientInfo)
    ArrayList clientList;

    //The main socket on which the server listens to the clients
    Socket serverSocket;

    byte[] byteData = new byte[1024];

    public static ServerSocket Instance
    {
        get;
        private set;
    }
    
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public ServerSocket() {
    }

    // Use this for initialization
    void Awake()
    {
        Instance = this;
    }

    public  void CreateRoom()
    {
        clientList = new ArrayList();
        try
        {
            //We are using TCP sockets
            serverSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream,
                                      ProtocolType.Tcp);

            //Assign the any IP of the machine and listen on port number 1000
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1000);

            //Bind and listen on the given address
            serverSocket.Bind(ipEndPoint);
            serverSocket.Listen(20);

            serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }


    private void OnAccept(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = serverSocket.EndAccept(ar);

            //Start listening for more clients
            serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

            //Once the client connects then start receiving the commands from her
            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(OnReceive), clientSocket);
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }
    
    private void OnReceive(IAsyncResult ar)
    {
        //Debug.WriteLine("sOnReceive");
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;

            clientSocket.EndReceive(ar);
            //Transform the array of bytes received from the user into an
            //intelligent form of object Data
            Data msgReceived = new Data(byteData);

            //We will send this object in response the users request
            Data msgToSend = new Data();

            byte[] message = new byte[1024];

            msgToSend = msgReceived;

            switch (msgReceived.cmdCommand)
            {
                case Command.Login:

                    //When a user logs in to the server then we add her to our
                    //list of clients

                    ClientInfo clientInfo = new ClientInfo();
                    clientInfo.socket = clientSocket;
                    clientInfo.strName = msgReceived.strName;

                    clientList.Add(clientInfo);

                    //Set the text of the message that we will broadcast to all users
                    msgToSend.strMessage = "<<<" + msgReceived.strName + " has joined the room>>>";
                    break;

                case Command.Logout:

                    //When a user wants to log out of the server then we search for her 
                    //in the list of clients and close the corresponding connection

                    int nIndex = 0;
                    foreach (ClientInfo client in clientList)
                    {
                        if (client.socket == clientSocket)
                        {
                            clientList.RemoveAt(nIndex);
                            break;
                        }
                        ++nIndex;
                    }

                    clientSocket.Close();

                    msgToSend.strMessage = "<<<" + msgReceived.strName + " has left the room>>>";
                    break;

                case Command.Message:

                    //Set the text of the message that we will broadcast to all users
                    msgToSend.strMessage = msgReceived.strName + ": " + msgReceived.strMessage;
                    break;

                case Command.List:

                    //Send the names of all users in the chat room to the new user
                    msgToSend.cmdCommand = Command.List;
                    msgToSend.strName = null;
                    msgToSend.strMessage = null;
                    msgToSend.function = Function.Null;
                    msgToSend.objects = null;

                    //Collect the names of the user in the chat room
                    foreach (ClientInfo client in clientList)
                    {
                        //To keep things simple we use asterisk as the marker to separate the user names
                        msgToSend.strMessage += client.strName + "*";
                    }

                    // message = msgToSend.ToByte();
                    Array.Copy(msgToSend.ToByte(), message, msgToSend.ToByte().Length);

                    //Send the name of the users in the chat room
                    clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None,
                            new AsyncCallback(OnSend), clientSocket);
                    break;

                case Command.Function:
                    //Debug.WriteLine(msgReceived.function.ToString());
                    break;
            }

            if (msgToSend.cmdCommand != Command.List)   //List messages are not broadcasted
            {
                //message = msgToSend.ToByte();
                Array.Copy(msgToSend.ToByte(), message, msgToSend.ToByte().Length);

                foreach (ClientInfo clientInfo in clientList)
                {
                    if (clientInfo.socket != clientSocket ||
                        msgToSend.cmdCommand != Command.Login)
                    {
                        //Send the message to all users
                        clientInfo.socket.BeginSend(message, 0, message.Length, SocketFlags.None,
                            new AsyncCallback(OnSend), clientInfo.socket);
                    }
                }

                //Debug.WriteLine(msgToSend.strMessage + "\r\n");
            }

            //If the user is logging out then we need not listen from her
            if (msgReceived.cmdCommand != Command.Logout)
            {
                //Start listening to the message send by the user
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                //Debug.WriteLine("Continue receiving");
            }
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }

    public void OnSend(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndSend(ar);
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }
}

//if (read > 0)
//    {
//        //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
//        state.result.AddRange(state.buffer);
//        clientSocket.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
//            new AsyncCallback(ReadCallback), state);
//    }
//    else
//    {
//        if (state.result.Count > 1)
//        {
//            // All the data has been read from the client;
//            // display it on the console.
//            byte[] content = state.result.ToArray();
//            //Transform the array of bytes received from the user int
