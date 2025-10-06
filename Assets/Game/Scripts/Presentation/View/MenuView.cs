using Zenject;
using MessagePipe;
using UnityEngine;
using UnityEngine.UI;
using Game.Scripts.Usecases.Application.Messages;

namespace Game.Scripts.Presentation.View
{
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;

        private IPublisher<LevelSaveEvent> _savePublisher;
        private IPublisher<LevelLoadEvent> _loadPublisher;
        
        [Inject]
        private void Constructor(IPublisher<LevelSaveEvent> savePublisher,
            IPublisher<LevelLoadEvent> loadPublisher)
        {
            _savePublisher = savePublisher;
            _loadPublisher = loadPublisher;
        }

        private void Awake()
        {
            _saveButton.onClick.AddListener(HandleSaveButton);
            _loadButton.onClick.AddListener(HandleLoadButton);
        }

        private void HandleSaveButton() => _savePublisher?.Publish(new LevelSaveEvent());
        private void HandleLoadButton() => _loadPublisher?.Publish(new LevelLoadEvent());
        
        private void OnDestroy()
        {
            _saveButton.onClick.RemoveListener(HandleSaveButton);
            _loadButton.onClick.RemoveListener(HandleLoadButton);
        }
    }
}