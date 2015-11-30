﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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

namespace Tests.Execution.Faults.ProvidedPorts
{
	using SafetySharp.Modeling;
	using Shouldly;
	using Utilities;

	internal class X8 : TestModel
	{
		protected sealed override void Check()
		{
			Create(new D());
			var d = (D)RootComponents[0];

			d.F1.OccurrenceKind = OccurrenceKind.Never;
			d.M().ShouldBe(101);

			d.F1.OccurrenceKind = OccurrenceKind.Always;
			d.M().ShouldBe(111);
		}

		private class C : Component
		{
			public readonly TransientFault F1 = new TransientFault();

			public virtual int M() => 1;

			[FaultEffect(Fault = nameof(F1))]
			public class F1Effect : C
			{
				public override int M() => base.M() + 10;
			}
		}

		private class D : C
		{
			public override int M() => base.M() + 100;
		}
	}
}