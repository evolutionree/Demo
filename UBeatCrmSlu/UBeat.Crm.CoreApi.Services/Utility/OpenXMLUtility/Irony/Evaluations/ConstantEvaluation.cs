using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony.Evaluations
{
    internal sealed class ConstantEvaluation : Evaluation
    {
        private readonly object value;

        public ConstantEvaluation(object value)
        {
            this.value = value;
        }

        public override object Value => value;
    }
}
