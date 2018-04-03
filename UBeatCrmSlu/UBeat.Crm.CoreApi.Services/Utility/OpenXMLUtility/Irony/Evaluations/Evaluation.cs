using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony.Evaluations
{
    public abstract class Evaluation
    {
        public abstract object Value { get; }

        public override string ToString() => Value?.ToString();
    }
}
