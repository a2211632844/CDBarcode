using CDBarcodeManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Helper
{
    class Class1
    {
        public FiledNameModel GetFieldName(string formid)
        {
            if (formid == "xxx")
            {
                return new FiledNameModel { Entity = "ddd", Qty = "dddd" };
            }


            return new FiledNameModel { Entity = "ddd", Qty = "dddd" };
        }
    }
}
