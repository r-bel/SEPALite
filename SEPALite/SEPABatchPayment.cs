using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SEPALite
{
    /// <summary>
    /// Class meant to serialize batch payments to SEPA XML based on PAIN.001.001.03
    /// Simply use AddPayment and at the end call SerializeToXML with a Stream as parameter.
    /// You can only use one payment block with one execution date (or none).
    /// Each SEPA has a message ID.
    /// Each payment block has a paymentBlockID
    /// Each payment has a uniqueCreditTransferID
    /// </summary>
    public class SEPABatchPayment
    {
        private Document document { get; set; }

        private SEPABatchPayment()
        {
            document = new Document();
            document.CstmrCdtTrfInitn = new CustomerCreditTransferInitiationV03();
            document.CstmrCdtTrfInitn.GrpHdr = new GroupHeader32();
            document.CstmrCdtTrfInitn.GrpHdr.CreDtTm = DateTime.Now;
        }

        private const int maxLengthName = 70;
        private const int maxLengthID = 30;
        private IList<CreditTransferTransactionInformation10> creditTransfers = new List<CreditTransferTransactionInformation10>();
        
        private static string Truncate(string s, uint maxLength)
        {
            if (maxLength > int.MaxValue)
                throw new ArgumentOutOfRangeException("maxLength");

            return s != null && s.Length > maxLength ? s.Substring(0, Convert.ToInt32(maxLength)) : s;
        }

        public static SEPABatchPayment CreateForOnePaymentBlock(string debtorName, IBANBIC debtorAccount, string messageID, string paymentBlockID, DateTime? paymentExecutionDate = null)
        {
            if (debtorAccount.NoBICprovided)
                throw new ArgumentException("No BIC provided for debtor account", "debtorAccount");

            var b = new SEPABatchPayment();

            b.document.CstmrCdtTrfInitn.GrpHdr.MsgId = Truncate(messageID, maxLengthID);

            b.document.CstmrCdtTrfInitn.GrpHdr.InitgPty = new PartyIdentification32
            {
                Nm = Truncate(debtorName, maxLengthName),
            };

            var pi = new PaymentInstructionInformation3
            {
                PmtInfId = Truncate(paymentBlockID, maxLengthID),
                PmtMtd = PaymentMethod3Code.TRF,
                BtchBookg = false,
                ReqdExctnDt = paymentExecutionDate ?? DateTime.Now,

                Dbtr = new PartyIdentification32
                {
                    Nm = Truncate(debtorName, maxLengthName), // only debtorname is enough
                },
                DbtrAcct = new CashAccount16
                {
                    Id = new AccountIdentification4Choice
                    {
                        Item = debtorAccount.IBAN,
                    }
                },
                DbtrAgt = new BranchAndFinancialInstitutionIdentification4
                {
                    FinInstnId = new FinancialInstitutionIdentification7
                    {
                        BIC = debtorAccount.BIC.ToString(),
                    }
                },
            };

            b.document.CstmrCdtTrfInitn.PmtInf = new[] { pi };

            return b;
        }


        public void AddPayment(decimal amount, string creditorName, IBANBIC creditorAccount, string uniqueCreditTransferID, string unstructuredInformation = null)
        {
            AddPayment(amount, creditorName, creditorAccount, uniqueCreditTransferID, unstructuredInformation == null ? null : 
                new RemittanceInformation5
                {
                    Ustrd = new[] { unstructuredInformation },
                });
        }

        public void AddPayment(decimal amount, string creditorName, IBANBIC creditorAccount, string uniqueCreditTransferID, BelgianStructuredCommunicationReference remittanceInformation)
        {
            var ri = new RemittanceInformation5
                {
                    Strd = new[] { new StructuredRemittanceInformation7
                    {
                        CdtrRefInf = new CreditorReferenceInformation2
                        {
                            Tp = new CreditorReferenceType2
                            {
                                CdOrPrtry = new CreditorReferenceType1Choice
                                {
                                    Item = DocumentType3Code.SCOR
                                },
                                Issr = "BBA",
                            },
                            Ref = remittanceInformation.ToString(),
                        }
                    }}
                };

            AddPayment(amount, creditorName, creditorAccount, uniqueCreditTransferID, ri);
        }

        private void AddPayment(decimal amount, string creditorName, IBANBIC creditorAccount, string uniqueCreditTransferID, RemittanceInformation5 remittanceInformation = null)
        {
            creditTransfers.Add(new CreditTransferTransactionInformation10
            {
                PmtId = new PaymentIdentification1
                {
                    EndToEndId = Truncate(uniqueCreditTransferID, maxLengthID),                    
                },
                Amt = new AmountType3Choice
                {
                    Item = new ActiveOrHistoricCurrencyAndAmount
                    {
                        Value = amount,
                        Ccy = "EUR",
                    },
                },
                Cdtr = new PartyIdentification32
                {
                    Nm = Truncate(creditorName, maxLengthName),
                },
                CdtrAcct = new CashAccount16
                {
                    Id = new AccountIdentification4Choice
                    {
                        Item = creditorAccount.IBAN,
                    },
                },
                RmtInf = remittanceInformation,
            });
        }

        public void SerializeToXml(Stream stream)
        {
            document.CstmrCdtTrfInitn.PmtInf[0].CdtTrfTxInf = creditTransfers.ToArray();
            document.CstmrCdtTrfInitn.GrpHdr.NbOfTxs = creditTransfers.Count.ToString();

            var ser = new XmlSerializer(typeof(Document));
            ser.Serialize(stream, document);
        }
    }
}
