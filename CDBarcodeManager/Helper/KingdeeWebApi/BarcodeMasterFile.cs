using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.WebApi.Client;
using CDBarcodeManager.Common;

namespace CDBarcodeManager.Helper.KingdeeWebApi
{
    public class BarcodeMasterFile
    {
        /// <summary>
        /// 条码主档WebApi
        /// </summary>
        /// <param name="jsonArrayText">数据组</param>
        /// <param name="user">用户名</param>
        /// <param name="psw">密码</param>
        /// <param name="dbid"></param>
        /// <param name="apiurl">路径</param>
        /// <returns></returns>
        public static string JsonAdd(string jsonArrayText, string dbid, K3CloudWebApiInfo apiInfo)
        {
            string strresult = string.Empty;
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //JArray jArray = (JArray)JsonConvert.DeserializeObject(jo["list"].ToString());//jsonArrayText必须是带[]数组格式字符串
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
                model.Add("FID", 0);

                // 开始设置单据字段值
                // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
                // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来

                // 普通字段
                model.Add("FCREATEORGID", new JObject() { { "FNumber", jo["StockOrgId"] } });//库存组织
                model.Add("FMATERIALID", new JObject() { { "FNumber", jo["MaterialId"] } });//物料id
                model.Add("FNumber", jo["FCDBarcode"]);//单据编号 保存后才出现(条码)
                //model.Add("FSUPPLYID", new JObject() { { "FNumber", jo["FSUPPLIERID"] } });
                model.Add("FName", "lhr");//名称
                model.Add("FINSTOCKDATE", jo["Date"]);

                model.Add("F_aaa_ZXBZ", jo["F_aaa_ZXBZ"]);
                model.Add("F_aaa_XH", jo["F_aaa_XH"]);
                model.Add("F_aaa_GG", jo["F_aaa_GG"]);
                model.Add("F_aaa_ZXXH", jo["F_aaa_ZXXH"]);
                model.Add("F_aaa_Weight", jo["F_aaa_Weight"]);
                model.Add("F_aaa_AxesNum", jo["F_aaa_AxesNum"]);
                model.Add("F_aaa_SCDate", jo["F_aaa_SCDate"]);
                model.Add("F_aaa_CaseNo", jo["F_aaa_CaseNo"]);
                model.Add("F_AAA_BZX",jo["F_AAA_BZX"]);
                model.Add("F_AAA_PM", jo["F_AAA_PM"]);
                // 基础资料，填写编码

                // 开始构建单据体参数：集合参数JArray
                JArray entryRows = new JArray();
                // 把单据体行集合，添加到model中，以单据体Key为标识
                string entityKey = "FEntityTrace";
                JObject entitymodel = new JObject();

              

                //单据名称单据编号
                entitymodel.Add("FStockDirect",0);
                entitymodel.Add("FBillFormId", new JObject() { { "FID", jo["BillFormID"] } }); //单据名称
                entitymodel.Add("FBILLNo",jo["BillNo"]);//单据编号
                entitymodel.Add("FUnitID", new JObject() { { "FNumber", jo["FUnitID"] } });//单位
                entitymodel.Add("FQty", jo["FBARCODEQTY"]);//数量
                entitymodel.Add("FSecQty",jo["FBARCODEQTY"]);//数量辅单位
                entitymodel.Add("FBILLDATE", jo["Date"]);//日期


                entryRows.Add(entitymodel);
                model.Add(entityKey, entryRows);
                // 通过循环创建单据体行：示例代码仅创建一行
                //for (int i = 0; i < jArray.Count(); i++)
                //{
                // 添加新行，把新行加入到单据体行集合
                // JObject entryRow = new JObject();
                //entryRows.Add(entryRow);


                // 给新行，设置关键字段值
                // 单据体主键：必须填写，系统据此判断是新增还是修改行
                // entryRow.Add("FEntryID", 0);

                // 基础资料，填写编码
                //var basedata = new JObject();
                //basedata.Add("FNumber", jArray[i]["FMATERIALNUMBER"]);
                //entryRow.Add("F_PAEZ_PType", basedata);

                // 普通字段
                //entryRow.Add("F_PAEZ_StartGG", 0);
                //}

                // 调用Web API接口服务，保存其他入库
                result = client.Execute<string>(
                    "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
                    new object[] { "k8152c172ab47452887cbf04bfad6547f", jsonRoot.ToString() });
            }
            return result;
         }

