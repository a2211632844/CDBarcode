using Kingdee.BOS.Core.CommonFilter.ConditionVariableAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.OutStock
{
    public class ImportBarcodeModel
    {
        public ImportBarcodeModel(string qty,string barcode,string BarcodeId)
        {
            this.qty = qty;
            this.barcode = barcode;
            this.BarcodeId = BarcodeId;
        }
        
        public string qty { get; set; }
          
        public string barcode  { get; set; }
        public string BarcodeId { get; set; }


        /// <summary>
        /// 仓位编号 （10001）
        /// </summary>
        public string FFlexid { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string FStockId { get; set; }
        /// <summary>
        /// 物料
        /// </summary>
        public string FMaterialId { get; set; }
        /// <summary>
        /// 仓位
        /// </summary>
        public string FSTOCKLOCIDId { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string FCDBarcode { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal FQty { get; set; }
    }

    
    
}
