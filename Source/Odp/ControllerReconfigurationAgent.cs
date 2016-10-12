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
	using System.Collections.Generic;
	using System.Linq;
	using Modeling;

	public class ControllerReconfigurationAgent<TAgent, TTask> : IReconfigurationAgent<TAgent, TTask>
		where TAgent : BaseAgent<TAgent, TTask>
		where TTask : class, ITask
	{
		// used in this class
		private readonly BaseAgent<TAgent, TTask> _baseAgent;
		private readonly ReconfigurationAgentHandler<TAgent, TTask> _reconfAgentHandler;
		private RoleCalculationAgent _roleCalculationAgent;
		private TTask _task;

		// passed on to RoleCalculationAgent
		private readonly BaseAgent<TAgent, TTask>[] _allBaseAgents;
		private readonly IController<TAgent, TTask> _controller;

		public ControllerReconfigurationAgent(
			BaseAgent<TAgent, TTask> baseAgent,
			BaseAgent<TAgent, TTask>[] allBaseAgents,
			IController<TAgent, TTask> controller
		)
		{
			_baseAgent = baseAgent;
			_reconfAgentHandler = baseAgent.ReconfigurationStrategy as ReconfigurationAgentHandler<TAgent, TTask>;

			_allBaseAgents = allBaseAgents;
			_controller = controller;
		}

		public void Acknowledge()
		{
			_roleCalculationAgent.Acknowledge(_task);
		}

		public void StartReconfiguration(TTask task, IAgent agent, BaseAgent<TAgent, TTask>.State baseAgentState)
		{
			_task = task;
			if (_roleCalculationAgent != null) // a configuration is already under way
			{
				_roleCalculationAgent.AcknowledgeReconfigurationRequest(task, this, baseAgentState);
				return;
			}

			if (agent == _baseAgent) // no previous reconf agent exists
			{
				_roleCalculationAgent = new RoleCalculationAgent(_allBaseAgents, _controller);
				_roleCalculationAgent.StartCentralReconfiguration(task, _baseAgent, baseAgentState);
			}
			else // a previous reconf agent already exists
			{
				_roleCalculationAgent = (RoleCalculationAgent)agent;
				_roleCalculationAgent.AcknowledgeReconfigurationRequest(task, this, baseAgentState);
			}
		}

		public void UpdateAllocatedRoles(object rs) // TODO: parameter
		{
			_reconfAgentHandler.UpdateAllocatedRoles(rs);
		}

		public void Go(TTask task)
		{
			_roleCalculationAgent = null;

			_reconfAgentHandler.Go(task);
			_reconfAgentHandler.Done(task);
		}

		protected class RoleCalculationAgent : IAgent
		{
			private readonly BaseAgent<TAgent, TTask>[] _functioningAgents;
			private readonly IController<TAgent, TTask> _controller;

			private readonly Dictionary<TAgent, ControllerReconfigurationAgent<TAgent, TTask>> _reconfAgents
				= new Dictionary<TAgent, ControllerReconfigurationAgent<TAgent, TTask>>();

			private int _ackCounter = 0;

			private enum State { Idle, GatherGlobalKnowledge, CalculateRoles, AllocateRoles }
			private readonly StateMachine<State> _stateMachine = State.Idle;

			public RoleCalculationAgent(BaseAgent<TAgent, TTask>[] allBaseAgents, IController<TAgent, TTask> controller)
			{
				_functioningAgents = allBaseAgents.Where(agent => agent.IsAlive).ToArray();
				_controller = controller;
			}

			public void StartCentralReconfiguration(TTask task, BaseAgent<TAgent, TTask> agent, object bastate)
			{
				_stateMachine.Transition(
					from: State.Idle,
					to: State.GatherGlobalKnowledge,
					action: () => {
						foreach (var baseAgent in _functioningAgents)
							baseAgent.RequestReconfiguration(this, task);
					}
				);
			}

			public void AcknowledgeReconfigurationRequest(TTask task, ControllerReconfigurationAgent<TAgent, TTask> agent, BaseAgent<TAgent, TTask>.State baseAgentState)
			{
				_stateMachine.Transition(
					from: State.GatherGlobalKnowledge,
					to: State.GatherGlobalKnowledge,
					action: () => _reconfAgents.Add(baseAgentState.Agent, agent)
				);
				_stateMachine.Transition(
					from: State.GatherGlobalKnowledge,
					to: State.CalculateRoles,
					guard: _reconfAgents.Count == _functioningAgents.Length,
					action: () => CalculateRoles(task)
				);
			}

			private void CalculateRoles(TTask task)
			{
				var configs = _controller.CalculateConfigurations(task);

				_stateMachine.Transition(
					from: State.CalculateRoles,
					to: State.AllocateRoles,
					action: () => {
						var emptyRoles = new Role<TAgent, TTask>[0];
						foreach (var agent in _functioningAgents.Cast<TAgent>())
						{
							_reconfAgents[agent].UpdateAllocatedRoles(
								configs.ContainsKey(agent) ? configs[agent] : emptyRoles
							);
						}
					}
				);
			}

			public void Acknowledge(TTask task)
			{
				_stateMachine.Transition(
					from: State.AllocateRoles,
					to: State.AllocateRoles,
					action: () => _ackCounter++
				);

				_stateMachine.Transition(
					from: State.AllocateRoles,
					to: State.Idle,
					guard: _ackCounter == _reconfAgents.Count,
					action: () => {
						foreach (var reconfAgent in _reconfAgents.Values)
							reconfAgent.Go(task);
					}
				);
			}
		}
	}
}