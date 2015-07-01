using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
public class ClientSocket{

    // Receiving byte array  
    public Socket clientSocket; //The main client socket
    public string strName;      //Name by which the user logs into the room
    public string ipInput;

    private byte[] byteData = new byte[1024];

    public static ClientSocket Instance
    {
        get;
        private set;
    }

    private static ManualResetEvent sendDone = new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = new ManualResetEvent(false);

	// Use this for initialization

    void Awake()
    {
        Instance = this;
    }
    public void JoinRoom()
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress = IPAddress.Parse(ipInput);
            //Server is listening on port 1000
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

            //Connect to the server
            clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);

            Debug.WriteLine("SGSclientTCP: " + strName);

            //The user has logged into the system so we now request the server to send
            //the names of all users who are in the chat room
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.List;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;
            msgToSend.function = Function.Null;

            byteData = msgToSend.ToByte();

            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);

            byteData = new byte[1024];
            //Start listening to the data asynchronously
            clientSocket.BeginReceive(byteData,
                                       0,
                                       byteData.Length,
                                       SocketFlags.None,
                                       new AsyncCallback(OnReceive),
                                       null);
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }
    private void OnConnect(IAsyncResult ar)
    {
        try
        {
            clientSocket.EndConnect(ar);

            //We are connected so we login into the server
            Data msgToSend = new Data();
            msgToSend.cmdCommand = Command.Login;
            msgToSend.strName = strName;
            msgToSend.strMessage = null;
            msgToSend.function = Function.Null;

            byte[] b = msgToSend.ToByte();

            //Send the message to the server
            clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }
    public void SendFunction(Function function, params object[] objects)
    {
        try
        {
            //Fill the info for the message to be send
            Data msgToSend = new Data();

            msgToSend.strName = strName;
            msgToSend.strMessage = null;
            msgToSend.cmdCommand = Command.Function;
            msgToSend.function = function;

            msgToSend.objects = objects;

            byte[] byteData = new byte[1024];
            Array.Copy(msgToSend.ToByte(), byteData, msgToSend.ToByte().Length);

            Send(clientSocket, byteData);
            sendDone.WaitOne();


            //Send it to the server
            //clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            //Debug.WriteLine("sendfunction");
        }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }

    private static void Send(Socket client, byte[] byteData)
    {
        // Begin sending the data to the remote device.
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private void OnSend(IAsyncResult ar)
    {
        try
        {
            clientSocket.EndSend(ar);
        }
        catch (ObjectDisposedException)
        { }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            int bytesSent = client.EndSend(ar);
            //Debug.WriteLine("Sent " + bytesSent + " bytes to server.");

            // Signal that all bytes have been sent.
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            clientSocket.EndReceive(ar);

            Data msgReceived = new Data(byteData);

            //Debug.WriteLine(msgReceived.strName);

            //Accordingly process the message received
            switch (msgReceived.cmdCommand)
            {
                case Command.Function:
                    SocketManager.Instance.DetermineFunction(msgReceived.function, msgReceived.objects);
                    break;

                case Command.Login:
                   Debug.WriteLine("Logged In:" + msgReceived.strName);
                    break;

                case Command.Logout:
                    Debug.WriteLine("Logged Out:" + msgReceived.strName);
                    break;

                case Command.Message:
                    break;

                case Command.List:
                    Debug.WriteLine( "<<<" + strName + " has joined the room>>>\r\n");
                    break;                  
            }

            //if (msgReceived.strMessage != null && msgReceived.cmdCommand != Command.List)
            //    txtChatBox.Text += msgReceived.strMessage + "\r\n";

            byteData = new byte[1024];

            clientSocket.BeginReceive(byteData,
                                      0,
                                      byteData.Length,
                                      SocketFlags.None,
                                      new AsyncCallback(OnReceive),
                                      null);

        }
        catch (ObjectDisposedException)
        { }
        catch (Exception exc) { Debug.WriteLine(exc.ToString()); }
    }

    // MISSING CLOSE / DISCONNECT
}
