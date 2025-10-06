using Zenject;
using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Scripts.Presentation.View;
using Game.Scripts.Presentation.Presenters;
using Game.Scripts.Presentation.View.Interfaces;

namespace Game.Scripts.Infrastructure.DI.Installers
{
    // Инсталлер для слоя презентера
    public class PresentationInstaller : MonoInstaller
    {
        [Header("Building")]
        [SerializeField] private BuildingPanelView _buildingPanelView;
        [SerializeField] private BuildingPreviewView _buildingPreviewPresenter;
        
        [Header("Map")]
        [SerializeField] private GridHighlightShaderMaskView _gridHighlighterView;
        [SerializeField] private Tilemap _mainTilemap; 
        
        [Header("Hud")]
        [SerializeField] private MenuView _menuView;
        
        public override void InstallBindings()
        {
            Container.Bind<MenuView>().FromInstance(_menuView).AsSingle();
            Container.Bind<Tilemap>().FromInstance(_mainTilemap).AsSingle();
            
            Container.Bind<IBuildingPanelView>().FromInstance(_buildingPanelView).AsSingle();
            Container.Bind<IGridHighlightView>().FromInstance(_gridHighlighterView).AsSingle();
            Container.Bind<IBuildingPreviewView>().FromInstance(_buildingPreviewPresenter).AsSingle();
            
            Container.BindInterfacesTo<BuildingPreviewPresenter>().AsSingle();
            Container.BindInterfacesTo<BuildingPanelPresenter>().AsSingle();
            Container.BindInterfacesTo<TilemapHighlightPresenter>().AsSingle();
            Container.BindInterfacesTo<BuildingInstancesPresenter>().AsSingle();
        }
    }
}