using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using Newtonsoft;

namespace UDP_v1
{
    public class UDPController : UDPHandle
    {
        UDPManager uManager = null;
        public UDPController()
        {
            msgList = new Queue<string>();
            uManager = UDPManager.Instance("235.6.7.8", 7721);
            //uManager.GroupCastInit("235.6.7.8", 7721);
            InputFunction();
        }

        Queue<string> msgList;

        public string HandleName => throw new NotImplementedException();

        public string HandleType => throw new NotImplementedException();

        public string IDSetting => throw new NotImplementedException();

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

        public void DataTransport(params Fields[] data)
        {
            throw new NotImplementedException();
        }

        public Fields[] OnSendData()
        {
            throw new NotImplementedException();
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