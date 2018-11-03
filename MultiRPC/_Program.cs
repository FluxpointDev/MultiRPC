using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MultiRPC
{
    public class IProgram
    {
        public string Client;
        public string Name;
        public bool Auto = false;
        public string ProcessName;
        public ProgramData Data;
        public IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

        public bool Check()
        {
            if (ProcessName == "")
                return true;
            Process[] p = Process.GetProcessesByName(ProcessName);
            if (p.Count() == 0)
                return false;
            else
                return true;
        }

        public Process GetProcess()
        {
            if (ProcessName == "")
                return null;
            Process[] p = Process.GetProcessesByName(ProcessName);
            return p.First();
        }

        public virtual void Update(DiscordRPC.RichPresence rp)
        {

        }

        public class ProgramData
        {
            #region LoadOrSave
            public ProgramData(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"/MultiRPC/";
                    File = Path + $"{name.ToLower()}.json";
                    if (!Directory.Exists(Path))
                        Directory.CreateDirectory(Path);
                    if (!System.IO.File.Exists(File))
                    {
                        using (StreamWriter file = System.IO.File.CreateText(File))
                        {
                            JsonSerializer serializer = new JsonSerializer
                            {
                                Formatting = Formatting.Indented
                            };
                            serializer.Serialize(file, this);
                        }
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(File))
                        {
                            JsonSerializer serializer = new JsonSerializer
                            {
                                Formatting = Formatting.Indented
                            };
                            ProgramData Data = (ProgramData)serializer.Deserialize(reader, typeof(ProgramData));
                            Priority = Data.Priority;
                            Enabled = Data.Enabled;
                        }
                    }
                }
            }
            #endregion

            public int Priority = 0;
            public bool Enabled = false;
            private readonly string Path;
            private readonly string File;
            public void Save()
            {
                using (StreamWriter file = System.IO.File.CreateText(File))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };
                    serializer.Serialize(file, this);
                }
            }
        }

    }
    public enum ContentType
    {
        Small, Random, Full, Custom
    }
}
