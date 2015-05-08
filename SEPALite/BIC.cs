using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEPALite
{
    public class BIC
    {
        private string BICstring;

        private readonly Regex regexpBIC;

        public BIC(string bic)
        {
            // We should check out this one also:
            //: [A‐Z]{6,6}[A‐Z2‐9][A‐NP‐Z0‐9]([A‐Z0‐9]{3,3}){0,1} 
            regexpBIC = new Regex("([a-zA-Z]{4}[a-zA-Z]{2}[a-zA-Z0-9]{2}([a-zA-Z0-9]{3})?)");

            if (!(regexpBIC.IsMatch(bic)))
                throw new ArgumentException("BIC code {0} is not valid", bic);

            BICstring = bic;
        }

        public override string ToString()
        {
            return BICstring;
        }
    }
}
