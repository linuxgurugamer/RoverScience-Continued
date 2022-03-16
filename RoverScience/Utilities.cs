#if false
using System;
using UnityEngine;
using static RoverScience.InitLog;

namespace RoverScience
{
    public static class Utilities
    {
        public static void Log(string msg)
        {
            InitLog.Log.Info(msg);
        }

        public static void LogError(string message)
        {
            InitLog.Log.Error(message);
        }

        public static void LogError(string message, Exception ex)
        {
            InitLog.Log.Error(message);
            InitLog.Log.Exception(ex);
        }

        public static void Log.Detail(string msg)
        {
                InitLog.Log.Detail(msg);
        }

    }
}
#endif
