using ProjectContext.Controller;

namespace Signals
{
    public class GameStateReaction
    {
        public readonly GameController.GameStatus GameStatus;
        public GameStateReaction(GameController.GameStatus gameStatus)
        {
            GameStatus = gameStatus;
        }
    }
}
