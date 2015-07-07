using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Threading;

namespace SocketsNetwork
{
    public partial class Form1 : Form
    {
        static readonly Form1 _instance = new Form1();
        public static Form1 Instance
        {
	    get
	    {
	        return _instance;
	    }
        }
       
        public Form1()
        {
            InitializeComponent();
        }


        private void createButton_Click(object sender, EventArgs e)
        {
            ServerSocket.Instance.CreateRoom();
            Debug.WriteLine("Server Created");
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            ClientSocket.Instance.JoinRoom(usernameTextbox.Text, ipTextbox.Text);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            ClientSocket.Instance.SendFunction(Function.DebugMessage, messageTextbox.Text);
        }

        public void DebugMessage(string s)
        {
            Debug.WriteLine(s);
           
        }
        public void TextBoxMessage(string s)
        {
           // Debug.WriteLine(s);
        }

        public void InitSuccess()
        {
            Debug.WriteLine("INITSUCCESS");
        }

        public void DetermineFunction(Function function, object[] objects)
        {
            switch (function)
            {
                case Function.InitSuccess:
                    InitSuccess();
                    break;
                case Function.DebugMessage:
                    DebugMessage((string)objects[0]);
                    break;
            }
        }
    }
}
