using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Compression;
using SimLauncher.DataTemplates;
using LiteDB;
using LiteDB.Engine;
using Avalonia.Threading;
using ReactiveUI;

namespace SimLauncher
{
    public static class ContentManager
    {
        public class DbManager : ILiteDatabase
        {
            private static int usingCount = 0;
            private static LiteDatabase db;

            public BsonMapper Mapper => db.Mapper;

            public ILiteStorage<string> FileStorage => db.FileStorage;

            public int UserVersion { get => db.UserVersion; set => db.UserVersion = value; }
            public TimeSpan Timeout { get => db.Timeout; set => db.Timeout = value; }
            public bool UtcDate { get => db.UtcDate; set => db.UtcDate = value; }
            public long LimitSize { get => db.LimitSize; set => db.LimitSize = value; }
            public int CheckpointSize { get => db.CheckpointSize; set => db.CheckpointSize = value; }

            public Collation Collation => throw new NotImplementedException();

            public DbManager()
            {
                if (usingCount < 1)
                {
                    string dbPath = Path.Combine(launcherDir, "launcherData.db");
                    db = new LiteDatabase(dbPath);
                }
                usingCount++;
            }

            public bool BeginTrans() => db.BeginTrans();
            public void Checkpoint() => db.Checkpoint();
            public bool CollectionExists(string name) => db.CollectionExists(name);
            public bool Commit() => db.Commit();
            public bool DropCollection(string name) => db.DropCollection(name);
            public IBsonDataReader Execute(TextReader commandReader, BsonDocument? parameters = null) => db.Execute(commandReader, parameters);
            public IBsonDataReader Execute(string command, BsonDocument? parameters = null) => db.Execute(command, parameters);
            public IBsonDataReader Execute(string command, params BsonValue[] args) => db.Execute(command, args);
            public ILiteCollection<T> GetCollection<T>(string name, BsonAutoId autoId = BsonAutoId.ObjectId) => db.GetCollection<T>(name, autoId);
            public ILiteCollection<T> GetCollection<T>() => db.GetCollection<T>();
            public ILiteCollection<T> GetCollection<T>(BsonAutoId autoId) => db.GetCollection<T>(autoId);
            public ILiteCollection<BsonDocument> GetCollection(string name, BsonAutoId autoId = BsonAutoId.ObjectId) => db.GetCollection(name, autoId);
            public IEnumerable<string> GetCollectionNames() => db.GetCollectionNames();
            public ILiteStorage<TFileId> GetStorage<TFileId>(string filesCollection = "_files", string chunksCollection = "_chunks") => db.GetStorage<TFileId>(filesCollection, chunksCollection);
            public BsonValue Pragma(string name) => db.Pragma(name);
            public BsonValue Pragma(string name, BsonValue value) => db.Pragma(name, value);
            public long Rebuild(RebuildOptions? options = null) => db.Rebuild(options);
            public bool RenameCollection(string oldName, string newName) => db.RenameCollection(oldName, newName);
            public bool Rollback() => db.Rollback();

            public void Dispose()
            {
                usingCount--;
                if (usingCount < 1) { db.Dispose(); }
            }
        }

        private static DbManager db;

        private static string ts3DocDir = @"C:\Users\wb116\Documents\Electronic Arts\Sims 3";
        private static string launcherDir = @"%LOCALAPPDATA%\SimLauncher";
        private static string modPath;

        private static string ResolvePath(string stringWEnvVars)
        {
            return Environment.ExpandEnvironmentVariables(stringWEnvVars);
        }

        public static string[] GetMods()
        {
            var files = new List<string>();
            var foldersToSearch = new Queue<string>();

            foldersToSearch.Enqueue(modPath);

            while (foldersToSearch.Count > 0)
            {
                var folder = foldersToSearch.Dequeue();
                Array.ForEach(Directory.GetDirectories(folder), f => foldersToSearch.Enqueue(f));
                files.AddRange(Directory.GetFiles(folder, "*.package"));
            }

            return files.ToArray();
        }

        private static string GetHashSHA1(this byte[] data)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                return string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }

        private static Tuple<bool, string[]>? GetPathMetaData(string path)
        {
            var match = Regex.Match(path, @"(Overrides|Packages)\\(.*(?=\\))");
            if (!match.Success) { return null; }
            return new Tuple<bool, string[]>(match.Groups[1].Value == "Overrides", match.Groups[2].Value.Split('\\'));
        }

        public static DbManager InitDatabase()
        {
            return new DbManager();
        }

        static ContentManager()
        {
            launcherDir = ResolvePath(launcherDir);
            ts3DocDir = ResolvePath(ts3DocDir);
            modPath = Path.Combine(ts3DocDir, "Mods");

            if (!Directory.Exists(launcherDir)) { Directory.CreateDirectory(launcherDir); }
        }

        internal class ModLoader : ProgressBarOperation
        {
            private List<Task> tasks = new List<Task>();
            private int max;

