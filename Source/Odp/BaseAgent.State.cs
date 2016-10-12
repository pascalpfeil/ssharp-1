﻿// The MIT License (MIT)
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

namespace SafetySharp.Odp
{
	public partial class BaseAgent<TAgent, TTask>
	{
		public class State
		{
			public State(BaseAgent<TAgent, TTask> agent, IAgent requestSource = null, params InvariantPredicate[] violatedPredicates)
			{
				ReconfRequestSource = requestSource;
				ViolatedPredicates = violatedPredicates;

				Agent = (TAgent)agent;

				Resource = agent.Resource;
				Inputs = agent.Inputs.ToArray();
				Outputs = agent.Outputs.ToArray();
				AllocatedRoles = agent.AllocatedRoles.ToArray();
			}

			public IAgent ReconfRequestSource { get; }
			public InvariantPredicate[] ViolatedPredicates { get; }

			// TODO: including the agent defeats the purpose of all the properties below
			public TAgent Agent { get; }

			public Resource<TTask> Resource { get; }
			public TAgent[] Inputs { get; }
			public TAgent[] Outputs { get; }
			public Role<TAgent, TTask>[] AllocatedRoles { get; }
		}
	}
}