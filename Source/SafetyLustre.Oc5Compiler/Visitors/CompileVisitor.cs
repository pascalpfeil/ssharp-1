﻿using Antlr4.Runtime.Misc;
using SafetyLustre.Oc5Compiler.Exceptions;
using SafetyLustre.Oc5Compiler.Oc5Objects;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static SafetyLustre.Oc5Compiler.Oc5Parser;

namespace SafetyLustre.Oc5Compiler.Visitors
{
    class CompileVisitor : Oc5BaseVisitor<Expression>
    {
        private LabelTarget CurrentReturnLabelTarget { get; set; }
        public Oc5Model Oc5Model { get; set; } = new Oc5Model();
        public Oc5State Oc5State { get; set; } = new Oc5State();

        public CompileVisitor([NotNull] OcfileContext context)
        {
            VisitOcfile(context);
        }

        public override Expression VisitOcfile([NotNull] OcfileContext context)
        {
            base.VisitOcfile(context);

            return null;
        }

        #region Constants

        public override Expression VisitConstants([NotNull] ConstantsContext context)
        {
            if (int.Parse(context.NUMBER().ToString()) != context.constant().Length)
                throw new InvalidSyntaxException("Count does not match number of constants", context.NUMBER().Symbol);

            VisitChildren(context);

            return null;
        }

        public override Expression VisitConstant([NotNull] ConstantContext context)
        {
            if (!IsPredefined(context.index()))
                throw new UnsupportedSyntaxException("User-defined constants", context.index().Start);

            var predefinedTypeIndex = int.Parse(context.index().NUMBER().ToString());
            var constantExpression = PredefinedObjects.GetConstantExpressionOfType(predefinedTypeIndex);
            Oc5Model.Constants.Add(constantExpression);

            return null;
        }

        #endregion

        #region Signals

        public override Expression VisitSignals([NotNull] SignalsContext context)
        {
            if (int.Parse(context.NUMBER().ToString()) != context.signal().Length)
                throw new InvalidSyntaxException("Count does not match number of signals", context.NUMBER().Symbol);

            VisitChildren(context);

            return null;
        }

        public override Expression VisitSignal([NotNull] SignalContext context)
        {
            Signal signal = null;

            if (context.channel().single() != null)
            {
                var varIndex = int.Parse(context.channel().single().index().NUMBER().ToString());

                if (context.nature().input() != null)
                {
                    if (context.nature().input().presAction().HYPHEN() == null)
                        throw new UnsupportedSyntaxException("Present action of signal", context.nature().input().presAction().Start);

                    signal = new SingleInputSignal { Name = context.nature().input().IDENTIFIER().ToString(), VarIndex = varIndex };
                }
                else if (context.nature().output() != null)
                {
                    if (context.nature().output().outAction().HYPHEN() == null)
                        throw new UnsupportedSyntaxException("Output action of signal", context.nature().output().outAction().Start);

                    signal = new SingleOutputSignal { Name = context.nature().output().IDENTIFIER().ToString(), VarIndex = varIndex };
                }
            }
            else
                throw new UnsupportedSyntaxException($"Channel {context.channel()}", context.channel().Start);

            if (context.@bool() != null)
            {
                signal.BoolIndex = int.Parse(context.@bool().index().NUMBER().ToString());
            }

            Oc5Model.Signals.Add(signal);

            return null;
        }

        #endregion

        #region Variables 

        public override Expression VisitVariables([NotNull] VariablesContext context)
        {
            if (int.Parse(context.NUMBER().ToString()) != context.variable().Length)
                throw new InvalidSyntaxException("Count does not match number of signals", context.NUMBER().Symbol);

            VisitChildren(context);

            return null;
        }

        public override Expression VisitVariable([NotNull] VariableContext context)
        {
            if (!IsPredefined(context.index()))
                throw new UnsupportedSyntaxException("User-defined index", context.index().Start);

            var predefinedTypeIndex = GetIndexNumber(context.index());
            var variableExpression = PredefinedObjects.GetVariableExpression(predefinedTypeIndex);
            Oc5Model.Variables.Add(variableExpression);

            PredefinedObjects.AddVariableToOc5State(Oc5State, predefinedTypeIndex);

            return null;
        }

        #endregion

        #region Actions

        public override Expression VisitActions([NotNull] ActionsContext context)
        {
            if (int.Parse(context.NUMBER().ToString()) != context.action().Length)
                throw new InvalidSyntaxException("Count does not match number of signals", context.NUMBER().Symbol);

            VisitChildren(context);

            return null;
        }

