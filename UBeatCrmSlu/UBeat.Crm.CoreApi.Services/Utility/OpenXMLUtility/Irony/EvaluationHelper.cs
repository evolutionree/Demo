using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony.Evaluations;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony
{
    public class EvaluationHelper
    {
        

        public static Evaluation PerformEvaluate(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
               
                case "BinaryExpression":
                    return ParsingBinaryExpression(node);
                case "Number":
                    return ParsingNumberExpression(node);
                case "String":
                    return new ConstantEvaluation(node.Token.Text.Trim().Trim('"'));
                case "Field":
                    return ParsingFieldExpression(node);
                case "FuncDefExpression":
                    return ParsingFuncNameExpression(node);
                case "BoolenExpression":
                    return ParsingBoolExpression(node);
               
                case "Term":
                case "Expression":
                    return ParsingTermExpression(node);
                default: break;
            }

            throw new InvalidOperationException($"Unrecognizable term {node.Term.Name}.");
        }
        private static Evaluation ParsingTermExpression(ParseTreeNode node)
        {
            if (node.ChildNodes.Count == 0)
            {
                return new ConstantEvaluation(null);
            }
            return PerformEvaluate(node.ChildNodes[0]);

        }

        private static Evaluation ParsingBoolExpression(ParseTreeNode node)
        {
            var leftNode = node.ChildNodes[0];
            var opNode = node.ChildNodes[1];
            var rightNode = node.ChildNodes[2];
            Evaluation left = PerformEvaluate(leftNode);
            Evaluation right = PerformEvaluate(rightNode);
            BoolOperation op = BoolOperation.Equal;
            switch (opNode.Term.Name)
            {
                case "==":
                    op = BoolOperation.Equal;
                    break;
                case ">":
                    op = BoolOperation.GreaterThan;
                    break;
                case ">=":
                    op = BoolOperation.GreaterThanEqual;
                    break;
                case "<":
                    op = BoolOperation.LessThan;
                    break;
                case "<=":
                    op = BoolOperation.LessThanEqual;
                    break;
                case "!=":
                    op = BoolOperation.NotEqual; break;
                case "&&":
                    op = BoolOperation.And; break;
                case "||":
                    op = BoolOperation.OR;
                    break;
            }
            return new BoolEvaluation(left, right, op);
        }

        private static Evaluation ParsingFieldExpression(ParseTreeNode node)
        {

            var value = Convert.ToDouble(node.Token.Text);
            return new ConstantEvaluation(value);
        }
        private static Evaluation ParsingFuncNameExpression(ParseTreeNode node)
        {
            var value = Convert.ToDouble(node.Token.Text);
            return new ConstantEvaluation(value);
        }
        private static Evaluation ParsingBinaryExpression(ParseTreeNode node)
        {
            var leftNode = node.ChildNodes[0];
            var opNode = node.ChildNodes[1];
            var rightNode = node.ChildNodes[2];
            Evaluation left = PerformEvaluate(leftNode);
            Evaluation right = PerformEvaluate(rightNode);
            BinaryOperation op = BinaryOperation.Add;
            switch (opNode.Term.Name)
            {
                case "+":
                    op = BinaryOperation.Add;
                    break;
                case "-":
                    op = BinaryOperation.Sub;
                    break;
                case "*":
                    op = BinaryOperation.Mul;
                    break;
                case "/":
                    op = BinaryOperation.Div;
                    break;
            }
            return new BinaryEvaluation(left, right, op);
        }
        private static Evaluation ParsingNumberExpression(ParseTreeNode node)
        {
            var value = Convert.ToDouble(node.Token.Text);
            return new ConstantEvaluation(value);
        }
    }
    
}


