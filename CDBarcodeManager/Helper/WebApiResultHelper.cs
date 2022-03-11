using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Helper
{
    public class WebApiResultHelper
    {
        public JObject JResult { get; private set; }

        public bool IsSuccess => (bool)JResult["Result"]["ResponseStatus"]["IsSuccess"];

        public string BillNo => JResult["Result"]["ResponseStatus"]["SuccessEntitys"][0]["Number"].ToString();
        public WebApiResultHelper(string strResult)
        {
            JResult = (JObject)JsonConvert.DeserializeObject(strResult);
        }

        public static string CreateResult(bool isSuccess, string msg)
        {
            string result = "{\"Result\":{\"ResponseStatus\":{\"ErrorCode\":\"\",\"IsSuccess\":\"false\",\"Errors\":[{\"FieldName\":\"\",\"Message\":\"\",\"DIndex\":0}],\"SuccessEntitys\":[{\"Id\":\"\",\"Number\":\"\",\"DIndex\":0}],\"SuccessMessages\":[{\"FieldName\":\"\",\"Message\":\"\",\"DIndex\":0}],\"MsgCode\":\"\"},\"Id\":\"\",\"Number\":\"\",\"NeedReturnData\":[{}]}}";

            JObject jo = (JObject)JsonConvert.DeserializeObject(result);
            jo["Result"]["ResponseStatus"]["IsSuccess"] = isSuccess;
            jo["Result"]["ResponseStatus"]["Errors"][0]["Message"] = msg;
            return jo.ToString();
        }

        public string Errors()
        {
            JArray errors = (JArray)JResult["Result"]["ResponseStatus"]["Errors"];
            string error = "";
            foreach (JObject jObject in errors)
            {
                error += $"第 {((int)jObject["DIndex"] + 1).ToString()} 行的字段名 \"{jObject["FieldName"].ToString()}\" 出现异常：{jObject["Message"].ToString()}"
                    + Environment.NewLine;
            }
            return error;
        }
    }
}
