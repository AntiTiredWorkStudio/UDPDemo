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
    public interface UDPHandle<T>
    {
        string UserIDSetting();
        void Log(string msg);
        void DataTransport(DataTransfer<T> data);
    }

    public class DataTransfer<T>
    {
        public string id;
        public T data;
    }
    

    public class UDPManager<T>
    {
        public UDPManager(UDPHandle<T> handle)
        {
            InitManager(handle);
        }
        UDPHandle<T> handleObject;
        Queue<T> TargetMsg;
        public void AddMsg(T msg)
        {
            if (TargetMsg != null) TargetMsg.Enqueue(msg);
        }
        Thread sendThread = null;
        Thread reciveThread = null;
        void InitManager(UDPHandle<T> handle)
        {
            handleObject = handle;
            handleObject.Log("用户:" + ID);
            TargetMsg = new Queue<T>();
            reciveThread = new Thread(ReciveFunc);
            reciveThread.IsBackground = false;
            reciveThread.Start();
            sendThread = new Thread(SendFunc);
            sendThread.IsBackground = false;
            sendThread.Start();
        }
        public void CloseManager()
        {
            try
            {
                sendThread.Abort();
                reciveThread.Abort();
            }
            catch(Exception e)
            {
                handleObject.Log(e.ToString());
            }
        }

        string id = "nan";
        public string ID
        {
            get
            {
                string idSetting = handleObject.UserIDSetting();
                if (id == "nan") {
                    if(idSetting != "")
                    {
                        id = idSetting;
                    }
                    else { 
                    Random rand = new Random(); id = "user_" + (1000000 + rand.Next() % 999999);
                    }
                }
                return id;
            }
        }


        void ReciveFunc()
        {
            int port = 8001;
            string hostName = Dns.GetHostName();
            IPHostEntry localhost = Dns.GetHostEntry(hostName);
            List<IPAddress> remoteIp = (
                new List<IPAddress>(
                    from IPAddress tip in localhost.AddressList
                    where tip.AddressFamily == AddressFamily.InterNetwork
                    select tip)
                 );
            int seek = 0;
            IPEndPoint local = new IPEndPoint(remoteIp[seek], port);
            UdpClient RecviceClient = null;
            while (true)
            {
                try
                {
                    RecviceClient = new UdpClient(local);
                    handleObject.Log("接收线程:" + local.ToString() + " 启动成功");
                    break;
                }
                catch
                {
                    handleObject.Log("端口占用:" + local.ToString() + " 重置端口");
                    if (remoteIp.Count > 1)
                    {
                        local = new IPEndPoint(remoteIp[++seek], port);
                    }
                    else { local.Port++; }
                    continue;
                }
            }
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, 8001);
            //IPEndPoint remote01 = new IPEndPoint(IPAddress.Any, 8002);
            while (true)
            {
                try
                {
                    byte[] recivcedata = RecviceClient.Receive(ref remote); //RecviceClient.Receive(ref remote01);
                    string strMsg = System.Text.Encoding.UTF8.GetString(recivcedata, 0, recivcedata.Length);
                    DataTransfer<T> data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTransfer<T>>(strMsg);
                    if (data.id != ID) handleObject.DataTransport(data);
                }
                catch
                {
                    break;
                }
            }
        }


        void SendFunc()
        {
            int port = 8001;
            UdpClient SendClient = null;
            while (true)
            {
                try
                {
                    SendClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                    break;
                }
                catch
                {

                    continue;
                }
            }
            IPAddress remoteIp = IPAddress.Parse("255.255.255.255");
            IPEndPoint ip = new IPEndPoint(remoteIp, port);
            handleObject.Log("发送线程启动成功:" + ip.ToString());
            while (true)
            {
                if (TargetMsg.Count == 0)
                {
                    Thread.Sleep(500);
                    continue;
                }
                try
                {
                    T content = TargetMsg.Dequeue();
                    if (content == null) continue;
                    DataTransfer<T> data = new DataTransfer<T>();

                    data.id = ID; data.data = content;
                    byte[] msgBuff = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                    SendClient.EnableBroadcast = true;
                    SendClient.Send(msgBuff, msgBuff.Length, ip);
                }
                catch (Exception ex)
                {
                    handleObject.Log("发送失败:" + ex.ToString());
                }
            }
        }
    }
}