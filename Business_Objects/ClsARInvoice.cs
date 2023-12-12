using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EInvoice.Common;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net.Http;
using System.Diagnostics;
using EInvoice.Models;
using static EInvoice.Common.clsGlobalMethods;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SAPbobsCOM;

namespace EInvoice.Business_Objects
{
    class ClsARInvoice
    {

        private string strSQL;
        private SAPbobsCOM.Recordset objRs;
        private bool blnRefresh;
        private bool blnprint;
        SAPbouiCOM.Button button;
        private SAPbouiCOM.Form objTempForm;


        #region ITEM EVENT
        public void Item_Event(string oFormUID, ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            try
            {
                //if (pVal.InnerEvent) return;
                SAPbouiCOM.Form oForm = clsModule.objaddon.objapplication.Forms.Item(oFormUID);               
                ClsARInvoice.EinvoiceMethod einvoiceMethod = ClsARInvoice.EinvoiceMethod.Default;
                string DocEntry = "";
                string TransType = "";
                string Type = "";
                SAPbouiCOM.Button button = null;
               
                if (pVal.BeforeAction)
                {
                    switch (pVal.EventType)
                    {                       
                        case SAPbouiCOM.BoEventTypes.et_FORM_LOAD:                         
                            Create_Customize_Fields(oForm);
                            break;                      

                    }
                }
                else
                {
                                        
                    switch (pVal.EventType)
                    {
                        case SAPbouiCOM.BoEventTypes.et_FORM_LOAD:                            
                            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                                clsModule.objaddon.Cleartext(oForm);                            
                            break;                      
                        case SAPbouiCOM.BoEventTypes.et_GOT_FOCUS:
                            if(clsModule.objaddon.objglobalmethods.isupdate)
                            {                                
                                clsModule.objaddon.objglobalmethods.isupdate = false;
                                buttonenable(oForm);
                                

                            }
                            break;

                        case SAPbouiCOM.BoEventTypes.et_FORM_CLOSE:
                            {
                                if (pVal.FormTypeEx != "425") return;
                                if (objTempForm!=null)
                                {
                                    clsModule.objaddon.Cleartext(objTempForm);
                                    objTempForm = null;
                                }
                                
                                break;
                            }

                        case SAPbouiCOM.BoEventTypes.et_FORM_DRAW:                            
                            if (pVal.FormTypeEx == "179")
                            {
                                objTempForm = clsModule.objaddon.objapplication.Forms.GetForm(pVal.FormTypeEx, pVal.FormTypeCount);
                            }                           
                            break;

                        case SAPbouiCOM.BoEventTypes.et_CLICK:
                            if (pVal.ItemUID == "einv")
                            {

                                oForm.PaneLevel = 26;
                            }
                            switch (pVal.FormType)
                            {
                                case 133:                                                                       
                                    if (pVal.ItemUID == "btneinv" && (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE | oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE))
                                    {
                                        DocEntry = oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocEntry", 0);
                                        TransType = "INV";
                                        button = (SAPbouiCOM.Button)oForm.Items.Item("btneinv").Specific;
                                        if (button.Item.Enabled)
                                        {
                                            einvoiceMethod = ClsARInvoice.EinvoiceMethod.CreateIRN;
                                            Type = "E-Invoice";
                                        }
                                    }
                                    break;
                                case 179:                                  
                                   
                                    if (pVal.ItemUID == "btneinv" && (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE | oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE))
                                    {
                                        DocEntry = oForm.DataSources.DBDataSources.Item("ORIN").GetValue("DocEntry", 0);
                                        TransType = "CRN";
                                        button = (SAPbouiCOM.Button)oForm.Items.Item("btneinv").Specific;
                                        if (button.Item.Enabled)
                                        {
                                            einvoiceMethod = ClsARInvoice.EinvoiceMethod.CreateIRN;
                                            Type = "E-Invoice";
                                        }
                                    }
                                    break;

                            }
                            bool docrefresh = false;
                            if (DocEntry != "" && TransType != "" && Type != "")
                            {
                                DataTable dt = new DataTable();
                                Generate_Cancel_IRN(einvoiceMethod, DocEntry, TransType, Type, ref dt, false);
                                button.Caption = "Generate E-invoice";
                                
                                if (dt.Rows.Count > 0)
                                {
                                    if (blnRefresh)
                                    {
                                        docrefresh = true;
                                    }
                                }
                                if (blnprint)
                                {
                                    docrefresh = true;
                                }
                                if (docrefresh)
                                {
                                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                                    clsModule.objaddon.objapplication.Menus.Item("1304").Activate();
                                    clsModule.objaddon.objapplication.StatusBar.SetText("Operation completed successfully[Message 200 - 48]", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                }
                            }
                            
                            

                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                return;
            }
            finally
            {

            }
        }
        #endregion

        public void EnabledMenu( SAPbouiCOM.Form oForm, bool Penable = false, string UDFormID = "")
        {
            try
            {


                //   Penable = true;
                oForm.Freeze(true);
                switch (oForm.TypeEx)
                {
                    case "133":
                    case "179":
                        oForm.Items.Item("txtPIH").Enabled = Penable;
                        oForm.Items.Item("txtUUID").Enabled = Penable;
                        oForm.Items.Item("txtInvHash").Enabled = Penable;
                        oForm.Items.Item("txtICV").Enabled = Penable;
                        oForm.Items.Item("txtEinvSt").Enabled = Penable;
                        oForm.Items.Item("txtWarn").Enabled = Penable;
                        oForm.Items.Item("txtError").Enabled = Penable;
                        oForm.Items.Item("txtIssueDt").Enabled = Penable;

                        SAPbouiCOM.Form oUDFForm;

                        if (!string.IsNullOrEmpty(oForm.UDFFormUID))
                        {
                            oUDFForm = clsModule.objaddon.objapplication.Forms.Item(oForm.UDFFormUID);
                            oUDFForm.Items.Item("U_PIHNo").Enabled = Penable;
                            oUDFForm.Items.Item("U_UUIDNo").Enabled = Penable;
                            oUDFForm.Items.Item("U_InvoiceHashNo").Enabled = Penable;
                            oUDFForm.Items.Item("U_ICVNo").Enabled = Penable;
                            oUDFForm.Items.Item("U_EinvStatus").Enabled = Penable;
                            oUDFForm.Items.Item("U_Warn").Enabled = Penable;
                            oUDFForm.Items.Item("U_Error").Enabled = Penable;
                            oUDFForm.Items.Item("U_Issuedt").Enabled = Penable;
                        }

                        break;
                }
            }
            catch (Exception)
            {
                return;

            }
            finally
            {
                oForm.Freeze(false);
            }

        }
        #region FORM DATA EVENT
        public void FormData_Event(ref SAPbouiCOM.BusinessObjectInfo BusinessObjectInfo, ref bool BubbleEvent)
        {
            try
            {

                if (BusinessObjectInfo.BeforeAction)
                {
                    switch (BusinessObjectInfo.EventType)
                    {
                        case SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE:
                            break;
                        case SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD:
                            break;
                        case SAPbouiCOM.BoEventTypes.et_FORM_LOAD:
                            break;
                    }
                }
                else
                {
                    SAPbouiCOM.Form activefrm = clsModule.objaddon.objapplication.Forms.Item(BusinessObjectInfo.FormUID);
                    switch (BusinessObjectInfo.EventType)
                    {
                        case SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD:
                            if (BusinessObjectInfo.ActionSuccess)
                            {  
                                
                                buttonenable(activefrm);
                            }
                            break;
                        case SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE:
                            if (BusinessObjectInfo.ActionSuccess)
                            {
                                switch (activefrm.Type.ToString())
                                {
                                    case "133":                                                                               
                                    case "179":
                                        clsModule.objaddon.objglobalmethods.isupdate = true;
                                        break;
                                    default:
                                        return;
                                }                              
                            }
                            break;
                   

                    }
                }
            }
            catch (Exception Ex)
            {
                clsModule.objaddon.objapplication.StatusBar.SetText(Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                BubbleEvent = false;
                return;
            }
            finally
            {
                // oForm.Freeze(false);
            }
        }
        #endregion

        public string GetInvoiceData(string DocEntry, string TransType)
        {
            DataTable dt = clsModule.objaddon.objglobalmethods.GetmultipleValue(@"SELECT ""U_SerConfig"" FROM ""@EICON""");

            Querycls qcls = new Querycls();
            if (dt.Rows.Count > 0)
            {

                if (!String.IsNullOrEmpty(Convert.ToString(dt.Rows[0]["U_SERCONFIG"])))
                {
                    qcls.docseries = Convert.ToString(dt.Rows[0]["U_SERCONFIG"]);
                }
            }

            switch (TransType)
            {
                case "INV":
                    strSQL = qcls.InvoiceQuery(DocEntry);
                    break;
                case "CRN":
                    strSQL = qcls.CreditNoteQuery(DocEntry);
                    break;
               

            }
            if (!clsModule.HANA)
            {
                strSQL = clsModule.objaddon.objglobalmethods.ChangeHANAtoSql(strSQL);
            }
            return strSQL;
        }
        public string GetTaxData(string DocEntry, string Transtype)
        {
            string maintb = "";
            string subtb1 = "";
            switch (Transtype)
            {
                case "INV":
                    maintb = "OINV";
                    subtb1 = "INV1";
                    break;
                case "CRN":
                    maintb = "ORIN";
                    subtb1 = "RIN1";
                    break;            
            }


            int Round = 2;

            strSQL = " with TaxCat AS (";
            strSQL += " SELECT t1.\"Code\" ,Max(t1.\"U_CatCode\") AS \"TaxCode\",max(t3.\"Code\") AS \"Reasoncode\", Max(t3.\"Name\") AS \"Reason\"  FROM \"@TAXCAT\" t1  ";
            strSQL += " left JOIN \"@TAXCAT1\" t2 ON t1.\"Code\" =t2.\"Code\" ";
            strSQL += " left JOIN \"@TAXRSN\" t3 ON t2.\"U_Reason\"  =t3.\"Code\"";
            strSQL += " GROUP BY t1.\"Code\" )";

            strSQL += "SELECT sum(Round((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END ," + Round + ")) as  \"taxable\" ,";
            strSQL += "round(sum(Round((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END ," + Round + ")) * (tax.\"Rate\" /100)," + Round + ") as  \"tax\" ,";
            //Export
            strSQL += "Max(CASE WHEN doc.\"DocRate\"=0 THEN 1 ELSE doc.\"DocRate\" END) as  \"DocRate\" ,";            
            
            strSQL += "   sum(Round(itm.\"VatSumSy\"," + Round + ")) AS \"taxold\",";            
    
            strSQL += " tax.\"Rate\"  as \"TaxRate\",TaxCat.\"TaxCode\"  as \"TaxCat\", " +
                      " TaxCat.\"Reason\"   AS \"TaxReason\", " +
                      "  TaxCat.\"Reasoncode\"  AS \"TaxReasoncode\", " +
                      "  Max(itm.\"Currency\") AS \"Currency\"  FROM " + subtb1 + " itm ";
            strSQL += " LEFT JOIN OVTG tax ON tax.\"Code\" = itm.\"VatGroup\" ";
            strSQL += " LEFT JOIN TaxCat  ON TaxCat.\"Code\" =tax.\"Code\"  ";
            strSQL += " LEFT JOIN " + maintb + " DOC ON DOC.\"DocEntry\" = itm.\"DocEntry\" ";
            strSQL += " WHERE itm.\"DocEntry\" = '" + DocEntry + "' GROUP BY tax.\"Rate\",TaxCat.\"TaxCode\",TaxCat.\"Reason\", TaxCat.\"Reasoncode\" ";

            if (!clsModule.HANA)
            {
                strSQL = clsModule.objaddon.objglobalmethods.ChangeHANAtoSql(strSQL);
            }

            return strSQL;
        }
        public string GetFrightData(string DocEntry, string Transtype)
        {
            string maintb = "";
            string subtb1 = "";
            switch (Transtype)
            {
                case "INV":
                    maintb = "INV3";
                    subtb1 = "INV4";
                    break;
                case "CRN":
                    maintb = "RIN3";
                    subtb1 = "RIN4";
                    break;
              
            }

            strSQL = @" Select 'Freight' as Dscription,1 as Quantity,'9965' as HSN,TF.""VatPrcnt"",TF.""LineTotal"",TF.""GrsAmount"" as ""Total Value"",";
            strSQL += @" IFNULL((select sum(""TaxSum"") from " + subtb1 + @" where ""DocEntry"" = TF.""DocEntry"" and ""LineNum"" = TF.""LineNum"" and ""staType"" = '-100'
                        AND ""ExpnsCode"" <> '-1'),0) as CGSTAmt,IFNULL((select sum(""TaxSum"") from " + subtb1 + @" where ""DocEntry"" = TF.""DocEntry"" 
                        and ""LineNum"" = TF.""LineNum"" and ""staType"" = -110 and ""ExpnsCode"" <> '-1'),0) as SGSTAmt,";
            strSQL += @"IFNULL((select sum(""TaxSum"") from " + subtb1 + @" where ""DocEntry"" = TF.""DocEntry"" and ""LineNum"" = TF.""LineNum"" and ""staType"" = '-120'
                        AND  ""ExpnsCode"" <> '-1'),0) as IGSTAmt from " + maintb + @" TF where TF.""DocEntry"" = " + DocEntry + @" and TF.""ExpnsCode"" <> '-1'";


            if (!clsModule.HANA)
            {
                strSQL = clsModule.objaddon.objglobalmethods.ChangeHANAtoSql(strSQL);
            }

            return strSQL;
        }
        public enum EinvoiceMethod
        {
            Default = 0,
            CreateIRN = 1,
            CancelIRN = 2,
            GetIrnByDocnum = 3,
            GETIRNDetails = 4


        }

