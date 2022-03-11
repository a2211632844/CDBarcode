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
    [Description("出库单插件")]
    [HotUpdate]
    public class OutStockBarocde : AbstractOperationServicePlugIn
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
            //e.FieldKeys.Add("OutQty");
            e.FieldKeys.Add("ActualQty");
            e.FieldKeys.Add("FBillEntry");
            e.FieldKeys.Add("FDate");
            e.FieldKeys.Add("FUnitID");
            e.FieldKeys.Add("FBillType");
            e.FieldKeys.Add("FCDBARCODE");
            e.FieldKeys.Add("FBARCODEQTY");
            e.FieldKeys.Add("FBillTypeID");
            e.FieldKeys.Add("DocumentStatus");

            //e.FieldKeys.Add("FUnitID");
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

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //this.OperationResult.IsSuccess
        }
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            string formId = this.BusinessInfo.GetForm().Id; ;
            if (e.SelectedRows.Count() < 1)
            {
                return;
            }
            decimal fQty = 0;
            string strStockId = "";
            string strStockLocId = "";
            string barcode = "";
            decimal barcodeQty = 0;
            string sqlck1 = "";
            string strFlexId = "";
            string rowBarcodeQty = "";
            string materialNumber = "";

            string STOCKLOCID = "";
            string date = "";
            string strBillTypeNumber = "";
            string strBillNo = "";
            string strBillFormId = "";
            string stockOrg = "";
            string strStockNumber = "";
            string documentStatus = "";
            //foreach (ExtendedDataEntity item in e.SelectedRows)
            //{
                

            //}
            foreach (ExtendedDataEntity extended in e.SelectedRows)
            {
                List<JObject> barcodeAlterJoList = new List<JObject>();
                List<JObject> invAlterJoList = new List<JObject>();
                List<JObject> invCreateJoList = new List<JObject>();
                string materialId = "";

                //DynamicObject stocklocid = item["PrdOrgId"] as DynamicObject;
                //STOCKLOCID = stocklocid["Number"].ToString();//组织
                date = extended[ReturnInfo.GetFieldName(formId).Date].ToString();  //日期

                documentStatus = extended["DocumentStatus"].ToString();

                

                if (extended[ReturnInfo.GetFieldName(formId).Billtype] != null)
                {
                    DynamicObject dyBillType = extended[ReturnInfo.GetFieldName(formId).Billtype] as DynamicObject;
                    strBillTypeNumber = dyBillType["Number"].ToString();//单据类型 scrko
                }
                
                //if (item["BillNo"]!=null) 
                //{
                //DynamicObject DJBH = item["BillNo"] as DynamicObject;
                //djbh = item["BillNo"].ToString();//单据编号
                //}
                //（单据编号）
                if (extended["BillNo"] != null)
                {
                    strBillNo = extended["BillNo"].ToString();//单据编号
                }
                else
                {
                    strBillNo = "";
                }


                if (extended["FFormId"] != null)
                {
                    strBillFormId = extended["FFormId"].ToString(); //单据业务对象内码
                }
                // 获取组织
                if (extended[ReturnInfo.GetFieldName(formId).ORG] != null)
                {
                    DynamicObject dyStockOrg = extended[ReturnInfo.GetFieldName(formId).ORG] as DynamicObject;
                    stockOrg = dyStockOrg["Number"].ToString();
                }


                DynamicObject dyBill = extended.DataEntity;
                //DynamicObjectCollection docEntity = dy["BillEntry"] as DynamicObjectCollection;
                DynamicObjectCollection dyEntity = dyBill[ReturnInfo.GetFieldName(formId).Entity] as DynamicObjectCollection;

                foreach (DynamicObject dyEntityRow in dyEntity)
                {
                    DynamicObject dyMaterial = dyEntityRow["MaterialId"] as DynamicObject;

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

                    DynamicObject dyStock = dyEntityRow[ReturnInfo.GetFieldName(formId).StockId] as DynamicObject;
                    strStockId = dyStock[0].ToString();//仓库
                    strStockNumber = dyStock["Number"].ToString();
                    DynamicObject dyUnit = dyEntityRow[ReturnInfo.GetFieldName(formId).Unit] as DynamicObject;
                    string strUnitNumber = dyUnit["Number"].ToString();//单位
                    fQty = Convert.ToDecimal(dyEntityRow[ReturnInfo.GetFieldName(formId).Qty]);  //数量

                    DynamicObject dyAuxprop = dyEntityRow["AuxpropId"] as DynamicObject;

                    int iAuxpropId = Convert.ToInt32(dyAuxprop["Id"]);

                    //DynamicObject dye = extended.DataEntity;
                    DynamicObjectCollection dySubEntity = dyEntityRow["FEntityBarcode"] as DynamicObjectCollection;

                    var sumQty = dySubEntity.ToList().Sum(o => Convert.ToDecimal(o["FBARCODEQTY"]));//当前子单据体数量汇总
                                                                                                  //当没仓位的时候
                    string stockLocNumber = "";
                    string strFlexid = "";
                    if ((dyEntityRow[ReturnInfo.GetFieldName(formId).StocklocId] as DynamicObject) != null) 
                    {
                        sqlck1 = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", strStockId);
                        DataSet ckds1 = DBServiceHelper.ExecuteDataSet(Context, sqlck1);
                        DataTable ckdt1 = ckds1.Tables[0];

                        strFlexId = ckdt1.Rows[0][0].ToString();

                        DynamicObject dyStockLoc = (dyEntityRow[ReturnInfo.GetFieldName(formId).StocklocId] as DynamicObject)[$"F{strFlexId}"] as DynamicObject;
                        stockLocNumber = dyStockLoc["Number"].ToString();//仓位(01)
                        string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", strStockId);
                        DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
                        DataTable ckdt = ckds.Tables[0];
                        strFlexid = ckdt.Rows[0][0].ToString();
                    }
                    foreach (DynamicObject entityenrty in dySubEntity)
                    {
                        JObject jo = new JObject();
                        barcode = entityenrty["FCDBARCODE"].ToString();//条码
                        rowBarcodeQty = entityenrty["FBARCODEQTY"].ToString();//对应数量
                        barcodeQty += Convert.ToDecimal(entityenrty["FBARCODEQTY"]);
                        //对应条码的数量
                        jo.Add("FCDBarcode", barcode);//条码
                        jo.Add("FBARCODEQTY", rowBarcodeQty);//对应条码的数量

                        //barcodelist.Add(BARCODE);
                        //barcodeqtylist.Add(barcodel);
                        #region box_a表

                        string sql2 = string.Format(@"select Text13 ,
                        Text4, Text3 , Text8 , Text25 , Text15,Text6, 1 as Text22,Text1,Text2 from Box_A where Barcode = '{0}'", barcode);
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
                        jo.Add("FSUPPLIERID", STOCKLOCID);
                        jo.Add("FBillTypeID", strBillTypeNumber);
                        jo.Add("BillNo", strBillNo);
                        jo.Add("BillFormID", strBillFormId);
                        jo.Add("StockOrgId", stockOrg);

                        jo.Add("Qty", fQty);//添加的数据的数量

                        jo.Add("FUnitID", strUnitNumber);
                        jo.Add("MaterialId", materialNumber);//物料编码
                        jo.Add("StockId", strStockNumber);//仓库
                        jo.Add("StockLocId", stockLocNumber);//仓位


                        jo.Add("FFlexid", strFlexid);
                        //仓库 仓位 物料 条码  相等的时候
                        DataTable dtInvResult = BarcodeInvHelper.CheckInventory(strFlexid, strStockId, materialId, stockLocNumber, barcode, Context);
                        string sql = $@"select FNUMBER from T_BD_BARCODEMASTER
                                        where FNumber = '{barcode}'";//查询这个物料这个条码的数量 和条码是否存在
                        DataSet dss = DBServiceHelper.ExecuteDataSet(Context, sql);
                        DataTable dtIskBarcodeExists = dss.Tables[0];
                        //点击保存 生成条码主档
                        if (this.FormOperation.Operation == "Save"
                            && documentStatus != "C")
                        {
                            if (sumQty != fQty)
                            {
                                throw new KDBusinessException("", $"条码{barcode}的数量与即时库存不匹配");
                            }
                            else
                            {
                                if (dtInvResult.Rows.Count == 0)
                                {
                                    throw new KDBusinessException("", $"条码 {barcode} 库存不足，无法出库");
                                    //条码主档
                                    // WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));
                                }
                                if (dtIskBarcodeExists.Rows.Count > 0)
                                {
                                    //if(Convert.ToDecimal(dts.Rows[0]["FBASEQTY"])< sumQty) 
                                    //{
                                    //    throw new KDBusinessException("", "出库数量大于库存数量，出现负库存");
                                    //}
                                }
                                else
                                {
                                    // 条码主档不存在
                                    throw new KDBusinessException("", $"条码 {barcode} 不存在，无法出库");
                                }
                            }
                        }
                        DataTable indt;
                        //点击审核操作 出库单审核扣库存
                        if (this.FormOperation.Operation == "Audit")
                        {
                            #region 当有重复的条码插入条码主档
                            string barcodeMasterSql = $@"select FLOTID,* from T_BD_BARCODEMASTER where FNUMBER ='{barcode}'";
                            DataSet barcodeMasterDs = DBServiceHelper.ExecuteDataSet(Context, barcodeMasterSql);
                            DataTable barcodeMasterDt = barcodeMasterDs.Tables[0];
                            int lotId = Convert.ToInt32(barcodeMasterDt.Rows[0]["FLOTID"]);
                            decimal num;
                            #endregion
                            if (barcodeMasterDt.Rows.Count > 0)
                            {
                                decimal newFQty = Convert.ToDecimal(dtInvResult.Rows[0]["FBASEQTY"]);//数据库查询出原有的数量
                                if (Convert.ToDecimal(rowBarcodeQty) > newFQty) //总数量>数据库数量     21.87
                                {
                                    throw new KDBusinessException("", $"条码 {barcode} 的数量大于库存数量");
                                }
                                //条码主档 单据体加数据
                                jo.Add("FLOTID", lotId);
                                jo.Add("IsAlterBill", true);
                                barcodeAlterJoList.Add(jo);
                                //WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonEntityAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));
                            }
                            if (dtInvResult.Rows.Count == 0)//|| dt2.Rows.Count>0
                            {
                                //条码主档
                                //WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));
                            }
                            if (dtInvResult.Rows.Count > 0)//条码主档含有这个条码
                            {
                                string fid = dtInvResult.Rows[0]["FID"].ToString();
                                decimal newFQty = Convert.ToDecimal(dtInvResult.Rows[0]["FBASEQTY"]);//数据库查询出原有的数量
                                if (Convert.ToDecimal(rowBarcodeQty) > newFQty) //总数量>数据库数量
                                {
                                    //throw new KDBusinessException("", "条码数量大于库存数量");
                                }
                                else
                                {
                                    int invUpdateIdx = invAlterJoList.FindIndex(j => j["FID"].ToString() == fid);

                                    num = Convert.ToDecimal(newFQty) - Convert.ToDecimal(rowBarcodeQty);

                                    if (invUpdateIdx > -1)
                                    {
                                        num = Convert.ToDecimal(invAlterJoList[invUpdateIdx]["Qty"].ToString()) - Convert.ToDecimal(rowBarcodeQty);
                                        invAlterJoList.RemoveAt(invUpdateIdx);
                                    }

                                    invAlterJoList.Add(getInvUpdateJo(fid, num));

                                    jo.Add("FID", fid);
                                    //bool ADDisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);
                                }
                            }
                            else
                            {
                                int invCreateIdx = invCreateJoList.FindIndex(j => j["FCDBarcode"].ToString() == barcode);

                                num = Convert.ToDecimal(rowBarcodeQty);//总数量+添加的数量

                                if (invCreateIdx > -1)
                                {
                                    num = Convert.ToDecimal(invCreateJoList[invCreateIdx]["FBARCODEQTY"].ToString()) - Convert.ToDecimal(rowBarcodeQty);
                                    invCreateJoList.RemoveAt(invCreateIdx);
                                }

                                jo["FBARCODEQTY"] = num.ToString();
                                invCreateJoList.Add(jo);

                                //(即时库存)
                                //WebApiResultHelper result2 = new WebApiResultHelper(BarcodeInventory.JsonAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));//, barcodelist, barcodeqtylist
                            }
                            if (strBillFormId == "STK_TransferDirect") //直接调拨单
                            {
                                string inStockId = "";
                                string inFFlexid = "";
                                string inStockNumber = "";
                                string inFSTOCKLOCIDId = "";
                                DynamicObject dyInStockId = dyEntityRow[ReturnInfo.GetFieldName(formId).inStockId] as DynamicObject;
                                inStockId = dyInStockId[0].ToString();//仓库
                                inStockNumber = dyInStockId["Number"].ToString();
                                if ((dyEntityRow[ReturnInfo.GetFieldName(formId).InStockLocId] as DynamicObject) != null)
                                {
                                    sqlck1 = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", inStockId);
                                    DataSet inckds = DBServiceHelper.ExecuteDataSet(Context, sqlck1);
                                    DataTable inckdt = inckds.Tables[0];
                                    inFFlexid = inckdt.Rows[0][0].ToString();
                                    DynamicObject infstocklocId = (dyEntityRow[ReturnInfo.GetFieldName(formId).InStockLocId] as DynamicObject)[$"F{inFFlexid}"] as DynamicObject;
                                    inFSTOCKLOCIDId = infstocklocId["Number"].ToString();//仓位(01)
                                }
                                indt = BarcodeInvHelper.CheckInventory(inFFlexid, inStockId, materialId,inFSTOCKLOCIDId, barcode, Context);
                                //throw new Exception(indt.Rows.Count.ToString());
                                if (indt.Rows.Count == 0)
                                {
                                    jo.Add("inStockId", inStockNumber);
                                    jo.Add("inStockLocId", inFSTOCKLOCIDId);
                                    jo.Add("inFFlexid", inFFlexid);
                                    //WebApiResultHelper result2 = new WebApiResultHelper(BarcodeInventory.inJsonAdd(jo.ToString(), "admin", "888888", Context.DBId, "http://192.168.6.6/k3cloud/"));

                                    invCreateJoList.Add(jo);
                                }
                                else if (indt.Rows.Count > 0)
                                {
                                    //indt = BarcodeInvHelper.DT(inFFlexid, inStockId, materialid, inFSTOCKLOCIDId, BARCODE, Context);
                                    string fid = indt.Rows[0]["FID"].ToString();
                                    decimal Fqty = Convert.ToDecimal(indt.Rows[0]["FBASEQTY"]);//修改前数量
                                    num = Fqty + Convert.ToDecimal(rowBarcodeQty);
                                    jo.Add("infid", fid);
                                    //bool inADDisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);

                                    invAlterJoList.Add(getInvUpdateJo(fid, num));
                                }
                            }

                        }
                        //点击反审核操作(反审核操作插件按钮) 出库单反审核加库存
                        else if (this.FormOperation.Operation == "UnAudit")
                        {
                            if (documentStatus == "C")
                            {
                                string fid = "";
                                decimal num;

                                // 数量为0的条码即时库存有可能会被删除
                                if (dtInvResult.Rows.Count > 0)
                                {
                                    fid = dtInvResult.Rows[0]["FID"].ToString();
                                    string fbaseqty = dtInvResult.Rows[0]["FBASEQTY"].ToString();//总数量

                                    int invUpdateIdx = invAlterJoList.FindIndex(j => j["FID"].ToString() == fid);

                                    num = decimal.Parse(fbaseqty) + Convert.ToDecimal(rowBarcodeQty);//总数量+添加的数量

                                    if (invUpdateIdx > -1)
                                    {
                                        num = Convert.ToDecimal(invAlterJoList[invUpdateIdx]["Qty"].ToString()) + Convert.ToDecimal(rowBarcodeQty);
                                        invAlterJoList.RemoveAt(invUpdateIdx);
                                    }

                                    jo.Add("FQty", num);//减少后数量
                                    jo.Add("FID", fid);
                                    //去除掉这张表添加的物料的数量  
                                    //bool ReduceisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);

                                    invAlterJoList.Add(getInvUpdateJo(fid, num));
                                }
                                else
                                {
                                    int invCreateIdx = invCreateJoList.FindIndex(j => j["FCDBarcode"].ToString() == barcode);

                                    num = Convert.ToDecimal(rowBarcodeQty);//总数量+添加的数量

                                    if (invCreateIdx > -1)
                                    {
                                        num = Convert.ToDecimal(invCreateJoList[invCreateIdx]["FBARCODEQTY"].ToString()) + Convert.ToDecimal(rowBarcodeQty);
                                        invCreateJoList.RemoveAt(invCreateIdx);
                                    }
                                    jo["FBARCODEQTY"] = num.ToString();
                                    invCreateJoList.Add(jo);
                                }

                                // 直接调拨单的处理
                                if (strBillFormId == "STK_TransferDirect")
                                {
                                    string inStockId = "";
                                    string inFFlexid = "";
                                    string inFSTOCKLOCIDId = "";
                                    DynamicObject dyInStockId = dyEntityRow[ReturnInfo.GetFieldName(formId).inStockId] as DynamicObject;
                                    inStockId = dyInStockId[0].ToString();//仓库//Stockld = dyInStockId["Number"].ToString();
                                    if ((dyEntityRow[ReturnInfo.GetFieldName(formId).StocklocId] as DynamicObject) != null)
                                    {
                                        sqlck1 = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", inStockId);

                                        DataSet inckds = DBServiceHelper.ExecuteDataSet(Context, sqlck1);
                                        DataTable inckdt = inckds.Tables[0];
                                        if (inckdt.Rows.Count > 0)
                                        {
                                            inFFlexid = inckdt.Rows[0][0].ToString();
                                            DynamicObject infstocklocId = (dyEntityRow[ReturnInfo.GetFieldName(formId).InStockLocId] as DynamicObject)[$"F{inFFlexid}"] as DynamicObject;
                                            inFSTOCKLOCIDId = infstocklocId["Number"].ToString();//仓位(01)
                                        }
                                    }
                                    indt = BarcodeInvHelper.CheckInventory(inFFlexid, inStockId, materialId, inFSTOCKLOCIDId, barcode, Context);

                                    // 如果0条码即时库存未被清除
                                    if (indt.Rows.Count > 0)
                                    {
                                        fid = indt.Rows[0]["FID"].ToString();
                                        decimal Fqty = Convert.ToDecimal(indt.Rows[0]["FBASEQTY"]);//修改前数量
                                        num = Fqty - Convert.ToDecimal(rowBarcodeQty);
                                        jo.Add("infid", fid);
                                        //jo.Add("innum", innum);
                                        invAlterJoList.Add(getInvUpdateJo(fid, num));
                                        //bool inADDisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);
                                    }
                                    else
                                    {
                                        jo.Add("inStockLocId", inFSTOCKLOCIDId);
                                        jo.Add("inStockId", inStockId);
                                        jo.Add("inFFlexid", inFFlexid);

                                        invCreateJoList.Add(jo);
                                    }
                                }
                            }
                        }
                    }
                }


                if (barcodeAlterJoList.Count > 0)
                {
                    BarcodeMasterFile.BatchSave(barcodeAlterJoList, Context.DBId, new K3CloudWebApiInfo());
                }

                if (invCreateJoList.Count > 0)
                {
                    BarcodeInventory.BatchSave(invCreateJoList, Context.DBId, new K3CloudWebApiInfo());
                }

                if (invAlterJoList.Count > 0)
                {
                    BarcodeInventory.JsonBatchEdit(Context.DBId, new K3CloudWebApiInfo(), invAlterJoList);
                }

                if (this.FormOperation.Operation == "UnAudit")
                {
                    string delsql = $@"delete  from T_BD_BARCODEMASTERTRACE where fbillno = '{strBillNo}'";
                    DBUtils.Execute(Context, delsql);
                }
            }
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
