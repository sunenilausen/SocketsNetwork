//The commands for interaction between the server and the client
public enum Command
{
    Login,      //Log into the server
    Logout,     //Logout of the server
    Message,    //Send a text message to all the chat clients
    List,       //Get a list of users in the chat room from the server
    Function,   //A function to be synchronized
    Null        //No command

}
