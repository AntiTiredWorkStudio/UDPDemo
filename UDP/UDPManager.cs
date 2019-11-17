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
        void DataTransport(DataTransfer<T> data);//接受消息
        T OnSendData();//发送消息
    }


     [System.Serializable]
    public class DataTransfer<T>
    {
        public string id;
        public T data;
    }
    

    public class UDPManager<T>
    {
        public UDPManager(string groupIP,int port,UDPHandle<T> handle)
        {
            groupAddress = IPAddress.Parse(groupIP);
            tport = port;
            InitManager(handle);
        }
        int tport;
        UDPHandle<T> handleObject;
       /* Queue<DataTransfer<T>> ResendMsg;
        public void AddMsg(T msg)
        {
            DataTransfer<T> targetTransfer = new DataTransfer<T>();
            targetTransfer.data = msg;
            targetTransfer.id = ID;
            if (ResendMsg != null) ResendMsg.Enqueue(targetTransfer);
        }*/
        Thread sendThread = null;
        Thread reciveThread = null;
        void InitManager(UDPHandle<T> handle)
        {
            handleObject = handle;
            handleObject.Log("用户:" + ID);
            //ResendMsg = new Queue<DataTransfer<T>>();
            reciveThread = new Thread(ReciveFunc);
            reciveThread.IsBackground = true;
            reciveThread.Start();

            sendThread = new Thread(SendFunc);
            sendThread.IsBackground = true;
            sendThread.Start();
        }
        public void CloseManager()
        {
            try
            {
                sendThread.Interrupt();
                reciveThread.Interrupt();
                sendThread.Abort();
                reciveThread.Abort();
                Sendclient.Close();
                ReceiveClient.Close();
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
                    System.Random rand = new System.Random(); id = "user_" + (1000000 + rand.Next() % 999999);
                    }
                }
                return id;
            }
        }

        IPAddress groupAddress = IPAddress.Parse("234.5.6.7");

        void ReciveFunc()
        {
            ReceiveClient = new UdpClient(tport);
            ReceiveClient.JoinMulticastGroup(groupAddress);
            handleObject.Log("加入组播:" + groupAddress.ToString());
            ReceivePort = new IPEndPoint(groupAddress, tport);
            while (true)
            {
                byte[] buf = ReceiveClient.Receive(ref ReceivePort);
                string msg = Encoding.UTF8.GetString(buf);
                DataTransfer<T> transfer = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTransfer<T>>(msg);
               
                if (transfer.id != ID)
                {
                    handleObject.DataTransport(transfer);
                }
            }
        }


        UdpClient Sendclient;
        UdpClient ReceiveClient;

        IPEndPoint SendPort;
        IPEndPoint ReceivePort;

        void SendFunc()
        {
            Sendclient = new UdpClient();
            //发送信息的端口一定要和接受的端口号一样
            SendPort = new IPEndPoint(groupAddress, tport);
            while (true)
            {
                T tData = handleObject.OnSendData();
                if (tData != null)
                {
                    DataTransfer<T> SendingData = new DataTransfer<T>();
                    SendingData.id = ID;
                    SendingData.data = tData;
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(SendingData);
                    byte[] bufs = Encoding.UTF8.GetBytes(json);
                    Sendclient.Send(bufs, bufs.Length, SendPort);
                    //Thread.Sleep(250);
                }
                else
                {
                    Thread.Sleep(20);
                }
            }
        }
    }
}



namespace UDP_v1
{
    public interface UDPHandle
    {
        /// <summary>
        /// 自定义控制器物体名称
        /// </summary>
        string HandleName { get; }//自定义名称
        /// <summary>
        /// 自定义控制器所属类别
        /// </summary>
        string HandleType { get; }//自定义类别
        /// <summary>
        /// //自定义用户ID,仅限SYSTEM控制器生效
        /// </summary>
        string IDSetting { get; }
      //  string HandleType();//句柄类别
      //  string IDSetting();//自定义用户ID
      /// <summary>
      /// 输出消息
      /// </summary>
      /// <param name="msg">消息内容</param>
        void Log(string msg);//输出错误消息
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="data"></param>
        void DataTransport(params Fields[] data);//接受消息
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <returns></returns>
        Fields[] OnSendData();//发送消息
    }


