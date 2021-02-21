using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimLauncher
{
    namespace Database
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
        }
    }
}
