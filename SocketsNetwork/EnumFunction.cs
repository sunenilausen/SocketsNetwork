//The commands for interaction between the server and the client
public enum Function
{
    Null,        //No command
    Login,      //Log into the server
    Logout,     //Logout of the server
    List,       //Get a list of users in the chat room from the server

    InitSuccess,
    DebugMessage,
    TextboxMessage
}