        /// <summary>
        /// 条码主档有条码新增单据体行
        /// </summary>
        /// <param name="jsonArrayText"></param>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        /// <param name="dbid"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static string JsonEntityAdd(string jsonArrayText, string dbid, K3CloudWebApiInfo apiInfo) 
        {
            string strresult = string.Empty;
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonArrayText);
            //JArray jArray = (JArray)JsonConvert.DeserializeObject(jo["list"].ToString());//jsonArrayText必须是带[]数组格式字符串
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
                jsonRoot.Add("IsDeleteEntry", "false");
                // Model: 单据详细数据参数
                JObject model = new JObject();
                jsonRoot.Add("Model", model);

               
                //model.Add("F_aaa_CaseNo", jo["F_aaa_CaseNo"]);
                // 基础资料，填写编码"IsDeleteEntry": "true"
                model.Add("FLOTID",jo["FLOTID"]);
                model.Add("IsDeleteEntry","false");
                // 开始构建单据体参数：集合参数JArray
                JArray entryRows = new JArray();
                // 把单据体行集合，添加到model中，以单据体Key为标识
                string entityKey = "FEntityTrace";
                JObject entitymodel = new JObject();
                //单据名称单据编号
                entitymodel.Add("FStockDirect", 0);
                entitymodel.Add("FBillFormId", new JObject() { { "FID", jo["BillFormID"] } }); //单据名称
                entitymodel.Add("FBILLNo", jo["BillNo"]);//单据编号
                entitymodel.Add("FUnitID", new JObject() { { "FNumber", jo["FUnitID"] } });//单位
                entitymodel.Add("FQty", jo["FBARCODEQTY"]);//数量
                entitymodel.Add("FSecQty", jo["FBARCODEQTY"]);//数量辅单位
                entitymodel.Add("FBILLDATE", jo["Date"]);//日期


                entryRows.Add(entitymodel);
                model.Add(entityKey, entryRows);
                // 通过循环创建单据体行：示例代码仅创建一行
                //for (int i = 0; i < jArray.Count(); i++)
                //{
                // 添加新行，把新行加入到单据体行集合
                // JObject entryRow = new JObject();
                //entryRows.Add(entryRow);


                // 给新行，设置关键字段值
                // 单据体主键：必须填写，系统据此判断是新增还是修改行
                // entryRow.Add("FEntryID", 0);

                // 基础资料，填写编码
                //var basedata = new JObject();
                //basedata.Add("FNumber", jArray[i]["FMATERIALNUMBER"]);
                //entryRow.Add("F_PAEZ_PType", basedata);

                // 普通字段
                //entryRow.Add("F_PAEZ_StartGG", 0);
                //}

