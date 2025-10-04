using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Game.Scripts.Infrastructure.Databases.Building;

namespace Game.Scripts.Editor.BuildingConfigWindow
{
    public static class BuildingDatabaseService
    {
        private const string DefaultRelativePath = "Assets/Game/Repository/buildings.json";

        private static string GetDatabasePath() => DefaultRelativePath;

        public static BuildingEntriesDb Load()
        {
            string path = GetDatabasePath();
            if (File.Exists(path) == false)
            {
                BuildingEntriesDb emptyDb = new();
                Save(emptyDb);
                AssetDatabase.Refresh();
                return emptyDb;
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<BuildingEntriesDb>(json) ?? new BuildingEntriesDb();
        }

        private static void Save(BuildingEntriesDb db)
        {
            string path = GetDatabasePath();
            string directory = Path.GetDirectoryName(path);
            if (directory != null && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(db, Formatting.Indented);
            File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }

        public static void Add(BuildingEntry entry)
        {
            BuildingEntriesDb db = Load();
            db.AddEntry(entry);
            Save(db);
        }
        
        public static bool Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            BuildingEntriesDb db = Load();
            if (db == null) return false;
    
            bool removed = db.RemoveEntry(id);
            if (!removed) return false;
            
            BackupDatabaseFile();
            Save(db);

            return true;
        }

        private static void BackupDatabaseFile()
        {
            try
            {
                string path = GetDatabasePath();
                if (File.Exists(path) == false)
                {
                    return;
                }

                string directory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory) == true)
                {
                    return;
                }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = string.Concat(Path.GetFileNameWithoutExtension(path), "_backup_", timestamp, Path.GetExtension(path));
                string backupPath = Path.Combine(directory, backupFileName);
                File.Copy(path, backupPath, true);
                AssetDatabase.ImportAsset(backupPath);
                AssetDatabase.Refresh();
                Debug.Log("BuildingDatabaseService: backup created -> " + backupPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("BuildingDatabaseService: failed to create backup - " + ex.Message);
            }
        }
    }
}