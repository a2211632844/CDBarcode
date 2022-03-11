using CDBarcodeManager.Common;
using CDBarcodeManager.Helper.KingdeeWebApi;
using CDBarcodeManager.OutStock;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Metadata.StatusElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mime;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Context = Kingdee.BOS.Context;

namespace CDBarcodeManager.Helper
{
    public class BarcodeInvHelper : AbstractDynamicFormPlugIn
    {
        /// <summary>
        /// 根据 仓库 仓位 物料编码 条码 查询出即时库存有没有数据 返回了DT表
        /// </summary>
        /// <param name="FFlexid"></param>
        /// <param name="FStockId"></param>
        /// <param name="FMaterialId"></param>
        /// <param name="FSTOCKLOCIDId"></param>
        /// <param name="FCDBarcode"></param>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static DataTable CheckInventory(string FFlexid, string FStockId, string FMaterialId, string FSTOCKLOCIDId, string FCDBarcode, Context Context)
        {
            string sql = "";
            if (FSTOCKLOCIDId == "")
            {
                sql = string.Format(@"select i.FID,bm.FNUMBER,FBASEQTY,i.*
                                        from 
                                        T_BARCODE_INVENTORY  i
                                        join T_BD_BARCODEMASTER bm on i.FBARCODE = bm.FMasterId                                        
                                        where FSTOCKID  = '{0}' and i.FMATERIALID='{1}'   and bm.FNUMBER='{2}'", FStockId, FMaterialId, FCDBarcode);
            }
            else
            {
                sql = string.Format(@"select i.FID,bm.FNUMBER,FBASEQTY,stockLoc1_l.*,i.*
                                        from 
                                        T_BARCODE_INVENTORY  i
                                        join T_BD_BARCODEMASTER bm on i.FBARCODE = bm.FMasterId
                                        join T_BAS_FLEXVALUESDETAIL stockLoc1 on stockLoc1.FID = i.FSTOCKLOCID				
	                                    join T_BAS_FLEXVALUESENTRY stockLoc1_L on stockLoc1_L.FENTRYID = stockLoc1.FF{0}  and stockLoc1_l.FNUMBER = '{1}'
                                        where FSTOCKID  = '{2}' and i.FMATERIALID='{3}'   and bm.FNUMBER='{4}'", FFlexid, FSTOCKLOCIDId, FStockId, FMaterialId, FCDBarcode);
            }
            DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql);
            DataTable dt = ds.Tables[0];
            return dt;
        }
        /// <summary>
        /// 把判断条码是否存在的部分封装在这  返回是否.
        /// </summary>
        public static bool CheckBarcode(string FFlexid, string FStockId, string FMaterialId, string FSTOCKLOCIDId, string FCDBarcode, Context Context)
        {

            string sql = string.Format(@"select i.FID,bm.FNUMBER,FBASEQTY,stockLoc1_l.*,i.*
                                        from 
                                        T_BARCODE_INVENTORY  i
                                        join T_BD_BARCODEMASTER bm on i.FBARCODE = bm.FMasterId
                                        join T_BAS_FLEXVALUESDETAIL stockLoc1 on stockLoc1.FID = i.FSTOCKLOCID							
	                                    join T_BAS_FLEXVALUESENTRY stockLoc1_L on stockLoc1_L.FENTRYID = stockLoc1.FF{0} 
                                        where FSTOCKID  = '{1}' and i.FMATERIALID='{2}' and stockLoc1_l.FNUMBER = '{3}'  and bm.FNUMBER='{4}'", FFlexid, FStockId, FMaterialId, FSTOCKLOCIDId, FCDBarcode);
            DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql);
            DataTable dt = ds.Tables[0];
            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 修改库存内容  添加 减少 方法一样 都是把原有数量±表内数量 
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="num"></param>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static bool UpdateBarcodeInv(string fid ,decimal num,Context Context)
        {
            //封装修改代码
            WebApiResultHelper result = new WebApiResultHelper(BarcodeInventory.JsonEdit(fid, num, Context.DBId, new K3CloudWebApiInfo()));
            //throw new ArgumentNullException();
            if (result.IsSuccess == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool inUpdateBarcodeInv(string infid, decimal innum, Context Context)
        {
            //封装修改代码
            WebApiResultHelper result = new WebApiResultHelper(BarcodeInventory.inJsonEdit(infid, innum, Context.DBId, new K3CloudWebApiInfo()));
            //throw new ArgumentNullException();
            if (result.IsSuccess == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
