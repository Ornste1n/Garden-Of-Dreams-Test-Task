using System;
using System.IO;
using Game.Scripts.Domain.Application;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Game.Scripts.Infrastructure.Data.Config;

namespace Game.Scripts.Editor.BuildingConfigWindow
{
    public static class BuildingDatabaseService
    {
        private const string DefaultRelativePath = "Assets/Game/Repository/buildings.json";

        private static string GetDatabasePath() => DefaultRelativePath;

        public static BuildingEntriesConfig Load()
        {
            string path = GetDatabasePath();
            if (File.Exists(path) == false)
            {
                BuildingEntriesConfig emptyEntriesConfig = new();
                Save(emptyEntriesConfig);
                AssetDatabase.Refresh();
                return emptyEntriesConfig;
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<BuildingEntriesConfig>(json) ?? new BuildingEntriesConfig();
        }

        private static void Save(BuildingEntriesConfig entriesConfig)
        {
            string path = GetDatabasePath();
            string directory = Path.GetDirectoryName(path);
            if (directory != null && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonConvert.SerializeObject(entriesConfig, Formatting.Indented);
            File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }

        public static void Add(BuildingEntry entry)
        {
            BuildingEntriesConfig entriesConfig = Load();
            entriesConfig.AddEntry(entry);
            Save(entriesConfig);
        }
        
        public static bool Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;

            BuildingEntriesConfig entriesConfig = Load();
            if (entriesConfig == null) return false;
    
            bool removed = entriesConfig.RemoveEntry(id);
            if (!removed) return false;
            
            BackupDatabaseFile();
            Save(entriesConfig);

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