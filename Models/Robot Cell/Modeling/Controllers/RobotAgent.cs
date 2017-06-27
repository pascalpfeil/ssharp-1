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

namespace SafetySharp.CaseStudies.RobotCell.Modeling.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Plants;
	using SafetySharp.Modeling;
	using Odp;
	using Odp.Reconfiguration;

	internal class RobotAgent : Agent, ICapabilityHandler<ProduceCapability>, ICapabilityHandler<ProcessCapability>, ICapabilityHandler<ConsumeCapability>
	{

        [Reliability(mttf: 10000, mttr: 100)]
        public readonly Fault Broken = new TransientFault();
        [Reliability(mttf: 1000, mttr: 100)]
        public readonly Fault ResourceTransportFault = new TransientFault();

        // In analyses without hardware components, these replace the Tool.Broken faults.
        // When hardware components are included, these faults are ignored.
        [Reliability(mttf: 1000, mttr: 100)]
        public readonly Fault DrillBroken = new TransientFault();
        [Reliability(mttf: 1000, mttr: 100)]
        public readonly Fault InsertBroken = new TransientFault();
        [Reliability(mttf: 1000, mttr: 100)]
        public readonly Fault TightenBroken = new TransientFault();
        [Reliability(mttf: 1000, mttr: 100)]
        public readonly Fault PolishBroken = new TransientFault();

		private ICapability _currentCapability;

		[Hidden(HideElements = true)]
		private readonly List<Task> _tasks;
		private readonly List<Resource> _resources;

	    [Hidden(HideElements = true)]
	    private Dictionary<ProductionAction, List<ICapability>> unusedProductionCapabilites = new Dictionary<ProductionAction, List<ICapability>>();

        public RobotAgent(ICapability[] capabilities, Robot robot, List<Task> tasks, List<Resource> resources)
			: base(capabilities)
		{
			Robot = robot;
			_tasks = tasks;
			_resources = resources;

			Broken.Name = $"{Name}.{nameof(Broken)}";
			ResourceTransportFault.Name = $"{Name}.{nameof(ResourceTransportFault)}";

			AddTolerableFaultEffects();

            foreach (ProductionAction type in Enum.GetValues(typeof(ProductionAction)))
            {
                unusedProductionCapabilites.Add(type, new List<ICapability>());
            }
		}

		protected RobotAgent() { } // for fault effects

		public override string Name => $"R{ID}";

		public Robot Robot { get; }

		protected override void DropResource()
		{
			// For now, the resource disappears magically...
			Robot?.DiscardWorkpiece();

			base.DropResource();
		}

		protected override bool CheckInput(Agent agent)
		{
			return Robot?.CanTransfer() ?? true;
		}

		protected override bool CheckOutput(Agent agent)
		{
			return Robot?.CanTransfer() ?? true;
		}

	    ///<summary> 
	    /// Enable to revaluate the currently available  capabilities
	    /// Thus, it is possible to add tools to an Agent at run time or repair defects 
	    ///</summary>
	    public override void EvaluateCurrentlyAvailableCapabilites()
	    {
	        throw  new NotImplementedException();
	    }

	    public void AddTool(ProcessCapability newCapability)
	    {
            _availableCapabilities.Add(newCapability);   
	    }

	    public void AddTools(IEnumerable<ProcessCapability> newCapabilities)
	    {
	        foreach (var capability in newCapabilities)
	        {
	            AddTool(capability);
	        }
	    }

        /// <summary>
        /// Adds all Capabilites that have been removed by fault
        /// </summary>
	    public void RestoreRobot(Fault fault)
        {
            List<ICapability> capaToAdd; 
            switch (fault.Name)
            {
                case "DrillBroken":
                    capaToAdd = unusedProductionCapabilites[ProductionAction.Drill];
                    break;
                case "InsertBroken":
                    capaToAdd = unusedProductionCapabilites[ProductionAction.Insert];
                    break;
                case "PolishBroken":
                    capaToAdd = unusedProductionCapabilites[ProductionAction.Polish];
                    break;
                case "TightenBroken":
                    capaToAdd = unusedProductionCapabilites[ProductionAction.Tighten];
                    break;
                case "NoneBroken":
                    capaToAdd = unusedProductionCapabilites[ProductionAction.None];
                    break;
                case "Broken":
                    RestoreRobot();
                    return;
                default:
                    return;
            }
            _availableCapabilities.AddRange(capaToAdd);
	    }

        /// <summary>
        /// Adds all Capabilites that have been removed before
        /// </summary>
	    public void RestoreRobot()
        {
            _availableCapabilities.AddRange(unusedProductionCapabilites.Values.SelectMany(x => x));
        }


        protected override bool CheckAllocatedCapability(ICapability capability)
		{
			if (!CanSwitchTools())
				return false;

			var processCapability = capability as ProcessCapability;
			return processCapability == null || CanApply(processCapability);
		}

		protected override void TakeResource(Odp.Resource resource)
		{
			var agent = (CartAgent)CurrentRole?.PreCondition.Port;

			// If we fail to transfer the resource, the robot loses all of its connections
			if (TakeResource(agent.Cart))
			{
				base.TakeResource(resource);
				return;
			}

			Robot?.DiscardWorkpiece();
			ClearConnections();
		}

		protected override void TransferResource()
		{
			var agent = (CartAgent)CurrentRole?.PostCondition.Port;

			// If we fail to transfer the resource, the robot loses all of its connections
			if (PlaceResource(agent.Cart))
			{
				base.TransferResource(); // inform the cart
				return;
			}

			Robot?.DiscardWorkpiece();
			ClearConnections();
		}

		private void ClearConnections()
		{
			// Using ToArray() to prevent removal during collection iteration

			foreach (var input in Inputs.ToArray())
				input.Disconnect(this);

			foreach (var output in Outputs.ToArray())
				Disconnect(output);
		}

		public override bool CanExecute(Role role)
		{
			if (role.CapabilitiesToApply.FirstOrDefault() is ProduceCapability)
			{
				var availableResourceCount = _resources.Count(resource => resource.Task == role.Task);
				return availableResourceCount > 0
					   && !_tasks.Any(task => task.IsResourceInProduction)
					   && base.CanExecute(role);
			}
			return base.CanExecute(role);
		}

		public void ApplyCapability(ProduceCapability capability)
		{
			var index = _resources.FindIndex(resource => resource.Task == CurrentRole?.Task);
			if (index == -1)
				throw new InvalidOperationException("All resources for this task have already been produced");

			Resource = _resources[index];
			_resources.RemoveAt(index);

			(Resource.Task as Task).IsResourceInProduction = true;
			Robot?.ProduceWorkpiece((Resource as Resource).Workpiece);
			Resource.OnCapabilityApplied(capability);
		}

		public void ApplyCapability(ProcessCapability capability)
		{
			if (Resource == null)
				throw new InvalidOperationException("Cannot process when no resource available");

			if (!Equals(_currentCapability, capability))
			{
				// Switch the capability; if we fail to do so, remove all other capabilities from the available ones
				if (SwitchCapability(capability))
					_currentCapability = capability;
				else
				{
				    foreach (var capa in _availableCapabilities.Where(c => !c.Equals(_currentCapability)))
				    {
                        if (capa.GetType() != typeof(ProcessCapability))
                            continue;
                        unusedProductionCapabilites[((ProcessCapability)capa).ProductionAction].Add(capa);
                    }
                    _availableCapabilities.RemoveAll(c => !c.Equals(_currentCapability));
                    return;
				}
			}

			// Apply the capability; if we fail to do so, remove it from the available ones
			if (!ApplyCurrentCapability())
			{
				_availableCapabilities.Remove(capability);
                unusedProductionCapabilites[capability.ProductionAction].Add(capability);
			}
			else
			{
				Resource.OnCapabilityApplied(capability);
			}
		}

		public void ApplyCapability(ConsumeCapability capability)
		{
			if (Resource == null)
				throw new InvalidOperationException("Cannot consume when no resource available");

			Resource.OnCapabilityApplied(capability);
			Robot?.ConsumeWorkpiece();
			(Resource.Task as Task).IsResourceInProduction = false;
			Resource = null;
		}

		public void RestoreRobot()
		{
			// ...
			ReconfigurationMonitor.AttemptTaskContinuance(ReconfigurationStrategy, new State(this));
		}

		public ReconfigurationMonitor ReconfigurationMonitor { get; set; }

		// robot delegation methods
		// This enables tolerable faults to be analyzed both with and without hardware models.
		protected virtual bool CanSwitchTools()			=> Robot?.CanSwitch() ?? true;
		protected virtual bool ApplyCurrentCapability() => Robot?.ApplyCapability() ?? true;
		protected virtual bool TakeResource(Cart cart)	=> Robot?.TakeResource(cart) ?? true;
		protected virtual bool PlaceResource(Cart cart)	=> Robot?.PlaceResource(cart) ?? true;
		protected virtual bool CanApply(ProcessCapability capability)			=> Robot?.CanApply(capability) ?? true;
		protected virtual bool SwitchCapability(ProcessCapability capability)	=> Robot?.SwitchCapability(capability) ?? true;

		private void AddTolerableFaultEffects()
		{
			Broken.Subsumes(ResourceTransportFault);
			if (Robot != null)
				Robot.AddTolerableFaultEffects(Broken, ResourceTransportFault);
			else
			{
				Broken.AddEffect<BrokenEffect>(this);
				ResourceTransportFault.AddEffect<ResourceTransportEffect>(this);

				DrillBroken.AddEffect<DrillBrokenEffect>(this);
				InsertBroken.AddEffect<InsertBrokenEffect>(this);
				TightenBroken.AddEffect<TightenBrokenEffect>(this);
				PolishBroken.AddEffect<PolishBrokenEffect>(this);

				Broken.Subsumes(DrillBroken, InsertBroken, TightenBroken, PolishBroken);
			}
		}

		[FaultEffect, Priority(5)]
		internal class BrokenEffect : RobotAgent
		{
			protected override bool ApplyCurrentCapability() => false;
			protected override bool CanApply(ProcessCapability capability) => false;
			protected override bool TakeResource(Cart cart) => false;
			protected override bool PlaceResource(Cart cart) => false;

			protected override bool CheckInput(Agent agent) => false;
			protected override bool CheckOutput(Agent agent) => false;
		}

		[FaultEffect]
		internal class ResourceTransportEffect : RobotAgent
		{
			protected override bool TakeResource(Cart cart) => false;
			protected override bool PlaceResource(Cart cart) => false;

			protected override bool CheckInput(Agent agent) => false;
			protected override bool CheckOutput(Agent agent) => false;
		}

		// TODO: a common base class for these effects would be nice (once S# supports it)
		[FaultEffect, Priority(1)]
		internal class DrillBrokenEffect : RobotAgent
		{
			protected override bool CanApply(ProcessCapability capability)
				=> capability.ProductionAction != ProductionAction.Drill && base.CanApply(capability);

			protected override bool ApplyCurrentCapability()
				=> (_currentCapability as ProcessCapability)?.ProductionAction != ProductionAction.Drill
					&& base.ApplyCurrentCapability();
		}

		[FaultEffect, Priority(2)]
		internal class InsertBrokenEffect : RobotAgent
		{
			protected override bool CanApply(ProcessCapability capability)
				=> capability.ProductionAction != ProductionAction.Insert && base.CanApply(capability);

			protected override bool ApplyCurrentCapability()
				=> (_currentCapability as ProcessCapability)?.ProductionAction != ProductionAction.Insert
					&& base.ApplyCurrentCapability();
		}

		[FaultEffect, Priority(3)]
		internal class TightenBrokenEffect : RobotAgent
		{
			protected override bool CanApply(ProcessCapability capability)
				=> capability.ProductionAction != ProductionAction.Tighten && base.CanApply(capability);

			protected override bool ApplyCurrentCapability()
				=> (_currentCapability as ProcessCapability)?.ProductionAction != ProductionAction.Tighten
					&& base.ApplyCurrentCapability();
		}

		[FaultEffect, Priority(4)]
		internal class PolishBrokenEffect : RobotAgent
		{
			protected override bool CanApply(ProcessCapability capability)
				=> capability.ProductionAction != ProductionAction.Polish && base.CanApply(capability);

			protected override bool ApplyCurrentCapability()
				=> (_currentCapability as ProcessCapability)?.ProductionAction != ProductionAction.Polish
					&& base.ApplyCurrentCapability();
		}
	}
}