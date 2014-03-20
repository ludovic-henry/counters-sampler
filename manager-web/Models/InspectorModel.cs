using System;

namespace MonoCounters.Web.Models
{
    public class InspectorModel
    {
        public static Inspector Inspector { get; private set; }

        public static void Initialize(Inspector history)
        {
            Inspector = history;
        }
    }
}

