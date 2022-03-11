using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
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

namespace CDBarcodeManager.RealTimeInv
{
    [Description("打开即时库存")]
    [HotUpdate]
    public class InvBarCode : AbstractListPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            if (e.BarItemKey == "BtnBarcode")
            {

                var lv = this.View as IListView;
                if (lv == null)
                {
                    return;
                }
                var selectedRows = lv.SelectedRowsInfo;
                if (selectedRows == null || selectedRows.Count == 0)
                {
                    this.View.ShowMessage("当前没有行被选中！");
                    return;
                }
                var rowIndexs = string.Join(",", selectedRows.Select(o => o.RowKey));
                //this.View.ShowMessage("当前选中行：" + rowIndexs);
                int row = Convert.ToInt32(rowIndexs);

                //if (rowIndexs.Length>0) { }
                //选择的行,获取所有信息,放在listcoll里面
                ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;
                var ss = listcoll[0].DataRow;

                DynamicObject fmaterialid = ss["FMaterialId_Ref"] as DynamicObject;//物料
                string FMaterialId = fmaterialid["Number"].ToString();
                string FMaterialId0 = fmaterialid[0].ToString();

                string FStockId = "";
                string FStockName = "";
                string FFlexid = "";
                string fstocklocid = "";
                string FSTOCKLOCID = "";
                string FstocklocName = "";
                //当仓库不为空
                if (!ss["FStockId_Ref"].IsNullOrEmpty())
                {
                    DynamicObject fstockid = ss["FStockId_Ref"] as DynamicObject;//仓库
                    FStockId = fstockid[0].ToString();
                    FStockName = fstockid["Name"].ToString();
                    string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", FStockId);
                    DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
                    DataTable ckdt = ckds.Tables[0];
                    FFlexid = ckdt.Rows[0][0].ToString();
                    //当仓位不为空
                    if (!ss[$"FStockLocId_FF{FFlexid}_Ref"].IsNullOrEmpty())
                    {
                        DynamicObject fstocklocId = ss[$"FStockLocId_FF{FFlexid}_Ref"] as DynamicObject;
                        fstocklocid = fstocklocId["ID"].ToString();//仓位(100001)
                        FSTOCKLOCID = fstocklocId["Number"].ToString();//仓位(01)
                        FstocklocName = fstocklocId["Name"].ToString();//仓位名称(A仓)
                    }
                    else 
                    {
                        fstocklocid = null;
                        FSTOCKLOCID = null;
                        FstocklocName = null;
                    }
                }
                else 
                {
                    FStockId = null;
                    FStockName = null; 
                }
                // 打开动态表单界面             
                var fname = this.Model.BillBusinessInfo.GetForm();
                string formmname = fname.Id;
                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "k0216699ab1c24b82ae7a458fff1b06b2";
                string formid = param.FormId;


                param.CustomParams.Add("Formid", "STK_INVENTORY"); //
                param.CustomParams.Add("FmaterialId", FMaterialId0);//物料编码id
                param.CustomParams.Add("FmaterialNumber", FMaterialId);
                param.CustomParams.Add("Fstockid", FStockId);//  仓库
                param.CustomParams.Add("FStockName", FStockName);//仓库名称
                param.CustomParams.Add("FSTOCKLOCID", FSTOCKLOCID);//仓位
                param.CustomParams.Add("FstocklocName", FstocklocName);//仓位名称
                param.CustomParams.Add("FormName", formmname);//表名
                param.CustomParams.Add("FFlexid", FFlexid);
                //param.CustomParams.Add("FORMID", "STK_INVENTORY");




                this.View.ShowForm(param, new Action<FormResult>((formResult) =>
                {
                    //页面赋值
                    if (formResult != null && formResult.ReturnData != null)
                    {
                    }
                    else
                    {
                        //this.View.ShowErrMessage("条码值为空");
                    }
                }));
            }
        }
    }
}
