﻿using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Interpreter.Evaluator;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility.Irony
{
    [Language("ExpressionEvaluator", "1.0", "Multi-line expression evaluator")]
    public class ExpressionEvaluatorGrammar : Grammar
    {
        public ExpressionEvaluatorGrammar() : base(caseSensitive: false)
        {
            // 1. Terminals
            var number = new NumberLiteral("number");
            //Let's allow big integers (with unlimited number of digits):
            number.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt };
            var identifier = new IdentifierTerminal("identifier");
            var comment = new CommentTerminal("comment", "#", "\n", "\r");
            //comment must be added to NonGrammarTerminals list; it is not used directly in grammar rules,
            // so we add it to this list to let Scanner know that it is also a valid terminal. 
            base.NonGrammarTerminals.Add(comment);
            var comma = ToTerm(",");

            //String literal with embedded expressions  ------------------------------------------------------------------
            var stringLit = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes | StringOptions.IsTemplate);
            stringLit.AddStartEnd("'", StringOptions.AllowsAllEscapes | StringOptions.IsTemplate);
            stringLit.AstConfig.NodeType = typeof(StringTemplateNode);
            var Expr = new NonTerminal("Expr"); //declare it here to use in template definition 
            var templateSettings = new StringTemplateSettings(); //by default set to Ruby-style settings 
            templateSettings.ExpressionRoot = Expr; //this defines how to evaluate expressions inside template
            this.SnippetRoots.Add(Expr);
            stringLit.AstConfig.Data = templateSettings;
            //--------------------------------------------------------------------------------------------------------

            // 2. Non-terminals
            var Term = new NonTerminal("Term");
            var BinExpr = new NonTerminal("BinExpr", typeof(BinaryOperationNode));
            var ParExpr = new NonTerminal("ParExpr");
            var UnExpr = new NonTerminal("UnExpr", typeof(UnaryOperationNode));
            var TernaryIfExpr = new NonTerminal("TernaryIf", typeof(IfNode));
            var ArgList = new NonTerminal("ArgList", typeof(ExpressionListNode));
            var FunctionCall = new NonTerminal("FunctionCall", typeof(FunctionCallNode));
            var MemberAccess = new NonTerminal("MemberAccess", typeof(MemberAccessNode));
            var ObjectRef = new NonTerminal("ObjectRef"); // foo, foo.bar or f['bar']
            var UnOp = new NonTerminal("UnOp");
            var BinOp = new NonTerminal("BinOp", "operator");
            var PrefixIncDec = new NonTerminal("PrefixIncDec", typeof(IncDecNode));
            var PostfixIncDec = new NonTerminal("PostfixIncDec", typeof(IncDecNode));
            var IncDecOp = new NonTerminal("IncDecOp");
            var AssignmentStmt = new NonTerminal("AssignmentStmt", typeof(AssignmentNode));
            var AssignmentOp = new NonTerminal("AssignmentOp", "assignment operator");
            var Statement = new NonTerminal("Statement");
            var Program = new NonTerminal("Program", typeof(StatementListNode));

            // 3. BNF rules
            Expr.Rule = Term | UnExpr | BinExpr | PrefixIncDec | PostfixIncDec | TernaryIfExpr;
            Term.Rule = number | ParExpr | stringLit | FunctionCall | identifier | MemberAccess ;
            ParExpr.Rule = "(" + Expr + ")";
            UnExpr.Rule = UnOp + Term + ReduceHere();
            UnOp.Rule = ToTerm("+") | "-" | "!";
            BinExpr.Rule = Expr + BinOp + Expr;
            BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "**" | "==" | "<" | "<=" | ">" | ">=" | "!=" | "&&" | "||" | "&" | "|";
            PrefixIncDec.Rule = IncDecOp + identifier;
            PostfixIncDec.Rule = identifier + PreferShiftHere() + IncDecOp;
            IncDecOp.Rule = ToTerm("++") | "--";
            TernaryIfExpr.Rule = Expr + "?" + Expr + ":" + Expr;
            MemberAccess.Rule = Expr + PreferShiftHere() + "." + identifier;
            AssignmentStmt.Rule = ObjectRef + AssignmentOp + Expr;
            AssignmentOp.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=";
            Statement.Rule = AssignmentStmt | Expr | Empty;
            ArgList.Rule = MakeStarRule(ArgList, comma, Expr);
            FunctionCall.Rule = Expr + PreferShiftHere() + "(" + ArgList + ")";
            FunctionCall.NodeCaptionTemplate = "call #{0}(...)";
            //ObjectRef.Rule = identifier | MemberAccess | IndexedAccess;

            Program.Rule = MakePlusRule(Program, NewLine, Statement);

            this.Root = Program;       // Set grammar root

            // 4. Operators precedence
            RegisterOperators(10, "?");
            RegisterOperators(15, "&", "&&", "|", "||");
            RegisterOperators(20, "==", "<", "<=", ">", ">=", "!=");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/");
            RegisterOperators(50, Associativity.Right, "**");
            RegisterOperators(60, "!");
            // For precedence to work, we need to take care of one more thing: BinOp. 
            //For BinOp which is or-combination of binary operators, we need to either 
            // 1) mark it transient or 2) set flag TermFlags.InheritPrecedence
            // We use first option, making it Transient.  

            // 5. Punctuation and transient terms
            MarkPunctuation("(", ")", "?", ":", "[", "]");
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            MarkTransient(Term, Expr, Statement, BinOp, UnOp, IncDecOp, AssignmentOp, ParExpr, ObjectRef);

            // 7. Syntax error reporting
            MarkNotReported("++", "--");
            AddToNoReportGroup("(", "++", "--");
            AddToNoReportGroup(NewLine);
            AddOperatorReportGroup("operator");
            AddTermsReportGroup("assignment operator", "=", "+=", "-=", "*=", "/=");


            //9. Language flags. 
            // Automatically add NewLine before EOF so that our BNF rules work correctly when there's no final line break in source
            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF | LanguageFlags.CreateAst | LanguageFlags.SupportsBigInt;
        }

    }
}
