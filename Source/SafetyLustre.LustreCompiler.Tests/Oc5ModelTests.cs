﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2018, Institute for Software & Systems Engineering
// Copyright (c) 2018, Pascal Pfeil
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace SafetyLustre.LustreCompiler.Tests
{
    [TestClass]
    public class Oc5ModelTests
    {
        [DataTestMethod]
        [DataRow(new object[] { 1, 2 }, 6, 0)]
        [DataRow(new object[] { 2, 2 }, 8, 0)]
        public void TestExample1(object[] input, int expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/example1.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;
            runner.Tick(input);

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }

        [DataTestMethod]
        [DataRow(new object[] { true }, false, 1)]
        [DataRow(new object[] { false }, false, 2)]
        public void TestExample2(object[] input, bool expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/example2.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;
            runner.Tick(input);

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }

        [DataTestMethod]
        [DataRow(new object[] { true }, false, 1)]
        [DataRow(new object[] { false }, false, 2)]
        public void TestExample3(object[] input, bool expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/example3.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;
            runner.Tick(input);

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }

        [DataTestMethod]
        [DataRow(new object[] { true, true, true }, true, 1)]
        [DataRow(new object[] { false, false, false }, false, 2)]
        public void TestExample4(object[] input, bool expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/example4.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;
            runner.Tick(input);

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }

        [DataTestMethod]
        [DataRow(new object[] { true, true, true }, true, 1)]
        [DataRow(new object[] { false, false, false }, false, 2)]
        public void TestExample5(object[] input, bool expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/example5.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;
            runner.Tick(input);

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }

        [DataTestMethod]
        [DataRow(new object[] { 2, 2, true, true }, 2, 1)]
        [DataRow(new object[] { 2, 2, false, false }, 2, 1)]
        public void TestExample6(object[] input, int expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/example6.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;
            runner.Tick(input);

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }

        [DataTestMethod]
        [DataRow(new object[] { true }, 15, 3)]
        [DataRow(new object[] { false }, 0, 1)]
        public void TestPressureTank(object[] input, int expectedOutput, int expectedState)
        {
            //Arrange
            var oc5Source = File.ReadAllText("Examples/pressureTank.oc");

            //Act
            var runner = new Oc5Runner(oc5Source);
            var oc5ModelState = runner.Oc5ModelState;

            for (int i = 0; i < 5; i++)
            {
                runner.Tick(input);
            }

            //Assert
            Assert.AreEqual(oc5ModelState.CurrentState, expectedState);
            Assert.AreEqual(oc5ModelState.GetOutputs().SingleOrDefault(), expectedOutput);
        }
    }
}
