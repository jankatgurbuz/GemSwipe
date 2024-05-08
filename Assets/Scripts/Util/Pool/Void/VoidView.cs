using BoardItems;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Util.Pool.Void
{
    public class VoidView : MonoBehaviour, IPoolable, IItemBehavior
    {
        private Transform _transform;
        private GameObject _gameObject;

        public void Awake()
        {
            _transform = transform;
            _gameObject = gameObject;
        }

        public void Create()
        {
        }

        public void Active()
        {
        }

        public void Inactive()
        {
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public Transform GetTransform()
        {
            return _transform;
        }

        public void SetPosition(Vector3 position)
        {
        }

        public Vector3 GetPosition()
        {
            return _transform.position;
        }

        public void SetActive(bool active)
        {
        }

        public UniTask Pop()
        {
            return UniTask.CompletedTask;
        }
    }
}