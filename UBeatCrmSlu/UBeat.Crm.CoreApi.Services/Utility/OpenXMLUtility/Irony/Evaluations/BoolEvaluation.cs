using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony.Evaluations
{
    internal sealed class BoolEvaluation : Evaluation
    {
        private readonly Evaluation left;
        private readonly Evaluation right;

        private readonly BoolOperation oper;

        public BoolEvaluation(Evaluation left, Evaluation right, BoolOperation oper)
        {
            this.left = left;
            this.right = right;
            this.oper = oper;
        }

        public override object Value
        {
            get
            {
                var leftValue = 0F;
                var rightValue = 0F;
                if (this.left.Value == null || this.right.Value == null)
                {
                    throw new InvalidOperationException("Either left or right value of the binary evaluation has been evaluated to null.");
                }
                if (!float.TryParse(this.left.Value.ToString(), out leftValue) ||
                    !float.TryParse(this.right.Value.ToString(), out rightValue))
                {
                    throw new InvalidOperationException("Either left or right value of the binary evaluation cannot be evaluated as a float value.");
                }
                switch (oper)
                {
                    case BoolOperation.Equal:
                        return leftValue == rightValue;
                    case BoolOperation.GreaterThan:
                        return leftValue > rightValue;
                    case BoolOperation.GreaterThanEqual:
                        return leftValue >= rightValue;
                    case BoolOperation.LessThan:
                        return leftValue < rightValue;
                    case BoolOperation.LessThanEqual:
                        return leftValue <= rightValue;
                    case BoolOperation.NotEqual:
                        return leftValue != rightValue;
                    default:
                        break;
                }

                throw new InvalidOperationException("Invalid binary operation.");
            }
        }

        public override string ToString() => $"{this.left?.ToString()} {oper} {this.right?.ToString()}";
    }
}
