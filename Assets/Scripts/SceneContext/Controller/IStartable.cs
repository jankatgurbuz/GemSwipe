using Cysharp.Threading.Tasks;

namespace SceneContext.Controller
{
    public interface IStartable
    {
        UniTask Start();
    }
}