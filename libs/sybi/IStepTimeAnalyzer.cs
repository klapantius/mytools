﻿using System;
using System.Collections.Generic;
using System.IO;


namespace sybi.RSFA
{
    public interface IStepTimeAnalyzer
    {
        TimeSpan GetScriptDuration(TextReader input);
        List<StepTimeAnalyzer.Result> FindLongestSteps(TextReader input, int maxCount = 10);
    }
}