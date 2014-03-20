using System;

namespace MonoCounters.Web.Models
{
    public class HistoryModel
    {
        public static History History { get; private set; }

        public static void Initialize(History history)
        {
            History = history;
        }
    }
}