    [System.Serializable]
    public class DataTransfer
    {
        /// <summary>
        /// 获取数据传输器实例
        /// </summary>
        /// <param name="_uid"></param>
        /// <param name="_type"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static DataTransfer GetInstance(string _uid, string _type, params Fields[] _data)
        {
            DataTransfer transfer = new DataTransfer();
            transfer.uid = _uid;
            transfer.type = _type;
            transfer.FieldsJoining(_data);
            return transfer;
        }
        public string uid;//用户id
        public string type;//消息类型
        public List<Fields> data;//消息实体
        /// <summary>
        /// 增加字段
        /// </summary>
        /// <param name="targetFields">字段</param>
        /// <returns></returns>
        public DataTransfer FieldsJoining(params Fields[] targetFields)
        {
            foreach (Fields tFields in targetFields)
            {
                if (tFields.IsNull())
                {
                    continue;
                }
                if (!data.Contains(tFields))
                {
                    tFields.AttachToDataTransfer(this);
                }
            }
            return this;
        }
        DataTransfer()
        {
            data = new List<Fields>();
        }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this,Newtonsoft.Json.Formatting.Indented);
        }
    }

    /// <summary>
    /// 数据容器转换接口
    /// </summary>
   /* public interface FieldsTranslater
    {
        Fields TranslateToFields();
        void RestoreToObject(Fields tFields);
    }*/

    /// <summary>
    /// 数据容器
    /// </summary>
    [System.Serializable]
    public class Fields
    {
        public string name;
        public List<Field> FieldList;

        /// <summary>
        /// 通过接口转化对象
        /// </summary>
        /// <param name="tTransalter"></param>
        /// <returns></returns>
        public static Fields GetFieldsInstance<LOCAL>(DataTranslater<LOCAL, Fields> tTransalter) where LOCAL:struct
        {
            return tTransalter.LocalToNetHandle(tTransalter.tOnlineObject.GetDynamic(),default);
        }

        public static Fields GetFieldsInstance(string name,params Field[] FieldsList)
        {
            Fields tFields = new Fields(name);
            tFields.AddFields(FieldsList);
            return tFields;
        }


        public Field this[string key]
        {
            get
            {
                List<Field> tField = new List<Field>(from Field tf in FieldList where tf.key == key select tf);
                if (tField.Count > 0) {
                    return tField[0];
                } else
                {
                    return null;
                }
            }
        }
        public Fields() { FieldList = new List<Field>();name = "";AttachTransfer = null; }
        public Fields(string tName,DataTransfer tTransfer)
        {
            FieldList = new List<Field>();
            name = tName;
            AttachTransfer = tTransfer;
        }
        public Fields(string tName)
        {
            FieldList = new List<Field>();
            name = tName;
            AttachTransfer = null;
        }
        DataTransfer AttachTransfer;
        /// <summary>
        /// 挂载数据交换器
        /// </summary>
        /// <returns></returns>
        public Fields AttachToDataTransfer(DataTransfer targetTransfer)
        {
            AttachTransfer = targetTransfer;
            if (!targetTransfer.data.Contains(this))
            {
                targetTransfer.data.Add(this);
            }
            return this;
        }
        /// <summary>
        /// 添加字段
        /// </summary>
        /// <param name="aFields"></param>
        /// <returns></returns>
        public Fields AddFields(params Field[] aFields)
        {
            foreach(Field f in aFields)
            {
                if (ContainsField(f.key))
                {
                    throw new Exception("已存在字段:" + f.key);
                }
                FieldList.Add(f);
            }
            return this;
        }

        public bool ContainsFields(params string[] keys)
        {
            foreach (string k in keys)
            {
                List<Field> sf = new List<Field>(from Field tf in FieldList where tf.key == k select tf);
                if (sf.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool ContainsField(string key)
        {
            List<Field> sf = new List<Field>(from Field tf in FieldList where tf.key == key select tf);
            return sf.Count > 0;
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// 判断Fields对象为空
        /// </summary>
        public bool IsNull()
        {
                return string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name) || FieldList == null || FieldList.Count == 0;
        }
    }

    /// <summary>
    /// 字段
    /// </summary>
    [System.Serializable]
    public class Field
    {
        /// <summary>
        /// 生成单个字段
        /// </summary>
        /// <param name="k"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Field Single(string k, string v) { return (new Field()).Set(k,v); }
        /// <summary>
        /// 生成多个字段
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static Field[] Multiple(params string[] paras) {
            int targetSeek = paras.Length;
            if(paras.Length%2 == 1)
            {
                targetSeek--;
            }
            List<string> keys = new List<string>();
            List<string> vals = new List<string>();
            int total = 0;
            for (int i = 0; i < targetSeek; i++)
            {
                if (i % 2 == 0)
                {
                    keys.Add(paras[i]);
                }
                else
                {
                    vals.Add(paras[i]);
                    total++;
                }
            }

            // Field[] ResultFields = new Field[total+1];
            List<Field> ResultFields = new List<Field>();
            for (int i = 0; i < total; i++)
            {
              //  UnityEngine.Debug.LogWarning("add:" + (new List<string>(from string f in keys where f == keys[i] select f)).Count);
              /*重点关照！！！*/
                if ((new List<Field>(from Field f in ResultFields where f.key==keys[i] select f)).Count >0)
                {
                    continue;
                }
                /*重点关照！！！*/
                //UnityEngine.Debug.Log(UnityEngine.JsonUtility.ToJson(Field.Single(keys[i], vals[i])));
                ResultFields.Add(Field.Single(keys[i],vals[i]));
               // UnityEngine.Debug.Log("ResultFields.Count:"+ResultFields.Count);
               // UnityEngine.Debug.Log(ResultFields[0].key+"||"+UnityEngine.JsonUtility.ToJson(ResultFields.ToArray()));
               /* if (ResultFields.Count > 0)
                {
                    UnityEngine.Debug.Log("vals[i]:"+vals[i]+","+ResultFields[0].key + " " + ResultFields[0].value);
                }*/
            }
            return ResultFields.ToArray();
        }
        public string key;//索引
        public string value;//值
        public Field() { }
        public Field Set(string k,string v) { key = k;value = v; return this; }
    }
    
    public class Route<T>
    {
        public string type;
        public UDPHandle HandleObject; 
    }

    /// <summary>
    /// UDP控制器
    /// </summary>
    public class UDPManager
    {
        /// <summary>
        /// 获得UDPManager的实例
        /// </summary>
        /// <param name="groupIP">组播IP地址(连接的用户需保持一致)</param>
        /// <param name="port">组播端口(连接的用户需保持一致)</param>
        /// <returns></returns>
        public static UDPManager Instance(string groupIP, int port)//单例模式串联
        {
            if(_Instance == null)
            {
                _Instance = new UDPManager(groupIP, port);
            }
            return _Instance;
        }

        /// <summary>
        /// 返回初始化后的UDPManager实例
        /// </summary>
        /// <returns></returns>
        public static UDPManager Instance()
        {
            if (_Instance == null)
            {
                return null;
            }
            return _Instance;
        }

        /// <summary>
        /// 返回是否实例化UDPManager
        /// </summary>
        public static bool HasInstance
        {
            get { return _Instance != null; }
        }

        static UDPManager _Instance = null;

        private UDPManager(string groupIP, int port)
        {
            OptionsCommands = new Dictionary<string, string>();
            groupAddress = IPAddress.Parse(groupIP);//设置组播IP地址
            tport = port;//设置组播端口
            DiscoveryLogServer = new Dictionary<string, List<UDPHandle>>();//初始化中转站
        }

        public static class Options
        {
            /// <summary>
            /// 是否接收自己的消息(开关)
            /// </summary>
            public const string OPTIONS_SWITCH_SelfRecive = "SELFMSG";
        }

        /// <summary>
        /// 配置属性
        /// </summary>
        Dictionary<string, string> OptionsCommands;
        public UDPManager SetOptions(string key,string value)
        {
            if (OptionsCommands.ContainsKey(key))
            {
                OptionsCommands[key] = value;
                return this;
            }
            OptionsCommands.Add(key, value);
            return this;
        }
        /// <summary>
        /// 设置开关属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        public UDPManager SetOptionsSwitch(string key,bool result)
        {
            if (OptionsCommands.ContainsKey(key))
            {
                OptionsCommands[key] = result.ToString();
                return this;
            }
            OptionsCommands.Add(key, result.ToString());
            return this;
        }
        /// <summary>
        /// 检查属性
        /// </summary>
        bool CheckOptions(string key,string value)
        {
            return OptionsCommands.ContainsKey(key) && OptionsCommands[key] == value;
        }
        /// <summary>
        /// 检查属性开关
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool CheckSwitchOptions(string key ,bool value)
        {
            return OptionsCommands.ContainsKey(key) && bool.Parse(OptionsCommands[key]);
        }

        /// <summary>
        /// 组播端口
        /// </summary>
        int tport;

        /// <summary>
        /// 消息处理中转站
        /// </summary>
        public Dictionary<string, List<UDPHandle>> DiscoveryLogServer;
        /// <summary>
        /// 增加处理消息中转站类别
        /// </summary>
        /// <param name="key"></param>
        void AddUDPHandleClassify(string key)
        {
            if (DiscoveryLogServer.ContainsKey(key))
            {
                if(DiscoveryLogServer[key] == null) { DiscoveryLogServer[key] = new List<UDPHandle>(); }
                return;
            }
            DiscoveryLogServer.Add(key, new List<UDPHandle>());
        }

        /// <summary>
        /// 系统消息返回类别
        /// </summary>
        public const string MANAGER_SYSTEM_TYPE = "SYSTEM";


        /// <summary>
        /// 控制器内联消息输出
        /// </summary>
        /// <param name="text"></param>
        void ManagerLog(string text)
        {
            if (!DiscoveryLogServer.ContainsKey(MANAGER_SYSTEM_TYPE))
            {
                return;
            }
            DiscoveryLogServer[MANAGER_SYSTEM_TYPE].ForEach(handle => handle.Log(text));//.Log(text);
        }
        /// <summary>
        /// 内联用户id定义
        /// </summary>
        /// <returns></returns>
        string ManagerIDName()
        {
            if (!DiscoveryLogServer.ContainsKey(MANAGER_SYSTEM_TYPE) || DiscoveryLogServer[MANAGER_SYSTEM_TYPE].Count<=0)
            {
                return "";
            }
            return DiscoveryLogServer[MANAGER_SYSTEM_TYPE][0].IDSetting;
        }


        Thread sendThread = null;
        Thread reciveThread = null;

        /// <summary>
        /// 移出处理句柄
        /// </summary>
        /// <param name="handleObjects"></param>
        /// <returns></returns>
        public UDPManager UnRegistHandle(params UDPHandle[] handleObjects)
        {
            foreach (UDPHandle handle in handleObjects)
            {
                string type = handle.HandleType;//获取类别
                if (string.IsNullOrEmpty(type))
                {
                    ManagerLog("类别定义错误");
                    continue;
                }
                if (!DiscoveryLogServer.ContainsKey(type))
                {
                    ManagerLog("未注册'"+type+"'");
                    continue;
                }
                DiscoveryLogServer.Remove(type);//注册句柄
            }
            return this;
        }

        /// <summary>
        /// 判断是否存在系统级的消息句柄
        /// </summary>
        public bool HasSystemHandle
        {
            get
            {
                return DiscoveryLogServer != null && DiscoveryLogServer.ContainsKey(MANAGER_SYSTEM_TYPE);
            }
        }

        /// <summary>
        /// 注册处理句柄,响应类型请在HandleType中定义
        /// </summary>
        /// <returns></returns>
        public UDPManager RegistHandle(params UDPHandle[] handleObjects)
        {
            foreach (UDPHandle handle in handleObjects)
            {
                string type = handle.HandleType;//获取类别
                if (string.IsNullOrEmpty(type))
                {
                    ManagerLog("类别定义错误");
                    continue;
                }
                AddUDPHandleClassify(type);
                if (!DiscoveryLogServer[type].Contains(handle))
                {
                    DiscoveryLogServer[type].Add(handle);//注册句柄
                    ManagerLog("注册:" + type);
                }
            }
            return this;
        }

        int sendMillSec = 20;
        public UDPManager RunManager(bool isBackground = true,int sendMillSeconds = 20)
        {
           /* if(DiscoveryLogServer.Count == 0)
            {
                ManagerLog("未设置侦听句柄");
                return this;
            }*/
            sendMillSec = sendMillSeconds;

           //启用线程
           reciveThread = new Thread(ReciveFunc);
            reciveThread.IsBackground = isBackground;
            reciveThread.Start();

            sendThread = new Thread(SendFunc);
            sendThread.IsBackground = isBackground;
            sendThread.Start();
            return this;
        }

        /// <summary>
        /// 终止UDPManager的线程及各类调用
        /// </summary>
        public void QuitManager()
        {
            try
            {
                sendThread.Interrupt();
                reciveThread.Interrupt();
                sendThread.Abort();
                reciveThread.Abort();
                Sendclient.Close();
                ReceiveClient.Close();
            }
            catch (Exception e)
            {
                ManagerLog(e.ToString());
            }
        }

        /// <summary>
        /// ID定义
        /// </summary>
        string id = "nan";
        public string ID
        {
            get
            {
                string idSetting = ManagerIDName();
                if (id == "nan")
                {
                    if (!string.IsNullOrEmpty(idSetting))
                    {
                        id = idSetting;
                    }
                    else
                    {
                        System.Random rand = new System.Random(); id = "user_" + (1000000 + rand.Next() % 999999);
                    }
                }
                return id;
            }
        }

        /// <summary>
        /// 组播地址
        /// </summary>
        IPAddress groupAddress = IPAddress.Parse("234.5.6.7");

        /// <summary>
        /// 接收消息
        /// </summary>
        UdpClient ReceiveClient;
        IPEndPoint ReceivePort;
        void ReciveFunc()
        {
            ReceiveClient = new UdpClient(tport);
            ReceiveClient.JoinMulticastGroup(groupAddress);
            ManagerLog("加入组播:" + groupAddress.ToString());
            ReceivePort = new IPEndPoint(groupAddress, tport);
            while (true)
            {
                byte[] buf = ReceiveClient.Receive(ref ReceivePort);
                string msg = Encoding.UTF8.GetString(buf);
                DataTransfer transfer = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTransfer>(msg);
                if (transfer.uid != ID)//非自己的消息
                {
                    DispatchDatas(transfer);
                }
                else
                {
                    if (CheckSwitchOptions(Options.OPTIONS_SWITCH_SelfRecive, true))DispatchDatas(transfer);
                    //ManagerLog("自身消息:"+ msg);
                }
            }
        }

        /// <summary>
        /// 分发数据
        /// </summary>
        /// <param name="target"></param>
        bool DispatchDatas(DataTransfer transfer)
        {
            //判断侦听器注册数量
            if (DiscoveryLogServer.Count == 0)
            {
                ManagerLog("未设置侦听器，无法响应消息");
                //LogFile("UDPManager", "未设置侦听器,无法响应消息");
                return false;
            }
            if (DiscoveryLogServer.ContainsKey(transfer.type))
            {
                DiscoveryLogServer[transfer.type].ForEach(handle=>handle.DataTransport(transfer.data.Where(cTrans=>cTrans.name == handle.HandleName).ToArray()));
                return true;
            }
            else
            {
                ManagerLog("未找到与'" + transfer.type + "'相匹配的接收器");
                return false;
            }
        }
        public string dataCollections;
        /// <summary>
        /// 发送消息
        /// </summary>
        UdpClient Sendclient;
        IPEndPoint SendPort;
        void SendFunc()
        {
            Sendclient = new UdpClient();
            //发送信息的端口一定要和接受的端口号一样
            SendPort = new IPEndPoint(groupAddress, tport);
            while (true)
            {
                //判断侦听器注册数量
                if (DiscoveryLogServer.Count == 0)
                {
                    ManagerLog("未设置侦听器，无法分拣消息");
                    //LogFile("UDPManager", "未设置侦听器，无法分拣消息");
                    continue;
                }
                try
                {
                    Queue<DataTransfer> datas = new Queue<DataTransfer>();
                    List<string> HandleKeys = new List<string>(DiscoveryLogServer.Keys);
                    foreach (string key in HandleKeys)//构建消息队列
                    {
                        List<UDPHandle> tHandle = DiscoveryLogServer[key];
                        if (tHandle == null || tHandle.Count == 0)
                        {
                            //Handle类别为空
                            continue;
                        }
                        DataTransfer transfer = DataTransfer.GetInstance(ID, tHandle[0].HandleType);
                        tHandle.ForEach(delegate (UDPHandle cHandle)
                        {
                            Fields[] tField = cHandle.OnSendData();
                            if (tField == null || tField.Length == 0) { return; }
                            for (int i = 0; i < tField.Length; i++)
                            {
                                tField[i].name = cHandle.HandleName;
                            }
                            transfer.FieldsJoining(tField);
                        });
                        if (transfer.data.Count != 0)
                        {
                            datas.Enqueue(transfer);
                            dataCollections = transfer.ToString();
                        }
                        /*
                         Fields[] tField = tHandle.OnSendData();
                        if(tField == null || tField.Length == 0) { continue; }
                        DataTransfer transfer = DataTransfer.GetInstance(ID, tHandle.HandleType(), tField);
                        datas.Enqueue(transfer);
                        */
                    }
                    if (datas.Count > 0)
                    {
                        while (datas.Count > 0)
                        {
                            DataTransfer SendingData = datas.Dequeue();//前面已经确保了DataTransfer不为空
                            try
                            {
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(SendingData);
                                byte[] bufs = Encoding.UTF8.GetBytes(json);
                                Sendclient.Send(bufs, bufs.Length, SendPort);
                            }
                            catch (Exception e)
                            {
                                DiscoveryLogServer[SendingData.type].ForEach(handle => handle.Log(e.ToString()));
                            }
                        }
                    }
                    else
                    {
                        //ManagerLog("空转");
                        Thread.Sleep(sendMillSec);
                    }
                }catch(Exception e)
                {
                    ManagerLog(e.ToString());
                }
            }
        }

        void LogFile(string name,string text)
        {
            System.IO.File.WriteAllText(System.Environment.CurrentDirectory+"/"+name+".txt",text);
        }
    }
}