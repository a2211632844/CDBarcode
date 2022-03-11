using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Metadata.FieldElement;
using CDBarcodeManager.Helper;
using System.ComponentModel;

namespace CDBarcodeManager.OutStock
{
    /// <summary>
    /// 热更新
    /// </summary>
    [HotUpdate]
    [Description("点击引入条码按钮")]
    public class ImportBarcodePlugIn : AbstractDynamicFormPlugIn
    {
        public List<ImportBarcodeModel> list = new List<ImportBarcodeModel>();
        /// <summary>
        /// 点击引入条码按钮点击事件 
        /// </summary>
        /// <param name="e"></param>


        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);
            //点击引入条码按钮
            if (e.BarItemKey == "tbGetBarcode")
            {
                string FormId = this.Model.BillBusinessInfo.GetForm().Id;
                //string FormId = this.BusinessInfo.GetForm().Id;
                //ReturnInfo.GetFieldName(FormId);
                //string entityu = ReturnInfo.GetFieldName(FormId).Entity;
                int idx = this.Model.GetEntryCurrentRowIndex("FEntity");
                DynamicObject fmaterialId = this.Model.GetValue("FMaterialID", idx) as DynamicObject;
                string FMATERIALID = fmaterialId["Number"].ToString();//物料(1005)
                string FMaterialId = fmaterialId[0].ToString();//物料（108716）

                DynamicObject fstockid = this.Model.GetValue("FStockID", idx) as DynamicObject;
                //string FSTOCKID = fstockid["Number"].ToString();//01
                string FStockId = fstockid[0].ToString();//仓库
                string FStockName = fstockid["Name"].ToString();//仓库名称

                string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", FStockId);
                DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
                DataTable ckdt = ckds.Tables[0];
                string FFlexid = ckdt.Rows[0][0].ToString();

                DynamicObject fstocklocId = (this.Model.GetValue("FStockLocId", idx) as DynamicObject)[$"F{FFlexid}"] as DynamicObject;
                string fstocklocid = fstocklocId["ID"].ToString();//仓位(100001)
                string FSTOCKLOCID = fstocklocId["Number"].ToString();//仓位(01)
                string FstocklocName = fstocklocId["Name"].ToString();//仓位名称(A仓)

                var fname = this.Model.BillBusinessInfo.GetForm();
                string formmname = fname.Id;
                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "k0216699ab1c24b82ae7a458fff1b06b2";
                string formid = param.FormId;
                //动态表单id唯一标识
                //传送参数到动态表单  

                param.CustomParams.Add("Formid", formid); //
                param.CustomParams.Add("FmaterialId", FMaterialId);//物料编码 
                param.CustomParams.Add("Fstockid", FStockId);//  仓库
                param.CustomParams.Add("FStockName", FStockName);//仓库名称
                param.CustomParams.Add("FSTOCKLOCID", FSTOCKLOCID);//仓位
                param.CustomParams.Add("FstocklocName", FstocklocName);//仓位名称
                param.CustomParams.Add("FormName", formmname);//表名
                param.CustomParams.Add("FFlexid", FFlexid);//表名


                //this.View.Model.DeleteEntryData("FEntity");
                //this.Model.ClearNoDataRow();
                //this.View.UpdateView("FEntity");
                this.View.Model.DeleteEntryData("FEntityBarcode");
                this.View.ShowForm(param, new Action<FormResult>((formResult) =>
                {
                    //页面赋值
                    if (formResult != null && formResult.ReturnData != null)
                    {
                        //var list = formResult.ReturnData as List<ImportBarcodeModel>;
                        var listcodeqty = formResult.ReturnData as List<ImportBarcode1>;

                        foreach (var model in listcodeqty)
                        {
                            int i = this.View.Model.GetEntryRowCount("FEntityBarcode");
                            this.Model.CreateNewEntryRow("FEntityBarcode");
                            this.View.Model.SetValue("FCDBARCODE", model.barcode, i);
                            this.View.Model.SetValue("FBARCODEQTY", model.qty, i);

                            //this.View.Model.BatchCreateNewEntryRow("FEntityBarcode", -1)

                        }
                        this.Model.ClearNoDataRow();
                        this.View.UpdateView("FEntityBarcode");
                    }
                    else
                    {
                        //this.View.ShowErrMessage("条码值为空");
                    }
                }
                )
                    );
            }
        }
    }
}
