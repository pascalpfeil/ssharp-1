﻿using SafetyLustre.Oc5Compiler.Oc5Objects;
using System;
using System.Collections.Generic;

namespace SafetyLustre.Oc5Compiler
{
    class Oc5Model
    {
        public List<Signal> Signals { get; set; } = new List<Signal>();




        public List<Func<Oc5State, int>> States { get; set; } = new List<Func<Oc5State, int>>();
    }
}
