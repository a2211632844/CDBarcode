using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.WebApi.Client;
using CDBarcodeManager.Helper.KingdeeWebApi;
using CDBarcodeManager.Helper;
using Kingdee.BOS.Core.List.PlugIn.Args;
using System.Data.Odbc;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.Core.Bill.PlugIn;
using System.Security.Cryptography;
using Kingdee.BOS.Core.Metadata.QueryElement;
using Kingdee.BOS.Core.Metadata.EntityElement;
using CDBarcodeManager.Common;

namespace CDBarcodeManager.InStock
{
    /// <summary>
    /// 热更新
    /// </summary>
    [HotUpdate]
    public class InStockBill : AbstractBillPlugIn
    {
        /// <summary>
        /// 点击审批按钮 
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            //点击审核按钮
            //获取到仓库信息
            int idx = this.Model.GetEntryCurrentRowIndex("FEntity");//获取当前选中行
            int index = this.Model.GetEntryRowCount("FEntity");//获取总行数
            for (int i = 0; i < index; i++)
            {
                DynamicObject fstockid = this.Model.GetValue("FStockID", i) as DynamicObject;
                string FSTOCKID = fstockid["Number"].ToString();//01
                string FStockId = fstockid[0].ToString();//仓库
                string sqlck = string.Format("select FFLEXID from T_BD_STOCKFLEXITEM where FSTOCKID='{0}'", FStockId);
                DataSet ckds = DBServiceHelper.ExecuteDataSet(Context, sqlck);
                DataTable ckdt = ckds.Tables[0];
                string FFlexid = ckdt.Rows[0][0].ToString();
                DynamicObject fstocklocId = (this.Model.GetValue("FStockLocId", i) as DynamicObject)[$"F{FFlexid}"] as DynamicObject;
                string FSTOCKLOCIDId = fstocklocId["Number"].ToString();//仓位(01)
                string fstocklocid = fstocklocId["ID"].ToString();//仓位(100001)
                DynamicObject FUnitID = this.Model.GetValue("FUnitID", i) as DynamicObject;
                string FUNITID = FUnitID["Number"].ToString();//单位

                DynamicObject fmaterialId = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                string FMATERIALID = fmaterialId["Number"].ToString();//物料(1005)
                string FMaterialId = fmaterialId[0].ToString();//物料（108716）
                string GGXH = fmaterialId["Specification"].ToString();//规格型号
                string FSUPPLIERID = "";
                if (this.Model.GetValue("FSUPPLIERID")!=null) 
                {
                    DynamicObject FSUPPLIERId = this.Model.GetValue("FSUPPLIERID") as DynamicObject;
                    FSUPPLIERID = FSUPPLIERId["Number"].ToString();//供应商
                }
                string Fid = "";
                if (this.View.Model.GetPKValue()!=null) 
                {
                    Fid = this.View.Model.GetPKValue().ToString();//单据编号
                }
                
                Decimal FQty = Convert.ToDecimal(this.Model.GetValue("FQTY", i));
                string FQTY = FQty.ToString();//数量
                DynamicObject FStockOrgId = this.Model.GetValue("FStockOrgId") as DynamicObject;
                string FSTOCKORGID = FStockOrgId["Number"].ToString();//库存组织
                string FData = this.Model.GetValue("FDATE").ToString();
                string FCDBarcode = "";
                decimal FCDBarcodeQty = 0;
                this.View.SetEntityFocusRow("FEntity", i);//聚焦事件
                int indexE = this.Model.GetEntryRowCount("FEntityBarcode");//子单据体有多少行
                for (int j = 0; j < indexE; j++)
                {
                    FCDBarcode = this.View.Model.GetValue("FCDBARCODE", j).ToString();

                    FCDBarcodeQty += Convert.ToDecimal(this.Model.GetValue("FBARCODEQTY", j));
                    if (FQty >= FCDBarcodeQty)
                    {
                        DataTable dt = BarcodeInvHelper.CheckInventory(FFlexid, FStockId, FMaterialId, FSTOCKLOCIDId, FCDBarcode, Context);
                        bool isSuccess = BarcodeInvHelper.CheckBarcode(FFlexid, FStockId, FMaterialId, FSTOCKLOCIDId, FCDBarcode, Context);
                        //X += FCDBarcodeQty;
                        //判断与 仓库 仓位 物料编码 条码 相匹配 是否有数据
                        if (e.BarItemKey == "tbSplitApprove") //tbApprove
                        {
                            JObject jo = new JObject();
                            jo.Add("FFlexid", FFlexid);
                            jo.Add("FMaterialId", FMATERIALID);//物料编码
                            jo.Add("FStockId", FSTOCKID);//仓库
                            jo.Add("FStockLocId", FSTOCKLOCIDId);//仓位
                            jo.Add("FCDBarcode", FCDBarcode);//条码
                            jo.Add("FSUPPLIERID", FSUPPLIERID);//供应商 
                            var aaa = this.Model.BillBusinessInfo.GetForm();
                            string Fbillno = aaa.Id;
                            jo.Add("FBillTypeID", Fbillno);//单据名称
                            jo.Add("FBillNo", Fid);//单据编号
                            jo.Add("FUnitID", FUNITID);//单位
                            jo.Add("FQty", FQTY);//添加的数据的数量
                            jo.Add("FStockOrgId", FSTOCKORGID);
                            jo.Add("FData", FData);
                            jo.Add("FGGXH", GGXH);


                            //int count = BarcodeInvHelper.CheckBarcode(FFlexid, FStockId, FMaterialId, FSTOCKLOCIDId, FCDBarcode, Context);
                            //判断该物料 条码即时库存 表中是否含有
                            if (isSuccess)
                            {
                                //含有 则添加该物料的数量
                                //StringBuilder sb = new StringBuilder();`
                                string fid = dt.Rows[0]["FID"].ToString();
                                string newFQty = dt.Rows[0]["FBASEQTY"].ToString();//数据库查询出原有的数量
                                decimal num = decimal.Parse(FQTY) + decimal.Parse(newFQty);//原本有的数量+添加的数量
                                                                                           //jo.Add("FQty", num);//数量
                                jo.Add("FID", fid);

                                bool ADDisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);
                                if (ADDisSuccess)
                                {
                                    this.View.ShowMessage("添加成功");
                                }
                                else
                                {
                                    this.View.ShowErrMessage("添加失败");
                                }
                            }
                            else
                            {
                                //没有  则添加一条数据
                                // 调用添加方法 (条码主档)
                                WebApiResultHelper result = new WebApiResultHelper(BarcodeMasterFile.JsonAdd(jo.ToString(), Context.DBId, new K3CloudWebApiInfo()));

                                //(即时库存)
                                //WebApiResultHelper result2 = new WebApiResultHelper(BarcodeInventory.JsonAdd(jo.ToString(), "admin", "666666", Context.DBId, "http://192.168.1.189:88/k3cloud/"));
                            }
                        }
                        //点击反审核按钮   
                        if (e.BarItemKey == "tbReject")
                        {
                            JObject jo = new JObject();
                            string fid = dt.Rows[0]["FID"].ToString();
                            string fbaseqty = dt.Rows[0]["FBASEQTY"].ToString();//总数量
                            FQty = Convert.ToDecimal(this.Model.GetValue("FQTY", i));
                            FQTY = FQty.ToString();//当前表单的数量数量

                            decimal num = decimal.Parse(fbaseqty) - decimal.Parse(FQTY);//原本有的数量-添加的数量
                            jo.Add("FQty", num);//数量
                            jo.Add("FID", fid);
                            //去除掉这张表添加的物料的数量  
                            bool ReduceisSuccess = BarcodeInvHelper.UpdateBarcodeInv(fid, num, Context);
                            if (ReduceisSuccess)
                            {
                                this.View.ShowMessage("反审核成功,修改成功");
                            }
                            else
                            {
                                this.View.ShowErrMessage("反审核失败,修改失败");
                            }
                        }
                    }
                    else 
                    {
                        this.View.ShowErrMessage("条码数量不能超过实际数量");
                    }
                }
                this.View.ShowMessage(FCDBarcodeQty.ToString());
            }
        }
    }
}

