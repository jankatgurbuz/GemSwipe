using System.Threading.Tasks;
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
            Restart
        }

        public GameController(SignalBus signalBus)
        {
            _signalBus = signalBus;
            Debug.Log("Game Controller has been started");
        }

        public void StartGame()
        {
           // _signalBus.Fire(new GameStateReaction(GameStatus.StartGame));
        }
    }
}