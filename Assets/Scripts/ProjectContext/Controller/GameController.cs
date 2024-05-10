using Signals;
using UnityEngine;
using Zenject;

namespace ProjectContext.Controller
{
    public class GameController
    {
        private SignalBus _signalBus;

        public enum GameStatus
        {
            Empty,
            StartGame,
            Restart,
            FailPanel,
            SuccessPanel
        }

        public enum ScoreAndMove
        {
            Empty,
            Score,
            Move
        }

        public GameController(SignalBus signalBus)
        {
            _signalBus = signalBus;
            Debug.Log("Game Controller has been started");
        }

        public void StartGame()
        {
            _signalBus.Fire(new GameStateReaction(GameStatus.StartGame));
        }

        public void RestartGame()
        {
            _signalBus.Fire(new GameStateReaction(GameStatus.Restart));
        }

        public void Fail()
        {
            _signalBus.Fire(new GameStateReaction(GameStatus.FailPanel));
        }

        public void Success()
        {
            _signalBus.Fire(new GameStateReaction(GameStatus.SuccessPanel));
        }
    }
}