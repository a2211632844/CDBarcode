using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CDBarcodeManager.Model;

namespace CDBarcodeManager.Helper
{
    public class ReturnInfo
    {
        /// <summary>
        ///动态获取 单据体标识  和  数量 标识
        /// </summary>
        /// <param name="formid"></param>
        /// <returns></returns>
        public static FileNameModel GetFieldName(string formid)
        {
            //生产入库单 生产退库单
            if (formid == "PRD_INSTOCK" || formid == "PRD_RetStock")
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "RealQty", Date = "Date", Billtype = "BillType", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StocklocId", Unit = "FUnitID", org = "" };
            }
            //盘亏  BillEntry  LOSSQTY
            else if (formid == "STK_StockCountLoss")
            {
                return new FileNameModel
                { Entity = "BillEntry", Qty = "CountQty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitId", org = "" };
            }
            //盘盈  BillEntry  GAINQTY
            else if (formid == "STK_StockCountGain")
            {
                return new FileNameModel
                { Entity = "BillEntry", Qty = "CountQty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitID", org = "" };
            }
            //调拨 BillEntry QTY
            else if (formid == "STK_TransferDirect")//"DestStockId"  "DestStockLocId"
            {
                return new FileNameModel
                { Entity = "TransferDirectEntry", Qty = "Qty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", inStockId= "DestStockId",InStockLocId= "DestStockLocId", StockId = "SRCSTOCKID", StocklocId = "SRCSTOCKLOCID", Unit = "UnitId", org = "" };
            }
            //采购入库 InStockEntry QTY
            else if (formid == "STK_InStock")
            {
                return new FileNameModel
                { Entity = "InStockEntry", Qty = "RealQty", Billtype = "FBillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitID", org = "" };
            }
            //采购退料  PURMRBENTRY RMREALQTY
            else if (formid == "PUR_MRB")
            {
                return new FileNameModel
                { Entity = "PUR_MRBENTRY", Qty = "RMREALQTY", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "STOCKID", StocklocId = "FSTOCKLOCID", Unit = "FUnitID", org = "" };
            }
            //简单生产入库 Entity  REALQTY
            else if (formid == "SP_InStock")
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "RealQty", Billtype = "BillType", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "FUnitID", org = "" };
            }
            //简单生产退库   Entity OUTQTY
            else if (formid == "SP_OUTSTOCK")
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "OutQty", Billtype = "BillType", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "FUnitID", org = "" };
            }
            //简单生产领料
            else if (formid == "SP_PickMtrl")
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "ActualQty", Billtype = "BillType", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "StockUnitId", org = "" };
            }
            //简单生产退料
            else if (formid=="SP_ReturnMtrl") 
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "Out", Billtype = "BillType", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitID", org = "" };
            }
            //生产领料单   Entity ACTUALQTY
            else if (formid == "PRD_PickMtrl")
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "ActualQty", Billtype = "BillType", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitId", org = "" };
            }
            //生产退料单   Entity QTY
            else if (formid == "PRD_ReturnMtrl")
            {
                return new FileNameModel
                { Entity = "Entity", Qty = "Qty", Billtype = "BillType", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitID", org = "" };
            }
            //其他入库单   Entity QTY
            else if (formid == "STK_MISCELLANEOUS")
            {
                return new FileNameModel
                { Entity = "STK_MISCELLANEOUSENTRY", Qty = "FQty", Billtype = "BillTypeID", Date = "FDate", ORG = "FStockOrgId", StockId = "FSTOCKID", StocklocId = "StockPlaceId", Unit = "FUnitID", org = "" };
            }
            //其他出库单   Entity QTY
            else if (formid == "STK_MisDelivery")
            {
                return new FileNameModel
                { Entity = "BillEntry", Qty = "Qty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StockLocId", Unit = "UnitID", org = "" };
            }
            //销售出库单   Entity REALQTY
            else if (formid == "SAL_OUTSTOCK")
            {
                return new FileNameModel
                { Entity = "SAL_OUTSTOCKENTRY", Qty = "RealQty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "StockID", StocklocId = "StockLocID", Unit = "UnitID", org = "" };
            }
            //销售退货单   Entity REALQTY
            else if (formid == "SAL_RETURNSTOCK")
            {
                return new FileNameModel
                { Entity = "SAL_RETURNSTOCKENTRY", Qty = "RealQty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgId", StockId = "StockId", StocklocId = "StocklocId", Unit = "UnitId", org = "" };
            }
            //组装拆卸单   Entity QTY
            else if (formid == "STK_AssembledApp")
            {
                return new FileNameModel
                { Entity = "ProductEntity", Qty = "FQty", Billtype = "FBillTypeID", Date = "FDate", ORG = "StockOrgID", StockId = "StockID", StocklocId = "StockPlaceID", Unit = "FUnitID", org = "" };
            }
            //期初库存   Entity QTY
            else if (formid == "STK_InvInit")
            {
                return new FileNameModel
                { Entity = "InvInitDetail", Qty = "Qty", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgID", StockId = "SubSTOCKID", StocklocId = "StockLocId", Unit = "UnitID", org = "" };
            }
            return new FileNameModel { Entity = "Entity", Qty = "QTY", Billtype = "BillTypeID", Date = "Date", ORG = "StockOrgID", StockId = "StockId", StocklocId = "StocklocId", Unit = "UnitId", org = "" };//默认 单据体标识  数量标识
        }
    }
}
