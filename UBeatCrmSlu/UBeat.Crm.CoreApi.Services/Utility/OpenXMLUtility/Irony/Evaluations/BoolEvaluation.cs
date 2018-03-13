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
                
                if (this.left.Value == null || this.right.Value == null)
                {
                    throw new InvalidOperationException("Either left or right value of the binary evaluation has been evaluated to null.");
                }
                
                var leftFloatValue = 0F;
                var rightFloatValue = 0F;
                DateTime leftDateTimeValue;
                DateTime rightDateTimeValue;
                bool leftboolValue;
                bool rightboolValue;
                //数字的比较
                if (float.TryParse(this.left.Value.ToString(), out leftFloatValue) && float.TryParse(this.right.Value.ToString(), out rightFloatValue))
                {
                    switch (oper)
                    {
                        case BoolOperation.Equal:
                            return leftFloatValue == rightFloatValue;
                        case BoolOperation.GreaterThan:
                            return leftFloatValue > rightFloatValue;
                        case BoolOperation.GreaterThanEqual:
                            return leftFloatValue >= rightFloatValue;
                        case BoolOperation.LessThan:
                            return leftFloatValue < rightFloatValue;
                        case BoolOperation.LessThanEqual:
                            return leftFloatValue <= rightFloatValue;
                        case BoolOperation.NotEqual:
                            return leftFloatValue != rightFloatValue;
                        default: throw new Exception("数字比较运算符格式错误，只能使用==,>,>=,<,<=,!=");
                    }
                }
                //时间的比较
                else if (DateTime.TryParse(this.left.Value.ToString(), out leftDateTimeValue) && DateTime.TryParse(this.right.Value.ToString(), out rightDateTimeValue))
                {
                    switch (oper)
                    {
                        case BoolOperation.Equal:
                            return leftDateTimeValue == rightDateTimeValue;
                        case BoolOperation.GreaterThan:
                            return leftDateTimeValue > rightDateTimeValue;
                        case BoolOperation.GreaterThanEqual:
                            return leftDateTimeValue >= rightDateTimeValue;
                        case BoolOperation.LessThan:
                            return leftDateTimeValue < rightDateTimeValue;
                        case BoolOperation.LessThanEqual:
                            return leftDateTimeValue <= rightDateTimeValue;
                        case BoolOperation.NotEqual:
                            return leftDateTimeValue != rightDateTimeValue;
                        default: throw new Exception("日期比较运算符格式错误，只能使用==,>,>=,<,<=,!=");
                    }
                }
                else if (bool.TryParse(this.left.Value.ToString(), out leftboolValue) && bool.TryParse(this.right.Value.ToString(), out rightboolValue))
                {
                    switch (oper)
                    {
                        case BoolOperation.And:
                            return leftboolValue && rightboolValue;
                        case BoolOperation.OR:
                            return leftboolValue || rightboolValue;
                      
                        default: throw new Exception("比较运算符格式错误，只能使用 && 或 ||");
                    }
                }
                //其他情况，一律当做字符串比较
                else 
                {
                    string leftValue = this.left.Value.ToString();
                    string rightValue = this.right.Value.ToString();
                    switch (oper)
                    {
                        case BoolOperation.Equal:
                            return leftValue == rightValue;
                        case BoolOperation.NotEqual:
                            return leftValue != rightValue;
                        default: throw new Exception("字符比较运算符格式错误，只能比较==或！=");
                    }
                }
               
                

                throw new InvalidOperationException("Invalid compare operation.");
            }
        }

       

        public override string ToString() => $"{this.left?.ToString()} {oper} {this.right?.ToString()}";
    }

    
}
