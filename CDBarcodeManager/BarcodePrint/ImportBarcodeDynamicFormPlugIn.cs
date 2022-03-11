using CDBarcodeManager.Common;
using CDBarcodeManager.Helper;
using CDBarcodeManager.Helper.KingdeeWebApi;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
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

namespace CDBarcodeManager.BarcodePrint
{
    [HotUpdate]
    [Description("Onload")]
    public class ImportBarcodeDynamicFormPlugIn : AbstractDynamicFormPlugIn
    {
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string DDRQ = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DDRQ"));//订单日期    
            string DDH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DDH"));//订单号
            string FSeq = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FSeq"));//订单行号
            string DDSL = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DDSL"));//订单数量
            string MaterialId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("MaterialId"));//物料编码    
            string MaterialName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("MaterialName"));//物料名称    
            string MaterialNumber = Convert.ToString(this.View.OpenParameter.GetCustomParameter("MaterialNumber"));//物料Number   
            string FunitId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FunitId"));//单位
            string FunitName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FunitName"));//单位
            //string FStockId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FStockId"));//仓库    
            string FunitNumber = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FunitNumber"));//单位
            //把值赋到动态表单内
            this.Model.SetValue("F_ora_DDRQ", DDRQ);//订单日期
            this.Model.SetValue("F_ora_MaterIalId", MaterialId);//物料ID
            this.Model.SetValue("F_ora_MaterialName", MaterialName);//物料名称
            this.Model.SetValue("F_ora_DDH", DDH);//订单号
            this.Model.SetValue("F_ora_DDSLQty", DDSL);//数量
            this.Model.SetValue("F_ora_DDHH", FSeq);//行号
            this.Model.SetValue("F_DEV_UNITID", FunitId);
        }

        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            //点击生成条码按钮
            if (e.Key.EqualsIgnoreCase("F_ora_CreatBarcode")) 
            {
                decimal sumBarcodeQty = Convert.ToDecimal(this.Model.GetValue("F_ora_AllQty"));//条码汇总数量

                if (Convert.ToDecimal(this.Model.GetValue("F_ora_AllQty")) == decimal.Zero
                    || Convert.ToDecimal(this.Model.GetValue("F_ora_MPSL")) == decimal.Zero)
                {
                    this.View.ShowMessage("每批数量和本次打印数量需要必填。");
                    return;
                }

                if (sumBarcodeQty + Convert.ToDecimal(this.View.Model.GetValue("F_ora_AllQty"))
                    > Convert.ToDecimal(this.View.OpenParameter.GetCustomParameter("DDSL")))
                {
                    this.View.ShowMessage("条码产品总数 > 订单数量，请问是否继续生成条码？", MessageBoxOptions.OKCancel, new Action<MessageBoxResult>(result =>
                    {
                        if (result == MessageBoxResult.Cancel)
                        {
                            // 用户选择了取消       
                            return;
                        }
                    }));
                }

                //获取单据头信息
                string DDRQ = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DDRQ"));//订单日期    
                string DDH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DDH"));//订单号
                string FSeq = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FSeq"));//订单行号
                string DDSL = Convert.ToString(this.View.OpenParameter.GetCustomParameter("DDSL"));//订单数量
                string MaterialId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("MaterialId"));//物料编码    
                string MaterialName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("MaterialName"));//物料名称    
                string MaterialNumber = Convert.ToString(this.View.OpenParameter.GetCustomParameter("MaterialNumber"));//物料Number   
                string FunitId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FunitId"));//单位
                string FunitName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FunitName"));//单位name
                string FunitNumber = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FunitNumber"));//单位


                string PrintSumQty = this.View.Model.GetValue("F_ora_AllQty").ToString();//本次打印总数105
                string mxsl = Convert.ToString(this.View.Model.GetValue("F_ora_MPSL"));//每批数量20
                decimal barcodeLine = Convert.ToDecimal(PrintSumQty) / Convert.ToDecimal(mxsl);//生成多少行条码
                decimal barcodeLinelast = Convert.ToDecimal(PrintSumQty) % Convert.ToDecimal(mxsl);//最后一行条码数量
                int st = Convert.ToInt32(Math.Floor(barcodeLine));//单据体要循环多少行
                int serialNum = 0;
                //根据订单号 和产品编号 获取 序号 即行号
                string sql = string.Format(" /*dialect*/ select F_ora_LineNo from T_BD_BARCODEMASTER  bcm join T_BD_BARCODEMASTERTRACE bcme on  bcm.FLOTID = bcme.FLOTID where FMATERIALID = '{0}' and   bcme.FBILLNO = '{1}'",  MaterialId, DDH);
                DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count != 0)
                {
                    serialNum = Convert.ToInt32(dt.AsEnumerable().Max(r => Convert.ToInt32(r["F_ora_LineNo"])));
                    //throw new Exception(serialNum.ToString());
                }


                Entity entity = this.View.BillBusinessInfo.GetEntity("F_SAL_OUTSTOCKENTITY");
                DynamicObjectCollection rows = this.Model.GetEntityDataObject(entity);
                DynamicObject row = new DynamicObject(entity.DynamicObjectType);

                //查询条码主当 条码  是否存在  存在则不生成 不存在就生成

                //string sqlTM = "";
                //DataSet dsTM = DBServiceHelper.ExecuteDataSet(this.Context, sqlTM);
                //DataTable dtTM = ds.Tables[0];

                for (int i = 0; i < st; i++)
                {
                    serialNum++;
                    // 条码规则：生产订单号.行号.物料编码.流水号
                    string lshlsh = serialNum.ToString().PadLeft(3, '0');
                    string fbillno = createBarcode(DDH, FSeq, MaterialNumber, lshlsh);

                    this.View.Model.CreateNewEntryRow(entity.Key);

                    int idx = this.Model.GetEntryRowCount(entity.Key) - 1;

                    this.Model.SetValue("F_ora_Barcode", fbillno, idx);
                    this.Model.SetValue("F_Qty", mxsl, idx);
                    //头 物料id 条码   日期   单据体 单据名称 单位 数量  数量辅单位 日期

                    JObject jo = new JObject();
                    jo.Add("MaterialId", MaterialNumber);//物料编码
                    jo.Add("FCDBarcode", fbillno);//条码
                    jo.Add("Date", DDRQ);//日期

                    var FBillNo = this.Model.BillBusinessInfo.GetForm().Id;
                    jo.Add("BillFormID", FBillNo);//单据名称
                    jo.Add("FBARCODEQTY", mxsl);//数量
                    jo.Add("FUnitID", FunitNumber);
                    //jo.Add("F_ora_ProductinOrdNum", DDH);//订单号
                    //jo.Add("FUnitID",FunitId);//单位
                    //jo.Add("F_ora_PercaseQty", mxsl);//每箱数量
                    //jo.Add("F_ora_BillNo", serialNum);//流水号
                    //jo.Add("F_ORA_PRINTQTY", PrintSumQty);//本次打印数量
                    jo.Add("FSeq", FSeq);//行号
                    sumBarcodeQty += Convert.ToDecimal(mxsl);
                    //条码主档
                    WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonAdd(jo.ToString(), Context.DBId, new K3CloudWebApiInfo()));
                }

                if (barcodeLinelast > 0)
                {
                    serialNum++;
                    this.View.Model.CreateNewEntryRow(entity.Key);
                    string lshlsh = serialNum.ToString().PadLeft(3, '0');
                    string fbillno = createBarcode(DDH, FSeq, MaterialNumber, lshlsh);

                    row["F_Qty"] = barcodeLinelast.ToString();
                    this.Model.SetValue("F_Qty", barcodeLinelast, this.Model.GetEntryRowCount(entity.Key) - 1);
                    this.Model.SetValue("F_ora_Barcode", fbillno, this.Model.GetEntryRowCount(entity.Key) - 1);

                    JObject jo = new JObject();

                    jo.Add("MaterialId", MaterialNumber);//物料编码
                    jo.Add("FCDBarcode", fbillno);//条码
                    jo.Add("Date", DDRQ);//日期

                    var FBillNo = this.Model.BillBusinessInfo.GetForm().Id;
                    jo.Add("BillFormID", FBillNo);//单据名称
                    jo.Add("F_ora_PercaseQty", barcodeLinelast);
                    jo.Add("FSeq", FSeq);//行号
                    jo.Add("FUnitID", FunitNumber);
                    //jo.Add("F_ora_BillNo", serialNum);
                    //jo.Add("F_ORA_PRINTQTY", PrintSumQty);//本次打印数量 


                    sumBarcodeQty += Convert.ToDecimal(barcodeLinelast);
                    WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonAdd(jo.ToString(), Context.DBId, new K3CloudWebApiInfo()));

                    //this.Model.SetValue("F_ora_BarcodeId", result.FID, this.Model.GetEntryRowCount(entity.Key) - 1);

                }
                this.View.UpdateView(entity.Key);
            }
        }



        private string createBarcode(string moBillNo, string seq, string matNumber, string no)
        {
            // 条码规则：生产订单号.行号.物料编码.流水号
            return $"{moBillNo}.{seq}.{matNumber}.{no}";
        }
    }
}
