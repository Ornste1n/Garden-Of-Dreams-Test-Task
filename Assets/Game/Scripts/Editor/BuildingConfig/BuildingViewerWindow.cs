using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Game.Scripts.Editor.BuildingConfig
{
    public sealed class BuildingViewerWindow : OdinEditorWindow
    {
        [SerializeField]
        private BuildingDatabase _database;

        [SerializeField]
        private string _selectedEntryId = string.Empty;

        [MenuItem("CityBuilder/Viewer")]
        private static void OpenWindow()
        {
            BuildingViewerWindow window = GetWindow<BuildingViewerWindow>("Buildings Viewer");
            window.position = new Rect(100f, 100f, 900f, 500f);
            window.LoadDatabase();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            _database = BuildingDatabaseService.Load();
            int count = 0;
            if (_database != null && _database.Entries != null)
            {
                count = _database.Entries.Count;
            }

            Debug.Log("BuildingViewerWindow: loaded entries = " + count);
            Repaint();
        }

        [ShowInInspector]
        [TableList]
        [ListDrawerSettings(Expanded = true, IsReadOnly = true, HideAddButton = true, HideRemoveButton = true)]
        public List<BuildingEntry> Entries
        {
            get
            {
                if (_database == null)
                {
                    return new List<BuildingEntry>();
                }

                return _database.Entries;
            }
        }

        [PropertyOrder(900)]
        [LabelText("Selected entry to delete")]
        [ValueDropdown("GetEntriesDropdown")]
        public string SelectedEntryId
        {
            get
            {
                return _selectedEntryId;
            }
            set
            {
                _selectedEntryId = value;
            }
        }

        private IEnumerable<ValueDropdownItem<string>> GetEntriesDropdown()
        {
            List<ValueDropdownItem<string>> items = new List<ValueDropdownItem<string>>();
            if (_database == null || _database.Entries == null)
            {
                return items;
            }

            for (int i = 0; i < _database.Entries.Count; i++)
            {
                BuildingEntry entry = _database.Entries[i];
                if (entry == null)
                {
                    continue;
                }

                string label = string.Concat(entry.DisplayName, " (", entry.Id, ")");
                items.Add(new ValueDropdownItem<string>(label, entry.Id));
            }

            return items;
        }

        [Button("Delete Selected", ButtonSizes.Large)]
        [PropertyOrder(1000)]
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
