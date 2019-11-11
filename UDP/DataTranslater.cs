using System;

namespace UDP_v1
{
    /// <summary>
    /// 类别转换器
    /// </summary>
    /// <typeparam name="LOCAL">本地析构数据模块(需要使用结构体控制)</typeparam>
    /// <typeparam name="NET">网络数据（通常为Fields类型）</typeparam>
    /// <typeparam name="OBJECT">控制器(DYNAMIC_HANDLE类型)</typeparam>
    public class DataTranslater<LOCAL,NET> where LOCAL:struct
    {
        public interface DYNAMIC_HANDLE{
            void SetDynamic(LOCAL data);
            LOCAL GetDynamic();
        }

        public delegate NET LOCAL_TO_NET(LOCAL tLocal);
        public delegate LOCAL NET_TO_LOCAL(NET tNet);
        public delegate bool IS_DYNAMIC_CHANGE(LOCAL New,LOCAL LAST);
        public delegate bool CONFIRM_NET(NET tNet);
        public delegate void Log(string log);

        public LOCAL_TO_NET LocalToNetHandle;
        public NET_TO_LOCAL NetToLocalHandle;
        public IS_DYNAMIC_CHANGE IsDynamicChange;
        public CONFIRM_NET ConfirmHandle;
        public Log LogHandle;
        public DYNAMIC_HANDLE tOnlineObject;


        LOCAL LastLocalObject;

        void SelfLog(string text)
        {
            if (LogHandle != null) { LogHandle(text); }
        }

        public DataTranslater<LOCAL, NET> Set(Log logHandle)
        {
            LogHandle = logHandle;
            return this;
        }

        public DataTranslater<LOCAL, NET> Set(IS_DYNAMIC_CHANGE isDynamicChange)
        {
            IsDynamicChange = isDynamicChange;
            return this;
        }
        public DataTranslater<LOCAL, NET> Set(LOCAL_TO_NET localNetHandle)
        {
            LocalToNetHandle = localNetHandle;
            return this;
        }
        
        public DataTranslater<LOCAL, NET> Set(NET_TO_LOCAL netLocalHandle)
        {
            NetToLocalHandle = netLocalHandle;
            return this;
        }

        public DataTranslater<LOCAL, NET> Set(CONFIRM_NET confirmHandle)
        {
            ConfirmHandle = confirmHandle;
            return this;
        }

        public DataTranslater<LOCAL, NET> Set(DYNAMIC_HANDLE tObject)  {
            tOnlineObject = tObject;
            LastLocalObject = tObject.GetDynamic();
            return this;
        }

        /// <summary>
        /// LOCAL消息到NET消息
        /// </summary>
        /// <returns></returns>
        public NET OnUpdate(){
            LOCAL dynamicState = tOnlineObject.GetDynamic();
            if (IsDynamicChange(dynamicState, LastLocalObject))
            {
                NET target = LocalToNetHandle(dynamicState);
                SelfLog(dynamicState.ToString());
                LastLocalObject = dynamicState;
                if (!ConfirmHandle(target))
                {
                    throw new Exception("不符合ConfirmHandle中的规定类型");
                }
                return target;
            }else{
                return default;
            }
        }

        /// <summary>
        /// NET消息到LOCAL消息
        /// </summary>
        /// <param name="netObject"></param>
        public void OnTranslater(NET netObject)
        {
            if (!ConfirmHandle(netObject))
            {
                throw new Exception("不符合ConfirmHandle中的规定类型");
            }
            LOCAL localObject = NetToLocalHandle(netObject);
            if (IsDynamicChange(LastLocalObject, localObject))
            {
                tOnlineObject.SetDynamic(localObject);
            }
        }

        DataTranslater(){
            LastLocalObject = default;
        }





        /// <summary>
        /// 创建翻译器实例
        /// </summary>
        /// <typeparam name="T">目标控制器</typeparam>
        /// <param name="tObject">析构数据产生器</param>
        /// <param name="localNetHandle">析构数据转为网络数据->原型: public delegate NET LOCAL_TO_NET(LOCAL tLocal);</param>
        /// <param name="netLocalHandle">网络数据转为析构数据->原型: public delegate LOCAL NET_TO_LOCAL(NET tNet);</param>
        /// <param name="confirmHandle">校验网络数据包->原型: public delegate bool CONFIRM_NET(NET tNet);</param>
        /// <param name="isDynamicChange">校验析构数据是否产生变化->原型: public delegate bool IS_DYNAMIC_CHANGE(LOCAL New, LOCAL LAST);</param>
        /// <returns></returns>
        public static DataTranslater<LOCAL, NET> TranslaterInstance<T>(
            T tObject,
            LOCAL_TO_NET localNetHandle,
            NET_TO_LOCAL netLocalHandle,
            CONFIRM_NET confirmHandle,
            IS_DYNAMIC_CHANGE isDynamicChange
            ) where T : class,DYNAMIC_HANDLE
        {
            DataTranslater<LOCAL, NET> tTranslater = new DataTranslater<LOCAL, NET>();
            tTranslater.Set(tObject);
            tTranslater.Set(localNetHandle);
            tTranslater.Set(netLocalHandle);
            tTranslater.Set(isDynamicChange);
            tTranslater.Set(confirmHandle);
            return tTranslater;
        }
    }
}
