using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UDPV1
{
    /// <summary>
    /// 类别转换器
    /// </summary>
    /// <typeparam name="LOCAL"></typeparam>
    /// <typeparam name="NET"></typeparam>
    /// <typeparam name="OBJECT"></typeparam>
    class DataTranslater<LOCAL,NET,OBJECT> where OBJECT: DataTranslater<LOCAL,NET,OBJECT>.DYNAMIC_HANDLE<LOCAL>
    {
        public interface DYNAMIC_HANDLE<LOCAL>{
            void SetDynamic(LOCAL data);
            LOCAL GetDynamic();
        }

        public delegate NET LOCAL_TO_NET(LOCAL tLocal);
        public delegate LOCAL NET_TO_LOCAL(NET tNet);
        public delegate bool IS_DYNAMIC_CHANGE(LOCAL New,LOCAL LAST);
        
        public LOCAL_TO_NET LocalToNetHandle;
        public NET_TO_LOCAL NetToLocalHandle;
        public IS_DYNAMIC_CHANGE IsDynamicChange;
        public OBJECT tOnlineObject;


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

        public void Set(OBJECT tObject)  {
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

        public static DataTranslater<LOCAL, NET, OBJECT> TranslaterInstance<LOCAL, NET, OBJECT>(
            OBJECT tObject,
            LOCAL_TO_NET localNetHandle,
            NET_TO_LOCAL netLocalHandle,
            IS_DYNAMIC_CHANGE isDynamicChange
            ) where OBJECT : DataTranslater<LOCAL, NET, OBJECT>.DYNAMIC_HANDLE<LOCAL>
        {
            DataTranslater<LOCAL, NET, OBJECT> tTranslater = new DataTranslater<LOCAL, NET, OBJECT>();
            tTranslater.Set(tObject);
            tTranslater.Set(localNetHandle);
            tTranslater.Set(netLocalHandle);
            tTranslater.Set(isDynamicChange);
            return tTranslater;
        }
    }
}