                // 调用Web API接口服务，保存其他入库
                result = client.Execute<string>(
                    "Kingdee.BOS.WebApi.ServicesStub.DynamicFormService.Save",
                    new object[] { "k8152c172ab47452887cbf04bfad6547f", jsonRoot.ToString() });
            }
            return result;
        }

        public static string BatchSave(List<JObject> saveJoList, string dbid, K3CloudWebApiInfo apiInfo)
        {
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
                jsonRoot.Add("IsDeleteEntry", "false");
                jsonRoot.Add("BatchCount", 20);
                // Model: 单据详细数据参数
                JArray models = new JArray();
                foreach (var saveJo in saveJoList)
                {
                    bool isAlterBill = Convert.ToBoolean(saveJo["IsAlterBill"].ToString());
                    if (isAlterBill)
                    {
                        models.Add(getAlterModel(saveJo));
                    }
                    else
                    {
                        models.Add(getCreateModel(saveJo));
                    }
                }
                jsonRoot.Add("Model", models);
                result = client.BatchSave("k8152c172ab47452887cbf04bfad6547f", jsonRoot.ToString());
            }

            return result;
        }

        private static JObject getAlterModel(JObject saveJo)
        {
            JObject model = new JObject();

            //model.Add("F_aaa_CaseNo", jo["F_aaa_CaseNo"]);
            // 基础资料，填写编码"IsDeleteEntry": "true"
            model.Add("FLOTID", saveJo["FLOTID"]);
            // 开始构建单据体参数：集合参数JArray
            JArray entryRows = new JArray();
            // 把单据体行集合，添加到model中，以单据体Key为标识
            string entityKey = "FEntityTrace";
            JObject entitymodel = new JObject();
            //单据名称单据编号
            entitymodel.Add("FStockDirect", 0);
            entitymodel.Add("FBillFormId", new JObject() { { "FID", saveJo["BillFormID"] } }); //单据名称
            entitymodel.Add("FBILLNo", saveJo["BillNo"]);//单据编号
            entitymodel.Add("FUnitID", new JObject() { { "FNumber", saveJo["FUnitID"] } });//单位
            entitymodel.Add("FQty", saveJo["FBARCODEQTY"]);//数量
            entitymodel.Add("FSecQty", saveJo["FBARCODEQTY"]);//数量辅单位
            entitymodel.Add("FBILLDATE", saveJo["Date"]);//日期

            entryRows.Add(entitymodel);
            model.Add(entityKey, entryRows);

            return model;
        }

        /// <summary>
        /// 组合创建条码主档的Model Json，用于批量保存
        /// </summary>
        /// <param name="saveJo"></param>
        /// <returns></returns>
        private static JObject getCreateModel(JObject saveJo)
        {
            JObject model = new JObject();
            // 单据主键：必须填写，系统据此判断是新增还是修改单据；新增单据，填0
            model.Add("FID", 0);

            // 开始设置单据字段值
            // 必须设置的字段：主键、单据类型、主业务组织，各必录且没有设置默认值的字段
            // 特别注意：字段Key大小写是敏感的，建议从BOS设计器中，直接复制字段的标识属性过来

            // 普通字段
            model.Add("FCREATEORGID", new JObject() { { "FNumber", saveJo["StockOrgId"] } });//库存组织
            model.Add("FMATERIALID", new JObject() { { "FNumber", saveJo["MaterialId"] } });//物料id
            model.Add("FNumber", saveJo["FCDBarcode"]);//单据编号 保存后才出现(条码)
                                                   //model.Add("FSUPPLYID", new JObject() { { "FNumber", jo["FSUPPLIERID"] } });
            model.Add("FName", "lhr");//名称
            model.Add("FINSTOCKDATE", saveJo["Date"]);

            model.Add("F_aaa_ZXBZ", saveJo["F_aaa_ZXBZ"]);
            model.Add("F_aaa_XH", saveJo["F_aaa_XH"]);
            model.Add("F_aaa_GG", saveJo["F_aaa_GG"]);
            model.Add("F_aaa_ZXXH", saveJo["F_aaa_ZXXH"]);
            model.Add("F_aaa_Weight", saveJo["F_aaa_Weight"]);
            model.Add("F_aaa_AxesNum", saveJo["F_aaa_AxesNum"]);
            model.Add("F_aaa_SCDate", saveJo["F_aaa_SCDate"]);
            model.Add("F_aaa_CaseNo", saveJo["F_aaa_CaseNo"]);
            model.Add("F_AAA_BZX", saveJo["F_AAA_BZX"]);
            model.Add("F_AAA_PM", saveJo["F_AAA_PM"]);
            // 基础资料，填写编码

            // 开始构建单据体参数：集合参数JArray
            JArray entryRows = new JArray();
            // 把单据体行集合，添加到model中，以单据体Key为标识
            string entityKey = "FEntityTrace";
            JObject entitymodel = new JObject();



            //单据名称单据编号
            entitymodel.Add("FStockDirect", 0);
            entitymodel.Add("FBillFormId", new JObject() { { "FID", saveJo["BillFormID"] } }); //单据名称
            entitymodel.Add("FBILLNo", saveJo["BillNo"]);//单据编号
            entitymodel.Add("FUnitID", new JObject() { { "FNumber", saveJo["FUnitID"] } });//单位
            entitymodel.Add("FQty", saveJo["FBARCODEQTY"]);//数量
            entitymodel.Add("FSecQty", saveJo["FBARCODEQTY"]);//数量辅单位
            entitymodel.Add("FBILLDATE", saveJo["Date"]);//日期


            entryRows.Add(entitymodel);
            model.Add(entityKey, entryRows);

            return model;
        }
    }
}
