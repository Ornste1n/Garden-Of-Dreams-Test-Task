using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace Game.Scripts.Editor.BuildingConfig
{
    public sealed class BuildingAdderWindow : OdinEditorWindow
    {
        #region Elements
        [HorizontalGroup("FrontSide", 125), HideLabel, Title("Front Side", HorizontalLine = false)]
        [PreviewField(125), SerializeField, AssetsOnly, LabelWidth(100), OnValueChanged("GetAssetInfo")]
        private GameObject _selectedPrefab;

        [VerticalGroup("FrontSide/Properties")]
        [SerializeField, PropertySpace(25), LabelWidth(100)]
        private string _customId = string.Empty;
        
        [VerticalGroup("FrontSide/Properties"), ShowInInspector, ReadOnly, LabelWidth(100)]
        private string _assetPath;

        [VerticalGroup("FrontSide/Properties"), ShowInInspector, ReadOnly, LabelWidth(100)] 
        private string _detectedGuid;
        
        [PropertySpace(SpaceBefore = 35)]
        [Button(ButtonSizes.Large)]
        [VerticalGroup("FrontSide/Properties")]
        private void AddSelectedPrefab()
        {
            if (_selectedPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Select a prefab first.", "Ok");
                return;
            }

            string path = AssetDatabase.GetAssetPath(_selectedPrefab);
            if (string.IsNullOrEmpty(path) == true)
            {
                EditorUtility.DisplayDialog("Error", "Selected asset path is empty.", "Ok");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid) == true)
            {
                EditorUtility.DisplayDialog("Error", "Could not resolve asset GUID.", "Ok");
                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                AddressableAssetEntry existingEntry = settings.FindAssetEntry(guid);
                if (existingEntry == null)
                {
                    bool addToAddressables = EditorUtility.DisplayDialog("Add to Addressables?",
                        "Asset is not in Addressables. Add it so it can be loaded via Addressables at runtime?",
                        "Add", "Skip");
                    if (addToAddressables == true)
                    {
                        AddressableAssetGroup group = settings.DefaultGroup;
                        if (group != null)
                        {
                            AddressableAssetEntry newEntry = settings.CreateOrMoveEntry(guid, group, false, false);
                            if (newEntry != null)
                            {
                                newEntry.address = Path.GetFileNameWithoutExtension(path);
                                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, newEntry, true);
                                EditorUtility.SetDirty(settings);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                            }
                        }
                    }
                }
            }

            string idToUse = string.IsNullOrEmpty(_customId) == false ? _customId : Path.GetFileNameWithoutExtension(path);

            BuildingEntry entry = new
            (
                id: idToUse,
                guid: guid,
                assetPath: path,
                displayName: idToUse
            );

            BuildingDatabaseService.Add(entry);
            EditorUtility.DisplayDialog("Added", "Prefab entry added to buildings.json (stored GUID).", "Ok");

            _selectedPrefab = null;
            _customId = string.Empty;
        }
        #endregion
        
        [MenuItem("CityBuilder/Add Building")]
        private static void OpenWindow()
        {
            BuildingAdderWindow window = GetWindow<BuildingAdderWindow>("Add Building");
            window.position = new Rect(Screen.width/2 - 325, Screen.height/2, 650f, 200f);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            _selectedPrefab = null;
            _customId = string.Empty;
        }
        
        private void GetAssetInfo(GameObject asset)
        {
            _assetPath = AssetDatabase.GetAssetPath(asset);
            _detectedGuid = AssetDatabase.AssetPathToGUID(_assetPath);
        }
    }
}
