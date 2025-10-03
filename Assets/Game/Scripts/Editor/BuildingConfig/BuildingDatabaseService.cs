using System;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Editor.BuildingConfig
{
    public static class BuildingDatabaseService
    {
        private const string DefaultRelativePath = "Assets/buildings.json";

        public static string GetDatabasePath() => DefaultRelativePath;

        public static BuildingDatabase Load()
        {
            string path = GetDatabasePath();
            if (File.Exists(path) == false)
            {
                BuildingDatabase emptyDb = new BuildingDatabase();
                Save(emptyDb);
                AssetDatabase.Refresh();
                return emptyDb;
            }

            string json = File.ReadAllText(path);
            BuildingDatabase result = JsonConvert.DeserializeObject<BuildingDatabase>(json);
            if (result == null)
            {
                result = new BuildingDatabase();
            }

            return result;
        }

        public static void Save(BuildingDatabase db)
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
            BuildingDatabase db = Load();
            db.AddEntry(entry);
            Save(db);
        }
        
        public static bool Remove(string id)
        {
            if (string.IsNullOrEmpty(id) == true)
            {
                return false;
            }

            BuildingDatabase db = Load();
            if (db == null)
            {
                return false;
            }

            bool removed = db.RemoveEntry(id);
            if (removed == true)
            {
                BackupDatabaseFile();
                Save(db);
            }

            return removed;
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