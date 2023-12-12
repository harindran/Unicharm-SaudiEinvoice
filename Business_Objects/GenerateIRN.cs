
using System;
using System.Collections.Generic;

namespace EInvoice.Models
{
  

    public class saplogin
    {
    public string CompanyDB { get; set; }
    public string Password { get; set; }
    public string UserName { get; set; }
    }
    
    public class AccountingCustomerParty
    {
        public Party Party { get; set; } = new Party();
    }

    public class AccountingSupplierParty
    {
        public Party Party { get; set; } = new Party();
    }

    public class AdditionalStreetName
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

 
    public class AllowanceCharge
    {
        public AllowanceChargeReason AllowanceChargeReason { get; set; } = new AllowanceChargeReason();
        public string AllowanceChargeReasonCode { get; set; }
        public Amount Amount { get; set; } = new Amount();
        public BaseAmount BaseAmount { get; set; } = new BaseAmount();
        public string ChargeIndicator { get; set; }
        public string MultiplierFactorNumeric { get; set; }
        public TaxCategory TaxCategory { get; set; } = new TaxCategory();
    }

    public class AllowanceChargeReason
    {
        public string ar { get; set; }
        public string en { get; set; }
    }

    public class AllowanceTotalAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class Amount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class BaseAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class BaseQuantity
    {
        public string unitCode { get; set; }
        public string value { get; set; }
    }

    public class BillingReference
    {
        public InvoiceDocumentReference InvoiceDocumentReference { get; set; } = new InvoiceDocumentReference();
    }

    public class BuildingNumber
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class BuyersItemIdentification
    {
        public ID ID { get; set; }
    }

    public class CityName
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class CitySubdivisionName
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class ClassifiedTaxCategory
    {
        public string ID { get; set; }
        public string Percent { get; set; }
        public TaxScheme TaxScheme { get; set; } = new TaxScheme();
    }

    public class ContractDocumentReference
    {
        public ID ID { get; set; }
    }

    public class Country
    {
        public string IdentificationCode { get; set; }
    }

    public class CountrySubentity
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class CustomFields
    {
        public string TotalBoxes { get; set; }
        public string TotalWreight { get; set; }
    }

    public class Delivery
    {
        public string ActualDeliveryDate { get; set; }
        public string LatestDeliveryDate { get; set; }
    }

    public class EInvoice
    {
        public string ProfileID { get; set; }
        public ID ID { get; set; } = new ID();
        public InvoiceTypeCode InvoiceTypeCode { get; set; } = new InvoiceTypeCode();
        public string IssueDate { get; set; }
        public string IssueTime { get; set; }
        public List<Delivery> Delivery { get; set; } = new List<Delivery>();
        public List<BillingReference> BillingReference = new List<BillingReference>();
        public OrderReference OrderReference { get; set; } = new OrderReference();
        public List<ContractDocumentReference> ContractDocumentReference = new List<ContractDocumentReference>();
        public string DocumentCurrencyCode { get; set; }
        public string TaxCurrencyCode { get; set; }
        public AccountingSupplierParty AccountingSupplierParty { get; set; } = new AccountingSupplierParty();
        public AccountingCustomerParty AccountingCustomerParty { get; set; } = new AccountingCustomerParty();
        public List<InvoiceLine> InvoiceLine = new List<InvoiceLine>();
        public List<AllowanceCharge> AllowanceCharge = new List<AllowanceCharge>();
        public List<TaxSubTotal> TaxTotal = new List<TaxSubTotal>();
        public LegalMonetaryTotal LegalMonetaryTotal { get; set; } = new LegalMonetaryTotal();
        public List<PaymentMean> PaymentMeans = new List<PaymentMean>();
        public Note Note { get; set; } = new Note();
    }

    public class ID
    {
        public string en { get; set; }
        public string ar { get; set; }     
    }

    public class InstructionNote
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class InvoiceDocumentReference
    {
        public ID ID { get; set; }
    }

    public class InvoicedQuantity
    {
        public string unitCode { get; set; }
        public string value { get; set; }
    }

    public class InvoiceLine
    {
        public string ID { get; set; }
        public Item Item { get; set; } = new Item();
        public Price Price { get; set; } = new Price();
        public InvoicedQuantity InvoicedQuantity { get; set; } = new InvoicedQuantity();        
        public LineExtensionAmount LineExtensionAmount { get; set; } = new LineExtensionAmount();
        public TaxTotal TaxTotal { get; set; } = new TaxTotal();
    }

    public class InvoiceTypeCode
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Item
    {
        public Name Name { get; set; }
        public BuyersItemIdentification BuyersItemIdentification { get; set; } = new BuyersItemIdentification();
        public SellersItemIdentification SellersItemIdentification { get; set; } = new SellersItemIdentification();
        public StandardItemIdentification StandardItemIdentification { get; set; } = new StandardItemIdentification();
        public ClassifiedTaxCategory ClassifiedTaxCategory { get; set; } = new ClassifiedTaxCategory();
    }

