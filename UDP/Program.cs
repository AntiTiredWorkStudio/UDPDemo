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
    public class DataStruct
    {
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
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            UDPController controller = new UDPController();
        }

    }
}
