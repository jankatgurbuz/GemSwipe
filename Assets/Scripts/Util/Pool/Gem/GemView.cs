using BoardItems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Util.Handlers.Strategies;

namespace Util.Pool.Gem
{
    public class GemView : MonoBehaviour, IPoolable, IItemBehavior, IMoveable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SOGemSettings _gemSettings;

        private Transform _transform;
        private GameObject _gameObject;
        private Vector3 _currentScale;
        private Quaternion _currentRotation;
        
        private Sequence _finalMovement;
        private Sequence _swipe;
        private Sequence _startMovement;

        public void Awake()
        {
            _transform = transform;
            _gameObject = gameObject;
        }

        public void Create()
        {
            _currentRotation = _transform.rotation;
            _currentScale = transform.localScale;
        }

        public void Active()
        {
        }

        public void Inactive()
        {
            _transform.DOKill();
            _startMovement?.Kill();
            _swipe?.Kill();
            _finalMovement.Kill();
            ResetItem();
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public Transform GetTransform()
        {
            return _transform;
        }

        private void ResetItem()
        {
            _transform.localScale = _currentScale;
            _transform.rotation = _currentRotation;
        }

        public void SetPosition(Vector3 position)
        {
            _transform.position = position;
        }

        public Vector3 GetPosition()
        {
            return _transform.position;
        }

        public void SetActive(bool active)
        {
            _gameObject.SetActive(active);
        }

        public async UniTask Pop()
        {
            await _transform.DOScale(Vector3.zero, 0.1f).AsyncWaitForCompletion().AsUniTask();
        }

        public void SetColorAndAddSprite(ItemColors color)
        {
            _spriteRenderer.sprite = _gemSettings[color];
        }

        public void StartMovement(IMovementStrategy strategy)
        {
            _finalMovement?.OnKill(ResetItem).Kill();
            _swipe?.OnKill(ResetItem).Kill();
            
            _startMovement = strategy.StartMovement(_transform);
            _startMovement.Restart();
        }

        public void FinalizeMovementWithBounce(IMovementStrategy strategy)
        {
            _startMovement?.OnKill(ResetItem).Kill();
            _swipe?.OnKill(ResetItem).Kill();
            
            _finalMovement = strategy.FinalMovement(_transform, _currentScale);
            _finalMovement.Restart();
        }

        public async UniTask Swipe(IMovementStrategy movementStrategy, Vector3 position)
        {
            _startMovement?.OnKill(ResetItem).Kill();
            _finalMovement?.OnKill(ResetItem).Kill();
            
            _swipe = movementStrategy.Swipe(_transform, position);
            _swipe.Restart();
            await _swipe.AsyncWaitForCompletion().AsUniTask();
        }
    }
}