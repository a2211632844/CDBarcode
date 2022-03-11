using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.NotePrint;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager
{
    [HotUpdate]
    [Description("点击按钮打开动态表单")]
    public class ImportBarocdePlugIn : AbstractBillPlugIn
    {


        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);
            //点击生产条码按钮
            if (e.BarItemKey == "BarcodePrintBtn")
            {
                //获取行号（采购订单 单据体标识（FPOOrderEntry））
                int idx = this.Model.GetEntryCurrentRowIndex("FPOOrderEntry");

                string DDRQ = this.Model.GetValue("FDATE", idx).ToString();//订单日期

                DynamicObject dyMaterialid = this.Model.GetValue("FMATERIALID", idx) as DynamicObject;//物料
                string MaterialId = dyMaterialid[0].ToString();//id
                string MaterialNumber = dyMaterialid["Number"].ToString();//Number
                string MaterialName = dyMaterialid["Name"].ToString();//Name

                string DDSL = this.Model.GetValue("FQTY", idx).ToString();//订单数量

                //string DDHH = this.Model.GetValue("FSEQ", idx).ToString();//订单行号
                string DDH = "";
                if (this.View.Model.GetValue("FBILLNO", idx) != null)
                {
                    DDH = this.View.Model.GetValue("FBILLNO", idx).ToString();//订单号
                }


                //DynamicObject dyStock = this.Model.GetValue("",idx) as DynamicObject;
                //string FStockId = dyStock[0].ToString();//仓库
                //string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", FStockId);
                //DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
                //DataTable ckdt = ckds.Tables[0];
                //string FFlexid = ckdt.Rows[0][0].ToString();
                //DynamicObject fstocklocId = (this.Model.GetValue("FStockLocId", idx) as DynamicObject)[$"F{FFlexid}"] as DynamicObject;
                //string  StockId = fstocklocId["ID"].ToString();//仓位(100001)
                //string  StockNumber = fstocklocId["Number"].ToString();//仓位(01)
                //string  StockNmae = fstocklocId["Name"].ToString();//仓位名称(A仓)


                DynamicObject dyFunit = this.Model.GetValue("FUNITID", idx) as DynamicObject;//单位
                string FunitId = dyFunit[0].ToString();
                string FunitNumber = dyFunit["Number"].ToString();
                string FunitName = dyFunit["Name"].ToString();



                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "k070aa0cf7a134bf8ad4fe155ec5bbfa1";//条码批量打印 动态表单


                param.CustomParams.Add("DDRQ", DDRQ); //订单日期
                param.CustomParams.Add("DDH", DDH);//订单号 
                var rowData = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FPOOrderEntry"), idx); ;
                param.CustomParams.Add("FSeq", (rowData["Seq"]).ToString());//当前行号
                param.CustomParams.Add("DDSL", DDSL);//订单数量

                param.CustomParams.Add("MaterialId", MaterialId);//产品编码
                param.CustomParams.Add("MaterialName", MaterialName);//产品名称
                param.CustomParams.Add("MaterialNumber", MaterialNumber);//产品Number
                param.CustomParams.Add("FunitId", FunitId);//单位ID
                param.CustomParams.Add("FunitName", FunitName);//单位名称
                param.CustomParams.Add("FunitNumber", FunitNumber);//单位名称

                //param.CustomParams.Add("",FStockId);

                this.View.ShowForm(param, new Action<FormResult>((formResult) =>
                {
                    //页面赋值
                    if (formResult != null && formResult.ReturnData != null)
                    {
                        //string sql = $@"select F_ora_DefaultPrinter from T_SEC_USER
                        //                where FUSERID = {this.Context.UserId}";
                        //var sqlResult = DBServiceHelper.ExecuteDynamicObject(Context, sql);
                        //正式账套：ad29504a-a54c-41f3-9aa3-d0501e688e72
                        //测试账套：7ea88099-9273-4ee3-9312-747453c7b6fc

                        //string printerName = string.Empty;
                        //if (sqlResult.Count > 0
                        //    && !sqlResult[0]["F_ora_DefaultPrinter"].IsNullOrEmptyOrWhiteSpace())
                        //{
                        //    printerName = sqlResult[0]["F_ora_DefaultPrinter"].ToString();
                        //}

                        //PrintView(formResult.ReturnData as List<string>, "ad29504a-a54c-41f3-9aa3-d0501e688e72", printerName);
                    }
                    else
                    {
                        //this.View.ShowErrMessage("条码值为空");
                    }

                }
               ));

            }
        }
    }
}
