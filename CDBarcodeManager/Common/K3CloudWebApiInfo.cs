using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Common
{
    public class K3CloudWebApiInfo
    {
        /// <summary>
        /// 金蝶云地址
        /// </summary>
        public string K3CloudUrl { get; private set; }
        /// <summary>
        /// 金蝶云账号
        /// </summary>
        public string K3CloudUser { get; private set; }
        /// <summary>
        /// 金蝶云密码
        /// </summary>
        public string K3CloudPassword { get; private set; }

        /// <summary>
        /// 以默认信息实例
        /// </summary>
        public K3CloudWebApiInfo()
        {
            K3CloudUrl = "http://192.168.6.6/k3cloud/";
            K3CloudUser = "admin";
            K3CloudPassword = "888888";
        }

        public K3CloudWebApiInfo(string url, string user, string password)
        {
            K3CloudUrl = url;
            K3CloudUser = user;
            K3CloudPassword = password;
        }
    }
}
