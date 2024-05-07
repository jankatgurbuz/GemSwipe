using BoardItems;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SceneContext.Controller
{
    public class InGameController : IStartable
    {
        public SOLevelData LevelData;
        public async UniTask Start()
        {
            const string assetPath = "Assets/Levels/Level1.asset";
            var locationHandle = Addressables.LoadResourceLocationsAsync(assetPath);
            await locationHandle.ToUniTask();
            if (locationHandle.Result.Count > 0)
            {
                LevelData = await Addressables.LoadAssetAsync<SOLevelData>(assetPath);
            }
            else
            {
                Debug.LogWarning("Scene not found in build settings: " + assetPath);
            }
        }
    }
}
