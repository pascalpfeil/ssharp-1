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

namespace Tests.Analysis.Invariants.CounterExamples
{
	using System;
	using SafetySharp.Analysis;
	using SafetySharp.Modeling;
	using Shouldly;

	internal class InitialStateException : AnalysisTestObject
	{
		protected override void Check()
		{
			Check(new C());
			Check(new D());
			Check(new E());
		}

		private void Check(Component c)
		{
			var e = Should.Throw<AnalysisException>(() => CheckInvariant(true, c));
			e.CounterExample.StepCount.ShouldBe(1);

			Should.Throw<InvalidOperationException>(() => SimulateCounterExample(e.CounterExample, simulator => { })).Message.ShouldBe("test");
		}

		private class C : Component
		{
			protected internal override void Initialize()
			{
				throw new InvalidOperationException("test");
			}
		}

		private class D : Component
		{
			protected internal override void Initialize()
			{
				if (Choose(true, false))
					throw new InvalidOperationException("test");
			}
		}

		private class E : Component
		{
			protected internal override void Initialize()
			{
				if (Choose(false, true))
					throw new InvalidOperationException("test");
			}
		}
	}
}