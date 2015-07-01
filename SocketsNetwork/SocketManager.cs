using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Reflection;

public class SocketManager : MonoBehaviour {

    public GameObject ipInput;
    public GameObject nameInput;
    private ServerSocket serverSocket;
    private ClientSocket clientSocket;

    public bool testOn;
    int testCount;
    float testTime;
    bool takeTime;
    bool takeStartTime;

    // Use this for initialization
    public static SocketManager Instance
    {
        get;
        private set;
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(transform.gameObject);
    }

    void Start()
    {
        serverSocket = ServerSocket.Instance;
        clientSocket = ClientSocket.Instance;

    }
    void Update()
    {
        //if (testOn)
        //    clientSocket.SendFunction(Function.ObjectParamsFunction, "hallo", 4, 4.3, true, numberList);
        if (takeStartTime)
        {
            testTime = Time.realtimeSinceStartup;
            takeStartTime = false;
        }
        if (takeTime)
        {
            Debug.Log(Time.realtimeSinceStartup - testTime);
            takeTime = false;
        }
        
    }


    public void ButtonStartServer()
    {
        Debug.Log("Trying to start server");
        serverSocket.CreateRoom();
        ButtonJoinServer();
    }

    public void ButtonJoinServer()
    {
        Debug.Log("Trying to join server");
        clientSocket.strName = nameInput.GetComponent<InputField>().text;
        clientSocket.ipInput = ipInput.GetComponent<InputField>().text;
        clientSocket.JoinRoom();
        clientSocket.SendFunction(Function.InitSuccess);
    }

    public void LoadLevel()
    {
        //TODO: make an alternate network scene
        Application.LoadLevel("Game");
    }

    //TODO: should use getmethod() instead https://msdn.microsoft.com/en-us/library/system.type.getmethod%28v=vs.110%29.aspx 
    public void DetermineFunction(Function function, object[] objects)
    {
       switch (function)
       {
           case Function.InitSuccess:
                InitSuccess();
                break;
           case Function.LoadGame:
                LoadLevel();
                break;
           case Function.EveryoneRaiseHand:
                EveryoneRaiseHand();
                break;
           case Function.RaiseHand:
                RaiseHand();
                break;
           case Function.StringFunction:
                StringFunction((string)objects[0]);
                break;
           case Function.IntFunction:
                IntFunction((int)objects[0]);
                break;
           case Function.ObjectParamsFunction:
                ObjectParamsFunction(objects);
                break;
           case Function.CountListFunction:
                CountListFunction((List<int>)objects[0]);
                break;
           case Function.DisplayCount:
                DisplayCount();
                break;
           case Function.StartTime:
                StartTime();
                break;
               
       }
    }


    /// <summary>
    /// Test Functions
    /// </summary>
    public void TestButton()
    {
        //testOn = true;
        //List<int> numberList = new List<int> { };
        //for (int j = 0; j < 50; j++)
        //{
        //    numberList.Add(j);
        //    //clientSocket.SendFunction(Function.ObjectParamsFunction, "hallo", 4, 4.3, true, numberList);
        //}
        //for (int i = 0; i < 1000; i++)
        //{
        //    clientSocket.SendFunction(Function.ObjectParamsFunction, "hallo", 4, 4.3, true, numberList);
        //}

        //testCount = 0;
        clientSocket.SendFunction(Function.StartTime);
        for (int i = 0; i < 50; i++)
        {
            //clientSocket.SendFunction(Function.CountListFunction, numberList);
            clientSocket.SendFunction(Function.ObjectParamsFunction, 2);
        }
        clientSocket.SendFunction(Function.DisplayCount);
        
    }

    public void EveryoneRaiseHand()
    {
        Debug.Log("EveryoneRaiseHand");
        clientSocket.SendFunction(Function.RaiseHand);
    }

    public void RaiseHand()
    {
        Debug.Log("*RaiseHand*");
    }
    public void InitSuccess()
    {
        Debug.Log("INITSUCCESS");
    }

    public void StringFunction(string myString)
    {
        Debug.Log(myString);
    }

    public void IntFunction(int myInt)
    {
        Debug.Log(myInt);
    }

    public void ObjectParamsFunction(params object[] myObjects)
    {
        //foreach (object myObject in myObjects)
        //{
        //    Debug.Log(myObject.ToString());
        //}
    }

    public void CountListFunction(List<int> myInts)
    {
        int myCount = 0;
        foreach (object myInt in myInts)
        {
            myCount += (int)myInt;
        }
        testCount += myCount;
    }

    public void DisplayCount()
    {
        //Debug.Log(testCount);
        takeTime = true;
    }

    public void StartTime()
    {
        takeStartTime = true;
    }
}
