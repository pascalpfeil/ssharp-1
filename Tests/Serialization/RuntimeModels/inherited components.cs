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

namespace Tests.Serialization.RuntimeModels
{
	using SafetySharp.Modeling;
	using Shouldly;

	internal class InheritedComponents : RuntimeModelTest
	{
		private static bool _hasConstructorRun;

		protected override void Check()
		{
			var c1 = new C1 { F = 99 };
			var c2 = new C2 { F = 45 };
			var c = new C { C1 = c1, C2 = c2 };
			var m = new Model(c);

			_hasConstructorRun = false;
			Create(m);

			StateFormulas.ShouldBeEmpty();
			RootComponents.Length.ShouldBe(1);

			var root = RootComponents[0];
			root.ShouldBeOfType<C>();

			((C)root).C1.ShouldBeOfType<C1>();
			((C)root).C2.ShouldBeOfType<C2>();

			((C)root).C1.F.ShouldBe(99);
			((C)root).C2.F.ShouldBe(45);

			_hasConstructorRun.ShouldBe(false);
		}

		private class C : Component
		{
			public Base C1;
			public Base C2;

			public C()
			{
				_hasConstructorRun = true;
			}
		}

		private class C1 : Base
		{
			public C1()
			{
				_hasConstructorRun = true;
			}

			public override int F { get; set; }
		}

		private class C2 : Base
		{
			public C2()
			{
				_hasConstructorRun = true;
			}

			public override int F { get; set; }
		}

		private abstract class Base : Component
		{
			public abstract int F { get; set; }
		}
	}
}