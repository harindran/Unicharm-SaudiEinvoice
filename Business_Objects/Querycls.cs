using EInvoice.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EInvoice.Business_Objects
{
    public class Querycls
    {
        public int HSNLength = 4;
        public int Round = 2;
        
        

        public string docseries = "doc.\"DocNum\" ";
        public string InvoiceQuery(string Docentry)
        {
            string retstring = "";

         

            retstring = " WITH Tottb AS ( ";
            retstring += " SELECT \"DocEntry\",sum(\"Totgross\") AS \"Totgross\",sum(\"Tottax\") AS \"Tottax\" ,sum(\"Totrndnet\") AS \"Totrndnet\",sum(\"Totnet\") AS \"Totnet\"  from ( ";
            retstring += " SELECT DOC.\"DocEntry\",";
            retstring += " sum(Round(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END)," + Round + ")) \"Totgross\",";
            retstring += " sum(Round((itm.\"VatSumSy\")," + Round + ")) AS \"Tottax\", ";
            retstring += " sum(Round(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END)," + Round + ")) " +
                " + round( sum(Round(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END)," + Round + ") " +
                " * (tax.\"Rate\" /100))," + Round + ") AS \"Totrndnet\", ";    
            retstring += " Round(sum(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END) + " +
                         " itm.\"VatSumSy\"), "+Round +") AS \"Totnet\" ";
            retstring += " FROM OINV DOC ";
            retstring += " LEFT JOIN INV1 itm ON itm.\"DocEntry\" =DOC.\"DocEntry\" ";
            retstring += " LEFT JOIN OVTG tax ON tax.\"Code\" = itm.\"VatGroup\" ";
            retstring += " GROUP BY DOC.\"DocEntry\",tax.\"Rate\" ) GROUP BY \"DocEntry\"), ";

            retstring += " TaxCat AS (";
            retstring += " SELECT t1.\"Code\" ,Max(t1.\"U_CatCode\") AS \"TaxCode\", max(t3.\"Code\") AS \"Reasoncode\",Max(t3.\"Name\") AS \"Reason\"  FROM \"@TAXCAT\" t1  ";
            retstring += " left JOIN \"@TAXCAT1\" t2 ON t1.\"Code\" =t2.\"Code\" ";
            retstring += " left JOIN \"@TAXRSN\" t3 ON t2.\"U_Reason\"  =t3.\"Code\"";
            retstring += " GROUP BY t1.\"Code\" )";

            retstring += " SELECT ";
            
            retstring += " case WHEN nnm1.\"SeriesName\" in (SELECT Replace(Max(CAST(\"U_Expctseries\" AS varchar)),'''','')   FROM \"@EICON\" e WHERE \"Code\" ='01') THEN '388'  when Doc.\"DocType\"='S' then '383' else '388' End as \"TaxType\", ";
            retstring += " case WHEN nnm1.\"SeriesName\" in (SELECT Replace(Max(CAST(\"U_Expctseries\" AS varchar)),'''','')   FROM \"@EICON\" e WHERE \"Code\" ='01') THEN 'I'  when DOC.\"DocType\"='S' then 'S' else 'I' End as \"DocType\", ";

            retstring += docseries + " \"DocNum\",";

            retstring += " doc.\"DocDate\" ,doc.\"DocTime\" ,doc.\"NumAtCard\" , ";        
            retstring += " doc.\"DocCur\",  CASE WHEN BPM.\"U_EType\"=1 THEN '0100000' WHEN \"U_EType\"=2 THEN '0200000' WHEN \"U_EType\"=3 THEN '0100100' ELSE '' END AS \"U_EType\",BPM.\"U_IDType\" , ";
            retstring += " BPM.\"CardName\" ,BPM.\"LicTradNum\"  ,BPM.\"AddID\", ";
            retstring += " CMP.\"CompnyName\" ,'CRN' as \"CmpId\",CMP.\"TaxIdNum\", CMP.\"TaxIdNum2\",CMPADD.\"Street\", cmp.\"SysCurrncy\", ";
            retstring += " CMPADD.\"StreetNo\" ,CMPADD.\"Building\" ,CMPADD.\"City\" ,CMPADD.\"County\" ,CMPADD.\"ZipCode\" , ";
            retstring += " CMPADD.\"Country\" \"CodeCountry\" ,CmpST.\"Name\" \"State\" ,CmpCY.\"Name\"  \"Country\",  ";


            retstring += " BUYADDR.\"StreetNoB\",BUYADDR.\"StreetB\" ,BUYADDR.\"BuildingB\",BUYADDR.\"CityB\" ,BUYADDR.\"CountyB\", BUYADDR.\"ZipCodeB\" ,";
            retstring += " buyaddr.\"CountryB\" \"CodeCountryB\" ,BuyST.\"Name\" \"StateB\" ,BuyCY.\"Name\"  \"CountryB\" ,";

            retstring += " itm.\"LineNum\"+1 \"LineNum\" ,itm.\"Dscription\",itm.\"Currency\",itm.\"SubCatNum\" as \"ItemBuyerID\" ,itm.\"ItemCode\" as \"ItemsellerID\", ";           
            retstring += " tax.\"Rate\" AS \"Taxrate\", ";

          //  retstring += " case when tax.\"Rate\"=0 then 'Z' else 'S' End AS \"TaxCat\", ";//need udf
            // retstring += " case when tax.\"Rate\"=0 then 'Zero Tax Product' else '' End AS \"TaxReason\", ";//need udf


            retstring += "TaxCat.\"TaxCode\" AS \"TaxCat\",";
            retstring += "TaxCat.\"Reason\" AS \"Reason\",";


            retstring += " Round(itm.\"VatSumSy\"," + Round + ") AS \"Taxamt\" , ";
            retstring += " itm.\"PriceBefDi\" AS \"BaseAmt\", ";
            retstring += " itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100 ) AS \"DiscAmt\" , ";
            retstring += " itm.\"PriceBefDi\" -cast(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100) as Decimal(20,6)) as \"PriceAmt\",  ";
            retstring += " Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" end as \"Quantity\",";
            retstring += " case when itm.\"UomCode\"='Manual' then 'E48' else itm.\"UomCode\" end as \"UomCode\",  ";
            retstring += " Round((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END," + Round + ") AS \"Gross\", ";
            retstring += " Round((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END," + Round + ")+ Round(itm.\"VatSumSy\" ," + Round + ")  AS \"Linenet\" , ";
            retstring += " Round(itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" * (itm.\"DiscPrcnt\" /100))- Case when DOC.\"DocType\"='S' then itm.\"Price\" else itm.\"INMPrice\" end  ," + Round + ") as \"LineAllow\", ";
            retstring += " ROUND(Doc.\"DiscSum\"," + Round + ") AS \"Allownace\", ";
            retstring += " Round(Tottb.\"Totgross\"," + Round + ") as \"Totgross\" ,";            
            retstring += " Round(Tottb.\"Totgross\"-Doc.\"DiscSum\"," + Round + ") as \"TaxExclusive\", ";

            retstring += " Round(Tottb.\"Tottax\"," + Round + ") as \"Tottax\",Round(Tottb.\"Totrndnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ") as \"Totnet\" , ";
            retstring += " Round(Tottb.\"Totnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ") as \"Totnet1\" , ";
            retstring += " (Round(Tottb.\"Totnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ")) - (Round(Tottb.\"Totrndnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ")) as \"Roundtot\" , ";


            retstring += " case when DOC.\"DocType\"='S' then DOC.\"Comments\" else '' End as \"BaseDoc\", ";


            retstring += " case when DOC.\"DocType\"='S' then Doc.\"U_CNRsn\" else '' End as \"Comments\", ";
            retstring += " case when DOC.\"DocType\"='S' then '1' else '' End as \"Paymeanscode\", ";// justi

            retstring += " DOC.\"U_EinvStatus\" as \"Einvsts\" ";


            retstring += " FROM oinv DOC ";
            retstring += " LEFT JOIN OADM cmp ON 1 = 1 ";
            retstring += " LEFT JOIN ADM1 cmpadd ON 1 = 1  ";
            retstring += " LEFT JOIN OCRD bpm ON bpm.\"CardCode\" = DOC.\"CardCode\"   ";
            retstring += " LEFT JOIN INV12 buyaddr ON buyaddr.\"DocEntry\" = doc.\"DocEntry\"  ";
            retstring += " LEFT JOIN OCST CmpST ON CMPST.\"Code\" = CMPADD.\"State\" AND   CMPST.\"Country\"=CMPADD.\"Country\"  ";
            retstring += " LEFT JOIN OCST BuyST ON BuyST.\"Code\" = buyaddr.\"StateB\"  AND   BuyST.\"Country\"=BUYADDR.\"CountryB\"  ";
            retstring += " LEFT JOIN OCRY CmpCY ON CmpCY.\"Code\" = CMPADD.\"Country\"  ";
            retstring += " LEFT JOIN OCRY BuyCY ON BuyCY.\"Code\" = buyaddr.\"CountryB\" ";
            retstring += " LEFT JOIN INV1 itm ON itm.\"DocEntry\" =DOc.\"DocEntry\"  ";
            retstring += " LEFT JOIN OVTG tax ON tax.\"Code\" =itm.\"VatGroup\"  ";         
            retstring += " LEFT JOIN TaxCat  ON TaxCat.\"Code\" =tax.\"Code\"  ";         
            retstring += " LEFT JOIN Tottb Tottb ON Tottb.\"DocEntry\" =DOC.\"DocEntry\"  ";
            retstring += " LEFT JOIN NNM1 nnm1 ON DOC.\"Series\" =nnm1.\"Series\"";
            retstring += " where DOC.\"DocEntry\"='" + Docentry + "'";

            retstring += " Order by itm.\"LineNum\" ";


            clsModule.objaddon.objglobalmethods.WriteErrorLog(retstring);
            return retstring;
         

        }



        public string CreditNoteQuery(string Docentry)
        {
            string retstring = "";


            retstring = " WITH Tottb AS ( ";
            retstring += " SELECT \"DocEntry\",sum(\"Totgross\") AS \"Totgross\",sum(\"Tottax\") AS \"Tottax\" ,sum(\"Totrndnet\") AS \"Totrndnet\",sum(\"Totnet\") AS \"Totnet\"  from ( ";
            retstring += " SELECT DOC.\"DocEntry\",";
            retstring += " sum(Round(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END)," + Round + ")) \"Totgross\",";
            retstring += " sum(Round((itm.\"VatSumSy\")," + Round + ")) AS \"Tottax\", ";
            retstring += " sum(Round(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END)," + Round + ")) " +
                " + round( sum(Round(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END)," + Round + ") " +
                " * (tax.\"Rate\" /100))," + Round + ") AS \"Totrndnet\", ";
            retstring += " Round(sum(((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END) + " +
                         " itm.\"VatSumSy\"), " + Round + ") AS \"Totnet\" ";
            retstring += " FROM ORIN DOC  ";
            retstring += " LEFT JOIN RIN1 itm ON itm.\"DocEntry\" =DOC.\"DocEntry\" ";
            retstring += " LEFT JOIN OVTG tax ON tax.\"Code\" = itm.\"VatGroup\" ";
            retstring += " GROUP BY DOC.\"DocEntry\",tax.\"Rate\" ) GROUP BY \"DocEntry\"), ";

            retstring += " TaxCat AS (";
            retstring += " SELECT t1.\"Code\" ,Max(t1.\"U_CatCode\") AS \"TaxCode\", max(t3.\"Code\") AS \"Reasoncode\",Max(t3.\"Name\") AS \"Reason\"  FROM \"@TAXCAT\" t1  ";
            retstring += " left JOIN \"@TAXCAT1\" t2 ON t1.\"Code\" =t2.\"Code\" ";
            retstring += " left JOIN \"@TAXRSN\" t3 ON t2.\"U_Reason\"  =t3.\"Code\"";
            retstring += " GROUP BY t1.\"Code\" )";

            retstring += " SELECT ";

            retstring += " '381' as \"TaxType\", ";
            retstring += " case  when DOC.\"DocType\"='S' then 'S' else 'I' End as \"DocType\", ";

            retstring += docseries + " \"DocNum\",";

            retstring += " doc.\"DocDate\" ,doc.\"DocTime\" ,doc.\"NumAtCard\" , ";
            retstring += " doc.\"DocCur\",  CASE WHEN BPM.\"U_EType\"=1 THEN '0100000' WHEN \"U_EType\"=2 THEN '0200000' WHEN \"U_EType\"=3 THEN '0100100'  ELSE '' END AS \"U_EType\",BPM.\"U_IDType\" , ";
            retstring += " BPM.\"CardName\" ,BPM.\"LicTradNum\"  ,BPM.\"AddID\", ";
            retstring += " CMP.\"CompnyName\",'CRN' as \"CmpId\",CMP.\"TaxIdNum\", CMP.\"TaxIdNum2\",CMPADD.\"Street\",cmp.\"SysCurrncy\",  ";
            retstring += " CMPADD.\"StreetNo\" ,CMPADD.\"Building\" ,CMPADD.\"City\" ,CMPADD.\"County\" ,CMPADD.\"ZipCode\" , ";
            retstring += " CMPADD.\"Country\" \"CodeCountry\" ,CmpST.\"Name\" \"State\" ,CmpCY.\"Name\"  \"Country\",  ";


            retstring += " BUYADDR.\"StreetNoB\",BUYADDR.\"StreetB\" ,BUYADDR.\"BuildingB\",BUYADDR.\"CityB\" ,BUYADDR.\"CountyB\", BUYADDR.\"ZipCodeB\" ,";
            retstring += " buyaddr.\"CountryB\" \"CodeCountryB\" ,BuyST.\"Name\" \"StateB\" ,BuyCY.\"Name\"  \"CountryB\" ,";

            retstring += " itm.\"LineNum\"+1 \"LineNum\" ,itm.\"Dscription\",itm.\"Currency\",itm.\"SubCatNum\" as \"ItemBuyerID\" ,itm.\"ItemCode\" as \"ItemsellerID\", ";
            retstring += " tax.\"Rate\" AS \"Taxrate\",  ";

            //retstring += " case when tax.\"Rate\"=0 then 'Z' else 'S' End AS \"TaxCat\", ";
            //retstring += " case when tax.\"Rate\"=0 then 'Zero Tax Product' else '' End AS \"TaxReason\", ";


            retstring += "TaxCat.\"TaxCode\" AS \"TaxCat\",";
            retstring += "TaxCat.\"Reason\" AS \"Reason\",";



            retstring += " Round(itm.\"VatSumSy\"," + Round + ") AS \"Taxamt\" , ";
            retstring += " itm.\"PriceBefDi\" AS \"BaseAmt\", ";
            retstring += " itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100 ) AS \"DiscAmt\" , ";
            retstring += " itm.\"PriceBefDi\" -cast(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100) as Decimal(20,6)) as \"PriceAmt\",  ";
            retstring += " Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" end as \"Quantity\",";
            retstring += " case when itm.\"UomCode\"='Manual' then 'E48' else itm.\"UomCode\" end as \"UomCode\",  ";
            retstring += " Round((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END," + Round + ") AS \"Gross\", ";
            retstring += " Round((itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" *(itm.\"DiscPrcnt\"/100))) * Case when DOC.\"DocType\"='S' then 1 else itm.\"Quantity\" END," + Round + ")+ Round(itm.\"VatSumSy\" ," + Round + ")  AS \"Linenet\" , ";
            retstring += " Round(itm.\"PriceBefDi\" -(itm.\"PriceBefDi\" * (itm.\"DiscPrcnt\" /100))-Case when DOC.\"DocType\"='S' then itm.\"Price\" else itm.\"INMPrice\" end ," + Round + ") as \"LineAllow\", ";
            retstring += " ROUND(Doc.\"DiscSum\"," + Round + ") AS \"Allownace\", ";
            retstring += " Round(Tottb.\"Totgross\"," + Round + ") as \"Totgross\" ,";
            retstring += " Round(Tottb.\"Totgross\"-Doc.\"DiscSum\"," + Round + ") as \"TaxExclusive\", ";

            retstring += " Round(Tottb.\"Tottax\"," + Round + ") as \"Tottax\",Round(Tottb.\"Totrndnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ") as \"Totnet\" , ";
            retstring += " Round(Tottb.\"Totnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ") as \"Totnet1\" , ";
            retstring += " (Round(Tottb.\"Totnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ")) - (Round(Tottb.\"Totrndnet\"-DOC.\"DiscSum\"+DOC .\"RoundDif\"," + Round + ")) as \"Roundtot\" , ";





            //retstring += " case when DOC.\"DocType\"='S' then '3833' else baseDoc.\"DocNum\" End as \"BaseDoc\", ";

            retstring += " case when DOC.\"DocType\"='S' then DOC.\"Comments\" WHEN COALESCE(baseDoc.\"DocNum\",0)= 0 " +
                         " THEN DOC.\"Comments\"  ELSE CAST(baseDoc.\"DocNum\" AS varchar(100)) End as \"BaseDoc\", ";
            
            retstring += " Doc.\"U_CNRsn\"  as \"Comments\", ";//need udf 
            retstring += " case when DOC.\"DocType\"='S' then '1' else '1' End as \"Paymeanscode\", ";// justi

            retstring += " Doc.\"U_EinvStatus\" as \"Einvsts\" ";

            retstring += " FROM ORIN DOC ";
            retstring += " LEFT JOIN OADM cmp ON 1 = 1 ";
            retstring += " LEFT JOIN ADM1 cmpadd ON 1 = 1  ";
            retstring += " LEFT JOIN OCRD bpm ON bpm.\"CardCode\" = DOC.\"CardCode\"   ";
            retstring += " LEFT JOIN RIN12 buyaddr ON buyaddr.\"DocEntry\" = doc.\"DocEntry\"  ";
            retstring += " LEFT JOIN OCST CmpST ON CMPST.\"Code\" = CMPADD.\"State\" AND   CMPST.\"Country\"=CMPADD.\"Country\"  ";
            retstring += " LEFT JOIN OCST BuyST ON BuyST.\"Code\" = buyaddr.\"StateB\"  AND   BuyST.\"Country\"=BUYADDR.\"CountryB\"  ";
            retstring += " LEFT JOIN OCRY CmpCY ON CmpCY.\"Code\" = CMPADD.\"Country\"  ";
            retstring += " LEFT JOIN OCRY BuyCY ON BuyCY.\"Code\" = buyaddr.\"CountryB\" ";
            retstring += " LEFT JOIN RIN1 itm ON itm.\"DocEntry\" =DOc.\"DocEntry\"  ";
            retstring += " LEFT JOIN OVTG tax ON tax.\"Code\" =itm.\"VatGroup\"  ";
            retstring += " LEFT JOIN TaxCat  ON TaxCat.\"Code\" =tax.\"Code\"  ";
            retstring += " LEFT JOIN Tottb Tottb ON Tottb.\"DocEntry\" =DOC.\"DocEntry\"  ";
            retstring += " LEFT JOIN NNM1 nnm1 ON DOC.\"Series\" =nnm1.\"Series\"";
            retstring += " LEFT JOIN oinv baseDoc ON itm.\"BaseEntry\"  =BASEDOC.\"DocEntry\" AND itm.\"BaseType\"=13";
            retstring += " where DOC.\"DocEntry\"='" + Docentry + "'";

            retstring += " Order by itm.\"LineNum\" ";


            clsModule.objaddon.objglobalmethods.WriteErrorLog(retstring);
            return retstring;

        }     
    }
}