            public override async Task Main()
            {
                db = InitDatabase();

                Status = "Reading mod list from database...";
                var col = db.GetCollection<Mod>("mods");
                Status = "Scanning mod directory...";
                var mods = GetMods();

                max = mods.Count() * 2;

                foreach (var mod in mods)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var modName = Path.GetFileNameWithoutExtension(mod);
              
                        if (col.Find(m => m.name == modName).Count() < 1)
                        {
                            lock (Status) { Status = $"Importing: {mod}..."; }
                            var record = LoadFromDisk(mod);
                            record.uid = col.Count();
                            lock (col)
                            {
                                col.Insert(record);
                                col.EnsureIndex(m => m.uid);
                            }

                            tasks.Add(ArchiveModAsync(mod, record.uid));
                        }
                    }));
                }

                mods = null;

                while (tasks.Count() > 0)
                {
                    tasks.Remove(await Task.WhenAny(tasks.ToArray()));
                    Current = (int)((Current / max) * 100f);
                }

                Status = "Cleaning up...";

                col = null;
                db.Dispose();
            }
        }

        private static Mod LoadFromDisk(string path)
        {
            var meta = GetPathMetaData(path);
            Debug.Assert(meta != null, "Unable to obtain meta data from mod file", path);

            var modName = Path.GetFileNameWithoutExtension(path);

            var mod = new Mod
            {
                name = modName,
                hash = File.ReadAllBytes(path).GetHashSHA1(),
                isOverride = meta.Item1,
                categories = meta.Item2
            };

            return mod;
        }

        private static async Task ArchiveModAsync(string path, int uid)
        {
            var file = await File.ReadAllBytesAsync(path);

            using (MemoryStream ms = new MemoryStream())
            using (var gzp = new GZipStream(ms, CompressionLevel.Optimal))
            {
                await gzp.WriteAsync(file, 0, file.Length);
                using (db = InitDatabase()) { db.FileStorage.Upload($"mod{uid}", path, ms); }
            }
        }
        public static void DumpModDb()
        {
            using (db = InitDatabase())
            {
                var sb = new StringBuilder();
                var col = db.GetCollection<Mod>("mods");
                foreach (var m in col.FindAll())
                {
                    sb.AppendLine($"\nMod[0x{m.uid.ToString("X")}]: {m.name}");
                    sb.AppendLine(m.hash);
                    sb.AppendLine(String.Join(", ", m.categories));
                    sb.AppendLine("Overrides=" + m.isOverride);
                }
                File.WriteAllText(Path.Combine(launcherDir, "dump.txt"), sb.ToString());
                sb.Clear();
            }
        }

        public class ResourceEditor
        {
            private static string resourcePath = modPath + @"\Resource.cfg";
            private List<string> lines;

            private int packagesIndex = -1;
            private int overridesIndex = -1;

            public List<string> activeMods;

            public ResourceEditor()
            {
                activeMods = new List<string>();
                lines = new List<string>();

                var rawText = File.ReadAllLines(resourcePath);
                for (int i = 0; i < rawText.Length; i++)
                {
                    string s = rawText[i];

                    var match = Regex.Match(s, @"^PackedFile (?=Overrides)(.*?\.package)$");
                    if (match.Success)
                    {
                        if (overridesIndex == -1)
                        {
                            overridesIndex = lines.Count();
                        }
                        else
                        {
                            if (match.Groups.Count < 2) { continue; }

                            var active = match.Groups[1].Value;
                            if (!active.Contains('*')) { activeMods.Add(active); }
                        }

                        continue;
                    }

                    match = Regex.Match(s, @"^PackedFile (?=Packages)(.*?\.package)$");
                    if (match.Success)
                    {
                        if (packagesIndex == -1)
                        {
                            packagesIndex = lines.Count();
                        }
                        else
                        {
                            if (match.Groups.Count < 2) { continue; }

                            var active = match.Groups[1].Value;
                            if (!active.Contains('*')) { activeMods.Add(active); }
                        }

                        continue;
                    }

                    lines.Add(rawText[i]);
                }
            }

            public bool Update()
            {
                if (activeMods.Count < 1)
                {
                    //var result = DialogueBox.SendWarning("You don't have any mods enabled! Are you sure you want to continue?", MessageBoxButton.OKCancel);
                    //if (result == MessageBoxResult.Cancel) { return false; }
                }

                var modPath = ContentManager.modPath;
                var sb = new StringBuilder();

                activeMods.ForEach(s => s.Replace('\\', '/'));
                lines.GetRange(0, overridesIndex).ForEach(s => sb.AppendLine(s));
                activeMods.Where(s => Regex.IsMatch(s, @"^Overrides.*")).ToList().ForEach(s => sb.AppendLine("PackagedFile " + s));
                lines.GetRange(overridesIndex, packagesIndex - overridesIndex).ForEach(s => sb.AppendLine(s));
                activeMods.Where(s => Regex.IsMatch(s, @"^Packages.*")).ToList().ForEach(s => sb.AppendLine("PackagedFile " + s));
                lines.GetRange(packagesIndex, lines.Count() - packagesIndex).ForEach(s => sb.AppendLine(s));

                File.WriteAllText(Path.Combine(modPath, @"resource1.cfg"), sb.ToString());

                return true;
            }
        }
    }
}
