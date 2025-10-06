using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.AddressableAssets;
using Game.Scripts.Domain.Application;
using UnityEditor.AddressableAssets.Settings;

namespace Game.Scripts.Editor.BuildingConfigWindow
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
        
        [VerticalGroup("FrontSide/Properties"), ShowInInspector, Space, ReadOnly, LabelWidth(100)] 
        private string _spriteName;
        
        [VerticalGroup("FrontSide/Properties"), ShowInInspector, ReadOnly, LabelWidth(100)] 
        private string _atlasGuid;
        
        [Button(ButtonSizes.Large)]
        private void AddSelectedPrefab()
        {
            if (_selectedPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Select a prefab first.", "Ok");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(_selectedPrefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorUtility.DisplayDialog("Error", "Selected asset path is empty.", "Ok");
                return;
            }

            string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
            if (string.IsNullOrEmpty(prefabGuid))
            {
                EditorUtility.DisplayDialog("Error", "Could not resolve asset GUID.", "Ok");
                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            TryAddToAddressables(settings, prefabPath, prefabGuid);
            TryAddToAddressables(settings, _spriteName, _atlasGuid);
            
            string idToUse = string.IsNullOrEmpty(_customId) == false ? _customId : Path.GetFileNameWithoutExtension(prefabPath);
            
            BuildingDatabaseService.Add(new BuildingEntry(idToUse, prefabGuid, prefabPath, _atlasGuid, _spriteName));
            EditorUtility.DisplayDialog("Added", "Prefab entry added to buildings.json.", "Ok");

            _selectedPrefab = null;
            _customId = string.Empty;
        }

        private void TryAddToAddressables(AddressableAssetSettings settings, string path, string guid)
        {
            AddressableAssetEntry existingEntry = settings.FindAssetEntry(guid);

            if (existingEntry != null) return;
            
            AddressableAssetGroup group = settings.DefaultGroup;
            if (group == null) return;
            
            AddressableAssetEntry newEntry = settings.CreateOrMoveEntry(guid, group, false, false);
            if (newEntry == null) return;
            
            newEntry.address = Path.GetFileNameWithoutExtension(path);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, newEntry, true);
            
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            
            if(asset == null || !asset.TryGetComponent(out SpriteRenderer renderer)) return;

            _spriteName = renderer.sprite.name;
            string assetPath = AssetDatabase.GetAssetPath(renderer.sprite);
            _atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
        }
    }
}
