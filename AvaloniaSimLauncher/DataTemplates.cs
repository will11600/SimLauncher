using LiteDB;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace SimLauncher
{
    namespace DataTemplates
    {
        public interface IRecord
        {
            int uid { get; set; }
        }

        public class Mod : IRecord
        {
            public int uid { get; set; }
            public string name { get; set; }
            public string[] categories { get; set; }
            public bool isOverride { get; set; }
            public string author { get; set; }
            public int version { get; set; }
            public string hash { get; set; }
            public string url { get; set; }

            public string uiCategories { get => '#' + String.Join(" #", categories); }
            public bool active { get => false; }
        }
        
        public abstract class ProgressBarOperation : ReactiveObject
        {
            public int Min = 0;
            public int Max = 100;

            private string status = "";
            public string Status
            {
                get => status;
                set => this.RaiseAndSetIfChanged(ref status, value);
            }

            private int current = 0;
            public int Current
            {
                get => current;
                set => this.RaiseAndSetIfChanged(ref current, value);
            }

            public abstract Task Main();
        }

        public class ModCollection : IRecord
        {
            public int uid { get; set; }
            public List<int> modUids { get; set; }
            public string author { get; set; }
        }
    }
}
