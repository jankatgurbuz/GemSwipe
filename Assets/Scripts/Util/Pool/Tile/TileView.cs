using UnityEngine;

namespace Util.Pool.Tile
{
    public class TileView : MonoBehaviour, IPoolable
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

        public void SetPosition(Vector3 vec)
        {
            _transform.position = vec;
        }
    }
}