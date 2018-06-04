﻿using SafetyLustre.LustreCompiler.Oc5Objects;
using System;
using System.Collections.Generic;

namespace SafetyLustre.LustreCompiler
{
    class Oc5Model
    {
        public List<Signal> Signals { get; set; } = new List<Signal>();
        public List<Func<Oc5ModelState, int>> Oc5States { get; set; } = new List<Func<Oc5ModelState, int>>();
    }
}