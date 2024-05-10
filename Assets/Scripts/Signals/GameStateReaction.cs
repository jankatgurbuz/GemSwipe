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

    public class ScoreAndMoveReaction
    {
        public readonly GameController.ScoreAndMove ScoreAndMove;
        public readonly int PopCount = 0;

        public ScoreAndMoveReaction(GameController.ScoreAndMove scoreAndMove, int popCount = 0)
        {
            ScoreAndMove = scoreAndMove;
            PopCount = popCount;
        }
    }
}