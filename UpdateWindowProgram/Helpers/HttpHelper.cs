using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace UpdateWindowProgram.Helpers
{
    public class HttpHelper
    {
        #region "单例"
        private static HttpHelper instance;
        private static object objLock = new object();
        public static HttpHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (objLock)
                    {
                        if (instance == null)
                        {
                            instance = new HttpHelper();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion


        /// <summary>
        /// 判断网络状况的方法,返回值true为连接，false为未连接  
        /// </summary>
        /// <param name="conState"></param>
        /// <param name="reder"></param>
        /// <returns></returns>
        [DllImport("wininet")]
        public extern static bool InternetGetConnectedState(out int conState, int reder);
        public bool IsInternet
        {
            get
            {
                int i;
                return InternetGetConnectedState(out i, 0);
            }

        }
       

    }
}
