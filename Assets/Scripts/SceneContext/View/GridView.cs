using UnityEngine;

namespace SceneContext.View
{
    public class GridView : MonoBehaviour
    {
        [SerializeField] private Grid _grid;
        public Grid Grid => _grid;
    }
}