using CDBarcodeManager.Helper;
using CDBarcodeManager.Helper.KingdeeWebApi;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.Client;
using Kingdee.K3.BD.Contracts.FIN;
using Kingdee.K3.Core.FIN;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.Validation;
using System.ComponentModel;
using CDBarcodeManager.Common;

namespace CDBarcodeManager
{
    [HotUpdate]
    [Description("入库单插件")]
    public class SaveBarcode : AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            e.FieldKeys.Add("FStocklocId");
            e.FieldKeys.Add("FStockId");
            e.FieldKeys.Add("FDestStockId");
            e.FieldKeys.Add("FStockOrgId");
            e.FieldKeys.Add("FREALQTY");
            e.FieldKeys.Add("FQTY");
            e.FieldKeys.Add("FOUTQTY");
            e.FieldKeys.Add("UnitId");
            e.FieldKeys.Add("FCountQty");
            e.FieldKeys.Add("RMREALQTY");
            e.FieldKeys.Add("OutQty");
            e.FieldKeys.Add("ActualQty");
            e.FieldKeys.Add("FBillEntry");
            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FBillType");
            e.FieldKeys.Add("FCDBARCODE");
            e.FieldKeys.Add("FBARCODEQTY");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("FSubSTOCKID");
            base.OnPreparePropertys(e);
        }

        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            if (this.FormOperation.Operation == "UnAudit")
            {

                StockBillUnAuditValidator validator = new StockBillUnAuditValidator();
                //校验实体对象
                //校验器依赖的数据在单据头，所以EntityKey使用单据头
                validator.EntityKey = "FBillHead";
                e.Validators.Add(validator);
            }
        }

        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            //string FormId = this.BusinessInfo.GetForm().FSrcFormID;
            if (e.SelectedRows.Count() < 1)
            {
                return;
            }
            DateTime start = DateTime.Now;

            string formId = this.BusinessInfo.GetForm().Id;
            ReturnInfo.GetFieldName(formId);
            string entityu = ReturnInfo.GetFieldName(formId).Entity;

            string materialId = "";
            decimal fQty = 0;
            string stockId = "";
            string stockLocId = "";
            string barcode = "";
            decimal barcodeQty = 0;
            string sqlck1 = "";
            string fFlexId1 = "";
            string rowBarcodeQty = "";
            string materialNumber = "";

            //string STOCKLOCID = "";
            string date = "";
            string strBillType = "";
            string strBillNo = "";
            string strBillName = "";
            string stcStockOrg = "";
            string stockNumber = "";
            string org = "";
            string strStockOrgId = "";
            string documentStatus = "";
            //foreach (ExtendedDataEntity item in e.SelectedRows)
            //{
                
            //}
            foreach (ExtendedDataEntity extended in e.SelectedRows)
            {

                List<JObject> barcodeSaveJoList = new List<JObject>();
                List<JObject> invUpdateJoList = new List<JObject>();
                List<JObject> invCreateJoList = new List<JObject>();
                //DynamicObject stocklocid = item["PrdOrgId"] as DynamicObject;
                //STOCKLOCID = stocklocid["Number"].ToString();//组织
                date = extended[ReturnInfo.GetFieldName(formId).Date].ToString();  //日期


                if (extended[ReturnInfo.GetFieldName(formId).ORG] != null)
                {
                    DynamicObject dyOrg = extended[ReturnInfo.GetFieldName(formId).ORG] as DynamicObject;
                    org = dyOrg["Number"].ToString();
                }

                documentStatus = extended["DocumentStatus"].ToString();

                //DynamicObject DJBH = item["BillNo"] as DynamicObject;
                if (extended["BillNo"] != null)
                {
                    strBillNo = extended["BillNo"].ToString();//单据编号
                }
                //throw new Exception(formId);
                //strBillName = extended["FFormId"].ToString();//单据名称
                strBillName = formId;//单据名称

                if (extended[ReturnInfo.GetFieldName(formId).ORG] != null) //组织
                {
                    DynamicObject dyStockOrg = extended[ReturnInfo.GetFieldName(formId).ORG] as DynamicObject;
                    stcStockOrg = dyStockOrg["Number"].ToString();
                    strStockOrgId = dyStockOrg[0].ToString();
                }

                DynamicObject dyBill = extended.DataEntity;
                //DynamicObjectCollection docEntity = dy["Entity"] as DynamicObjectCollection;
                DynamicObjectCollection dyEntity = dyBill[ReturnInfo.GetFieldName(formId).Entity] as DynamicObjectCollection;
                foreach (DynamicObject entityRow in dyEntity)
                {
                    DynamicObject dyMaterial = entityRow["MaterialId"] as DynamicObject;

                    // 防止空行
                    if (dyMaterial.IsNullOrEmptyOrWhiteSpace())
                    {
                        continue;
                    }

                    materialId = dyMaterial[0].ToString();//物料编码
                    materialNumber = dyMaterial["Number"].ToString();//物料编码

                    // 如果没启用条码管理
                    if (!ValidateHelper.CheckIsMatEnableBarcode(this.Context, materialId))
                    {
                        continue;
                    }

                    DynamicObject dyStock = entityRow[ReturnInfo.GetFieldName(formId).StockId] as DynamicObject;
                    stockId = dyStock[0].ToString();//仓库
                    stockNumber = dyStock["Number"].ToString();

                    DynamicObject dyUnit = entityRow[ReturnInfo.GetFieldName(formId).Unit] as DynamicObject;
                    string strUnitId = dyUnit["Number"].ToString();//单位
                    fQty = Convert.ToDecimal(entityRow[ReturnInfo.GetFieldName(formId).Qty]);  //数量

                    // 辅助属性
                    DynamicObject dyAuxprop = entityRow["AuxpropId"] as DynamicObject;

                    int iAuxpropId = Convert.ToInt32(dyAuxprop["Id"]);
                    //DynamicObject dye = extended.DataEntity;
                    DynamicObjectCollection dySubEntity = entityRow["FEntityBarcode"] as DynamicObjectCollection;

                    //当前子单据体数量汇总
                    var sumQty = dySubEntity.ToList().Sum(o => Convert.ToDecimal(o["FBARCODEQTY"]));

                    //当没仓位的时候
                    string strStockLocNumber = "";
                    string strFlexId = "";
                    if ((entityRow[ReturnInfo.GetFieldName(formId).StocklocId] as DynamicObject) != null)
                    {
                        // 查询仓库对应的仓位集维度ID
                        sqlck1 = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", stockId);

                        DataSet ckds1 = DBServiceHelper.ExecuteDataSet(Context, sqlck1);
                        DataTable ckdt1 = ckds1.Tables[0];

                        fFlexId1 = ckdt1.Rows[0][0].ToString();

                        DynamicObject dyStockLoc = (entityRow[ReturnInfo.GetFieldName(formId).StocklocId] as DynamicObject)[$"F{fFlexId1}"] as DynamicObject;
                        strStockLocNumber = dyStockLoc["Number"].ToString();//仓位(01)


                        string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", stockId);
                        DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
                        DataTable ckdt = ckds.Tables[0];

                        strFlexId = ckdt.Rows[0][0].ToString();
                    }
                    //
                    foreach (DynamicObject dySubEntityRow in dySubEntity)
                    {
                        JObject jo = new JObject();
                        barcode = dySubEntityRow["FCDBARCODE"].ToString();//条码

                        // 界面上填写的条码数量
                        rowBarcodeQty = dySubEntityRow["FBARCODEQTY"].ToString();

                        barcodeQty += Convert.ToDecimal(dySubEntityRow["FBARCODEQTY"]);
                        //对应条码的数量
                        jo.Add("FCDBarcode", barcode);//条码
                        jo.Add("FBARCODEQTY", rowBarcodeQty);//对应条码的数量
                        //barcodelist.Add(BARCODE);
                        //barcodeqtylist.Add(barcodel);
                        #region box_a表

                        string sql2 = string.Format(@"select Text13 ,
                            Text4, Text3 , Text8 , Text25 , Text15,Text6,1 as Text22,Text1,Text2 from Box_A where Barcode = '{0}'", barcode);
                        DataSet ds2 = DBServiceHelper.ExecuteDataSet(Context, sql2);
                        DataTable dt2 = ds2.Tables[0];
                        string boxQty = "";//箱数
                        string strFNo = "";//执行标准
                        string model = "";//型号
                        string spec = "";//规格
                        string axisModel = "";//轴型型号
                        string weight = "";//重量
                        string axisQty = "";//轴数
                        string productDate = "";//生产日期
                        string packageLine = ""; //包装线
                        string productName = ""; //品名
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            strFNo = dt2.Rows[i]["Text13"].ToString();
                            model = dt2.Rows[i]["Text4"].ToString();
                            spec = dt2.Rows[i]["Text3"].ToString();
                            axisModel = dt2.Rows[i]["Text8"].ToString();
                            weight = dt2.Rows[i]["Text25"].ToString();
                            axisQty = dt2.Rows[i]["Text15"].ToString();
                            productDate = dt2.Rows[i]["Text6"].ToString();
                            boxQty = dt2.Rows[i]["Text22"].ToString();
                            packageLine = dt2.Rows[i]["Text1"].ToString();
                            productName = dt2.Rows[i]["Text2"].ToString();
                        }
                        #endregion

                        jo.Add("F_aaa_ZXBZ", strFNo);
                        jo.Add("F_aaa_XH", model);
                        jo.Add("F_aaa_GG", spec);
                        jo.Add("F_aaa_ZXXH", axisModel);
                        jo.Add("F_aaa_Weight", weight);
                        jo.Add("F_aaa_AxesNum", axisQty);
                        jo.Add("F_aaa_SCDate", productDate);
                        jo.Add("F_aaa_CaseNo", boxQty);
                        jo.Add("F_AAA_BZX", packageLine);
                        jo.Add("F_AAA_PM", productName);

                        jo.Add("FAUXPROPID", iAuxpropId);//辅助属性
                        jo.Add("Date", date);
                        //jo.Add("FSUPPLIERID", STOCKLOCID);
                        jo.Add("FBillTypeID", strBillType);
                        jo.Add("BillNo", strBillNo);
                        jo.Add("BillFormID", strBillName);
                        jo.Add("StockOrgId", stcStockOrg);

                        jo.Add("Qty", fQty);//添加的数据的数量

                        jo.Add("FUnitID", strUnitId);
                        jo.Add("MaterialId", materialNumber);//物料编码
                        jo.Add("StockId", stockNumber);//仓库
                        jo.Add("StockLocId", strStockLocNumber);//仓位

                        jo.Add("FFlexid", strFlexId);

                        //仓库 仓位 物料 条码  相等的时候
                        DataTable dt1 = BarcodeInvHelper.CheckInventory(strFlexId, stockId, materialId, strStockLocNumber, barcode, Context);
                        //string sql = $@"select FLOTID,FNUMBER,FBASEQTY from T_BARCODE_INVENTORY inv 
                        //                join T_BD_BARCODEMASTER bm on inv.FBARCODE = bm.FMasterId
                        //                where inv.FMATERIALID = '{materialId}' and FNumber = '{barcode}'";
                        //DataSet dss = DBServiceHelper.ExecuteDataSet(Context, sql);
                        //DataTable dts = dss.Tables[0];
                        ////string sqls = $@" exec  w_sp_getdownloadinfosum '{fzsx}'";
                        //DataSet dsss = DBServiceHelper.ExecuteDataSet

                        //点击保存 生成条码主档
                        if (this.FormOperation.Operation == "Save"
                            && documentStatus != "C")
                        {
                            //InvokeFormOperation("Save");
                            if (dt2.Rows.Count == 0)
                            {
                                throw new Exception("在BOX_A表内不含有" + barcode);
                            }

                            //子单据体汇总数量 
                            if (sumQty != fQty)
                            {
                                throw new KDBusinessException("", $"条码{barcode}的数量与即时库存不匹配");
                                //break;
                            }
                            else
                            {
                                //当 表内仓库仓位物料条码都不想等    BOX_A表 有这个条码
                            }
                        }
                        //点击审核操作
                        if (this.FormOperation.Operation == "Audit")
                        {
                            #region 当有重复的条码插入条码主档
                            string tmzdsql = $@"select FLOTID,* from T_BD_BARCODEMASTER where FNUMBER ='{barcode}'";
                            DataSet tmzdds = DBServiceHelper.ExecuteDataSet(Context, tmzdsql);
                            DataTable tmzddt = tmzdds.Tables[0];
                            #endregion
                            if (tmzddt.Rows.Count == 0)//|| dt2.Rows.Count>0
                            {
                                //条码主档
                                jo.Add("IsAlterBill", false);
                                barcodeSaveJoList.Add(jo);
                                //WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));
                            }
                            else if (tmzddt.Rows.Count > 0)
                            {
                                //条码主档 单据体加数据
                                jo.Add("FLOTID", Convert.ToInt32(tmzddt.Rows[0]["FLOTID"]));
                                
                                jo.Add("IsAlterBill", true);
                                barcodeSaveJoList.Add(jo);
                                //WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonEntityAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));
                            }
                            if (dt1.Rows.Count > 0)
                            {
                                string fid = dt1.Rows[0]["FID"].ToString();

                                int invUpdateIdx = invUpdateJoList.FindIndex(j => j["FID"].ToString() == fid);

                                decimal newFQty = Convert.ToDecimal(dt1.Rows[0]["FBASEQTY"]);//数据库查询出原有的数量
                                decimal num = Convert.ToDecimal(rowBarcodeQty) + newFQty;//原本有的数量+添加的数量

                                if (invUpdateIdx > -1)
                                {
                                    num = Convert.ToDecimal(invUpdateJoList[invUpdateIdx]["Qty"].ToString()) + Convert.ToDecimal(rowBarcodeQty);
                                    invUpdateJoList.RemoveAt(invUpdateIdx);
                                }

                                invUpdateJoList.Add(getInvUpdateJo(fid, num));
                                jo.Add("FID", fid);
                                //bool ADDisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);
                            }
                            else
                            {
                                int invCreateIdx = invCreateJoList.FindIndex(j => j["FCDBarcode"].ToString() == barcode);

                                decimal num = Convert.ToDecimal(rowBarcodeQty);//总数量+添加的数量

                                if (invCreateIdx > -1)
                                {
                                    num = Convert.ToDecimal(invCreateJoList[invCreateIdx]["FBARCODEQTY"].ToString()) + Convert.ToDecimal(rowBarcodeQty);
                                    invCreateJoList.RemoveAt(invCreateIdx);
                                }

                                jo["FBARCODEQTY"] = num.ToString();
                                invCreateJoList.Add(jo);
                                //WebApiResultHelper result2 = new WebApiResultHelper(BarcodeInventory.JsonAdd(jo.ToString(), "administrator", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));
                            }
                        }
                        //点击反审核操作(反审核操作插件按钮)
                        else if (this.FormOperation.Operation == "UnAudit")
                        {
                            if (documentStatus == "C") 
                            { 
                                if (dt1.Rows.Count > 0)
                                {
                                    string fid = dt1.Rows[0]["FID"].ToString();
                                    decimal fbaseqty = Convert.ToDecimal(dt1.Rows[0]["FBASEQTY"]);//总数量
                                    int invUpdateIdx = invUpdateJoList.FindIndex(j => j["FID"].ToString() == fid);

                                    decimal num = fbaseqty - Convert.ToDecimal(rowBarcodeQty);//总数量-添加的数量

                                    if (invUpdateIdx > -1)
                                    {
                                        num = Convert.ToDecimal(invUpdateJoList[invUpdateIdx]["Qty"].ToString()) - Convert.ToDecimal(rowBarcodeQty);
                                        invUpdateJoList.RemoveAt(invUpdateIdx);
                                    }

                                    jo.Add("FQty", num);//减少后数量
                                    jo.Add("FID", fid);

                                    invUpdateJoList.Add(getInvUpdateJo(fid, num));
                                    //去除掉这张表添加的物料的数量  
                                    //bool ReduceisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);
                                     
                                }
                                else
                                {
                                    int invCreateIdx = invCreateJoList.FindIndex(j => j["FCDBarcode"].ToString() == barcode);

                                    decimal num = Convert.ToDecimal(rowBarcodeQty);//总数量+添加的数量

                                    if (invCreateIdx > -1)
                                    {
                                        num = Convert.ToDecimal(invCreateJoList[invCreateIdx]["FBARCODEQTY"].ToString()) - Convert.ToDecimal(rowBarcodeQty);
                                        invCreateJoList.RemoveAt(invCreateIdx);
                                    }

                                    jo["FBARCODEQTY"] = num.ToString();
                                    invCreateJoList.Add(jo);
                                }
                            }
                        }
                    }
                }


                if (barcodeSaveJoList.Count > 0)
                {
                    BarcodeMasterFile.BatchSave(barcodeSaveJoList, Context.DBId, new K3CloudWebApiInfo());
                }

                if (invCreateJoList.Count > 0)
                {
                    BarcodeInventory.BatchSave(invCreateJoList, Context.DBId, new K3CloudWebApiInfo());
                }

                if (invUpdateJoList.Count > 0)
                {
                    BarcodeInventory.JsonBatchEdit(Context.DBId, new K3CloudWebApiInfo(), invUpdateJoList);
                }

                if (this.FormOperation.Operation == "UnAudit")
                {
                    string delSql = $@"delete from  T_BD_BARCODEMASTERTRACE   where  FBILLNO = '{strBillNo}'";
                    DBUtils.Execute(this.Context, delSql);
                }
            }

            DateTime stop = DateTime.Now;

            TimeSpan time = stop.Subtract(start);
            //this.OperationResult.OperateResult.Add(new Kingdee.BOS.Core.DynamicForm.OperateResult()
            //{
            //    Message = time.TotalSeconds.ToString()
            //});
        }

        private JObject getInvUpdateJo(string fid, decimal qty)
        {
            JObject jo = new JObject();

            jo.Add("FID", fid);
            jo.Add("Qty", qty);

            return jo;
        }
    }
}
