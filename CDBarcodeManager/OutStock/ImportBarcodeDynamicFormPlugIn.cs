using Kingdee.BOS.App.Core.Warn.Variable;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.OutStock
{
    /// <summary>
    /// 热更新
    /// </summary>
    /*引入条码页面*/
    [HotUpdate]
    [Description("引入条码OnLoad")]
    public class ImportBarcodeDynamicFormPlugIn : AbstractDynamicFormPlugIn
    {

        public List<ImportBarcodeModel> list = new List<ImportBarcodeModel>();
       

        /// <summary>
        /// 销售出库单动态表单
        /// </summary>
        /// <param name="e"></param>
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //判断是否打开动态表单
            string FmaterialId = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FmaterialId"));//物料编码id    
            string Fmaterialnumber = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FmaterialNumber"));//物料编码Number
            string Fstockid = Convert.ToString(this.View.OpenParameter.GetCustomParameter("Fstockid"));//仓库
            string FSTOCKLOCID = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FSTOCKLOCID"));//仓位
            string FStockName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FStockName"));//仓库名称
            string FstocklocName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FstocklocName"));//仓库名称
            string FormName = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FormName")) ;
            string FFlexid = Convert.ToString(this.View.OpenParameter.GetCustomParameter("FFlexid"));

            //string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", Fstockid);
            //DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
            //DataTable ckdt = ckds.Tables[0];
            //string FFlexid = ckdt.Rows[0][0].ToString();

            string sql2 = $"select FBASEUNITID  from t_BD_MaterialBase where FMATERIALID = '{FmaterialId}'";
            DataSet ds2 = DBServiceHelper.ExecuteDataSet(this.Context, sql2);
            DataTable dt2 = ds2.Tables[0];
            //把值赋到文本框内
            this.Model.SetValue("FmaterialId", Fmaterialnumber);
            this.Model.SetValue("Fstockid", FStockName);
            this.Model.SetValue("FSTOCKLOCID", FstocklocName);
            this.Model.SetValue("F_ora_Material",FmaterialId);
            this.Model.SetValue("F_ORA_UNITID", dt2.Rows[0][0]);
        

            StringBuilder sql = new StringBuilder();
            sql.AppendLine(@"select i.FID,bm.FNUMBER,FBASEQTY,bm.FLOTID
                                        from 
                                        T_BARCODE_INVENTORY  i
                                        join T_BD_BARCODEMASTER bm on i.FBARCODE = bm.FMasterId");
            string filter = $" WHERE i.FBASEQTY > 0 AND i.FMATERIALID = '{FmaterialId}' ";

            //仓位为空
            if (FSTOCKLOCID != null)
            {
                sql.AppendLine("left join T_BAS_FLEXVALUESDETAIL stockLoc1 on stockLoc1.FID = i.FSTOCKLOCID");
                sql.AppendLine($"left join T_BAS_FLEXVALUESENTRY stockLoc1_L on stockLoc1_L.FENTRYID = stockLoc1.FF{FFlexid}");
                filter += $" AND  stockLoc1_l.FNUMBER = '{FSTOCKLOCID}' ";

            }

            //
            //                            
            //FSTOCKID  = '{1}'  and stockLoc1_l.FNUMBER = '{2}' and 
            //仓库为空
            if (Fstockid != null)
            {
                filter += $"  and  FSTOCKID  = '{Fstockid}'  and stockLoc1_l.FNUMBER = '{FSTOCKLOCID}' ";
            }
            sql.AppendLine(filter);

            DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql.ToString());
            DataTable dt = ds.Tables[0];
            //List<ImportBarcodeModel> list = new List<ImportBarcodeModel>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string barcode = dt.Rows[i]["FNumber"].ToString();//条码
                string BarcodeId= dt.Rows[i]["FLOTID"].ToString();//条码ID
                string qty = Convert.ToString(dt.Rows[i]["FBASEQTY"]);
                //decimal FBASEQTY = dt.Rows[i]["FBASEQTY"].ToString();
                list.Add(new ImportBarcodeModel(qty, barcode,BarcodeId));
            }
            //循环出动态表单里的值  
            for (int i = 0; i < list.Count; i++)
            {
                this.View.Model.BatchCreateNewEntryRow("F_SAL_OUTSTOCKENTITY", 0);
                this.Model.SetValue("F_SAL_OUTSTOCK_BarCode", list[i].BarcodeId, i);//条码编号   
                this.Model.SetValue("F_SAL_OUTSTOCK_Qty", list[i].qty, i);//数量 
            }
        }
        /// <summary>
        /// 返回条码和数量
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            List<ImportBarcode1> listcodeqty = new List<ImportBarcode1>();
            if (e.BarItemKey== "ora_tbButtonReturn") 
            {
                //List<ImportBarcodeModel> list = new List<ImportBarcodeModel>();
               
                for (int i = 0; i < list.Count; i++)
                {
                    string s2 = this.Model.GetValue("F_ora_InStock", i).ToString();
                    DynamicObject Barcodedy = this.Model.GetValue("F_SAL_OUTSTOCK_BarCode", i) as DynamicObject;
                    string BarocdeId = Barcodedy[0].ToString();
                    string Barcode = Barcodedy["Number"].ToString();
                    string BarcodeQty = this.Model.GetValue("F_SAL_OUTSTOCK_Qty",i).ToString();
                    //如果勾选了 复选框
                    if (Convert.ToBoolean( this.Model.GetValue("F_ora_InStock",i))==true)
                    {
                        this.Model.SetValue("F_SAL_OUTSTOCK_BarCode", BarocdeId, i);//条码编号   
                        this.Model.SetValue("F_SAL_OUTSTOCK_Qty", BarcodeQty, i);//数量 

                        listcodeqty.Add(new ImportBarcode1(BarcodeQty, Barcode, BarocdeId));
                    }
                }
            }
            this.View.ReturnToParentWindow(new FormResult(listcodeqty));//返回数据给上级单据 
            this.View.Close();
        }

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            if (this.View.OpenParameter.GetCustomParameter("formid").ToString().Equals("STK_INVENTORY"))
            { 
                this.View.GetControl("F_ora_InStock").Visible = false; //隐藏是否引入
            }

            if (this.View.OpenParameter.GetCustomParameter("formid").ToString().Equals("STK_INVENTORY"))
            {
                this.View.GetMainBarItem("ora_tbButtonReturn").Visible = false;//隐藏返回数据
            }
        }
    }
      
}
