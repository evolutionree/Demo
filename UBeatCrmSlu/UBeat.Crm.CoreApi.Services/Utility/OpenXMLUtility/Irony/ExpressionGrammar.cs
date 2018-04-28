using Irony.Parsing;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony
{
    [Language("Expression Grammar", "1.0", "abc")]
    public class ExpressionGrammar : Grammar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGrammar"/> class.
        /// </summary>
        public ExpressionGrammar() : base(false)
        {
            // 1. Terminals
            var number = new NumberLiteral("Number");
            number.DefaultIntTypes = new TypeCode[] { TypeCode.Int16, TypeCode.Int32, TypeCode.Int64 };
            number.DefaultFloatType = TypeCode.Single;

            var stringLiteral = new RegexBasedTerminal("String", "\"\\s*[^ \\f\\n\\r\\t\\v\"]+\\s*\"");
            //var field = new IdentifierTerminal("Field", "#】", "【#");

            var identifier = new IdentifierTerminal("Identifier");
            var funcName = new IdentifierTerminal("FuncName");
            var fieldterm = new RegexBasedTerminal("Field", @"【#\s*[^\s|(【#)]+\s*#】");

            // 2. Non-terminals
            var expr = new NonTerminal("Expression");
            var term = new NonTerminal("Term");//术语
            var binOp = new NonTerminal("BinaryOperator", "operator");
            var parExpr = new NonTerminal("ParenthesisExpression");
            var binExpr = new NonTerminal("BinaryExpression", typeof(BinaryOperationNode));
            var boolExpr = new NonTerminal("BoolenExpression", typeof(IfNode));
            var childBoolExprs = new NonTerminal("ChildBoolExpression");
            var boolOp = new NonTerminal("BoolenOperator");
            

            var funcDefExpr = new NonTerminal("FuncDefExpression", typeof(FunctionDefNode));
            var funcArgsExpr = new NonTerminal("FuncArgsExpression");
            //var paramListTerm = new NonTerminal("ParamListTerm");
            var argExpr = new NonTerminal("Arg");

            var Program = new NonTerminal("Program", typeof(StatementListNode));

            // 3. BNF rules
            expr.Rule = term | funcDefExpr  | parExpr | binExpr | boolExpr | childBoolExprs; 
            term.Rule = number | stringLiteral | identifier  | fieldterm;


            //boolExpr.Rule = expr + boolOp + expr;
            //boolOp.Rule = ToTerm("==") | ">" | "<" | ">=" | "<=" | "!=";
            //childBoolExprs.Rule = ToTerm("(") + MakePlusRule(childBoolExprs, ToTerm("&&") | "||", boolExpr) + ")";


            boolExpr.Rule= childBoolExprs;
            boolOp.Rule= ToTerm("==") | ">" | "<" | ">=" | "<=" | "!=" ; ;
            childBoolExprs.Rule =  MakePlusRule(childBoolExprs, ToTerm("&&") | "||", expr + boolOp + expr) 
                   | ToTerm("(") + MakePlusRule(childBoolExprs, ToTerm("&&") | "||", expr + boolOp + expr) + ")";

            funcDefExpr.Rule = (funcName + ToTerm("(") + funcArgsExpr + ")") ;
            funcArgsExpr.Rule =  MakePlusRule(funcArgsExpr, ToTerm(","), argExpr) ;
            argExpr.Rule = number | stringLiteral | identifier | fieldterm | expr;


            parExpr.Rule = "(" + expr + ")";
            
            binExpr.Rule = expr + binOp + expr;
            binOp.Rule = ToTerm("+") | "-" | "*" | "/";

            RegisterOperators(10, "+", "-");
            RegisterOperators(20, "*", "/");

            MarkPunctuation("(", ")");//标点符号
            RegisterBracePair("(", ")");//注册组合对象
            MarkTransient(expr, term, binOp, parExpr, boolOp);

            this.Root = expr;
        }
    }
}
