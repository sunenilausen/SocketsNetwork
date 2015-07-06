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
            //Debug.WriteLine(this.receivedTextbox.IsHandleCreated);
            //this.receivedTextbox.CreateControl();
            //if (this.receivedTextbox.Handle != null)
            //Debug.WriteLine(this.receivedTextbox.IsHandleCreated);
        }


        private void createButton_Click(object sender, EventArgs e)
        {
            ServerSocket.Instance.CreateRoom();
            Debug.WriteLine("Server Created");
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            ClientSocket.Instance.JoinRoom(usernameTextbox.Text, ipTextbox.Text);
            ClientSocket.Instance.SendFunction(Function.InitSuccess);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            ClientSocket.Instance.SendFunction(Function.StringMessage, messageTextbox.Text);
        }

        public void AddMessage(string s)
        {
            Debug.WriteLine(s);
            //if(this.receivedTextbox.IsHandleCreated)
            //if (receivedTextbox.InvokeRequired)
            //{
            //    this.Invoke(new Action<string>(AddMessage), new object[] { s });
            //    return;
            //}

            //receivedTextbox.Text += s + "\n";
            //receivedTextbox.Invoke(new Action(() => receivedTextbox.Text += s + "\n"));
            //this.Dispatcher.Invoke((Action)(() =>
            //{ 
            //    receivedTextbox.AppendText(s);
            //}));
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
                case Function.StringMessage:
                    AddMessage((string)objects[0]);
                    break;
            }
        }
    }
}
