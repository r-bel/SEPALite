using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEPALite
{
    public class IBANBIC
    {
        public string IBAN { get; private set; }
        public BIC BIC { get; private set; }
        public bool NoBICprovided { get; private set; }

        //[A‐Z]{2,2}[0‐9]{2,2}[a‐zA‐Z0‐9]{1,30})
        private readonly Regex regexpIBAN = new Regex("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}");

        public IBANBIC(string iban, BIC bic)
        {
            if (!(regexpIBAN.IsMatch(iban)))
                throw new ArgumentException("IBAN code {0} is not valid", iban);
            
            IBAN = iban;

            BIC = bic;
        }
        public IBANBIC(string iban) : this(iban, null)
        {
            NoBICprovided = true;
        }
    }
}
