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
    public Function function;  //Functionname
    public object[] objects; //Function Parameters

    //Default constructor
    public Data()
    {
        this.strName = null;
        this.function = Function.Null;
        this.objects = null;
    }

    //Converts the bytes into an object of type Data
    public Data(byte[] data)
    {

        //The first four bytes store the length of the name
        int nameLen = BitConverter.ToInt32(data, 0);

        //This check makes sure that strName has been passed in the array of bytes
        if (nameLen > 0)
            this.strName = Encoding.UTF8.GetString(data, 4, nameLen);
        else
            this.strName = null;

        //The next four store the function
        this.function = (Function)BitConverter.ToInt32(data, 4 + nameLen);

        
        //The rest store the dynamically stored parameters
        byte[] objectData = new byte[1024];

        //I am making a new array with only the parameters, could probably find a better way
        Array.Copy(data, 8 + nameLen, objectData, 0, 512);

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(objectData);

        this.objects = (object[])formatter.Deserialize(ms);

    }

    //Converts the Data structure into an array of bytes
    public byte[] ToByte()
    {
        List<byte> result = new List<byte>();

        //Add the length of the name
        if (strName != null)
            result.AddRange(BitConverter.GetBytes(strName.Length));
        else
            result.AddRange(BitConverter.GetBytes(0));

        //Add the name
        if (strName != null)
            result.AddRange(Encoding.UTF8.GetBytes(strName));

        
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
       
        return result.ToArray();
    }
}
