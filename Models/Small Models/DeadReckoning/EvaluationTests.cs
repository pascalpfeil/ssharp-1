﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2017, Institute for Software & Systems Engineering
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace SafetySharp.CaseStudies.SmallModels.DeadReckoning
{
	using System;
	using System.Collections.Generic;
	using Analysis;
	using Bayesian;
	using ISSE.SafetyChecking;
	using ISSE.SafetyChecking.MinimalCriticalSetAnalysis;
	using ISSE.SafetyChecking.Modeling;
	using ModelChecking;
	using NUnit.Framework;
	using System.Linq;
	using ISSE.SafetyChecking.DiscreteTimeMarkovChain;
	using ISSE.SafetyChecking.Formula;
	using Runtime;

	public class EvaluationTests
	{
        [Test]
        public void ManuallyCalculateProbabilities()
        {
            var model = new DeadReckoningModel();
            Func<bool> hazard = () => model.Component.Hazard;

            var config = BayesianLearningConfiguration.Default;
			var tolerance = 0.000000001;
	        var stepBounds = 10; //230 for railroad crossing

			var allVars = new List<RandomVariable>();
	        var randomVariableFactory = new RandomVariableFactory(model);
	        var rvFaultF = randomVariableFactory.FromFault(model.Component.FF);
			var rvFaultC = randomVariableFactory.FromFault(model.Component.FC);
			var rvFaultS = randomVariableFactory.FromFault(model.Component.FS);
			var mcs = new MinimalCriticalSet(new HashSet<Fault>() { model.Component.FC, model.Component.FS });
	        var rvMcs = new McsRandomVariable(mcs, new[] { rvFaultC, rvFaultS }, "mcs_FC_FS");
	        var rvHazard = randomVariableFactory.FromState(hazard, "H");
			allVars.AddRange(new RandomVariable[] { rvFaultF, rvFaultC, rvFaultS, rvMcs, rvHazard});

			var probCalculator = new OnDemandProbabilityDistributionCalculator(model, allVars, stepBounds, tolerance, config);


	        var result = probCalculator.CalculateProbability(new RandomVariable[] { rvHazard }, new RandomVariable[] { });
			Console.Out.WriteLine($"Probability of {rvHazard}+: {result}");
			Console.Out.WriteLine();
			GC.Collect();

			result = probCalculator.CalculateProbability(new RandomVariable[] { rvHazard }, new RandomVariable[] { rvFaultF });
			Console.Out.WriteLine($"Probability of {rvHazard}+,{rvFaultF}- : {result}");
			Console.Out.WriteLine();
			GC.Collect();

			probCalculator.WriteProbsToConsole();
		}
		[Test]
		public void CreateMarkovChainWithFalseFormula()
		{
			var model = new DeadReckoningModel();

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.AddFormulaToCheck(new ExecutableStateFormula(() => false));
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateMarkovChainWithHazards()
		{
			var model = new DeadReckoningModel();

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			var result = SafetySharpModelChecker.CalculateProbabilityToReachStateBounded(model, model.Component.Hazard, 50);
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CreateMarkovChainWithHazardRetraversal1()
		{
			var model = new DeadReckoningModel();

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.AddFormulaToCheck(model.Component.Hazard);
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			markovChainGenerator.Configuration.UseAtomarPropositionsAsStateLabels = true;
			var markovChain = markovChainGenerator.GenerateLabeledMarkovChain();

			var retraversalMarkovChainGenerator = new MarkovChainFromMarkovChainGenerator(markovChain);
			retraversalMarkovChainGenerator.Configuration.SuccessorCapacity *= 2;
			retraversalMarkovChainGenerator.AddFormulaToCheck(model.Component.Hazard);
			retraversalMarkovChainGenerator.Configuration.UseCompactStateStorage = true;
			retraversalMarkovChainGenerator.Configuration.UseAtomarPropositionsAsStateLabels = true;
			retraversalMarkovChainGenerator.GenerateLabeledMarkovChain();
		}

		[Test]
		public void CreateMarkovChainWithHazardsRetraversal2()
		{
			var model = new DeadReckoningModel();

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.AddFormulaToCheck(model.Component.Hazard);
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			markovChainGenerator.Configuration.UseAtomarPropositionsAsStateLabels = false;
			var markovChain = markovChainGenerator.GenerateLabeledMarkovChain();

			var retraversalMarkovChainGenerator = new MarkovChainFromMarkovChainGenerator(markovChain);
			retraversalMarkovChainGenerator.Configuration.SuccessorCapacity *= 2;
			retraversalMarkovChainGenerator.AddFormulaToCheck(model.Component.Hazard);
			retraversalMarkovChainGenerator.Configuration.UseCompactStateStorage = true;
			retraversalMarkovChainGenerator.Configuration.UseAtomarPropositionsAsStateLabels = false;
			retraversalMarkovChainGenerator.GenerateLabeledMarkovChain();
		}


		[Test]
		public void CreateMarkovChainWithHazardFaultsInState()
		{
			var model = new DeadReckoningModel();

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			var result = SafetySharpModelChecker.CalculateProbabilityToReachStateBounded(model, model.Component.Hazard, 50);
			foreach (var fault in model.Faults)
			{
				var faultFormula = new FaultFormula(fault);
				markovChainGenerator.AddFormulaToCheck(faultFormula);
				markovChainGenerator.AddFormulaToPlainlyIntegrateIntoStateSpace(faultFormula);
			}
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}


		[Test]
		public void CreateFaultAwareMarkovChainAllFaults()
		{
			var model = new DeadReckoningModel();

			var createModel = SafetySharpRuntimeModel.CreateExecutedModelFromFormulasCreator(model);

			var markovChainGenerator = new MarkovChainFromExecutableModelGenerator<SafetySharpRuntimeModel>(createModel) { Configuration = SafetySharpModelChecker.TraversalConfiguration };
			markovChainGenerator.Configuration.SuccessorCapacity *= 2;
			markovChainGenerator.Configuration.MomentOfIndependentFaultActivation = MomentOfIndependentFaultActivation.OnFirstMethodWithoutUndo;
			foreach (var fault in model.Faults)
			{
				var faultFormula = new FaultFormula(fault);
				markovChainGenerator.AddFormulaToCheck(faultFormula);
			}
			var result = SafetySharpModelChecker.CalculateProbabilityToReachStateBounded(model, model.Component.Hazard, 50);
			markovChainGenerator.Configuration.UseCompactStateStorage = true;
			var markovChain = markovChainGenerator.GenerateMarkovChain();
		}

		[Test]
		public void CalculateHazardSingleCore()
		{
			var model = new DeadReckoningModel();

			SafetySharpModelChecker.TraversalConfiguration.CpuCount = 1;
			var result = SafetySharpModelChecker.CalculateProbabilityToReachStateBounded(model, model.Component.Hazard, 50);
			SafetySharpModelChecker.TraversalConfiguration.CpuCount = Int32.MaxValue;
			Console.Write($"Probability of hazard: {result}");
		}



		[Test]
		public void CalculateHazardWithoutEarlyTermination()
		{
			var model = new DeadReckoningModel();

			SafetySharpModelChecker.TraversalConfiguration.EnableEarlyTermination = false;
			var result = SafetySharpModelChecker.CalculateProbabilityToReachStateBounded(model, model.Component.Hazard, 50);
			SafetySharpModelChecker.TraversalConfiguration.EnableEarlyTermination = true;
			Console.Write($"Probability of hazard: {result}");
		}

	}
}