using CDBarcodeManager.Common;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Helper.KingdeeWebApi
{
    public class BarcodeInventory
    {
        /// <summary>
        /// 条码即时库存WebApi 
        /// </summary>
        /// <param name="jsonArrayText"></param>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        /// <param name="dbid"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static string JsonAdd(string jsonArrayText, string dbid, K3CloudWebApiInfo apiInfo)//, List<string> Barcodelist,List<string> barcodeqtylist
        {

            string strresult = string.Empty;
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();

                // Creator: 创建用户
                //jsonRoot.Add("FCreatorId", user);

                // NeedUpDateFields: 哪些字段需要更新？为空则表示参数中全部字段，均需要更新
                //jsonRoot.Add("NeedUpDateFields", new JArray(""));

                // Model: 单据详细数据参数
                JObject model = new JObject();


                jsonRoot.Add("Model", model);
                // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
                model.Add("FID", 0);
                // 开始设置单据字段值
                // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
                // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来 


                model.Add("FMATERIALID", new JObject() { { "FNumber", jo["MaterialId"] } });//物料编码
                model.Add("FStockOrgId", new JObject() { { "FNumber", jo["StockOrgId"] } });// 库存组织
                model.Add("FStockId", new JObject() { { "FNumber", jo["StockId"] } });//仓库编码
                if (jo["StockLocId"] != null)
                {
                    JObject stockLocJo = new JObject() { { $"FSTOCKLOCID__FF{jo["FFlexid"]}", new JObject() { { "FNumber", jo["StockLocId"] } } } };
                    model.Add("FStockLocId", stockLocJo);//仓位
                }
                model.Add("FBarcode", new JObject() { { "FNUMBER", jo["FCDBarcode"] } });//条码
                model.Add("FBaseUnitId", new JObject() { { "FNUMBER", jo["FUnitID"] } });//基本单位
                model.Add("FBaseQty", jo["FBARCODEQTY"]);//数量
                //JObject fzsx = new JObject() { { $"FAUXPROPID__FF100001", new JObject() { { "FNumber", jo["FAUXPROPID"] } } } };
                model.Add("FAUXPROPID", jo["FAUXPROPID"]);


                result = client.Execute<string>(
               "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
               new object[] { "kc3745afa915645be9afb37de603a11cf", jsonRoot.ToString() });
            }

            // 调用Web API接口服务，保存即时库存汇总


            return result;
        }

        public static string BatchSave(List<JObject> saveJoList, string dbid, K3CloudWebApiInfo apiInfo)
        {
            string strresult = string.Empty;
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            JArray models = new JArray();
            if (loginResult == true)
            {
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();
                foreach (var saveJo in saveJoList)
                {
                    if (saveJo["inStockId"] is null)
                    {
                        models.Add(getSaveJo(saveJo));
                    }
                    else
                    {
                        // 若调拨单反审时需要生成条码即时库存
                        models.Add(getTransOutSaveJo(saveJo));
                    }
                }

                jsonRoot.Add("Model", models);
                result = client.BatchSave("kc3745afa915645be9afb37de603a11cf", jsonRoot.ToString());

            }

            return result;
        }

        public static string inJsonAdd(string jsonArrayText, string dbid, K3CloudWebApiInfo apiInfo)
        {

            string strresult = string.Empty;
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();

                // Creator: 创建用户
                //jsonRoot.Add("FCreatorId", user);

                // NeedUpDateFields: 哪些字段需要更新？为空则表示参数中全部字段，均需要更新
                //jsonRoot.Add("NeedUpDateFields", new JArray(""));

                // Model: 单据详细数据参数
                JObject model = new JObject();
                jsonRoot.Add("Model", model);
                // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
                model.Add("FID", 0);
                // 开始设置单据字段值
                // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
                // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来 


                model.Add("FMATERIALID", new JObject() { { "FNumber", jo["MaterialId"] } });//物料编码
                model.Add("FStockOrgId", new JObject() { { "FNumber", jo["StockOrgId"] } });// 库存组织
                model.Add("FStockId", new JObject() { { "FNumber", jo["inStockId"] } });//仓库编码
                if (jo["StockLocId"] != null)
                {
                    JObject stockLocJo = new JObject() { { $"FSTOCKLOCID__FF{jo["inFFlexid"]}", new JObject() { { "FNumber", jo["inStockLocId"] } } } };
                    model.Add("FStockLocId", stockLocJo);//仓位
                }
                model.Add("FBarcode", new JObject() { { "FNUMBER", jo["FCDBarcode"] } });//条码
                model.Add("FBaseUnitId", new JObject() { { "FNUMBER", jo["FUnitID"] } });//基本单位
                model.Add("FBaseQty", jo["FBARCODEQTY"]);//数量
                //JObject fzsx = new JObject() { { $"FAUXPROPID__FF100001", new JObject() { { "FNumber", jo["FAUXPROPID"] } } } };
                model.Add("FAUXPROPID", jo["FAUXPROPID"]);
                result = client.Execute<string>(
               "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
               new object[] { "kc3745afa915645be9afb37de603a11cf", jsonRoot.ToString() });
                
            }

            // 调用Web API接口服务，保存即时库存汇总


            return result;
        }
        /// <summary>
        /// 点击反审核时如果去除掉数量
        /// </summary>
        /// <param name="jsonArrayText"></param>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        /// <param name="dbid"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static string JsonEdit(string fid, decimal num, string dbid, K3CloudWebApiInfo apiInfo)
        {

            string strresult = string.Empty;
            //JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();

                // Model: 单据详细数据参数
                JObject model = new JObject();
                jsonRoot.Add("Model", model);

                // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0     
                model.Add("FID", fid);//
                model.Add("FBaseQty", num);//数量

                // 调用Web API接口服务，保存即时库存汇总
                result = client.Execute<string>(
                    "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
                    new object[] { "kc3745afa915645be9afb37de603a11cf", jsonRoot.ToString() });
            }
            return result;
        }

        public static string JsonBatchEdit(string dbid, K3CloudWebApiInfo apiInfo, List<JObject> editJoList)
        {
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";
            // 开始构建Web API参数对象
            // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
            JObject jsonRoot = new JObject();

            JArray models = new JArray();

            if (loginResult == true)
            {
                foreach (var editJo in editJoList)
                {
                    models.Add(getEditJo(editJo));
                }
                jsonRoot.Add("Model", models);

                result = client.BatchSave("kc3745afa915645be9afb37de603a11cf", jsonRoot.ToString());
            }

            return result;
        }

        private static JObject getEditJo(JObject editJo)
        {
            JObject model = new JObject();

            // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0     
            model.Add("FID", editJo["FID"]);//
            model.Add("FBaseQty", editJo["Qty"]);//数量

            return model;
        }



        private static JObject getSaveJo(JObject saveJo)
        {
            JObject model = new JObject();

            // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
            model.Add("FID", 0);
            // 开始设置单据字段值
            // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
            // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来 


            model.Add("FMATERIALID", new JObject() { { "FNumber", saveJo["MaterialId"] } });//物料编码
            model.Add("FStockOrgId", new JObject() { { "FNumber", saveJo["StockOrgId"] } });// 库存组织
            model.Add("FStockId", new JObject() { { "FNumber", saveJo["StockId"] } });//仓库编码
            if (saveJo["StockLocId"] != null)
            {
                JObject stockLocJo = new JObject() { { $"FSTOCKLOCID__FF{saveJo["FFlexid"]}", new JObject() { { "FNumber", saveJo["StockLocId"] } } } };
                model.Add("FStockLocId", stockLocJo);//仓位
            }
            model.Add("FBarcode", new JObject() { { "FNUMBER", saveJo["FCDBarcode"] } });//条码
            model.Add("FBaseUnitId", new JObject() { { "FNUMBER", saveJo["FUnitID"] } });//基本单位
            model.Add("FBaseQty", saveJo["FBARCODEQTY"]);//数量
            //JObject fzsx = new JObject() { { $"FAUXPROPID__FF100001", new JObject() { { "FNumber", jo["FAUXPROPID"] } } } };
            model.Add("FAUXPROPID", saveJo["FAUXPROPID"]);

            return model;
        }

        private static JObject getTransOutSaveJo(JObject saveJo)
        {
            JObject model = new JObject();

            // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
            model.Add("FID", 0);
            // 开始设置单据字段值
            // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
            // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来 


            model.Add("FMATERIALID", new JObject() { { "FNumber", saveJo["MaterialId"] } });//物料编码
            model.Add("FStockOrgId", new JObject() { { "FNumber", saveJo["StockOrgId"] } });// 库存组织
            model.Add("FStockId", new JObject() { { "FNumber", saveJo["inStockId"] } });//仓库编码
            if (saveJo["StockLocId"] != null)
            {
                JObject stockLocJo = new JObject() { { $"FSTOCKLOCID__FF{saveJo["inFFlexid"]}", new JObject() { { "FNumber", saveJo["inStockLocId"] } } } };
                model.Add("FStockLocId", stockLocJo);//仓位
            }
            model.Add("FBarcode", new JObject() { { "FNUMBER", saveJo["FCDBarcode"] } });//条码
            model.Add("FBaseUnitId", new JObject() { { "FNUMBER", saveJo["FUnitID"] } });//基本单位
            model.Add("FBaseQty", saveJo["FBARCODEQTY"]);//数量
            //JObject fzsx = new JObject() { { $"FAUXPROPID__FF100001", new JObject() { { "FNumber", jo["FAUXPROPID"] } } } };
            model.Add("FAUXPROPID", saveJo["FAUXPROPID"]);

            return model;
        }

        public static string inJsonEdit(string infid, decimal innum, string dbid, K3CloudWebApiInfo apiInfo)
        {

            string strresult = string.Empty;
            //JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //金蝶云组件
            K3CloudApiClient client = new K3CloudApiClient(apiInfo.K3CloudUrl);
            var loginResult = client.Login(
                   dbid,
                   apiInfo.K3CloudUser,
                   apiInfo.K3CloudPassword,
                   2052);

            string result = "登录失败，请检查与站点地址、数据中心Id，用户名及密码！";

            if (loginResult == true)
            {
                // 开始构建Web API参数对象
                // 参数根对象：包含Creator、NeedUpDateFields、Model这三个子参数
                JObject jsonRoot = new JObject();

                // Model: 单据详细数据参数
                JObject model = new JObject();
                jsonRoot.Add("Model", model);

                // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0     
                model.Add("infid", infid);//
                model.Add("FBaseQty", innum);//数量

                // 调用Web API接口服务，保存即时库存汇总
                result = client.Execute<string>(
                    "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
                    new object[] { "kc3745afa915645be9afb37de603a11cf", jsonRoot.ToString() });
            }
            return result;
        }
    }
}
