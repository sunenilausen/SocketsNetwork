using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class Data {

    public string strName;      //Name by which the client logs into the room
    public string strMessage;   //Message text
    public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
    public Function function;  //Functionname
    public object[] objects; //Function Parameters

    //Default constructor
    public Data()
    {
        this.cmdCommand = Command.Null;
        this.function = Function.Null;
        this.strMessage = null;
        this.strName = null;
        this.objects = null;
    }

    //Converts the bytes into an object of type Data
    public Data(byte[] data)
    {
        //The first four bytes are for the Command
        this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

        //The next four store the length of the name
        int nameLen = BitConverter.ToInt32(data, 4);

        //This check makes sure that strName has been passed in the array of bytes
        if (nameLen > 0)
            this.strName = Encoding.UTF8.GetString(data, 8, nameLen);
        else
            this.strName = null;

        if (this.cmdCommand == Command.Function)
        {
            //The next four store the function
            this.function = (Function)BitConverter.ToInt32(data, 8 + nameLen);

            //The rest store the dynamically stored parameters
            byte[] objectData = new byte[1024];

            //I am making a new array with only the parameters, could probably find a better way
            Array.Copy(data, 12 + nameLen, objectData, 0, 512);

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(objectData);
            
            this.objects = (object[])formatter.Deserialize(ms);
        }
        else
        {
            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 8 + nameLen);

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
        }
    }

    //Converts the Data structure into an array of bytes
    public byte[] ToByte()
    {
        List<byte> result = new List<byte>();

        //First four are for the Command
        result.AddRange(BitConverter.GetBytes((int)cmdCommand));

        //Add the length of the name
        if (strName != null)
            result.AddRange(BitConverter.GetBytes(strName.Length));
        else
            result.AddRange(BitConverter.GetBytes(0));

        //Add the name
        if (strName != null)
            result.AddRange(Encoding.UTF8.GetBytes(strName));

        if (cmdCommand == Command.Function)
        {
            result.AddRange(BitConverter.GetBytes((int)function));

            //Convert each parameter(objects) to byte array
            if (objects != null)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                formatter.Serialize(ms, objects);
                result.AddRange(ms.ToArray());
            }
            else 
                result.AddRange(BitConverter.GetBytes(0));

        }
        else
        {
            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));
        }

        return result.ToArray();
    }
}
