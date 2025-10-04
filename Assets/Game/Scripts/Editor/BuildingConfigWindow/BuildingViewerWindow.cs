using System.Linq;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Game.Scripts.Infrastructure.Databases.Building;

namespace Game.Scripts.Editor.BuildingConfigWindow
{
    public sealed class BuildingViewerWindow : OdinEditorWindow
    {
        [SerializeField] private BuildingEntriesDb _entriesDb;

        [SerializeField]
        [LabelText("Selected Entry Id")]
        [ValueDropdown("GetEntryIds", IsUniqueList = true)]
        private string _selectedEntryId = string.Empty;

        [MenuItem("CityBuilder/Viewer")]
        private static void OpenWindow()
        {
            BuildingViewerWindow window = GetWindow<BuildingViewerWindow>("Buildings Viewer");
            window.position = new Rect(Screen.width/2 - 225, Screen.height/2 - 325, 550f, 650f);
            window.LoadDatabase();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            _entriesDb = BuildingDatabaseService.Load();

            // Если выбранный id отсутствует в новой базе — сбросить выбор
            if (string.IsNullOrEmpty(_selectedEntryId) == false &&
                (_entriesDb == null || _entriesDb.Entries.All(e => e == null || e.Id != _selectedEntryId)))
            {
                _selectedEntryId = string.Empty;
            }

            Repaint();
        }

        [TableList, ShowInInspector]
        [ListDrawerSettings(Expanded = true, IsReadOnly = true, HideAddButton = true, HideRemoveButton = true)]
        public IReadOnlyList<BuildingEntry> Entries => _entriesDb == null ? new List<BuildingEntry>() : _entriesDb.Entries;
        
        private IEnumerable<string> GetEntryIds()
        {
            if (_entriesDb == null || _entriesDb.Entries == null) yield break;

            foreach (var entry in _entriesDb.Entries)
            {
                if (entry == null) continue;
                if (string.IsNullOrEmpty(entry.Id)) continue;
                yield return entry.Id;
            }
        }

        [PropertyOrder(1000)]
        [Button("Delete Selected", ButtonSizes.Large)]
        private void DeleteSelected()
        {
            if (string.IsNullOrEmpty(_selectedEntryId) == true)
            {
                EditorUtility.DisplayDialog("Error", "Select an entry to delete.", "Ok");
                return;
            }

            string title = "Confirm Deletion";
            string message = string.Concat("Delete entry '", _selectedEntryId, "' from buildings.json? A backup will be created.");
            bool confirmed = EditorUtility.DisplayDialog(title, message, "Delete", "Cancel");
            if (confirmed == false)
            {
                return;
            }

            bool removed = BuildingDatabaseService.Remove(_selectedEntryId);
            if (removed == true)
            {
                EditorUtility.DisplayDialog("Deleted", "Entry has been removed and database saved. Backup created.", "Ok");
                _selectedEntryId = string.Empty;
                LoadDatabase();
            }
            else
            {
                EditorUtility.DisplayDialog("Not found", "Entry with selected id was not found.", "Ok");
                LoadDatabase();
            }
        }

        [Button("Refresh Database")]
        private void Refresh()
        {
            LoadDatabase();
        }
    }
}