        public override Expression VisitPresentAction([NotNull] PresentActionContext context)
        {
            var signalIndex = GetIndexNumber(context.index());
            var signal = Oc5Model.Signals[signalIndex];
            var variable = Oc5Model.Variables[signal.VarIndex];
            var action = PredefinedObjects.GetPresentAction(variable);
            Oc5Model.Actions.Add(action);

            return null;
        }

        public override Expression VisitIfAction([NotNull] IfActionContext context)
        {
            var action = VisitExpression(context.expression());
            action = PredefinedObjects.GetIfAction(action);
            Oc5Model.Actions.Add(action);

            return null;
        }

        public override Expression VisitDszAction([NotNull] DszActionContext context)
        {
            var index = GetIndexNumber(context.index());
            var variable = Oc5Model.Variables[index];
            var action = PredefinedObjects.GetDszAction(variable);
            Oc5Model.Actions.Add(action);

            return null;
        }

        #endregion

        #region Expression

        public override Expression VisitAtomExpression([NotNull] AtomExpressionContext context)
        {
            return VisitAtomValue(context.atomValue());
        }

        public override Expression VisitAtomValue([NotNull] AtomValueContext context)
        {
            if (context.NUMBER() != null)
            {
                var value = int.Parse(context.NUMBER().ToString());
                return Expression.Constant(value);
            }
            else if (context.DECIMAL() != null)
            {
                var value = double.Parse(context.DECIMAL().ToString());
                return Expression.Constant(value);
            }
            else if (context.STRING() != null)
            {
                var value = context.STRING().ToString();
                return Expression.Constant(value);
            }
            else
                throw new InvalidOperationException("VisitAtomValue");
        }

        public override Expression VisitConstantExpression([NotNull] ConstantExpressionContext context)
        {
            if (IsPredefined(context.index()))
            {
                var predefinedConstantIndex = GetIndexNumber(context.index());
                return PredefinedObjects.GetConstantExpression(predefinedConstantIndex);
            }
            else
            {
                var index = GetIndexNumber(context.index());
                return Oc5Model.Constants[index];
            }
        }

        public override Expression VisitVariableExpression([NotNull] VariableExpressionContext context)
        {
            var index = GetIndexNumber(context.index());
            return Oc5Model.Variables[index];
        }

        public override Expression VisitFunctionCallExpression([NotNull] FunctionCallExpressionContext context)
        {
            if (!IsPredefined(context.index()))
                throw new UnsupportedSyntaxException("User-defined functions", context.index().Start);

            var functionIndex = GetIndexNumber(context.index());

            var expressions = new List<Expression>();

            foreach (var child in context.expressionList().children)
            {
                if (child is ExpressionContext)
                    expressions.Add(VisitExpression(child as ExpressionContext));
            }

            return PredefinedObjects.GetFunctionExpression(functionIndex, expressions.ToArray());
        }

        public override Expression VisitCallAction([NotNull] CallActionContext context)
        {
            var procedureIndex = GetIndexNumber(context.index()[0]);
            var variableIndex = GetIndexNumber(context.index()[1]);

            var variable = Oc5Model.Variables[variableIndex];
            var expression = VisitExpression(context.expression());

            var action = PredefinedObjects.GetProcedure(procedureIndex, variable, expression);
            Oc5Model.Actions.Add(action);

            return null;
        }

        public override Expression VisitOutputAction([NotNull] OutputActionContext context)
        {
            var index = GetIndexNumber(context.index());
            var signal = Oc5Model.Signals[index];
            var variableindex = signal.VarIndex;
            var variable = Oc5Model.Variables[variableindex];
            var variableAsObject = Expression.Convert(variable, typeof(object));

            var action = Expression.Call(
                typeof(Console).GetMethod("WriteLine", new Type[] { typeof(object) }),
                variableAsObject
            );

            Oc5Model.Actions.Add(action);
            return null;
        }

        #endregion

        #region Automaton

        public override Expression VisitStates([NotNull] StatesContext context)
        {
            if (int.Parse(context.NUMBER().ToString()) != (context.parent as AutomatonContext).state().Length)
                throw new InvalidSyntaxException("Count does not match number of states", context.NUMBER().Symbol);

            return null;
        }

        public override Expression VisitStartpoint([NotNull] StartpointContext context)
        {
            Oc5State.CurrentState = GetIndexNumber(context.index());

            return null;
        }

        public override Expression VisitState([NotNull] StateContext context)
        {
            CurrentReturnLabelTarget = Expression.Label(typeof(int), "Newstate return label");
            var returnLabelExpression = Expression.Label(CurrentReturnLabelTarget, Expression.Constant(-1));
            var actiontreeExpression = VisitActionTree(context.actionTree());

            Expression state = Expression.Block(actiontreeExpression, returnLabelExpression);
            Oc5Model.States.Add(state);

            return null;
        }

