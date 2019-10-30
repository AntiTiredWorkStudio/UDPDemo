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
    
    class Program
    {
        static string id = "nan";
        static string ID
        {
            get
            {
                if (id == "nan") { Random rand = new Random(); id = "user_"+(1000000+rand.Next()%999999); }
                return id;
            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine("用户:" + ID);
            Console.Read();
            Console.Clear();

            Thread sendThread = null;
            Thread reciveThread = null;
            //Newtonsoft.Json.JsonConvert.SerializeObject(
            reciveThread = new Thread(ReciveFunc); reciveThread.IsBackground = false;
            reciveThread.Start();
            sendThread = new Thread(SendFunc); sendThread.IsBackground = false;
            sendThread.Start();
        }
        static void ReciveFunc()
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
                    Console.WriteLine("接收线程:"+local.ToString()+" 启动成功");
                    break;
                }
                catch
                {
                    Console.WriteLine("端口占用:" + local.ToString() + " 重置端口");
                    if (remoteIp.Count > 1)
                    {
                        local = new IPEndPoint(remoteIp[++seek], port);
                    } else { local.Port++; }
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
                    DataStruct data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataStruct>(strMsg);
                    if(data.id != ID)
                        Console.WriteLine(data.id+":"+data.msg);
                }
                catch
                {
                    break;
                }
            }
        }
        static void SendFunc()
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
            Console.WriteLine("发送线程启动成功:"+ip.ToString());
            
            while (true)
            {
                try
                {
                    string msg = Console.ReadLine();
                    if(msg == "")continue;
                    DataStruct data = new DataStruct();
                    data.id = ID; data.msg = msg;
                    byte[] msgBuff = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                    SendClient.EnableBroadcast = true;//.Send(msgBuff, msgBuff.Length);
                    SendClient.Send(msgBuff, msgBuff.Length, ip);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送失败:" + ex.ToString());
                }
            }
        }
    }
}
