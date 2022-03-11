using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.OutStock
{
    public class ImportBarcode1
    {
        public ImportBarcode1(string qty, string barcode, string BarcodeId)
        {
            this.qty = qty;
            this.barcode = barcode;
            this.BarcodeId = BarcodeId;
        }

        public string qty { get; set; }

        public string barcode { get; set; }
        public string BarcodeId { get; set; }

    }
}
