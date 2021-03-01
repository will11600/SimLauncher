using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimLauncher;
using SimLauncher.DataTemplates;
using System.Collections.ObjectModel;

namespace AvaloniaSimLauncher.ViewModels
{

    public class MainWindowViewModel : ViewModelBase
    {
        public bool ModListEnabled { get; set; } = true;
        public ObservableCollection<Mod> Items { get; protected set; }


        private string modSearchText = "";
        public string ModSearchText
        {
            get => modSearchText;
            set
            {
                modSearchText = value;
                PopulateModList();
            }
        }

        private void PopulateModList()
        {
            using (var db = ContentManager.InitDatabase())
            {
                var collection = db.GetCollection<Mod>("mods");
                if (ModSearchText.Length < 1) { Items = new ObservableCollection<Mod>(collection.FindAll()); }
                else
                {
                    var nameSearch = collection.Find(m => m.name.Contains(ModSearchText, StringComparison.OrdinalIgnoreCase));

                    IEnumerable<Mod>? tagSearch = null;
                    try { tagSearch = collection.Find(m => string.Join(" ", m.categories).Contains(ModSearchText, StringComparison.OrdinalIgnoreCase)); }
                    catch (NotImplementedException e) { Debug.WriteLine(e); }

                    if (tagSearch == null) { Items = new ObservableCollection<Mod>(nameSearch); }
                    else { Items = new ObservableCollection<Mod>(nameSearch.Union(tagSearch)); }
                }
            }
        }

        public MainWindowViewModel()
        {
            PopulateModList();
        }
    }

}
