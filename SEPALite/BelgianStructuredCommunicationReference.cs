using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SEPALite
{    
    public class BelgianStructuredCommunicationReference
    {
        private string structuredBelgianReference;

        public BelgianStructuredCommunicationReference(string structuredBelgianReference)
        {
            var regexpOGM = new Regex("^[0-9]{12}$");

            if (!(regexpOGM.IsMatch(structuredBelgianReference)))
                throw new ArgumentOutOfRangeException("structuredBelgianReference", "Valid OGM is 12 digits");

            this.structuredBelgianReference = structuredBelgianReference;
        }

        public static BelgianStructuredCommunicationReference CreateWithoutControlDigits(string structuredBelgianReference)
        {
            var regexpOGM = new Regex("^[0-9]{10}$");

            if (!(regexpOGM.IsMatch(structuredBelgianReference)))
                throw new ArgumentOutOfRangeException("structuredBelgianReference", "Valid base for OGM is 10 digits");

            var remainder = long.Parse(structuredBelgianReference) % 97;

            return new BelgianStructuredCommunicationReference(structuredBelgianReference + (remainder == 0 ? 97 : remainder).ToString().PadLeft(2, '0'));
        }

        public override string ToString()
        {
            return structuredBelgianReference;
        }
    }
}
