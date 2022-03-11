using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Helper
{
    public static class ValidateHelper
    {
        /// <summary>
        /// 检查出入库单据中的条码是否已经有下游业务单据产生
        /// </summary>
        /// <param name="billNo">当前的单据编号</param>
        /// <returns></returns>
        public static bool CheckIsBarcodeHasDownStreamBill(Context ctx, string billNo, out string msg)
        {
            msg = "";
            string sql = $@"SELECT
	                            A.FLOTID, A.FNUMBER
                            INTO #TEMP_BARCODE
                            FROM
	                            T_BD_BARCODEMASTER A
	                            JOIN T_BD_BARCODEMASTERTRACE B ON A.FLOTID = B.FLOTID
                            WHERE
	                            B.FBILLNO = '{billNo}'

                            SELECT
	                            ROW_NUMBER() over(partition by A.FLOTID order by B.FSeq desc) FROWID,A.FNUMBER, B.FBILLNO
                            INTO #TEMP
                            FROM
	                            T_BD_BARCODEMASTER A
	                            JOIN T_BD_BARCODEMASTERTRACE B ON A.FLOTID = B.FLOTID
	                            JOIN #TEMP_BARCODE C ON A.FLOTID = C.FLOTID


                            SELECT
	                            *
                            FROM
	                            #TEMP
                            WHERE
	                            FROWID = 1
	                            AND FBILLNO <> '{billNo}'";

            var checkResults = DBServiceHelper.ExecuteDynamicObject(ctx, sql);
            if (checkResults.Count > 0 )
            {
                List<string> barcodeList = new List<string>();
                checkResults.ToList().ForEach(r => barcodeList.Add(r["FNUMBER"].ToString()));
                msg = string.Join(",", barcodeList);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查物料是否启用了条码管理
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="materialId">物料内码</param>
        /// <returns></returns>
        public static bool CheckIsMatEnableBarcode(Context ctx, string materialId)
        {
            List<SelectorItemInfo> lstSelectorInfos = new List<SelectorItemInfo>
            {
                new SelectorItemInfo("FIsCDBatchManage"),
                new SelectorItemInfo("FIsCDSNManage")
            };
            DynamicObject[] objs =
                BusinessDataServiceHelper.Load(
                ctx,
                "BD_Material",
                lstSelectorInfos,
                OQLFilter.CreateHeadEntityFilter(string.Format("FMATERIALID = {0}", materialId)));

            if (objs != null && objs.Count() > 0)
            {
                var stockBase = objs[0]["MaterialStock"] as DynamicObjectCollection;

                bool isEnableBatchManage = Convert.ToBoolean(stockBase[0]["FIsCDBatchManage"]);
                bool isEnableSnManage = Convert.ToBoolean(stockBase[0]["FIsCDSNManage"]);

                return isEnableBatchManage || isEnableSnManage;
            }

            return false;
        }
    }
}
