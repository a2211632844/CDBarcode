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
 
namespace SAL_OUT
{
    /// <summary>
    /// 热更新
    /// </summary>
    [HotUpdate]
    public class SAL_OUT: AbstractDynamicFormPlugIn
    {
        /// <summary>
        /// 点击引入条码按钮点击事件
        /// </summary>
        /// <param name="e"></param>
        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);
            //点击引入条码按钮
            if (e.BarItemKey== "tbGetBarcode")
            {
                //string FMATERIALID = string.Empty;
                //string FSTOCKID = string.Empty;
                //string FSTOCKLOCID = string.Empty;
                //string BarCode = string.Empty;
                
                int idx = this.Model.GetEntryCurrentRowIndex("FEntity");
                
                
                DynamicObject fmaterialId = this.Model.GetValue("FMaterialID", idx) as DynamicObject;
                string FMATERIALID = fmaterialId[0].ToString();
                DynamicObject fstockid = this.Model.GetValue("FStockID", idx) as DynamicObject;
                string FSTOCKID = fstockid[0].ToString();
                DynamicObject fstocklocid = this.Model.GetValue("FStockLocID", idx) as DynamicObject;
                string FSTOCKLOCID = fstocklocid[0].ToString();
                
                
                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "k0216699ab1c24b82ae7a458fff1b06b2";
                //动态表单id唯一标识
                //传送参数到动态表单
                //param.CustomParams.Add("FmaterialId", this.Model.GetValue("FMaterialID", idx)==null?"" : this.Model.GetValue("FMaterialID", idx).ToString());//物料编码 key value
                //param.CustomParams.Add("Fstockid", this.Model.GetValue("FStockID", idx)==null?"": this.Model.GetValue("FStockID", idx).ToString());//仓库
                //param.CustomParams.Add("FSTOCKLOCID", this.Model.GetValue("FStockLocID", idx)==null?"": this.Model.GetValue("FStockLocID", idx).ToString());//仓位

                param.CustomParams.Add("FmaterialId", FMATERIALID);//物料编码 key value
                param.CustomParams.Add("Fstockid", FSTOCKID);//仓库
                param.CustomParams.Add("FSTOCKLOCID", FSTOCKLOCID);//仓位
                this.View.ShowForm(param, new Action<FormResult>((formResult) =>
                {
                    if (formResult != null && formResult.ReturnData != null)
                    {
                        //处理动态表单返回的数据 返回的条码值
                        this.View.ShowMessage(formResult.ReturnData.ToString());
                        //处理条码 处理数量
                        this.Model.SetValue("",formResult.ReturnData);

                     }
                    else
                    {
                        this.View.ShowErrMessage("条码值为空");
                    }
                }));
            }
        }
        
    }
}
