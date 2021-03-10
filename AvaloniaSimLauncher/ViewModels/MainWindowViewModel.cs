using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimLauncher;
using SimLauncher.DataTemplates;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Reactive;

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
                FilterModList();
            }
        }

        private List<Mod> GetModsFromDb()
        {
            var mods = new List<Mod>();
            using (var db = ContentManager.InitDatabase())
            {
                mods.InsertRange(0, db.GetCollection<Mod>("mods").FindAll());
                return mods;
            }
        }

        private void FilterModList()
        {
            GetModsFromDb().ForEach(m => { if (!Items.Contains(m)) { Items.Add(m); } });
            if (ModSearchText.Length < 1) { return; }
            Items.Where(i => !i.name.Contains(ModSearchText, StringComparison.OrdinalIgnoreCase)).ToList().ForEach(i => Items.Remove(i));
            Items.Where(i => !string.Join(" ", i.categories).Contains(ModSearchText, StringComparison.OrdinalIgnoreCase)).ToList().ForEach(i => Items.Remove(i));
        }

        public MainWindowViewModel()
        {
            Items = new ObservableCollection<Mod>(GetModsFromDb());
        }
    }

}
