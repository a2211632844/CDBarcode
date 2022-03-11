using CDBarcodeManager.Helper;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Common
{
    [HotUpdate]
    [Description("库存单据反审核校验器")]
    public class StockBillUnAuditValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            for (int ii = 0; ii < dataEntities.Length; ii++)
            {
                var mainObj = dataEntities[ii].DataEntity;

                string msg = "";
                if (ValidateHelper.CheckIsBarcodeHasDownStreamBill(this.Context, mainObj["BillNo"].ToString(), out msg))
                {
                    validateContext.AddError(mainObj, new ValidationErrorInfo(
                        ""
                        , dataEntities[ii]["Id"].ToString()
                        , dataEntities[ii].DataEntityIndex
                        , dataEntities[ii].RowIndex
                        , "001"
                        , $"单据中的条码 {msg} 已有下游业务单据，禁止反审！"
                        , ""
                        , ErrorLevel.Error));
                }
            }
        }
    }
}
