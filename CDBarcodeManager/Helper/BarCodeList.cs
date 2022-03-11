using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Helper
{
    public class BarCodeList
    {
        public BarCodeList( string barcode, string BarcodeQty)
        {
           
            this.barcode = barcode;
            this.BarcodeQty = BarcodeQty;
        }
        public string barcode { get; set; }
        public string BarcodeQty { get; set; }
    }
}
