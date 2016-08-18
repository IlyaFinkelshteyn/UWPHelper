﻿using System;
using System.Diagnostics;

namespace UWPHelper.Utilities
{
    public static class DebugMessages
    {
        [Conditional("DEBUG")]
        public static void OperationInfo(string name, string operation, bool success)
        {
            Debug.WriteLine($"{name} {operation} {(success ? "succeeded" : "failed")} at {DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}");
        }
    }
}