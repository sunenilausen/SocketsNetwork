using System.Collections;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
namespace SocketsNetwork
{
    public class ClientSocket
    {

        // Receiving byte array  
        public Socket clientSocket; //The main client socket
        private string strName;
        private string ipInput;
        private byte[] byteData = new byte[1024];

        static readonly ClientSocket _instance = new ClientSocket();
        public static ClientSocket Instance
        {
	    get
	    {
	        return _instance;
	    }
        }
        ClientSocket()
        {
        }

        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public void JoinRoom(string userName, string targetIP)
        {
            try
            {
                strName = userName;
                ipInput = targetIP;
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ipAddress = IPAddress.Parse(ipInput);
                //Server is listening on port 1000
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1000);

                //Connect to the server
                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);

                Debug.WriteLine("Username: " + strName);

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
                msgToSend.strName = strName;
                msgToSend.function = Function.Login;
                msgToSend.objects = new object[] {null};

                byte[] b = msgToSend.ToByte();

                //Send the message to the server
                clientSocket.BeginSend(b, 0, b.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
                SendFunction(Function.DebugMessage, strName + " has connected.");
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
                msgToSend.function = function;
                msgToSend.objects = objects;

                byte[] byteData = new byte[1024];
                Array.Copy(msgToSend.ToByte(), byteData, msgToSend.ToByte().Length);
             
                //Send it to the server
                Send(clientSocket, byteData);
                sendDone.WaitOne();
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

                //Accordingly process the message received
                if (msgReceived.function == Function.Login)
                    Debug.WriteLine("Logged In:" + msgReceived.strName);
                else if (msgReceived.function == Function.Logout) 
                    Debug.WriteLine("Logged Out:" + msgReceived.strName);
                else if (msgReceived.function == Function.List)
                    Debug.WriteLine("<<<" + strName + " has joined the room>>>\r\n");
                else
                    Form1.Instance.DetermineFunction(msgReceived.function, msgReceived.objects);
               
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
}