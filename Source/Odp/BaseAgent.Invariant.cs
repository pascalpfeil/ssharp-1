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
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public partial class BaseAgent<TAgent, TTask>
		where TAgent : BaseAgent<TAgent, TTask>
		where TTask : class, ITask
	{
		public static class Invariant
		{
			public static IEnumerable<TTask> IOConsistency(TAgent agent)
			{
				return RoleInvariant(
					agent,
					role => (role.PreCondition.Port == null || agent.Inputs.Contains(role.PreCondition.Port))
						&& (role.PostCondition.Port == null || agent.Outputs.Contains(role.PostCondition.Port))
				);
			}

			public static IEnumerable<TTask> ResourceConsistency(TAgent agent)
			{
				if (agent.Resource != null
					&& !agent.AllocatedRoles.Any(role => role.PreCondition.Task == agent.Resource.Task
					   && role.PreCondition.State.SequenceEqual(agent.Resource.State)))
					return new[] { agent.Resource.Task };
				return Enumerable.Empty<TTask>();
			}

			public static IEnumerable<TTask> CapabilityConsistency(TAgent agent)
			{
				return RoleInvariant(
					agent,
					role => role.CapabilitiesToApply.All(cap => agent.AvailableCapabilities.Contains(cap))
				);
			}

			public static IEnumerable<TTask> PrePostConditionConsistency(TAgent agent)
			{
				return RoleInvariant(
					agent,
					role =>
						// consistent input:
						(role.PreCondition.Port == null // no input...
						|| role.PreCondition.Port.AllocatedRoles.Exists( // ... or there's a matching role at the input
							sendingRole => sendingRole.PostCondition.StateMatches(role.PreCondition)
								&& sendingRole.PostCondition.Port == agent
							)
						)
						// consistent output:
						&& (role.PostCondition.Port == null // no output...
						|| role.PostCondition.Port.AllocatedRoles.Exists( // ... or there's a matching role at the output
							receivingRole => receivingRole.PreCondition.StateMatches(role.PostCondition)
								&& receivingRole.PreCondition.Port == agent
							)
						)
				);
			}

			public static IEnumerable<TTask> TaskEquality(TAgent agent)
			{
				return RoleInvariant(agent, role => role.PreCondition.Task == role.PostCondition.Task);
			}

			public static IEnumerable<TTask> StateConsistency(TAgent agent)
			{
				return RoleInvariant(agent, role => role.PreCondition.State.Concat(role.CapabilitiesToApply)
					.SequenceEqual(role.PostCondition.State));
			}

			// TODO: ProduceConsumeAssurance, ProducerConsumerRoles need to know the difference between
			// produce, process, consume capabilities. They also need global knowledge.

			// StateIsPrefix invariant: ensured by Condition.State implementation

			public static IEnumerable<TTask> NeighborsAliveGuarantee(TAgent agent)
			{
				return RoleInvariant(
					agent,
					role => (role.PreCondition.Port?.IsAlive ?? true) && (role.PostCondition.Port?.IsAlive ?? true)
				);
			}

			private static IEnumerable<TTask> RoleInvariant(TAgent agent, Predicate<Role<TAgent, TTask>> invariant)
			{
				return (from role in agent.AllocatedRoles
						where !invariant(role)
						select role.Task).Distinct();
			}

			public static InvariantPredicate RoleInvariant(Predicate<Role<TAgent, TTask>> invariant)
			{
				return (agent) => RoleInvariant(agent, invariant);
			}
		}
	}
}