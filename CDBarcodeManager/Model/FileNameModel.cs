using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Model
{
    public class FileNameModel
    {
        /// <summary>
        /// 但具体标识
        /// </summary>
        public string Entity { get; set; }
        /// <summary>
        /// 数量标识
        /// </summary>
        public string Qty { get; set; }

        /// <summary>
        /// 单据名称标识
        /// </summary>
        public string Billtype { get; set; }
        /// <summary>
        /// 日期标识
        /// </summary>
        public string  Date { get; set; }
        /// <summary>
        /// 组织标识
        /// </summary>
        public string ORG { get; set; }
        /// <summary>
        /// 仓库标识
        /// </summary>
        public string StockId { get; set; }
        /// <summary>
        /// 仓位标识
        /// </summary>
        public string  StocklocId { get; set; }
        /// <summary>
        /// 单位标识
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 组织
        /// </summary>
        public string org { get; set; }
        /// <summary>
        /// 调入仓库
        /// </summary>
        public string inStockId { get; set; }

        public string InStockLocId { get; set; }
    }
}
