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

namespace SafetySharp.Odp.Reconfiguration
{
	using System.Collections.Generic;
	using System.Linq;

	public class Coalition
	{
		public ITask Task { get; }

		public CoalitionReconfigurationAgent Leader { get; }

		public List<CoalitionReconfigurationAgent> Members { get; }
			= new List<CoalitionReconfigurationAgent>();

		public HashSet<ICapability> AvailableCapabilities { get; } = new HashSet<ICapability>();
		public HashSet<ICapability> NeededCapabilities { get; } = new HashSet<ICapability>();

		// TODO: hide or make readonly properties
		internal int _ctfStart = -1;
		internal int _ctfEnd = -1; // exclusive

		public Coalition(CoalitionReconfigurationAgent leader, ITask task)
		{
			Task = task;
			Join(Leader = leader);
		}

		public void Join(CoalitionReconfigurationAgent newMember)
		{
			Members.Add(newMember);
			AvailableCapabilities.UnionWith(newMember.BaseAgentState.AvailableCapabilities);
			UpdateCTF(newMember);
		}

		private void UpdateCTF(CoalitionReconfigurationAgent newMember)
		{
			Role firstRole, lastRole;
			if (!newMember.GetConfigurationBounds(Task, out firstRole, out lastRole))
				return;

			if (_ctfStart == -1) // && _ctfEnd == -1
			{
				// first initialization
				_ctfStart = firstRole.PreCondition.StateLength;
				_ctfEnd = lastRole.PostCondition.StateLength;

				NeededCapabilities.UnionWith(
					Task.RequiredCapabilities.Skip(_ctfStart).Take(_ctfEnd - _ctfStart)
				);
				return;
			}

			// interval is enlarged
			if (firstRole.PreCondition.StateLength < _ctfStart)
			{
				//  add [firstRole.PreCondition.StateLength, _ctfStart)
				NeededCapabilities.UnionWith(
					Task.RequiredCapabilities.Skip(firstRole.PreCondition.StateLength)
						 .Take(_ctfStart - firstRole.PreCondition.StateLength)
				);

				_ctfStart = firstRole.PreCondition.StateLength;
			}

			// interval is enlarged
			if (lastRole.PostCondition.StateLength > _ctfEnd || _ctfEnd == -1)
			{
				// add [_ctfEnd, lastRole.PostCondition.StateLength)
				NeededCapabilities.UnionWith(
					Task.RequiredCapabilities.Skip(_ctfEnd)
						 .Take(lastRole.PostCondition.StateLength - _ctfEnd)
				);

				_ctfEnd = lastRole.PostCondition.StateLength;
			}
		}

		public bool CapabilitiesSatisfied() => NeededCapabilities.IsSubsetOf(AvailableCapabilities);
	}
}