    public class LegalMonetaryTotal
    {
        public LineExtensionAmount LineExtensionAmount { get; set; } = new LineExtensionAmount();
        public AllowanceTotalAmount AllowanceTotalAmount { get; set; } = new AllowanceTotalAmount();
        public TaxExclusiveAmount TaxExclusiveAmount { get; set; } = new TaxExclusiveAmount();
        public TaxInclusiveAmount TaxInclusiveAmount { get; set; } = new TaxInclusiveAmount();
        public PrepaidAmount PrepaidAmount { get; set; } = new PrepaidAmount();
        public PayableAmount PayableAmount { get; set; } = new PayableAmount();
        public PayableRoundingAmount PayableRoundingAmount { get; set; } = new PayableRoundingAmount();
    }
    public class PayableRoundingAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class LineExtensionAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class Name
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class Note
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class OrderReference
    {
        public ID ID { get; set; } = new ID();
    }

    public class Party
    {
        public PartyLegalEntity PartyLegalEntity { get; set; } = new PartyLegalEntity();
        public PartyTaxScheme PartyTaxScheme { get; set; } = new PartyTaxScheme();
        public PartyIdentification PartyIdentification { get; set; } = new PartyIdentification();
        public PostalAddress PostalAddress { get; set; } = new PostalAddress();
    }

    public class PartyIdentification
    {
        public PIID ID { get; set; } = new PIID();
    }

    public class PIID
    {
        public string schemeID { get; set; }
        public string value { get; set; }
    }

    public class PartyLegalEntity
    {
        public RegistrationName RegistrationName { get; set; } = new RegistrationName();
    }

    public class PartyTaxScheme
    {
        public string CompanyID { get; set; }
        public TaxScheme TaxScheme { get; set; } = new TaxScheme();
    }

    public class PayableAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class PayeeFinancialAccount
    {
        public PaymentNote PaymentNote { get; set; } = new PaymentNote();
    }

    public class PaymentMean
    {
        public string PaymentMeansCode { get; set; }
        public InstructionNote InstructionNote { get; set; } = new InstructionNote();
        public PayeeFinancialAccount PayeeFinancialAccount { get; set; } = new PayeeFinancialAccount();
    }

    public class PaymentNote
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class PlotIdentification
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class PostalAddress
    {
        public StreetName StreetName { get; set; } = new StreetName();
        public AdditionalStreetName AdditionalStreetName { get; set; } = new AdditionalStreetName();
        public BuildingNumber BuildingNumber { get; set; } = new BuildingNumber();
        public PlotIdentification PlotIdentification { get; set; } = new PlotIdentification();
        public CityName CityName { get; set; } = new CityName();
        public CitySubdivisionName CitySubdivisionName { get; set; } = new CitySubdivisionName();
        public string PostalZone { get; set; }
        public CountrySubentity CountrySubentity { get; set; } = new CountrySubentity();
        public Country Country { get; set; } = new Country();
    }

    public class PrepaidAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class Price
    {
        public AllowanceCharge AllowanceCharge { get; set; } = new AllowanceCharge();
        public PriceAmount PriceAmount { get; set; } = new PriceAmount();
        public BaseQuantity BaseQuantity { get; set; } = new BaseQuantity();
    }

    public class PriceAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class RegistrationName
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class GenerateIRN
    {
        public string DeviceId { get; set; }
        public EInvoice EInvoice { get; set; } = new EInvoice();
        public CustomFields CustomFields { get; set; } = new CustomFields();
    }

    public class RoundingAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class SellersItemIdentification
    {
        public ID ID { get; set; } = new ID();
    }

    public class StandardItemIdentification
    {
        public ID ID { get; set; } = new ID();
    }

    public class StreetName
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class TaxableAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class TaxAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class TaxCategory
    {
        public string ID { get; set; }
        public string Percent { get; set; }
        public TaxScheme TaxScheme { get; set; } = new TaxScheme();
        public string TaxExemptionReasonCode { get; set; }
        public TaxExemptionReason TaxExemptionReason { get; set; } = new TaxExemptionReason();
    }

    public class TaxExclusiveAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class TaxExemptionReason
    {
        public string en { get; set; }
        public string ar { get; set; }
    }

    public class TaxInclusiveAmount
    {
        public string currencyID { get; set; }
        public string value { get; set; }
    }

    public class TaxScheme
    {
        public string ID { get; set; }
    }

    public class TaxSubtotal
    {
        public TaxableAmount TaxableAmount { get; set; } = new TaxableAmount();
        public TaxAmount TaxAmount { get; set; } = new TaxAmount();
        public TaxCategory TaxCategory { get; set; } = new TaxCategory();
    }

    public class TaxSubTotal
    {
        public TaxAmount TaxAmount { get; set; } = new TaxAmount();       
        public List<TaxSubtotal> TaxSubtotal = new List<TaxSubtotal>();
    }

    public class TaxTotal
    {
        public TaxAmount TaxAmount { get; set; } = new TaxAmount();
        public RoundingAmount RoundingAmount { get; set; } = new RoundingAmount();
        
    }

}

