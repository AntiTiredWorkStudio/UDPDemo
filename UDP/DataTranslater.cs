using System;

namespace UDPV1
{
    /// <summary>
    /// 类别转换器
    /// </summary>
    /// <typeparam name="LOCAL">本地数据</typeparam>
    /// <typeparam name="NET">网络数据</typeparam>
    /// <typeparam name="OBJECT">控制器(DYNAMIC_HANDLE类型)</typeparam>
    class DataTranslater<LOCAL,NET>
    {
        public interface DYNAMIC_HANDLE{
            void SetDynamic(LOCAL data);
            LOCAL GetDynamic();
        }

        public delegate NET LOCAL_TO_NET(LOCAL tLocal);
        public delegate LOCAL NET_TO_LOCAL(NET tNet);
        public delegate bool IS_DYNAMIC_CHANGE(LOCAL New,LOCAL LAST);
        
        public LOCAL_TO_NET LocalToNetHandle;
        public NET_TO_LOCAL NetToLocalHandle;
        public IS_DYNAMIC_CHANGE IsDynamicChange;
        public DYNAMIC_HANDLE tOnlineObject;


        LOCAL LastLocalObject;

        public void Set(IS_DYNAMIC_CHANGE isDynamicChange)
        {
            IsDynamicChange = isDynamicChange;
        }

        public void Set(LOCAL_TO_NET localNetHandle)
        {
            LocalToNetHandle = localNetHandle;
        }
        
        public void Set(NET_TO_LOCAL netLocalHandle)
        {
            NetToLocalHandle = netLocalHandle;
        }

        public void Set(DYNAMIC_HANDLE tObject)  {
            tOnlineObject = tObject;
            LastLocalObject = tObject.GetDynamic();
        }

        /// <summary>
        /// LOCAL消息到NET消息
        /// </summary>
        /// <returns></returns>
        public NET OnUpdate(){
            LOCAL dynamicState = tOnlineObject.GetDynamic();
            if (IsDynamicChange(dynamicState, LastLocalObject))
            {
                return LocalToNetHandle(dynamicState);
            }else{
                return default(NET);
            }
        }

        /// <summary>
        /// NET消息到LOCAL消息
        /// </summary>
        /// <param name="netObject"></param>
        public void OnTranslater(NET netObject)
        {
            LOCAL localObject = NetToLocalHandle(netObject);
            if (IsDynamicChange(LastLocalObject, localObject))
            {
                tOnlineObject.SetDynamic(localObject);
            }
        }

        DataTranslater(){
            LastLocalObject = default(LOCAL);
        }

        /// <summary>
        /// 获取类别转换器实例
        /// </summary>
        /// <typeparam name="LOCAL"></typeparam>
        /// <typeparam name="NET"></typeparam>
        /// <typeparam name="OBJECT"></typeparam>
        /// <param name="tObject"></param>
        /// <param name="localNetHandle"></param>
        /// <param name="netLocalHandle"></param>
        /// <param name="isDynamicChange"></param>
        /// <returns></returns>
        public static DataTranslater<LOCAL, NET> TranslaterInstance<T>(
            DYNAMIC_HANDLE tObject,
            LOCAL_TO_NET localNetHandle,
            NET_TO_LOCAL netLocalHandle,
            T isDynamicChange
            ) where T:DYNAMIC_HANDLE
        {
            DataTranslater<LOCAL, NET> tTranslater = new DataTranslater<LOCAL, NET>();
            tTranslater.Set(tObject);
            tTranslater.Set(localNetHandle);
            tTranslater.Set(netLocalHandle);
            tTranslater.Set(isDynamicChange);
            return tTranslater;
        }
    }
}
