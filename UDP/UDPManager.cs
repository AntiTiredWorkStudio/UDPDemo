using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace UDP
{
    public interface UDPInterface<T> {
        T GetSendData();
        void Log(string msg);
        void Recived(T data);
    }
    public class UDPManager<T>
    {
        UDPInterface<T> handleObject = null;

        string bindIP;
        int bindPort;
        IPEndPoint reciveIPEndPoint = null;

        string sendIP;
        int sendPort;
        IPEndPoint sendIPEndPoint = null;

        Thread reciveThread = null;
        Thread sendThread = null;

        UdpClient sendUdp = null;
        UdpClient reciveUdp = null;

        public UDPManager(UDPInterface<T> handle)
        {
            handleObject = handle;
        }

        public UDPManager<T> GroupCastInit(string groupIP, int groupPort)
        {
            sendUdp = new UdpClient();
            sendIP = groupIP;
            sendPort = groupPort;
            sendIPEndPoint = new IPEndPoint(IPAddress.Parse(sendIP), sendPort);
            sendUdp.EnableBroadcast = true;

            bindIP = groupIP;
            bindPort = groupPort;
            reciveIPEndPoint = new IPEndPoint(IPAddress.Parse(bindIP), bindPort);
            reciveUdp = new UdpClient(bindPort);
            reciveUdp.JoinMulticastGroup(IPAddress.Parse(bindIP));
            reciveUdp.EnableBroadcast = true;

            InitThread();
            return this;
        }



        void InitThread()
        {
            sendThread = new Thread(SendAction);
            reciveThread = new Thread(ReciveAction);
            sendThread.Start();
            reciveThread.Start();
        }

        public void AbortManager() {
            sendThread.Interrupt();
            reciveThread.Interrupt();
            sendThread.Abort();
            reciveThread.Abort();
            sendUdp.Close();
            reciveUdp.Close();
        }


        public byte[] S2B(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static string B2S(byte[]  buf,int length)
        {
            return Encoding.UTF8.GetString(buf, 0, length);
        }

        void SendAction()
        {
            while (true) {
                T sendData = handleObject.GetSendData();
                if (sendData != null)
                {
                     string json = "{}";
                    try
                    {
                        json = JsonConvert.SerializeObject(sendData);
                        byte[] buffer = S2B(json);
                        sendUdp.Send(buffer, buffer.Length,sendIPEndPoint);
                    }
                    catch(Exception e) {
                        handleObject.Log(e.ToString());
                        continue;
                    }
                }
                else
                {
                    Thread.Sleep(150);
                }
            }
        }

        void ReciveAction()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = reciveUdp.Receive(ref reciveIPEndPoint);
                    string json = B2S(buffer, buffer.Length);
                    T dataObject = JsonConvert.DeserializeObject<T>(json);
                    handleObject.Recived(dataObject);
                }
                catch (Exception e)
                {
                    handleObject.Log(e.ToString());
                }
            }
        }
    }
}
