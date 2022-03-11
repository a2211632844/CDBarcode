using Kingdee.BOS;
using Kingdee.BOS.BusinessEntity.BusinessFlow;
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
    [Description("不能填写重复的条码")]
    public class BarcodeDistinct : AbstractDynamicFormPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.FieldName == "FCDBARCODE") 
            {
                if (this.Model.GetValue("FCDBARCODE", e.Row) != null)
                {

                    int index = this.Model.GetEntryRowCount("FEntityBarcode");//获取行数
                    int idx = this.Model.GetEntryCurrentRowIndex("FEntityBarcode");//获取当前行还号
                    //this.View.InvokeFieldUpdateService("FCDBARCODE", idx);
                    //this.View.InvokeFieldUpdateService("FBARCODEQTY", idx);
                    string Barcode = "";
                    List<string> barcodelist = new List<string>();
                    //for (int i = 0; i < index; i++)
                    //{
                    var subEntityRows = this.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntityBarcode")).ToList();
                    subEntityRows.RemoveAt(e.Row);
                    string NowBarcode = this.Model.GetValue("FCDBARCODE", e.Row).ToString();//获取当前行输入的条码
                    //if (idx != 0)
                    {
                        bool exists = subEntityRows.Any(x => x["FCDBARCODE"]?.ToString() == NowBarcode);//判断 list里是否含有这个条码
                        if (exists)
                        {
                            throw new KDBusinessException("", "条码不能出现重复 ");
                        }
                    }
                    //if (this.Model.GetValue("FCDBARCODE", e.Row) != null)
                    //{
                    //    Barcode = this.Model.GetValue("FCDBARCODE", e.Row).ToString();
                    //    barcodelist.Add(Barcode);
                    //}
                    //}

                }
            }
        }
    }
}
