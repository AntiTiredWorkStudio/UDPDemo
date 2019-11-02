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
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            new UDPScene();
        }
    }
}
