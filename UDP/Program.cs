using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using Newtonsoft;
namespace UDP
{
    class UDPScene : UDPHandle<string>
    {
<<<<<<< HEAD
        public string id;
        public string msg;
    }

    public class UDPController : UDPInterface<string>
    {
        UDPManager<string> uManager;

        public UDPController() {
            msgList = new Queue<string>();
            uManager = new UDPManager<string>(this);
            uManager.GroupCastInit("235.6.7.8",7721);
            InputFunction();
        }

        Queue<string> msgList;
        public void InputFunction()
        {
            while (true)
            {
                string sendMsg = Console.ReadLine();
                msgList.Enqueue(sendMsg);
            }
        }

        public string GetSendData()
        {
            if (msgList.Count > 0)
            {
                return msgList.Dequeue();
            }
            else
            {
                return null;
            }
        }

        public void Log(string msg)
        {
            Console.WriteLine("[Log]" + msg);
        }

        public void Recived(string data)
        {
            Console.WriteLine(data);
=======
        UDPManager<string> chatManager;
        public UDPScene()
        {
            chatManager = new UDPManager<string>(this) ;
            Chat();
        }

        void Chat()
        {
            while (true)
            {
                chatManager.AddMsg(Console.ReadLine());
            }
        }

        public void DataTransport(DataTransfer<string> data)
        {
            Console.WriteLine(data.id + ":" + data.data);
        }

        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        string UDPHandle<string>.UserIDSetting()
        {
            return "";
>>>>>>> c04c3f7b747218a59ca520e69da7b3a77ab95215
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
<<<<<<< HEAD
            UDPController controller = new UDPController();
=======
            new UDPScene();
>>>>>>> c04c3f7b747218a59ca520e69da7b3a77ab95215
        }

    }
}
