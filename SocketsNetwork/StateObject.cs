using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Collections;

// State object for receiving data from remote device.
public class StateObject
{
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int bufferSize = 1024;
    // Receive buffer.
    public byte[] buffer = new byte[bufferSize];
    // Result data
    public List<byte> result = new List<byte>();
}
