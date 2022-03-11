using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager
{
    [HotUpdate]
    public class DelBarcode : AbstractListPlugIn
    {
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            //点击清除库存为0的信息createStkTransferByType
            if (e.BarItemKey== "DelZero") 
            {
                this.View.ShowMessage("是否清除库存为0的数据？", MessageBoxOptions.OKCancel, new Action<MessageBoxResult>(result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sql = "delete from T_BARCODE_INVENTORY where FBASEQTY = 0 ";
                        DBUtils.Execute(this.Context, sql);

                        this.View.Refresh();
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        // 用户选择了取消    
                        this.View.ShowMessage("删除操作已取消");
                    }
                }));
            }
        }
    }
}
