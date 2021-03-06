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

namespace Tests.SimpleExecutableModel.Analysis.ProbabilisticNondeterministic
{
	using ISSE.SafetyChecking.MarkovDecisionProcess.Unoptimized;
	using System;
	using ISSE.SafetyChecking;
	using ISSE.SafetyChecking.ExecutedModel;
	using ISSE.SafetyChecking.Formula;
	using ISSE.SafetyChecking.MarkovDecisionProcess;
	using ISSE.SafetyChecking.Modeling;
	using Shouldly;
	using Xunit;
	using Xunit.Abstractions;
	using LtmdpModelChecker = ISSE.SafetyChecking.MarkovDecisionProcess.Unoptimized.LtmdpModelChecker;

	public class SimpleExample1b : AnalysisTest
	{
		public SimpleExample1b(ITestOutputHelper output = null) : base(output)
		{
		}

		private void Check(AnalysisConfiguration configuration)
		{
			var m = new Model();
			Probability minProbabilityOfFinal1;
			Probability minProbabilityOfFinal2;
			Probability minProbabilityOfFinal3;
			Probability maxProbabilityOfFinal1;
			Probability maxProbabilityOfFinal2;
			Probability maxProbabilityOfFinal3;

			var final1Formula = new UnaryFormula(new SimpleStateInRangeFormula(1), UnaryOperator.Finally);
			var final2Formula = new UnaryFormula(new SimpleStateInRangeFormula(2), UnaryOperator.Finally);
			var final3Formula = new UnaryFormula(new SimpleStateInRangeFormula(3), UnaryOperator.Finally);

			var mdpGenerator = new SimpleMarkovDecisionProcessFromExecutableModelGenerator(m);
			mdpGenerator.Configuration = configuration;
			mdpGenerator.AddFormulaToCheck(final1Formula);
			mdpGenerator.AddFormulaToCheck(final2Formula);
			mdpGenerator.AddFormulaToCheck(final3Formula);
			var mdp = mdpGenerator.GenerateLabeledTransitionMarkovDecisionProcess();
			var modelChecker = new ConfigurationDependentLtmdpModelChecker(configuration, mdp, Output.TextWriterAdapter());
			using (modelChecker)
			{
				minProbabilityOfFinal1 = modelChecker.CalculateMinimalProbability(final1Formula);
				minProbabilityOfFinal2 = modelChecker.CalculateMinimalProbability(final2Formula);
				minProbabilityOfFinal3 = modelChecker.CalculateMinimalProbability(final3Formula);
				maxProbabilityOfFinal1 = modelChecker.CalculateMaximalProbability(final1Formula);
				maxProbabilityOfFinal2 = modelChecker.CalculateMaximalProbability(final2Formula);
				maxProbabilityOfFinal3 = modelChecker.CalculateMaximalProbability(final3Formula);
			}

			minProbabilityOfFinal1.Is(0.4, 0.000001).ShouldBe(true);
			minProbabilityOfFinal2.Is(0.0, 0.000001).ShouldBe(true);
			minProbabilityOfFinal3.Is(0.0, 0.000001).ShouldBe(true);
			maxProbabilityOfFinal1.Is(0.4, 0.000001).ShouldBe(true);
			maxProbabilityOfFinal2.Is(0.6, 0.000001).ShouldBe(true);
			maxProbabilityOfFinal3.Is(0.6, 0.000001).ShouldBe(true);
		}
		

		[Fact]
		public void CheckLtmdp()
		{
			var configuration = AnalysisConfiguration.Default;
			configuration.ModelCapacity = ModelCapacityByMemorySize.Small;
			configuration.UseCompactStateStorage = true;
			configuration.CpuCount = 1;
			configuration.DefaultTraceOutput = Output.TextWriterAdapter();
			configuration.WriteGraphvizModels = true;
			configuration.LtmdpModelChecker = ISSE.SafetyChecking.LtmdpModelChecker.BuiltInLtmdp;

			Check(configuration);
		}

		[Fact(Skip = "NotImplementedYet")]
		public void CheckNmdp()
		{
			var configuration = AnalysisConfiguration.Default;
			configuration.ModelCapacity = ModelCapacityByMemorySize.Small;
			configuration.DefaultTraceOutput = Output.TextWriterAdapter();
			configuration.WriteGraphvizModels = true;
			configuration.LtmdpModelChecker = ISSE.SafetyChecking.LtmdpModelChecker.BuiltInNmdp;

			Check(configuration);
		}

		[Fact]
		public void CheckMdpWithNewStates()
		{
			var configuration = AnalysisConfiguration.Default;
			configuration.ModelCapacity = ModelCapacityByMemorySize.Small;
			configuration.DefaultTraceOutput = Output.TextWriterAdapter();
			configuration.UseCompactStateStorage = true;
			configuration.CpuCount = 1;
			configuration.WriteGraphvizModels = true;
			configuration.LtmdpModelChecker = ISSE.SafetyChecking.LtmdpModelChecker.BuildInMdpWithNewStates;
			Check(configuration);
		}

		[Fact]
		public void CheckMdpWithFlattening()
		{
			var configuration = AnalysisConfiguration.Default;
			configuration.ModelCapacity = ModelCapacityByMemorySize.Small;
			configuration.DefaultTraceOutput = Output.TextWriterAdapter();
			configuration.WriteGraphvizModels = true;
			configuration.LtmdpModelChecker = ISSE.SafetyChecking.LtmdpModelChecker.BuildInMdpWithFlattening;
			Check(configuration);
		}

		public class Model : SimpleModelBase
		{
			public override Fault[] Faults { get; } = new Fault[0];
			public override bool[] LocalBools { get; } = { false };
			public override int[] LocalInts { get; } = new int[0];

			private bool L
			{
				get { return LocalBools[0]; }
				set { LocalBools[0] = value; }
			}

			private int Y
			{
				get { return State; }
				set { State = value; }
			}

			public override void SetInitialState()
			{
				State = 0;
			}

			public override void Update()
			{
				if (Y != 0)
					return;
				L = Choice.Choose(
					new Option<bool>(new Probability(0.6), true),
					new Option<bool>(new Probability(0.4), false));
				Y = 1;
				if (L)
				{
					Y = Choice.Choose(2, 3);
				}
			}
		}
	}
}