        public override Expression VisitActionTree([NotNull] ActionTreeContext context)
        {
            if (context.openTestList() != null)
                return Expression.Block(VisitOpenTestList(context.openTestList()), VisitClosedDag(context.closedDag()));

            return VisitClosedDag(context.closedDag());
        }

        public override Expression VisitNewState([NotNull] NewStateContext context)
        {
            var lalExpression = VisitLinearActionList(context.linearActionList());

            var stateIndex = GetIndexNumber(context.index());
            var returnValueExpression = Expression.Constant(stateIndex);
            var returnExpression = Expression.Return(CurrentReturnLabelTarget, returnValueExpression);

            return Expression.Block(lalExpression, returnExpression);
        }

        public override Expression VisitClosedTest([NotNull] ClosedTestContext context)
        {
            var lalExpression = VisitLinearActionList(context.linearActionList());

            var index = GetIndexNumber(context.index());
            var test = Expression.Convert(Oc5Model.Actions[index], typeof(bool));
            var ifTrue = VisitActionTree(context.actionTree()[0]);
            var ifFalse = VisitActionTree(context.actionTree()[1]);

            var ifExpression = Expression.IfThenElse(test, ifTrue, ifFalse);

            return Expression.Block(lalExpression, ifExpression);
        }

        public override Expression VisitOpenTestList([NotNull] OpenTestListContext context)
        {
            var expressions = new List<Expression>();

            foreach (var opentest in context.openTest())
            {
                var expression = VisitOpenTest(opentest);
                expressions.Add(expression);
            }

            return Expression.Block(expressions);
        }

        public override Expression VisitOpenTest([NotNull] OpenTestContext context)
        {
            var lalExpression = VisitLinearActionList(context.linearActionList());

            int testActionIndex = GetIndexNumber(context.index());

            var test = Oc5Model.Actions[testActionIndex];

            var ifTrue = default(Expression);
            var ifFalse = default(Expression);

            // linearActionList index OPEN_PARENTHESIS openDag CLOSE_PARENTHESIS
            // OPEN_PARENTHESIS actionTree CLOSE_PARENTHESIS
            if (context.children[3] is OpenDagContext && context.children[6] is ActionTreeContext)
            {
                ifTrue = VisitOpenDag(context.openDag()[0]);
                ifFalse = VisitActionTree(context.actionTree());
            }
            // linearActionList index OPEN_PARENTHESIS actionTree CLOSE_PARENTHESIS
            // OPEN_PARENTHESIS openDag CLOSE_PARENTHESIS
            else if (context.children[3] is ActionTreeContext && context.children[6] is OpenDagContext)
            {
                ifTrue = VisitActionTree(context.actionTree());
                ifFalse = VisitOpenDag(context.openDag()[0]);
            }
            // linearActionList index OPEN_PARENTHESIS openDag CLOSE_PARENTHESIS
            // OPEN_PARENTHESIS openDag CLOSE_PARENTHESIS
            else
            // if (context.children[3] is OpenDagContext && context.children[6] is OpenDagContext)
            {
                ifTrue = VisitOpenDag(context.openDag()[0]);
                ifFalse = VisitOpenDag(context.openDag()[1]);
            }

            var openTestExpression = Expression.IfThenElse(test, ifTrue, ifFalse);

            return Expression.Block(lalExpression, openTestExpression);
        }

        public override Expression VisitOpenDag([NotNull] OpenDagContext context)
        {
            Expression lalExpression = VisitLinearActionList(context.linearActionList());

            if (context.openTestList() != null)
            {
                var otlExpression = VisitOpenTestList(context.openTestList());
                return Expression.Block(otlExpression, lalExpression);
            }

            return lalExpression;
        }

        public override Expression VisitLinearActionList([NotNull] LinearActionListContext context)
        {
            if (context.NUMBER().Length == 0)
                return Expression.Empty();


            var expressions = new List<Expression>();

            foreach (var num in context.NUMBER())
            {
                var index = int.Parse(num.ToString());
                var action = Oc5Model.Actions[index];
                expressions.Add(action);
            }

            return Expression.Block(expressions);
        }

        #endregion

        #region Utility

        private bool IsPredefined(IndexContext context)
        {
            return context.DOLLAR_SIGN() != null;
        }

        private int GetIndexNumber(IndexContext context)
        {
            return int.Parse(context.NUMBER().ToString());
        }

        #endregion
    }
}
