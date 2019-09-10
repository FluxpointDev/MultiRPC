using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MultiRPC.JsonClasses
{
    public class CustomProfile
    {
        public string ClientID = "";
        public string LargeKey = "";
        public string LargeText = "";
        public string Name = "";
        public bool ShowTime = false;
        public string SmallKey = "";
        public string SmallText = "";
        public string Text1 = "";
        public string Text2 = "";
        public Triggers Triggers = new Triggers();
    }

    public class Triggers
    {
        /// <summary>
        /// Trigger for when a process is started/close
        /// </summary>
        public string Process = null;

        /// <summary>
        /// Trigger for when contents of a folder have been changed
        /// </summary>
        public string FolderChange = "";

        /// <summary>
        /// Trigger for when contents of a file have been changed
        /// </summary>
        public string FileChange = "";

        public TimeSpan TimerLength = TimeSpan.Zero;
        public Time Time = new Time();
    }

    public class Time
    {
        public TimeSpan StartTime = TimeSpan.Zero;
        public TimeSpan EndTime = TimeSpan.Zero;
        public List<DayOfWeek> Days = new List<DayOfWeek>(7);
    }
}