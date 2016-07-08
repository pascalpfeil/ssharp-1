// The MIT License (MIT)
// 
// Copyright (c) 2014-2016, Institute for Software & Systems Engineering
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

namespace SafetySharp.Analysis.ModelChecking
{
	using System;
	using System.Linq;
	using FormulaVisitors;
	using ModelTraversal.TraversalModifiers;
	using Runtime;

	/// <summary>
	///   Generates a <see cref="StateGraph" /> for an <see cref="AnalysisModel" />.
	/// </summary>
	internal class MarkovChainGenerator : ModelTraverser
	{
		private readonly MarkovChain _markovChain;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="createModel">Creates the model that should be checked.</param>
		/// <param name="stateFormulas">The state formulas that can be evaluated over the generated state graph.</param>
		/// <param name="output">The callback that should be used to output messages.</param>
		/// <param name="configuration">The analysis configuration that should be used.</param>
		internal MarkovChainGenerator(Func<AnalysisModel> createModel, Formula[] stateFormulas,
									 Action<string> output, AnalysisConfiguration configuration)
			: base(createModel, output, configuration)
		{
			var stateLabelCollector = new CollectStateFormulaLabelsVisitor();
			foreach (var stateFormula in stateFormulas)
			{
				stateLabelCollector.Visit(stateFormula);
			}

			_markovChain = new MarkovChain();
			_markovChain.StateFormulaLabels = stateLabelCollector.Labels.ToArray();

			Context.TraversalParameters.BatchedTransitionActions.Add(() => new MarkovChainBuilder(_markovChain));
		}

		/// <summary>
		///   Generates the state graph.
		/// </summary>
		internal MarkovChain GenerateStateGraph()
		{
			if (!Context.Configuration.ProgressReportsOnly)
			{
				Context.Output($"Generating markov chain using {AnalyzedModels.Count()} CPU cores.");
				Context.Output($"State vector has {AnalyzedModels.First().StateVectorSize} bytes.");
			}

			TraverseModel();

			if (!Context.Configuration.ProgressReportsOnly)
				Context.Report();

			RethrowTraversalException();

			return _markovChain;
		}
	}
}