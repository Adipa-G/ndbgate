﻿using System;

namespace dbgate
{
    public class GsInfo
    {
        public static int Error = 1;
        public static int Sucess = 2;
        public static int Warning = 3;

        public GsInfo()
        {
        }

        public GsInfo(int no, String message) : this(no, message, null)
        {
        }

        public GsInfo(int no, String message, Object returnData)
        {
            No = no;
            Message = message;
            ReturnData = returnData;
        }

        public int No { get; set; }

        public string Message { get; set; }

        public object ReturnData { get; set; }
    }
}