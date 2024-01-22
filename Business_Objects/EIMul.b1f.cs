using EInvoice.Common;
using SAPbouiCOM.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static EInvoice.Common.clsGlobalMethods;

namespace EInvoice.Business_Objects
{
    [FormAttribute("EINVMUL", "Business_Objects/EIMul.b1f")]
    public class EIMul : UserFormBase
    {
        public static SAPbouiCOM.Form objform;
        private clsGlobalMethods stf = new clsGlobalMethods();
        private SAPbouiCOM.ProgressBar obar;
        private string ComboQr = "";
        public EIMul()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("Item_0").Specific));
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_1").Specific));
            this.ComboBox0 = ((SAPbouiCOM.ComboBox)(this.GetItem("Item_2").Specific));
            this.StaticText1 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_3").Specific));
            this.EditText1 = ((SAPbouiCOM.EditText)(this.GetItem("Item_4").Specific));
            this.StaticText2 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_5").Specific));
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("Item_8").Specific));
            this.Button0.ClickAfter += new SAPbouiCOM._IButtonEvents_ClickAfterEventHandler(this.Button0_ClickAfter);
            this.Button0.ClickBefore += new SAPbouiCOM._IButtonEvents_ClickBeforeEventHandler(this.Button0_ClickBefore);
            this.Grid1 = ((SAPbouiCOM.Grid)(this.GetItem("grd").Specific));
            this.Grid1.LinkPressedBefore += new SAPbouiCOM._IGridEvents_LinkPressedBeforeEventHandler(this.Grid1_LinkPressedBefore);
            this.Button1 = ((SAPbouiCOM.Button)(this.GetItem("Item_10").Specific));
            this.Button1.ClickBefore += new SAPbouiCOM._IButtonEvents_ClickBeforeEventHandler(this.Button1_ClickBefore);
            this.Button1.ClickAfter += new SAPbouiCOM._IButtonEvents_ClickAfterEventHandler(this.Button1_ClickAfter);
            this.Button2 = ((SAPbouiCOM.Button)(this.GetItem("Item_6").Specific));
            this.Button2.ClickAfter += new SAPbouiCOM._IButtonEvents_ClickAfterEventHandler(this.Button2_ClickAfter);
            this.Button2.ClickBefore += new SAPbouiCOM._IButtonEvents_ClickBeforeEventHandler(this.Button2_ClickBefore);
            this.StaticText3 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_7").Specific));
            this.ComboBox1 = ((SAPbouiCOM.ComboBox)(this.GetItem("sts").Specific));
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.LoadAfter += new LoadAfterHandler(this.Form_LoadAfter);

        }

        private SAPbouiCOM.EditText EditText0;

        private void OnCustomInitialize()
        {
          string  strSQL = clsModule.objaddon.objglobalmethods.getSingleValue("Select \"U_Genmulstus\" from \"@EICON\" where \"Code\"='01'");
            if (strSQL != "True")
            {
                clsModule.objaddon.objglobalmethods.Load_Combo(ComboBox1, "", new[] { "NOT_SENT,NOT_SENT"});
            }
            else
            {
                ComboQr = "SELECT DISTINCT  \"U_EINVStat\" as \"col1\" ,\"U_EINVStat\"  as \"col2\" FROM \"@EILOG\" e WHERE  \"U_EINVStat\" not in( 'CLEARED','REPORTED','') order by   \"col1\" ";
                clsModule.objaddon.objglobalmethods.Load_Combo(ComboBox1, ComboQr, new[] { "NOT_SENT,NOT_SENT", "ALL,ALL" });
            }
            ComboBox0.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            ComboBox1.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            EditText1.Value = DateTime.Today.ToString("yyyyMMdd");
            EditText0.Value = DateTime.Today.ToString("yyyyMMdd");
           // loaddata();
        }

        private SAPbouiCOM.StaticText StaticText0;
        private SAPbouiCOM.ComboBox ComboBox0;
        private SAPbouiCOM.StaticText StaticText1;
        private SAPbouiCOM.EditText EditText1;
        private SAPbouiCOM.StaticText StaticText2;
        private SAPbouiCOM.Button Button0;
        private SAPbouiCOM.Grid Grid1;
        private SAPbouiCOM.Button Button1;

        private void Button0_ClickBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

        }
        private void loaddata(bool fromDisplay = false)
        {
            List<string> listsd = new List<string>();
            string lstrquery = "";
            DataTable dt = new DataTable();
            string sts = "";
            SAPbobsCOM.Recordset rs;
            switch (ComboBox1.Selected.Value)
            {
                case "All":
                    sts = "'','FAILED','CLEARED','REPORTED','NOT_CLEARED'";
                    break;

                case "Failed":
                    sts = "'FAILED','NOT_CLEARED'";
                    break;

                case "Not Generated":
                    sts = "''";
                    break;
                case "Cleared":
                    sts = "'CLEARED','REPORTED'";
                    break;
            }

            if (!string.IsNullOrEmpty(ComboBox1.Value.ToString()))
            {
                switch (ComboBox1.Value.ToString())
                {
                    case "ALL":                        
                        rs = clsModule.objaddon.objglobalmethods.GetmultipleRS(ComboQr);
                        for (int i = 0; i < rs.RecordCount; i++)
                        {
                            sts +="'"+ rs.Fields.Item("col1").Value+"',";

                            rs.MoveNext();
                        }
                        sts += "''";
                        break;
                    case "NOT_SENT":
                        sts = "''"; ;
                        break;
                    default:
                        sts = "'" + ComboBox1.Value.ToString() + "'";
                        break;
                }
            }
            switch (ComboBox0.Selected.Value)
            {
                case "ALL":
                    listsd.Add("INV");
                    listsd.Add("CRN");
                    listsd.Add("DBN");
                    break;
                case "INV":
                    listsd.Add("INV");
                    break;
                case "DBN":
                    listsd.Add("DBN");
                    break;                    
                case "CRN":
                    listsd.Add("CRN");
                    break;
            }
            try
            {
                int inc = 0;


                string series = "";
                 lstrquery = "SELECT o2.\"U_Prefix\"  FROM OUSR o LEFT JOIN OUBR o2 ON o2.\"Code\" = o.\"Branch\" WHERE o.USER_CODE = '" + clsModule.objaddon.objcompany.UserName + "'  AND o2.\"U_Z_FrgnName\" ='Y' ";
                rs = clsModule.objaddon.objglobalmethods.GetmultipleRS(lstrquery);
                if (rs.RecordCount > 0)
                {

                    series = rs.Fields.Item("U_Prefix").Value.ToString();
                }
                lstrquery = "";
                foreach (string item in listsd)
                {
                    if (inc!=0)
                    {
                        lstrquery += " Union  all ";
                    }
                    //Grid1.DataTable.SetValue("", i, "N");
                    //Grid1.DataTable.SetValue("", i, rs.Fields.Item("DocNum").Value.ToString());
                    //Grid1.DataTable.SetValue("", i, rs.Fields.Item("DocEntry").Value.ToString());
                    //Grid1.DataTable.SetValue("", i, rs.Fields.Item("DocDate").Value.ToString());
                    //Grid1.DataTable.SetValue("Customer", i, rs.Fields.Item("CardName").Value.ToString());
                    //Grid1.DataTable.SetValue("", i, rs.Fields.Item("DocTotal").Value.ToString());
                    //Grid1.DataTable.SetValue("Status", i, rs.Fields.Item("status").Value.ToString());
                    //Grid1.DataTable.SetValue("Warning", i, rs.Fields.Item("warn").Value.ToString());
                    //Grid1.DataTable.SetValue("Error", i, rs.Fields.Item("Error").Value.ToString());
                    //Grid1.DataTable.SetValue("Type", i, rs.Fields.Item("ttype").Value.ToString());

                    lstrquery += "SELECT 'N' as \"Checkbox\" ,t1.\"DocEntry\" as \"DocEntry\",t1.\"DocNum\" as \"Doc Number\"," +
                        " t1.\"DocDate\" as \"Doc Date\",t1.\"CardName\" as \"Customer\" ,t1.\"DocTotal\" as \"Total\",  ";
                    lstrquery += " COALESCE(\"U_EinvStatus\",'Not Generated') as \"status\" ,\"U_Warn\"  as \"Warning\",\"U_Error\" as \"Error\", ";
                    lstrquery += "'"+item + "' as \"Type\" ";
                    lstrquery += "from  ";
                    switch (item)
                    {
                        case "INV":
                        case "DBN":
                            lstrquery += "oinv t1 ";
                            break;
                        case "CRN":
                            lstrquery += "ORIN t1 ";
                            break;
                    }
                    

                    lstrquery += @" LEFT JOIN OUSR o ON o.USERID =t1.""UserSign"" ";
                    lstrquery += @" WHERE T1.""DocDate"">='" + EditText0.Value + "'";
                    lstrquery += @" And t1.""DocDate"" <='" + EditText1.Value + "'";
                    if (!fromDisplay)
                    {
                        lstrquery += @" AND t1.""DocStatus"" ='O'";
                    }
                    if (!string.IsNullOrEmpty(clsModule.objaddon.objglobalmethods.getSingleValue("SELECT Max(CAST(\"U_ExpctUser\" AS nvarchar)) AS \"ExpctUser\" FROM \"@EICON\" e;")))
                    {
                        lstrquery += @" AND o.USER_CODE NOT in(" + clsModule.objaddon.objglobalmethods.getSingleValue("SELECT Max(CAST(\"U_ExpctUser\" AS nvarchar)) AS \"ExpctUser\" FROM \"@EICON\" e;") + ") ";
                    }

                    if (!string.IsNullOrEmpty(series))

                    {

                      

                        lstrquery += @" AND t1.""Series""  in(" + "SELECT \"Series\" FROM nnm1 WHERE \"SeriesName\" ='" + series + "' AND \"ObjectCode\"   in('14','13')" + ")";
                    }

                  
                    switch (item)
                    {
                        case "INV":
                            lstrquery += " and t1.\"DocType\" <>'S' and COALESCE(t1.\"U_EinvStatus\",'')in (" + (fromDisplay ? "'CLEARED'" : sts) +")";
                            break;
                        case "DBN":
                            lstrquery += " and t1.\"DocType\" ='S'and COALESCE(t1.\"U_EinvStatus\",'')in (" + (fromDisplay ? "'CLEARED'" : sts) + ")";
                            break;
                        case "CRN":
                            lstrquery += " and t1.\"DocType\" <>'S' and COALESCE(t1.\"U_EinvStatus\",'')in( " + (fromDisplay ? "'CLEARED'" : sts) + ")";
                            break;
                    }

                                  
                    inc++;
                }
                lstrquery += @" Order by ""Doc Date""";
                if (!clsModule.HANA)
                {
                    lstrquery = clsModule.objaddon.objglobalmethods.ChangeHANAtoSql(lstrquery);
                }
              //  dt = clsModule.objaddon.objglobalmethods.GetmultipleValue(lstrquery);
                objform.Freeze(true);
               
                Grid1= (SAPbouiCOM.Grid)objform.Items.Item("grd").Specific;
                //  rs= clsModule.objaddon.objglobalmethods.GetmultipleRS(lstrquery);
                //if (rs.RecordCount > 0)
                // {
                if (obar != null)
                    obar.Stop();

                obar = clsModule.objaddon.objapplication.StatusBar.CreateProgressBar("Loading Please Wait", rs.RecordCount, true);
                clsModule.objaddon.objapplication.StatusBar.SetText("Preparing Data..", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
            
                Grid1.DataTable.ExecuteQuery(lstrquery);

                //Grid1.DataTable.Rows.Add(rs.RecordCount);
                //for (int i = 0; i < rs.RecordCount; i++)
                //{
               

                    //for (int i = 0; i < Grid1.Rows.Count; i++)
                    //{

                    //    this.Grid1.RowHeaders.SetText(i, (i + 1).ToString());

                    //    // Grid1.DataTable.SetValue("Checkbox", i, "N");
                    //    //Grid1.DataTable.SetValue("Doc Number", i, rs.Fields.Item("DocNum").Value.ToString());
                    //    //Grid1.DataTable.SetValue("DocEntry", i, rs.Fields.Item("DocEntry").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Doc Date", i, rs.Fields.Item("DocDate").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Customer", i, rs.Fields.Item("CardName").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Total", i, rs.Fields.Item("DocTotal").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Status", i, rs.Fields.Item("status").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Warning", i, rs.Fields.Item("warn").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Error", i, rs.Fields.Item("Error").Value.ToString());
                    //    //Grid1.DataTable.SetValue("Type", i, rs.Fields.Item("ttype").Value.ToString());
                    //    switch (Grid1.DataTable.GetValue("status", i).ToString())
                    //    {
                    //        case "FAILED":
                    //        case "CLEARED":
                    //        case "REPORTED":
                    //        case "NOT_CLEARED":
                    //            Grid1.CommonSetting.SetRowEditable(i + 1, false);
                    //            break;

                    //        default:
                    //            Grid1.CommonSetting.SetRowEditable(i + 1, true);
                    //            break;
                    //    }

                    //    obar.Value += 1;
                    //    // rs.MoveNext();
                    //}
                
                    SAPbouiCOM.EditTextColumn oColumns;
                    oColumns = (SAPbouiCOM.EditTextColumn)Grid1.Columns.Item("DocEntry");
                    oColumns.LinkedObjectType = "13";
                    oColumns = (SAPbouiCOM.EditTextColumn)Grid1.Columns.Item("Checkbox");
                    oColumns.Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox;
                Colsetting();
                clsModule.objaddon.objapplication.StatusBar.SetText("Opereation Completed Successfully...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //  }                                   
            }
            catch (Exception ex)
            {

                return;
            }
            finally
            {
                objform.Freeze(false);
                if (obar!=null)
                    obar.Stop();
              
            }
        }
      
        private void Button0_ClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            loaddata();


        }

        private void Form_LoadAfter(SAPbouiCOM.SBOItemEventArg pVal)
        {

            objform = clsModule.objaddon.objapplication.Forms.GetForm("EINVMUL", pVal.FormTypeCount);

        }

        private void Button1_ClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            try
            {


                objform.Freeze(true);
                if (Grid1.Rows.Count > 0)
                {



                    string xmlString = Grid1.DataTable.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_All);
                    XDocument xdoc = XDocument.Parse(xmlString);
                 


                    var filteredData = xdoc.Descendants("Row")
          .Select((row, index) => new
          {
             index=index+1,
              ColumnUid = row.Element("Cells").Element("Cell").Element("ColumnUid").Value,
              Value = row.Element("Cells").Element("Cell").Element("Value").Value
          })
          .Where(cell => cell.ColumnUid == "Checkbox" && cell.Value=="Y")
          .ToList();


                    for (int filrow = 0; filrow < filteredData.Count; filrow++)
                    {
                        int i=(filteredData[filrow].index)-1;

                        {
                            string lstrdocentry = Grid1.DataTable.Columns.Item("DocEntry").Cells.Item(i).Value.ToString();
                            string lstrcheckbox = Grid1.DataTable.Columns.Item("Checkbox").Cells.Item(i).Value.ToString();
                            string trnasType = "";

                            switch (Grid1.DataTable.Columns.Item("Type").Cells.Item(i).Value.ToString())
                            {
                                case "INV":
                                case "DBN":
                                    trnasType = "INV";
                                    break;
                                case "CRN":
                                    trnasType = "CRN";
                                    break;
                            }
                            DataTable dt = new DataTable();

                            clsModule.objaddon.objapplication.StatusBar.SetText("Progress...."+lstrdocentry , SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                            if (lstrcheckbox == "Y")
                            {


                                clsModule.objaddon.objInvoice.Generate_Cancel_IRN(ClsARInvoice.EinvoiceMethod.CreateIRN, lstrdocentry, trnasType, "E-Invoice", ref dt, true);
                                Grid1.DataTable.SetValue("status", i, columnFind(dt, "InvoiceStatus", 0));
                                Grid1.DataTable.SetValue("Warning", i, columnFind(dt, "WarningList", 0));
                                Grid1.DataTable.SetValue("Error", i, columnFind(dt, "ErrorList", 0));
                                Grid1.DataTable.SetValue("Checkbox", i, "N");
                                Grid1.CommonSetting.SetRowEditable(i + 1, false);

                            }

                        }

                    }

                    #region "com"
                    //for (int i = 0; i < Grid1.Rows.Count; i++)
                    //{
                    //    string lstrdocentry = Grid1.DataTable.Columns.Item("DocEntry").Cells.Item(i).Value.ToString();
                    //    string lstrcheckbox = Grid1.DataTable.Columns.Item("Checkbox").Cells.Item(i).Value.ToString();
                    //    string trnasType ="" ;

                    //    switch (Grid1.DataTable.Columns.Item("Type").Cells.Item(i).Value.ToString())
                    //    {
                    //        case "INV":
                    //        case "DBN":
                    //            trnasType = "INV";
                    //            break;
                    //        case "CRN":
                    //            trnasType = "CRN";
                    //            break;
                    //    }
                    //    DataTable dt = new DataTable();                        
                    //    clsModule.objaddon.objapplication.StatusBar.SetText("Progress...." + (i+1) + "/" + Grid1.Rows.Count, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);

                    //    if (lstrcheckbox == "Y")
                    //    {


                    //        clsModule.objaddon.objInvoice.Generate_Cancel_IRN(ClsARInvoice.EinvoiceMethod.CreateIRN, lstrdocentry, trnasType, "E-Invoice", ref dt, true);
                    //        Grid1.DataTable.SetValue("status", i, columnFind(dt, "InvoiceStatus", 0));
                    //        Grid1.DataTable.SetValue("Warning", i, columnFind(dt, "WarningList", 0));
                    //        Grid1.DataTable.SetValue("Error", i, columnFind(dt, "ErrorList", 0));
                    //        Grid1.DataTable.SetValue("Checkbox", i, "N");
                    //        Grid1.CommonSetting.SetRowEditable(i+1, false);

                    //    }

                    //}
                    //   loaddata(true);
                    #endregion "com"
                    clsModule.objaddon.objapplication.StatusBar.SetText("Operation Completed successfully....", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                    objform.Items.Item("Item_0").Click();
                }
            }
            catch (Exception ex)
            {

                return;
            }
            finally
            {
                objform.Freeze(false);
            }

        }

        private SAPbouiCOM.Button Button2;
        private void Colsetting()
        {
            for (int i = 0; i < this.Grid1.Columns.Count; i++)
            {
                this.Grid1.Columns.Item(i).TitleObject.Sortable = true;
               
                Grid1.Columns.Item(i).Editable = true;
            }
            this.Grid1.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single;
            Grid1.AutoResizeColumns();
        }
        private void Button2_ClickBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;

        }

        private void Button2_ClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {

            loaddata(true);
        }

        private SAPbouiCOM.StaticText StaticText3;
        private SAPbouiCOM.ComboBox ComboBox1;

        private void Grid1_LinkPressedBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {
           
            BubbleEvent = true;
            SAPbouiCOM.EditTextColumn oColumns;
            oColumns = (SAPbouiCOM.EditTextColumn)Grid1.Columns.Item("DocEntry");
            string lstrtype = Grid1.DataTable.Columns.Item("Type").Cells.Item(pVal.Row).Value.ToString();
            switch (lstrtype)
            {                          
                case "CRN":
                    oColumns.LinkedObjectType ="14";
                    
                    break;
                default:
                    oColumns.LinkedObjectType = "13";
                    break;
            }

        }

        private void Button1_ClickBefore(object sboObject, SAPbouiCOM.SBOItemEventArg pVal, out bool BubbleEvent)
        {            
            BubbleEvent = true;
            bool checkvalue = false; 
            Grid1.Columns.Item("Checkbox").Visible = true;


            string xmlString = Grid1.DataTable.SerializeAsXML(SAPbouiCOM.BoDataTableXmlSelect.dxs_All);
            XDocument xdoc = XDocument.Parse(xmlString);



            var filteredData = xdoc.Descendants("Row")
  .Select((row, index) => new
  {
      index = index + 1,
      ColumnUid = row.Element("Cells").Element("Cell").Element("ColumnUid").Value,
      Value = row.Element("Cells").Element("Cell").Element("Value").Value
  })
  .Where(cell => cell.ColumnUid == "Checkbox" && cell.Value == "Y")
  .ToList();

            if (filteredData.Count>0)
            {
                checkvalue = true;
            }

            if (!checkvalue)
            {
                Application.SBO_Application.SetStatusBarMessage("Please Select Checkbox !!!!", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                BubbleEvent = false;
            }

        }
    }
}
