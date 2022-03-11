using Kingdee.BOS.BusinessEntity.CloudHealthCenter;
using Kingdee.BOS.Core.CommonFilter.ConditionVariableAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBarcodeManager.Helper
{
    public class ModelHelper
    {
       /// <summary>
       /// 仓库
       /// </summary>
        public int FStockID { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public int FMaterialID { get; set; }
        /// <summary>
        /// 仓位
        /// </summary>
        public int FStockLocId { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public int FSUPPLIERID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FBillTypeID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FUnitID { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int FQTY { get; set; }
        /// <summary>
        /// 仓库组织
        /// </summary>
        public int FStockOrgId { get; set; }
        //
        //public static string Returninfo(int FStockID, int FMaterialID, int FStockLocId) 
        //{
        //    int idx = this.Model.GetEntryCurrentRowIndex("FEntity");
        //    DynamicObject fstockid = this.Model.GetValue("FStockID", idx) as DynamicObject;
        //    string FSTOCKID = fstockid["Number"].ToString();
            
            
            
        //    //我们所要添加物料的物料编码
        //    DynamicObject fmaterialId = this.Model.GetValue("FMaterialID", idx) as DynamicObject;
        //    string FMATERIALID = fmaterialId["Number"].ToString();
        //    DynamicObject fstocklocId = this.Model.GetValue("FStockLocId", idx) as DynamicObject;
        //    string FSTOCKLOCIDId = fstocklocId[0].ToString();
        //    //DynamicObject fnumber = this.Model.GetValue("FNumber", idx) as DynamicObject;
        //    //string FNUMBER = fnumber[0].ToString();FSUPPLIERID
        //    DynamicObject FSUPPLIERId = this.Model.GetValue("FSUPPLIERID") as DynamicObject;
        //    string FSUPPLIERID = FSUPPLIERId["Number"].ToString();
        //    DynamicObject FBillTypeID = this.Model.GetValue("FBillTypeID") as DynamicObject;
        //    string FBILLTYPEID = FBillTypeID[0].ToString();
        //    //DynamicObject FBillNo = this.Model.GetValue("FBillNo") as DynamicObject;
        //    //string FBILLNO = FBillNo[0].ToString();
        //    DynamicObject FUnitID = this.Model.GetValue("FUnitID") as DynamicObject;
        //    string FUNITID = FUnitID["Number"].ToString();
        //    Decimal FQty = Convert.ToDecimal(this.Model.GetValue("FQTY", idx));
        //    string FQTY = FQty.ToString();
        //    DynamicObject FStockOrgId = this.Model.GetValue("FStockOrgId") as DynamicObject;
        //    string FSTOCKORGID = FStockOrgId["Number"].ToString();

        //    JObject jo = new JObject();
        //    jo.Add("FMaterialId", FMATERIALID);//物料编码
        //    jo.Add("FStockId", FSTOCKID);//仓库
        //    jo.Add("FStockLocId", FSTOCKLOCIDId);//仓位
        //                                         //jo.Add("FNumber",FNUMBER);//条码
        //    jo.Add("FSUPPLIERID", FSUPPLIERID);//供应商
        //    jo.Add("FBillTypeID", FBILLTYPEID);//单据名称   
        //                                       //jo.Add("FBillNo",FBILLNO);//单据编号
        //    jo.Add("FUnitID", FUNITID);//单位
        //    jo.Add("FQty", FQTY);

        //    jo.Add("FStockOrgId", FSTOCKORGID);
        //    return  ;
        //}
    }
}