        private void Create_Customize_Fields(SAPbouiCOM.Form oForm)
        {                     
            try
            {
                switch (oForm.TypeEx)
                {
                    case "133":
                    case "179":
                        break;
                    default:
                        return;
                }

                SAPbouiCOM.Item oItem;
                clsModule.objaddon.objglobalmethods.WriteErrorLog("Customize Field Start");

                try
                {
                    if (oForm.Items.Item("btneinv").UniqueID == "btneinv")
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {

                }
                switch (oForm.TypeEx)
                {
                    case "133":
                    case "179":

                        SAPbouiCOM.Folder objfolder;
                        oItem = oForm.Items.Add("einv", SAPbouiCOM.BoFormItemTypes.it_FOLDER);
                        objfolder = (SAPbouiCOM.Folder)oItem.Specific;
                        oItem.AffectsFormMode = false;
                        objfolder.Caption = "E-Invoice Details";
                        objfolder.GroupWith("1320002137");
                        objfolder.Pane = 26;
                        oItem.Width = 125;
                        oItem.Visible = true;
                        // oForm.PaneLevel = 1;
                        oItem.Left = oForm.Items.Item("1320002137").Left + oForm.Items.Item("1320002137").Width;
                        oItem.Enabled = true;
                        break;
                  
                }
                switch (oForm.TypeEx)
                {
                    case "133":
                    case "179":


                        oItem = oForm.Items.Add("btneinv", SAPbouiCOM.BoFormItemTypes.it_BUTTON);
                        button = (SAPbouiCOM.Button)oItem.Specific;
                        button.Caption = "Generate E-invoice";
                        oItem.Left = oForm.Items.Item("2").Left + oForm.Items.Item("2").Width + 5;
                        oItem.Top = oForm.Items.Item("2").Top;
                        oItem.Height = oForm.Items.Item("2").Height;
                        oItem.LinkTo = "2";
                        Size Fieldsize = System.Windows.Forms.TextRenderer.MeasureText("Generate E-Invoice", new Font("Arial", 12.0f));
                        oItem.Width = Fieldsize.Width;
                        oForm.Items.Item("btneinv").SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, Convert.ToInt32(SAPbouiCOM.BoAutoFormMode.afm_All), SAPbouiCOM.BoModeVisualBehavior.mvb_False);                      
                                         
                        break;
                    default:
                        return;
                }





                SAPbouiCOM.Item newTextBox;
                SAPbouiCOM.EditText otxt;
                SAPbouiCOM.StaticText olbl;
                string tablename = "";
                oForm.Freeze(true);

                switch (oForm.TypeEx)
                {
                    case "133":
                        tablename = "OINV";
                        break;
                    case "179":
                        tablename = "ORIN";
                        break;
                    default:
                        return;
                }


                int top = oForm.Items.Item("112").Top + 25;
                int space = 15;
                int labelwidth = 200;
                int textboxwidth = 300;
                int textboxheight = 15;


                CreateLabel(oForm, "lblPIH", "PIH No", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtPIH", tablename, "U_PIHNo", 26, 26, oForm.Items.Item("lblPIH").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;

                CreateLabel(oForm, "lblUUID", "UUID No", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtUUID", tablename, "U_UUIDNo", 26, 26, oForm.Items.Item("lblUUID").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;

                CreateLabel(oForm, "lblInvHash", "InvoiceHash No", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtInvHash", tablename, "U_InvoiceHashNo", 26, 26, oForm.Items.Item("lblInvHash").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;

                CreateLabel(oForm, "lblICV", "ICV No", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtICV", tablename, "U_ICVNo", 26, 26, oForm.Items.Item("lblICV").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;

                CreateLabel(oForm, "lblEinvSt", "E-Inv Status", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtEinvSt", tablename, "U_EinvStatus", 26, 26, oForm.Items.Item("lblEinvSt").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;

                CreateLabel(oForm, "lblIssueDt", "Issue Date", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtIssueDt", tablename, "U_Issuedt", 26, 26, oForm.Items.Item("lblIssueDt").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;
                CreateLabel(oForm, "lblWarn", "Warning", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtWarn", tablename, "U_Warn", 26, 26, oForm.Items.Item("lblWarn").Left + 80, top, textboxwidth, textboxheight);
                top = top + space;
                CreateLabel(oForm, "lblError", "Error", 26, 26, oForm.Items.Item("112").Left + 20, top, labelwidth);
                CreateTextbox(oForm, "txtError", tablename, "U_Error", 26, 26, oForm.Items.Item("lblError").Left + 80, top, textboxwidth, textboxheight);

                oForm.Freeze(false);

                clsModule.objaddon.objglobalmethods.WriteErrorLog("Customize Field Completed");
            }
            catch (Exception ex)
            {
            }
        }






        private void CreateLabel(SAPbouiCOM.Form oForm, string name, string caption, int fromPane, int toPane, int left, int top, int width)
        {
            SAPbouiCOM.Item newTextBox;
            SAPbouiCOM.StaticText olbl;

            newTextBox = oForm.Items.Add(name, SAPbouiCOM.BoFormItemTypes.it_STATIC);
            newTextBox.FromPane = fromPane;
            newTextBox.ToPane = toPane;
            newTextBox.Left = left;
            newTextBox.Top = top;
            newTextBox.Width = width;
            olbl = (SAPbouiCOM.StaticText)oForm.Items.Item(name).Specific;
            ((SAPbouiCOM.StaticText)(olbl.Item.Specific)).Caption = caption;
        }

        private void CreateTextbox(SAPbouiCOM.Form oForm, string name, string tablename, string Feildname, int fromPane, int toPane, int left, int top, int width, int height)
        {
            SAPbouiCOM.Item newTextBox;
            SAPbouiCOM.EditText olbl;

            newTextBox = oForm.Items.Add(name, SAPbouiCOM.BoFormItemTypes.it_EDIT);
            newTextBox.FromPane = fromPane;
            newTextBox.ToPane = toPane;
            newTextBox.Left = left;
            newTextBox.Top = top;
            newTextBox.Width = width;
            newTextBox.Height = height;
            oForm.Items.Item(name).SetAutoManagedAttribute(SAPbouiCOM.BoAutoManagedAttr.ama_Editable, Convert.ToInt32(SAPbouiCOM.BoAutoFormMode.afm_All), SAPbouiCOM.BoModeVisualBehavior.mvb_False);

            olbl = (SAPbouiCOM.EditText)oForm.Items.Item(name).Specific;
            try
            {
                olbl.DataBind.SetBound(true, tablename, Feildname);
            }
            catch (Exception ex)
            {


            }

        }
        public DataTable GetEinvoiceStatus(string DocEntry, string TransType)
        {
            SAPbobsCOM.Recordset invrecordset;
            objRs = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            invrecordset = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);

            DataTable dataTable = new DataTable();
            try
            {


                strSQL = @"Select T0.""U_Live"",T0.""U_UATUrl"",T0.""U_LiveUrl"",T0.""U_AuthKey"",T0.""U_SerConfig"",T1.""U_URLType"",T1.""U_URL"", ";
                strSQL += @"Case when T0.""U_Live""='N' then CONCAT(T0.""U_UATUrl"",T1.""U_URL"") Else CONCAT(T0.""U_LiveUrl"",T1.""U_URL"") End as URL";
                strSQL += @" from ""@EICON"" T0 join ""@EICON1"" T1 on T0.""Code""=T1.""Code"" where T0.""Code""='01'";
                strSQL += @" and T1.""U_URLType""='Get E-Invoice' ";

                objRs.DoQuery(strSQL);
                if (objRs.RecordCount == 0)
                {
                    clsModule.objaddon.objapplication.StatusBar.SetText("API is Missing for \"Get E-Invoice\". Please update in E-invoice Configuration... ", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return dataTable;
                }
                strSQL = GetInvoiceData(DocEntry, TransType);
                invrecordset.DoQuery(strSQL);
                if (invrecordset.RecordCount > 0)
                {
                    string Cleartype = "";
                    switch (TransType)
                    {
                        case "INV":                           
                            Cleartype = (invrecordset.Fields.Item("DocType").Value.ToString() == "I") ? "INV" : "DBN";                          
                            break;
                        case "CRN":                          
                            Cleartype = "CRN";                           
                            break;
                    }
                    string url = objRs.Fields.Item("URL").Value.ToString();
                    url += "?invoiceNumber=" + invrecordset.Fields.Item("DocNum").Value.ToString() + "&invoiceType=" + Cleartype + "&issueDate=" + clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(invrecordset.Fields.Item("DocDate").Value.ToString()), "dd/MM/yyyy", "yyyy-MM-dd") + "&vat=" + invrecordset.Fields.Item("TaxIdNum").Value.ToString() + "";
                    Dictionary<string, string> head = new Dictionary<string, string>();
                  
                    string Accesstkn = objRs.Fields.Item("U_AuthKey").Value.ToString();
                    head.Add("x-cleartax-auth-token", Accesstkn);
                    head.Add("vat", invrecordset.Fields.Item("TaxIdNum").Value.ToString());              

                    dataTable = Get_API_Response("", url, "GET", headers: head);

                   
                }
                return dataTable;
            }
            catch (Exception)
            {

                return dataTable;
            }
        }
        public bool PrintEmbedded(string DocEntry, string TransType)
        {
            try { 
            SAPbobsCOM.Recordset invrecordset;
            objRs = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            invrecordset = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
            string TypeCode = "";
                string Cleartype = "";

            strSQL = @"Select T0.""U_Live"",T0.""U_UATUrl"",T0.""U_LiveUrl"",T0.""U_AuthKey"",T0.""U_SerConfig"",T1.""U_URLType"",T1.""U_URL"", ";
            strSQL += @"Case when T0.""U_Live""='N' then CONCAT(T0.""U_UATUrl"",T1.""U_URL"") Else CONCAT(T0.""U_LiveUrl"",T1.""U_URL"") End as URL,";
            strSQL += @"T0.""U_DBUser"" ,T0.""U_DBPass"",T0.""U_Cryspath"" ";
            strSQL += @" from ""@EICON"" T0 join ""@EICON1"" T1 on T0.""Code""=T1.""Code"" where T0.""Code""='01'";
            strSQL += @" and T1.""U_URLType""='PDF A3' ";

            objRs.DoQuery(strSQL);
            if (objRs.RecordCount == 0)
            {
                clsModule.objaddon.objapplication.StatusBar.SetText("API is Missing for \"PDF A\". Please update in E-invoice Configuration... ", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
            strSQL = GetInvoiceData(DocEntry, TransType);
            invrecordset.DoQuery(strSQL);
            if (invrecordset.RecordCount > 0)
            {
                switch (TransType)
                {
                    case "INV":
                        TypeCode = invrecordset.Fields.Item("DocType").Value.ToString() == "S" ? "INV1" : "INV2";
                        break;
                    case "CRN":
                        TypeCode = invrecordset.Fields.Item("DocType").Value.ToString() == "S" ? "RIN1" : "RIN2";
                        break;
                }

                string Crystalquery = " SELECT COALESCE(D.\"DocCode\",ASSD.\"DocCode\" ) AS \"DocCode\",COALESCE(D.\"DocName\",ASSD.\"DocName\" ) AS \"DocName\" FROM RDFL r " +
                   " LEFT JOIN prs1 P ON P.\"SeqID\" =r.\"DfltSeq\" " +
                   " LEFT JOIN RDOC D ON D.\"DocCode\"  =p.\"LaytCode\" AND D.\"TypeCode\" ='" + TypeCode + "' " +
                   " LEFT JOIN RDOC AssD ON AssD.\"DocCode\"  =r.\"DfltReport\" AND D.\"TypeCode\" ='" + TypeCode + "'" +
                   " LEFT JOIN OUSR Usr  ON USR.USERID = r.\"UserId\"";
                Crystalquery += " WHERE Usr.USER_CODE = '" + clsModule.objaddon.objcompany.UserName + "'";


                DataTable dtcry = clsModule.objaddon.objglobalmethods.GetmultipleValue(Crystalquery);
                if (dtcry.Rows.Count == 0)
                {

                    //  clsModule.objaddon.objapplication.StatusBar.SetText("Kindly Set Default Print in this User("+ clsModule.objaddon.objcompany.UserName + ").... ", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    // return false;
                }


                string BaseSysPath = Getbasepath();
                string SysPath = BaseSysPath + invrecordset.Fields.Item("DocNum").Value.ToString() + "_";
                SysPath += clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(invrecordset.Fields.Item("DocDate").Value.ToString()), "dd/MM/yyyy", "yyyy-MM-dd");

                clsModule.objaddon.objapplication.StatusBar.SetText("Getting Data from  Crysatl Report. Please Wait...." + DocEntry, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                string crytalpath = "";
                string endpath = "";
                    string files = "";
                    string fileType = "";
                switch (TransType)
                {
                    case "INV":
                             fileType = (invrecordset.Fields.Item("DocType").Value.ToString() == "I") ? "Item" : "Service";
                            Cleartype = (invrecordset.Fields.Item("DocType").Value.ToString() == "I") ? "INV" : "DBN";
                            files = $"SELECT \"U_FileNm\" FROM \"@EICON2\" WHERE \"U_DocType\" = 'A/R Invoice' AND \"U_TransType\" = '{fileType}'";

                            endpath = clsModule.objaddon.objglobalmethods.getSingleValue(files);
                                                          
                        break;
                    case "CRN":
                            fileType = (invrecordset.Fields.Item("DocType").Value.ToString() == "I") ? "Item" : "Service";
                            Cleartype = "CRN";
                             files = $"SELECT \"U_FileNm\" FROM \"@EICON2\" WHERE \"U_DocType\" = 'A/R Credit Memo' AND \"U_TransType\" = '{fileType}'";
                            endpath = clsModule.objaddon.objglobalmethods.getSingleValue(files);
                           
                            break;
                }
                 
                    crytalpath = objRs.Fields.Item("U_Cryspath").Value.ToString() + endpath + ".rpt";
                // clsModule.objaddon.objglobalmethods.GetCrystalReportFile(dtcry.Rows[0]["DocCode"].ToString(), crytalpath); 

                string FileName = SysPath + "_PDF.pdf";
                    clsModule.objaddon.objglobalmethods.Create_RPT_To_PDF(crytalpath, clsModule.objaddon.objcompany.Server,
                    clsModule.objaddon.objcompany.CompanyDB, objRs.Fields.Item("U_DBUser").Value.ToString(), objRs.Fields.Item("U_DBPass").Value.ToString(), DocEntry, FileName);

                    string FilePDFA = "";
                if (File.Exists(FileName))
                {
                    clsModule.objaddon.objapplication.StatusBar.SetText("Creating PDF A3 . Please Wait...." + DocEntry, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                    string url = objRs.Fields.Item("URL").Value.ToString();
                    url += "?invoiceNumber=" + invrecordset.Fields.Item("DocNum").Value.ToString() + "&invoiceType=" + Cleartype + "&issueDate=" + clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(invrecordset.Fields.Item("DocDate").Value.ToString()), "dd/MM/yyyy", "yyyy-MM-dd") + "&vat=" + invrecordset.Fields.Item("TaxIdNum").Value.ToString() + "";

                    Dictionary<string, string> head = new Dictionary<string, string>();
                    string Accesstkn = objRs.Fields.Item("U_AuthKey").Value.ToString();
                    head.Add("x-cleartax-auth-token", Accesstkn);
                    head.Add("vat", invrecordset.Fields.Item("TaxIdNum").Value.ToString());

                    byte[] pdfBytes = File.ReadAllBytes(FileName);
                    MultipartFormDataContent formContent = new MultipartFormDataContent();
                    String base64EncodedPdfFile = "\"\"" + Convert.ToBase64String(pdfBytes) + "\"\"";
                    formContent.Add(new StringContent(base64EncodedPdfFile), "base64EncodedPdfFile");
                    var formData1 = new NameValueCollection
                         {
                         { "base64EncodedPdfFile", base64EncodedPdfFile },
                            };

                    FilePDFA = SysPath + "_PDFA.pdf";

                    Get_API_Response("", url, "POST", "multipart/form-data", head, formData1, FilePDFA);
                }

                clsModule.objaddon.objapplication.StatusBar.SetText("Creating XML. Please Wait...." + DocEntry, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                string Xml64 = clsModule.objaddon.objglobalmethods.getSingleValue("SELECT COALESCE (CAST(\"U_INVXml\" AS varchar),'')  from \"@EILOG\"  where \"U_DocEntry\"=" + DocEntry + " AND (CAST(\"U_INVXml\" AS Varchar) <>'' or CAST(\"U_INVXml2\" AS Varchar) <>'') and \"U_INVTyp\"='" + Cleartype + "' order by \"DocEntry\" desc ");
                Xml64 += clsModule.objaddon.objglobalmethods.getSingleValue("SELECT COALESCE (CAST(\"U_INVXml2\" AS varchar),'')  from \"@EILOG\"  where \"U_DocEntry\"=" + DocEntry + " AND (CAST(\"U_INVXml\" AS Varchar) <>'' or CAST(\"U_INVXml2\" AS Varchar) <>'') and \"U_INVTyp\"='" + Cleartype + "' order by \"DocEntry\" desc ");
                Xml64 += clsModule.objaddon.objglobalmethods.getSingleValue("SELECT COALESCE (CAST(\"U_INVXml3\" AS varchar),'')  from \"@EILOG\"  where \"U_DocEntry\"=" + DocEntry + " AND (CAST(\"U_INVXml\" AS Varchar) <>'' or CAST(\"U_INVXml2\" AS Varchar) <>'') and \"U_INVTyp\"='" + Cleartype + "' order by \"DocEntry\" desc ");
                Xml64 += clsModule.objaddon.objglobalmethods.getSingleValue("SELECT COALESCE (CAST(\"U_INVXml4\" AS varchar),'')  from \"@EILOG\"  where \"U_DocEntry\"=" + DocEntry + " AND (CAST(\"U_INVXml\" AS Varchar) <>'' or CAST(\"U_INVXml2\" AS Varchar) <>'') and \"U_INVTyp\"='" + Cleartype + "' order by \"DocEntry\" desc ");
                Xml64 += clsModule.objaddon.objglobalmethods.getSingleValue("SELECT COALESCE (CAST(\"U_INVXml5\" AS varchar),'')  from \"@EILOG\"  where \"U_DocEntry\"=" + DocEntry + " AND (CAST(\"U_INVXml\" AS Varchar) <>'' or CAST(\"U_INVXml2\" AS Varchar) <>'') and \"U_INVTyp\"='" + Cleartype + "' order by \"DocEntry\" desc ");

                string Xmlpath = SysPath + "_XML.XMl";

                if (!string.IsNullOrEmpty(Xml64))
                {
                    clsModule.objaddon.objglobalmethods.Convertbase64toxml(Xml64, Xmlpath);
                }
                List<string> PathDOCList = new List<string>();

                PathDOCList.Add(FileName);
                PathDOCList.Add(FilePDFA);
                PathDOCList.Add(Xmlpath);

                clsModule.objaddon.objglobalmethods.saveattachment(DocEntry, PathDOCList, Cleartype);
            }


            return true;
            }
            catch (Exception ex)
            {

                return true;
            }
        }

       
        public bool Generate_Cancel_IRN(EinvoiceMethod Create_Cancel, string DocEntry, string TransType, string Type, ref DataTable datatable,
            bool frommul)
        {
            string requestParams;            
            string Tempstatus;
            bool Einvlog =false;
            try
            {


                SAPbobsCOM.Recordset invrecordset, Taxrecset;
                objRs = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                Taxrecset = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                if (Create_Cancel == EinvoiceMethod.CreateIRN)
                {
                    GenerateIRN GenerateIRNGetJson = new GenerateIRN();

                    strSQL = GetInvoiceData(DocEntry, TransType);
                    invrecordset = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);


                    invrecordset.DoQuery(strSQL);
                    if (invrecordset.RecordCount > 0)
                    {
                        strSQL = @"Select T0.""U_Live"",T0.""U_UATUrl"",T0.""U_LiveUrl"",T0.""U_AuthKey"",T0.""U_SerConfig"",T1.""U_URLType"",T1.""U_URL"", ";
                        strSQL += @"Case when T0.""U_Live""='N' then CONCAT(T0.""U_UATUrl"",T1.""U_URL"") Else CONCAT(T0.""U_LiveUrl"",T1.""U_URL"") End as URL,T0.""U_DevID"",T0.""U_Startdate"",T0.""U_LiveDB"" ";
                        strSQL += @" from ""@EICON"" T0 join ""@EICON1"" T1 on T0.""Code""=T1.""Code"" where T0.""Code""='01'";
                        strSQL += @" and T1.""U_URLType""='E-invoice -Tax' ";

                        objRs.DoQuery(strSQL);
                        if (objRs.RecordCount == 0)
                        {
                            clsModule.objaddon.objapplication.StatusBar.SetText("API is Missing for \"Create Invoice\". Please update in E-invoice Configuration... ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        if (objRs.Fields.Item("U_LiveDB").Value.ToString()!=clsModule.objaddon.objcompany.CompanyDB)
                        {
                            if (objRs.Fields.Item("U_Live").Value.ToString() !="N")
                            {
                                clsModule.objaddon.objapplication.StatusBar.SetText("It's Not  Live DB Change   E-invoice Configuration... ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                        }


                        string Einvstus = "";
                        Einvstus = invrecordset.Fields.Item("Einvsts").Value.ToString();                        
                        switch (Einvstus)
                        {
                            case "CLEARED":
                            case "REPORTED":
                                Einvstus = "CLEARED";
                                break;
                        }
                        Tempstatus = Einvstus;
                        if (!string.IsNullOrEmpty(Einvstus))
                        {
                            datatable = GetEinvoiceStatus(DocEntry, TransType);
                            if (datatable.Rows.Count > 0)
                            {
                                Einvstus = columnFind(datatable, "InvoiceStatus", 0);
                                switch (Einvstus)
                                {
                                    case "CLEARED":
                                    case "REPORTED":
                                        Einvstus = "CLEARED";
                                        break;
                                }
                            }
                        }

                        if (!(Einvstus == "CLEARED"))
                        {
                            strSQL = @"Select T0.""U_Live"",T0.""U_UATUrl"",T0.""U_LiveUrl"",T0.""U_AuthKey"",T0.""U_SerConfig"",T1.""U_URLType"",T1.""U_URL"", ";
                            strSQL += @"Case when T0.""U_Live""='N' then CONCAT(T0.""U_UATUrl"",T1.""U_URL"") Else CONCAT(T0.""U_LiveUrl"",T1.""U_URL"") End as URL,T0.""U_DevID"",T0.""U_Startdate"",T0.""U_LiveDB"" ";
                            strSQL += @" from ""@EICON"" T0 join ""@EICON1"" T1 on T0.""Code""=T1.""Code"" where T0.""Code""='01'";
                            strSQL += @" and T1.""U_URLType""='E-invoice -Tax' ";

                            objRs.DoQuery(strSQL);

                            DateTime stdt;
                            DateTime docdt;

                            DateTime.TryParseExact(objRs.Fields.Item("U_Startdate").Value.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns(), CultureInfo.InvariantCulture, DateTimeStyles.None, out stdt);
                            DateTime.TryParseExact(invrecordset.Fields.Item("DocDate").Value.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns(), CultureInfo.InvariantCulture, DateTimeStyles.None, out docdt);
                            if (!string.IsNullOrEmpty(objRs.Fields.Item("U_Startdate").Value.ToString()))
                            {
                                if (!(docdt >= stdt))
                                {
                                    clsModule.objaddon.objapplication.StatusBar.SetText("Cannot Generate E-invoice Before valid Date(" + stdt + ")", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                    return false;
                                }
                            }
                       

                             clsModule.objaddon.objapplication.StatusBar.SetText("Generating Einvoice. Please Wait...." + DocEntry, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            string Accesstkn = objRs.Fields.Item("U_AuthKey").Value.ToString();
                            string vatid = invrecordset.Fields.Item("TaxIdNum").Value.ToString();
                            string syscur = invrecordset.Fields.Item("SysCurrncy").Value.ToString();

                            GenerateIRNGetJson.DeviceId = objRs.Fields.Item("U_DevID").Value.ToString(); //"4e0a6294-19fb-4774-90ef-65b023c89276";
                            GenerateIRNGetJson.EInvoice.ProfileID = "reporting:1.0";
                            GenerateIRNGetJson.EInvoice.ID.en = invrecordset.Fields.Item("DocNum").Value.ToString();
                            GenerateIRNGetJson.EInvoice.ID.ar = null;

                            GenerateIRNGetJson.EInvoice.InvoiceTypeCode.name = invrecordset.Fields.Item("U_EType").Value.ToString();
                            GenerateIRNGetJson.EInvoice.InvoiceTypeCode.value = invrecordset.Fields.Item("TaxType").Value.ToString();


                            GenerateIRNGetJson.EInvoice.IssueDate = clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(invrecordset.Fields.Item("DocDate").Value.ToString()), "dd/MM/yyyy", "yyyy-MM-dd");
                            GenerateIRNGetJson.EInvoice.IssueTime = clsModule.objaddon.objglobalmethods.ConverttoTime(invrecordset.Fields.Item("DocTime").Value.ToString());

                            //need to check
                            GenerateIRNGetJson.EInvoice.Delivery.Add(new Delivery
                            {
                                ActualDeliveryDate = clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(invrecordset.Fields.Item("DocDate").Value.ToString()), "dd/MM/yyyy", "yyyy-MM-dd"),
                                LatestDeliveryDate = ""
                            });



                            //need to check
                            GenerateIRNGetJson.EInvoice.OrderReference = new OrderReference
                            {
                                ID = new ID
                                {
                                    ar = null,
                                    en = ""
                                }
                            };

                            GenerateIRNGetJson.EInvoice.ContractDocumentReference.Add(new ContractDocumentReference
                            {
                                ID = new ID
                                {
                                    ar = null,
                                    en = invrecordset.Fields.Item("NumAtCard").Value.ToString()
                                }

                            });

                            GenerateIRNGetJson.EInvoice.DocumentCurrencyCode = invrecordset.Fields.Item("DocCur").Value.ToString();

                            GenerateIRNGetJson.EInvoice.TaxCurrencyCode = invrecordset.Fields.Item("SysCurrncy").Value.ToString();

                            clsModule.objaddon.objglobalmethods.WriteErrorLog("Document Details Complete");

                            //Seller Details
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PartyLegalEntity.RegistrationName.ar = null;
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PartyLegalEntity.RegistrationName.en = invrecordset.Fields.Item("CompnyName").Value.ToString();

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PartyTaxScheme.CompanyID = invrecordset.Fields.Item("TaxIdNum").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PartyTaxScheme.TaxScheme.ID = "VAT";

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PartyIdentification.ID.schemeID = invrecordset.Fields.Item("CmpId").Value.ToString(); 
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PartyIdentification.ID.value = invrecordset.Fields.Item("TaxIdNum2").Value.ToString();


                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.StreetName.en = invrecordset.Fields.Item("StreetNo").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.StreetName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.AdditionalStreetName.en = invrecordset.Fields.Item("Street").Value.ToString(); ;
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.AdditionalStreetName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.BuildingNumber.en = invrecordset.Fields.Item("Building").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.BuildingNumber.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.PlotIdentification.en = "";
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.PlotIdentification.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.CityName.en = invrecordset.Fields.Item("City").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.CityName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.CitySubdivisionName.en = invrecordset.Fields.Item("County").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.CitySubdivisionName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.PostalZone = invrecordset.Fields.Item("ZipCode").Value.ToString();

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.CountrySubentity.en = invrecordset.Fields.Item("State").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.CountrySubentity.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingSupplierParty.Party.PostalAddress.Country.IdentificationCode = invrecordset.Fields.Item("CodeCountry").Value.ToString();


                            clsModule.objaddon.objglobalmethods.WriteErrorLog("Seller Details Complete");

                            //Buyer Details
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PartyLegalEntity.RegistrationName.ar = null;
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PartyLegalEntity.RegistrationName.en = invrecordset.Fields.Item("CardName").Value.ToString();

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PartyTaxScheme.CompanyID = invrecordset.Fields.Item("LicTradNum").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PartyTaxScheme.TaxScheme.ID = "VAT";

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PartyIdentification.ID.schemeID = invrecordset.Fields.Item("U_IDType").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PartyIdentification.ID.value = invrecordset.Fields.Item("AddID").Value.ToString();


                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.StreetName.en = invrecordset.Fields.Item("StreetNoB").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.StreetName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.AdditionalStreetName.en = invrecordset.Fields.Item("StreetB").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.AdditionalStreetName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.BuildingNumber.en = invrecordset.Fields.Item("BuildingB").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.BuildingNumber.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.PlotIdentification.en = "";
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.PlotIdentification.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.CityName.en = invrecordset.Fields.Item("CityB").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.CityName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.CitySubdivisionName.en = invrecordset.Fields.Item("CountyB").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.CitySubdivisionName.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.PostalZone = invrecordset.Fields.Item("ZipCodeB").Value.ToString();

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.CountrySubentity.en = invrecordset.Fields.Item("StateB").Value.ToString();
                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.CountrySubentity.ar = null;

                            GenerateIRNGetJson.EInvoice.AccountingCustomerParty.Party.PostalAddress.Country.IdentificationCode = invrecordset.Fields.Item("CodeCountryB").Value.ToString();

                            clsModule.objaddon.objglobalmethods.WriteErrorLog("Buyer Details Complete");



                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.LineExtensionAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.LineExtensionAmount.value = invrecordset.Fields.Item("Totgross").Value.ToString(); //totgross

                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.AllowanceTotalAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.AllowanceTotalAmount.value = invrecordset.Fields.Item("Allownace").Value.ToString(); //DiscSum

                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.TaxExclusiveAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.TaxExclusiveAmount.value = invrecordset.Fields.Item("TaxExclusive").Value.ToString();


                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.TaxInclusiveAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.TaxInclusiveAmount.value = invrecordset.Fields.Item("Totnet").Value.ToString(); //totnet

                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.PrepaidAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.PrepaidAmount.value = "0.00";//no use

                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.PayableRoundingAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();                            
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.PayableRoundingAmount.value = invrecordset.Fields.Item("Roundtot").Value.ToString();//totnet

                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.PayableAmount.currencyID = invrecordset.Fields.Item("DocCur").Value.ToString();
                            //totgross (suppose PrepaidAmount cames totnet-PrepaidAmount)
                            GenerateIRNGetJson.EInvoice.LegalMonetaryTotal.PayableAmount.value = invrecordset.Fields.Item("Totnet1").Value.ToString();//totnet



                            //  No use
                            GenerateIRNGetJson.EInvoice.PaymentMeans.Add(new PaymentMean
                            {
                                PaymentMeansCode = invrecordset.Fields.Item("Paymeanscode").Value.ToString(),                              
                                InstructionNote = new InstructionNote
                                {
                                    en = invrecordset.Fields.Item("Comments").Value.ToString(),
                                    ar = null
                                }
                            });

                            //No Use
                            GenerateIRNGetJson.EInvoice.Note.en = "This is a computer generated invoice.";
                            GenerateIRNGetJson.EInvoice.Note.ar = null;

                            GenerateIRNGetJson.CustomFields.TotalBoxes = "";
                            GenerateIRNGetJson.CustomFields.TotalWreight = "";

                            clsModule.objaddon.objglobalmethods.WriteErrorLog("Document Details Complete");


                            //Line Details
                            for (int i = 0; i < invrecordset.RecordCount; i++)
                            {
                                GenerateIRNGetJson.EInvoice.InvoiceLine.Add(new InvoiceLine
                                {
                                    ID = invrecordset.Fields.Item("LineNum").Value.ToString(),
                                    Item = new Item
                                    {
                                        Name = new Name { ar = null, en = invrecordset.Fields.Item("Dscription").Value.ToString() },
                                        BuyersItemIdentification = new BuyersItemIdentification
                                        {
                                            ID = new ID { ar = null, en = invrecordset.Fields.Item("ItemBuyerID").Value.ToString() }
                                        },
                                        SellersItemIdentification = new SellersItemIdentification
                                        {
                                            ID = new ID { ar = null, en = invrecordset.Fields.Item("ItemsellerID").Value.ToString() }
                                        },
                                        StandardItemIdentification = new StandardItemIdentification
                                        {
                                            ID = new ID { ar = null, en = "" }
                                        },
                                        ClassifiedTaxCategory = new ClassifiedTaxCategory
                                        {
                                            ID = invrecordset.Fields.Item("TaxCat").Value.ToString(),
                                            Percent = invrecordset.Fields.Item("Taxrate").Value.ToString(),
                                            TaxScheme = new TaxScheme { ID = "VAT" }
                                        }
                                    },




                                    Price = new Price
                                    {

                                        AllowanceCharge = new AllowanceCharge
                                        {
                                            ChargeIndicator = "False",
                                            BaseAmount = new BaseAmount
                                            {
                                                currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                                value = invrecordset.Fields.Item("BaseAmt").Value.ToString() //BaseAmt
                                            },
                                            Amount = new Amount
                                            {
                                                currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                                value = invrecordset.Fields.Item("DiscAmt").Value.ToString()//dis
                                            },
                                            MultiplierFactorNumeric = "0",
                                            AllowanceChargeReason = new AllowanceChargeReason { ar = null, en = "" },
                                            AllowanceChargeReasonCode = null
                                        },


                                        PriceAmount = new PriceAmount
                                        {
                                            currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                            value = invrecordset.Fields.Item("PriceAmt").Value.ToString() // PriceAmt
                                        },
                                        //need check no useno use
                                        BaseQuantity = new BaseQuantity
                                        {
                                            unitCode = "",//UomCode
                                            value = ""//Quantity
                                        }
                                    },
                                    InvoicedQuantity = new InvoicedQuantity
                                    {
                                        unitCode = invrecordset.Fields.Item("UomCode").Value.ToString(),//UomCode
                                        value = invrecordset.Fields.Item("Quantity").Value.ToString() //Quantity
                                    },
                                    LineExtensionAmount = new LineExtensionAmount
                                    {
                                        currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                        value = invrecordset.Fields.Item("Gross").Value.ToString()//Gross
                                    },
                                    TaxTotal = new TaxTotal
                                    {
                                        TaxAmount = new TaxAmount
                                        {
                                            currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                            value = invrecordset.Fields.Item("taxamt").Value.ToString()//taxamt
                                        },
                                        RoundingAmount = new RoundingAmount
                                        {
                                            currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                            value = invrecordset.Fields.Item("Linenet").Value.ToString()//net amount
                                        }
                                    }
                                });

                                GenerateIRNGetJson.EInvoice.BillingReference.Add(new BillingReference
                                {
                                    InvoiceDocumentReference = new InvoiceDocumentReference
                                    {
                                        ID = new ID
                                        {
                                            ar = null,
                                            en = invrecordset.Fields.Item("BaseDoc").Value.ToString()
                                        }
                                    }
                                });
                                if (invrecordset.Fields.Item("LineAllow").Value.ToString() != "0")
                                {
                                    GenerateIRNGetJson.EInvoice.AllowanceCharge.Add(new AllowanceCharge
                                    {
                                        ChargeIndicator = "False",
                                        Amount = new Amount
                                        {
                                            currencyID = invrecordset.Fields.Item("DocCur").Value.ToString(),//Doccur
                                            value = invrecordset.Fields.Item("LineAllow").Value.ToString()
                                        },
                                        TaxCategory = new TaxCategory
                                        {
                                            ID = invrecordset.Fields.Item("TaxCat").Value.ToString(),
                                            Percent = invrecordset.Fields.Item("Taxrate").Value.ToString(),
                                            TaxScheme = new TaxScheme { ID = "VAT" },
                                            TaxExemptionReason = new TaxExemptionReason
                                            {
                                                en = "",
                                                ar = invrecordset.Fields.Item("Reason").Value.ToString(),
                                            }
                                        }
                                    });
                                }
                                invrecordset.MoveNext();
                            }



                            //taxloop
                            string Taxquery;
                            Taxquery = GetTaxData(DocEntry, TransType);
                            Taxrecset.DoQuery(Taxquery);
                            List<TaxSubtotal> taxSub = new List<TaxSubtotal>();
                            decimal taxtot = 0;
                            string curr = "";
                            for (int i = 0; i < Taxrecset.RecordCount; i++)

                            {
                                taxtot +=Math.Round( Convert.ToDecimal(Taxrecset.Fields.Item("tax").Value) * Convert.ToDecimal(Taxrecset.Fields.Item("DocRate").Value),2);
                                curr = syscur;
                                taxSub.Add(new TaxSubtotal
                                {

                                    TaxableAmount = new TaxableAmount
                                    {
                                        currencyID = Taxrecset.Fields.Item("Currency").Value.ToString(),
                                        value = Taxrecset.Fields.Item("taxable").Value.ToString()
                                    },//taxable
                                    TaxAmount = new TaxAmount
                                    {
                                        currencyID = Taxrecset.Fields.Item("Currency").Value.ToString(),
                                        value = Taxrecset.Fields.Item("tax").Value.ToString()
                                    },//tax
                                    TaxCategory = new TaxCategory
                                    {
                                        ID = Taxrecset.Fields.Item("TaxCat").Value.ToString(),
                                        Percent = Taxrecset.Fields.Item("TaxRate").Value.ToString(),
                                        TaxScheme = new TaxScheme { ID = "VAT" },//tax rate
                                        TaxExemptionReasonCode = Taxrecset.Fields.Item("TaxReasoncode").Value.ToString(), //expetionT
                                        TaxExemptionReason = new TaxExemptionReason
                                        {
                                            en = Taxrecset.Fields.Item("TaxReason").Value.ToString(),
                                            ar = null
                                        }
                                    }


                                });
                                Taxrecset.MoveNext();
                            }
                            GenerateIRNGetJson.EInvoice.TaxTotal.Add(new TaxSubTotal
                            {
                                TaxSubtotal = taxSub,
                                TaxAmount = new TaxAmount { value = taxtot.ToString(), currencyID = curr }
                            });

                            requestParams = JsonConvert.SerializeObject(GenerateIRNGetJson);

                            Dictionary<string, string> head = new Dictionary<string, string>();
                            head.Add("x-cleartax-auth-token", Accesstkn);
                            head.Add("vat", vatid);

                            datatable = Get_API_Response(requestParams, objRs.Fields.Item("URL").Value.ToString(), headers: head);

                            string msg = "";
                            if (datatable.Rows.Count > 0)
                            {
                                if (!frommul)
                                {
                                    if (datatable.Rows[0]["ErrorList"].ToString() != "[]")
                                    {
                                        msg = datatable.Rows[0]["ErrorList"].ToString();
                                        clsModule.objaddon.objapplication.MessageBox("Generate: " + msg);
                                    }
                                }
                                Einvlog =E_Invoice_Logs(DocEntry, datatable, TransType, "Create", Type);
                                
                                Einvstus = columnFind(datatable, "InvoiceStatus", 0);
                               

                                switch (Einvstus)
                                {
                                    case "CLEARED":
                                    case "REPORTED":
                                        Einvstus = "CLEARED";
                                        break;
                                }
                            }
                        }


                        if (Einvstus == "CLEARED")
                        {
                            if (Tempstatus != Einvstus && (!string.IsNullOrEmpty(Tempstatus)))
                            {                                
                                if (!Einvlog)
                                {                                    
                                    E_Invoice_Logs(DocEntry, datatable, TransType, "Create", Type);
                                }

                            }

                            if (PrintEmbedded(DocEntry, TransType))
                            {
                                blnprint = true;
                            }
                        }
                    }

                    else
                    {
                        clsModule.objaddon.objapplication.StatusBar.SetText("No data found for this invoice...", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_None);
                    }
                    GenerateIRNGetJson = null;

                }

            }
            catch (Exception ex)
            {
                clsModule.objaddon.objglobalmethods.WriteErrorLog(ex.StackTrace);
                clsModule.objaddon.objapplication.StatusBar.SetText("Error_IRN: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            return true;
        }
        private bool E_Invoice_Logs(string InvDocEntry, DataTable einvDT, string ObjType, string Type, string TranType)
        {
            try
            {
                blnRefresh = false;
                string obj = "";

                SAPbobsCOM.GeneralData oGeneralData;
                SAPbobsCOM.GeneralDataParams oGeneralParams;
                SAPbobsCOM.GeneralService oGeneralService;

                oGeneralService = clsModule.objaddon.objcompany.GetCompanyService().GetGeneralService("EINVLOG");
                oGeneralData = (SAPbobsCOM.GeneralData)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData);
                oGeneralParams = (SAPbobsCOM.GeneralDataParams)oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams);

                switch (ObjType)
                {
                    case "INV":
                        obj = "13";
                        break;
                    case "CRN":
                        obj = "14";
                        break;
                   
                }

               

                objRs = (SAPbobsCOM.Recordset)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                if (Type == "Create")
                {
                    if (TranType == "E-Invoice")
                    {
                        saveEinvfields(InvDocEntry, einvDT, ObjType);
                        blnRefresh = true;
                    }

                    oGeneralData.SetProperty("U_QRCod", columnFind(einvDT, "QRCode", 0));
                    oGeneralData.SetProperty("U_RawQR", columnFind(einvDT, "RawQRCode", 0));
                    oGeneralData.SetProperty("U_UUID", columnFind(einvDT, "UUID", 0));
                    oGeneralData.SetProperty("U_PIH", columnFind(einvDT, "PIH", 0));
                    oGeneralData.SetProperty("U_InvHash", columnFind(einvDT, "InvoiceHash", 0));
                    oGeneralData.SetProperty("U_ICV", columnFind(einvDT, "ICV", 0));
                    oGeneralData.SetProperty("U_DeviceId", columnFind(einvDT, "DeviceId", 0));
                    oGeneralData.SetProperty("U_SellVat", columnFind(einvDT, "SellerVatNumber", 0));
                    oGeneralData.SetProperty("U_BuyVat", columnFind(einvDT, "BuyerVatNumber", 0));
                    oGeneralData.SetProperty("U_Status", columnFind(einvDT, "Status", 0));
                    oGeneralData.SetProperty("U_QrStat", columnFind(einvDT, "QrCodeStatus", 0));
                    oGeneralData.SetProperty("U_EINVStat", string.IsNullOrEmpty(columnFind(einvDT, "InvoiceStatus", 0)) ? "FAILED" : columnFind(einvDT, "InvoiceStatus", 0));
                    oGeneralData.SetProperty("U_INVTyp", string.IsNullOrEmpty(columnFind(einvDT, "InvoiceType", 0)) ? ObjType : columnFind(einvDT, "InvoiceType", 0));
                    oGeneralData.SetProperty("U_IssueDt", columnFind(einvDT, "IssueDate", 0));
                    oGeneralData.SetProperty("U_Issuetm", columnFind(einvDT, "IssueTime", 0));
                    oGeneralData.SetProperty("U_GenDt", columnFind(einvDT, "GeneratedDate", 0));
                    oGeneralData.SetProperty("U_Gentm", columnFind(einvDT, "GeneratedTime", 0));

                    string strlen = columnFind(einvDT, "InvoiceXml", 0);
                    int xmllen= strlen.Length;

                    var chunks = Enumerable.Range(0, (int)Math.Ceiling((double)strlen.Length / 250000))
                            .Select(i => strlen
                                .Skip(i * 250000)
                                .Take(250000));
                    int loop = 1;
                    foreach (var chunk in chunks)
                    {                        
                        oGeneralData.SetProperty("U_INVXml"+(loop==1?"":loop.ToString()), string.Join("", chunk));
                        loop += 1;
                    }

                    //if (xmllen > 250000)
                    //{
                    //    oGeneralData.SetProperty("U_INVXml", strlen.Substring(0, 250000));
                    //    oGeneralData.SetProperty("U_INVXml2", strlen.Substring(250000,strlen.Length-250000));
                    //}
                    //else
                    //{
                    //    oGeneralData.SetProperty("U_INVXml", strlen);
                    //}
                    
                    oGeneralData.SetProperty("U_WarnList", columnFind(einvDT, "WarningList", 0));
                    oGeneralData.SetProperty("U_ErrList", columnFind(einvDT, "ErrorList", 0));
                    oGeneralData.SetProperty("U_msg", columnFind(einvDT, "Message", 0));
                    oGeneralData.SetProperty("U_Valid", columnFind(einvDT, "ValidationsSuccess", 0));
                    oGeneralData.SetProperty("U_UniqID", columnFind(einvDT, "UniqueId", 0));
                    oGeneralData.SetProperty("U_UniqReqID", columnFind(einvDT, "UniqueReqIdentifier", 0));
                    oGeneralData.SetProperty("U_Id", columnFind(einvDT, "Id", 0));
                    oGeneralData.SetProperty("U_Vat", columnFind(einvDT, "Vat", 0));
                    oGeneralData.SetProperty("U_DocEntry", InvDocEntry);
                    oGeneralParams = oGeneralService.Add(oGeneralData);
                    
                  

                }

                objRs = null;
                return true;
            }
            catch (Exception ex)
            {
                clsModule.objaddon.objglobalmethods.WriteErrorLog(ex.ToString());
                clsModule.objaddon.objapplication.StatusBar.SetText("E_Invoice_Logs: " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                return false;
            }
        }

        private bool saveEinvfields(string DocEntry, DataTable einvDT, string TransType)
        {

            SAPbobsCOM.Documents objsalesinvoice =null;
            switch (TransType)
            {
                case "INV": 
                    objsalesinvoice = (SAPbobsCOM.Documents)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    break;
                case "CRN":
                    objsalesinvoice = (SAPbobsCOM.Documents)clsModule.objaddon.objcompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    break;
            }
            objsalesinvoice.GetByKey(Convert.ToInt32( DocEntry));
            objsalesinvoice.UserFields.Fields.Item("U_PIHNo").Value = columnFind(einvDT, "PIH", 0);
            objsalesinvoice.UserFields.Fields.Item("U_UUIDNo").Value = columnFind(einvDT, "UUID", 0);
            objsalesinvoice.UserFields.Fields.Item("U_InvoiceHashNo").Value = columnFind(einvDT, "InvoiceHash", 0);
            objsalesinvoice.UserFields.Fields.Item("U_EinvStatus").Value = (string.IsNullOrEmpty(columnFind(einvDT, "InvoiceStatus", 0)) ? "FAILED" : columnFind(einvDT, "InvoiceStatus", 0));
            objsalesinvoice.UserFields.Fields.Item("U_Issuedt").Value = columnFind(einvDT, "GeneratedDate", 0);
            objsalesinvoice.UserFields.Fields.Item("U_Warn").Value = columnFind(einvDT, "WarningList", 0);
            objsalesinvoice.UserFields.Fields.Item("U_Error").Value = columnFind(einvDT, "ErrorList", 0);
            objsalesinvoice.UserFields.Fields.Item("U_ICVNo").Value = columnFind(einvDT, "ICV", 0);
            objsalesinvoice.CreateQRCodeFrom = columnFind(einvDT, "RawQRCode", 0);
            objsalesinvoice.Update();


            return true;
        }

        private bool saveQrcode(string DocEntry, string qrcode, string TransType)
        {
            int objtype = 0;
            switch (TransType)
            {
                case "INV":
                    objtype = 13;
                    break;
                case "CRN":
                    objtype = 14;
                    break;
            }

            SAPbobsCOM.QRCodeService qRCodeService = (SAPbobsCOM.QRCodeService)clsModule.objaddon.objcompany.GetCompanyService().GetBusinessService(ServiceTypes.QRCodeService);
            SAPbobsCOM.QRCodeData qRCodeData = (SAPbobsCOM.QRCodeData)qRCodeService.GetDataInterface(QRCodeServiceDataInterfaces.qrcsQRCodeData);
            qRCodeData.QRCodeText = qrcode;
            qRCodeData.ObjectAbsEntry = DocEntry;
            qRCodeData.ObjectType = objtype;
            qRCodeData.FieldName = "QRCodeSrc";
            qRCodeService.AddOrUpdateQRCode(qRCodeData);
            return true;
        }

        private DataTable Get_API_Response(string JSON, string URL, string httpMethod = "POST", string contenttype = "application/json",
           Dictionary<string, string> headers = null, NameValueCollection formdata1 = null, string pdfpath = "")
        {
            try
            {
                clsModule.objaddon.objglobalmethods.WriteErrorLog(URL);
                clsModule.objaddon.objglobalmethods.WriteErrorLog(JSON);

                DataTable datatable = new DataTable();
                HttpWebRequest webRequest;
                webRequest = (HttpWebRequest)WebRequest.Create(URL);
                webRequest.Method = httpMethod;
                byte[] byteArray = new byte[] { };
                if (!string.IsNullOrEmpty(JSON))
                {
                    webRequest.ContentType = contenttype;
                    byteArray = Encoding.UTF8.GetBytes(JSON);
                    webRequest.ContentLength = byteArray.Length;
                }
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        webRequest.Headers.Add(item.Key, item.Value);
                    }
                }

                if (formdata1 != null)
                {
                    string boundary = "----" + Guid.NewGuid().ToString("N");

                    string formDataString = clsModule.objaddon.objglobalmethods.BuildFormData(formdata1, boundary);


                    byte[] formDataBytes = Encoding.UTF8.GetBytes(formDataString);
                    webRequest.ContentType = contenttype + "; boundary=" + boundary;

                    webRequest.ContentLength = formDataBytes.Length;
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(formDataBytes, 0, formDataBytes.Length);
                    }

                }
                else
                {
                    if (byteArray.Length != 0)
                    {
                        webRequest.ContentType = contenttype;
                        using (Stream requestStream = webRequest.GetRequestStream())
                        {
                            requestStream.Write(byteArray, 0, byteArray.Length);
                        }
                    }
                }

                try
                {
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        if (response is HttpWebResponse httpResponse)
                        {
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                switch (httpResponse.ContentType)
                                {
                                    case "application/pdf":

                                        using (Stream responseStream = response.GetResponseStream())
                                        {
                                            if (responseStream != null)
                                            {
                                                string outputPath = pdfpath;
                                                if (File.Exists(outputPath))
                                                    File.Delete(outputPath);

                                                using (FileStream fileStream =  new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                                                {
                                                    byte[] buffer = new byte[4096];
                                                    int bytesRead;

                                                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                                    {
                                                        fileStream.Write(buffer, 0, bytesRead);
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    default:
                                        {
                                            using (Stream responseStream = response.GetResponseStream())
                                            {
                                                StreamReader rdr = new StreamReader(responseStream, Encoding.UTF8);
                                                string Json = rdr.ReadToEnd();
                                                clsModule.objaddon.objglobalmethods.WriteErrorLog(Json);
                                                datatable = clsModule.objaddon.objglobalmethods.Jsontodt(Json);
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                    }

                }
                catch (WebException webEx)
                {
                    if (webEx.Response is HttpWebResponse httpWebResponse)
                    {
                        if (httpWebResponse.StatusCode == HttpStatusCode.BadRequest)
                        {
                            using (Stream errorResponseStream = httpWebResponse.GetResponseStream())
                            {
                                StreamReader rdr = new StreamReader(errorResponseStream, Encoding.UTF8);
                                string Json = rdr.ReadToEnd();
                                clsModule.objaddon.objglobalmethods.WriteErrorLog(Json);
                                datatable = clsModule.objaddon.objglobalmethods.Jsontodt(Json);
                            }
                        }
                    }
                }



                return datatable;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private string Getbasepath()
        {
            string path;
            string lstrquery;
            path = clsModule.objaddon.objglobalmethods.getSingleValue("Select \"AttachPath\" from OADP");
            lstrquery = "SELECT CAST(t2.\"AttachPath\" AS nvarchar) AS \"Apath\"  FROM OUSR t1 LEFT JOIN OUDG t2 ON t1.\"DfltsGroup\" = t2.\"Code\" WHERE USER_CODE = '" + clsModule.objaddon.objcompany.UserName + "' ";
            path += clsModule.objaddon.objglobalmethods.getSingleValue(lstrquery);

            return path;
        }
        public void buttonenable(SAPbouiCOM.Form oForm)
        {
            try
            {


                SAPbouiCOM.Form oUDFForms;
                SAPbouiCOM.Button button = null;
                string Einvsts;
                string status;
                string DocEntry;
                string user;
                string tablename = "";
                switch (oForm.Type.ToString())
                {
                    case "133":
                        tablename = "OINV";

                        break;
                    case "179":
                        tablename = "ORIN";
                        break;
                    default:
                        return;
                }
                
                button = (SAPbouiCOM.Button)oForm.Items.Item("btneinv").Specific;
                EnabledMenu(oForm);
                Einvsts = oForm.DataSources.DBDataSources.Item(tablename).GetValue("U_EinvStatus", 0); 
                status = oForm.DataSources.DBDataSources.Item(tablename).GetValue("DocStatus", 0);
                DocEntry = oForm.DataSources.DBDataSources.Item(tablename).GetValue("DocEntry", 0);
                user = oForm.DataSources.DBDataSources.Item(tablename).GetValue("Usersign", 0);

              

                if (string.IsNullOrEmpty(DocEntry))
                {
                    button.Item.Enabled = true;
                    return;
                }
                string Docuser = "";
                string expectuser = clsModule.objaddon.objglobalmethods.getSingleValue("SELECT Max(CAST(\"U_ExpctUser\" AS nvarchar)) AS \"ExpctUser\" FROM \"@EICON\" e;");
                Docuser = oForm.DataSources.DBDataSources.Item(tablename).GetValue("Usersign", 0);
                if (!string.IsNullOrEmpty(Docuser))
                {
                    user = clsModule.objaddon.objglobalmethods.getSingleValue(" SELECT \"USER_CODE\"  FROM OUSR o WHERE o.USERID = " + Docuser);
                    List<string> outputList = new List<string>(expectuser.Split(','));

                    foreach (string item in outputList)
                    {
                        string repl = item.Replace("'", "");
                        if (user == repl)
                        {
                            button.Item.Enabled = false;
                            return;
                        }

                    }
                }

                if (status == "C")
                {
                    string Confset = clsModule.objaddon.objglobalmethods.getSingleValue("SELECT  \"U_CloseInv\" FROM \"@EICON\" e WHERE \"Code\" = '01'");
                    if (Confset == "False")
                    {
                        button.Item.Enabled = false;
                        return;
                    }
                }
                string cancel = oForm.DataSources.DBDataSources.Item(tablename).GetValue("CANCELED", 0);

                if (cancel == "Y" || cancel =="C") //N
                {
                    button.Item.Enabled = false;
                    return;
                }
                if (string.IsNullOrEmpty(DocEntry))
                {
                    button.Item.Enabled = true;
                    return;
                }

                switch (Einvsts)
                {
                    case "CLEARED":
                    case "REPORTED":
                        Einvsts = "CLEARED";
                        break;
                }

                if (string.IsNullOrEmpty(Einvsts))
                {
                    button.Item.Enabled = true;
                    return;
                }

                else if (Einvsts!="CLEARED")
                {
                    button.Item.Enabled =true;
                    return;
                }

              

                List<string> Checkdoc = new List<string>();
                List<string> savedoc = new List<string>();
                string strsql;
                DataTable dt = new DataTable();


                strsql = "select \"DocNum\",\"DocDate\" from  " + tablename + " where \"DocEntry\"=" + DocEntry;
                dt = clsModule.objaddon.objglobalmethods.GetmultipleValue(strsql);

                if (dt.Rows.Count>0)
                {

                    DateTime stdt;
                    DateTime docdt;

                    string stdate = clsModule.objaddon.objglobalmethods.getSingleValue("SELECT  \"U_Startdate\" FROM \"@EICON\" e WHERE \"Code\" = '01'");

                    DateTime.TryParseExact(stdate, CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns(), CultureInfo.InvariantCulture, DateTimeStyles.None, out stdt);
                    DateTime.TryParseExact(clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(Convert.ToString(dt.Rows[0]["DocDate"])), "dd/MM/yyyy", "yyyy-MM-dd"), CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns(), CultureInfo.InvariantCulture, DateTimeStyles.None, out docdt);
                    if (!string.IsNullOrEmpty(stdate))
                    {
                        if (!(docdt >= stdt))
                        {
                            button.Item.Enabled = false;
                            return;
                        }
                    }

                }

                if (dt.Rows.Count > 0)
                {
                    string BaseSysPath = Getbasepath();
                    string SysPath = BaseSysPath + Convert.ToString(dt.Rows[0]["DocNum"]) + "_";
                    SysPath += clsModule.objaddon.objglobalmethods.DateFormat(clsModule.objaddon.objglobalmethods.Getdateformat(Convert.ToString(dt.Rows[0]["DocDate"])), "dd/MM/yyyy", "yyyy-MM-dd");
                    Checkdoc.Add(SysPath + "_PDF.pdf");
                    Checkdoc.Add(SysPath + "_PDFA.pdf");
                    Checkdoc.Add(SysPath + "_XML.XMl");
                }

                strsql = "SELECT CAST(T1.\"trgtPath\" AS varchar)AS \"Trgtpath\",CAST(T1.\"FileName\" AS varchar) AS \"Filename\"," +
                     " CAST(T1.\"FileExt\" AS varchar) AS \"FileExt\"  FROM " + tablename + " T0 left join ATC1 T1 on T0.\"AtcEntry\" = T1.\"AbsEntry\" Where T0.\"DocEntry\" =" + DocEntry;
                dt = clsModule.objaddon.objglobalmethods.GetmultipleValue(strsql);
                foreach (DataRow path in dt.Rows)
                {
                    string FileName = path["Trgtpath"].ToString() + "\\" + path["Filename"].ToString() + "." + path["FileExt"].ToString();
                    savedoc.Add(Path.GetFileName(FileName));
                }
                bool notfound = false;
                foreach (string item in Checkdoc)
                {
                    string checkfileName = Path.GetFileName(item);
                 

                    if (savedoc.IndexOf(checkfileName) == -1)
                    {
                        notfound = true;
                        break;
                    }
                }

                if (status == "C" && !notfound)
                {
                    button.Item.Enabled = false;
                    return;
                }

               

                button.Item.Enabled = notfound;


            }

            catch (Exception ex)
            {
                return;
            }
        }

    }
}
