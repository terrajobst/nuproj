using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuProj.Tasks.Utilities
{
    public class ExpansionToken
    {
        public ExpansionToken(ExpansionTokenType tokenType, string tokenValue)
        {
            Type = tokenType;
            Value = tokenValue;
        }

        public ExpansionTokenType Type { get; private set; }

        public string Value { get; private set; }
    }
}
