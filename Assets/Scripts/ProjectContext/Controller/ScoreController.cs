using System;
using Cysharp.Threading.Tasks;
using SceneContext.Controller;
using Signals;
using UnityEngine;
using Zenject;

namespace ProjectContext.Controller
{
    public class ScoreController : IStartable
    {
        private GameController _gameController;
        private int _score = 0;
        private int _move = 0;
        public int Score => _score;
        public int Move => _move;

        public ScoreController(SignalBus signalBus,GameController gameController)
        {
            signalBus.Subscribe<GameStateReaction>(GameStateOnReaction);
            signalBus.Subscribe<ScoreAndMoveReaction>(ScoreAndMoveOnReaction);
            _gameController = gameController;
        }

        public UniTask Start()
        {
            Restart();
            return UniTask.CompletedTask;
        }

        private void GameStateOnReaction(GameStateReaction reaction)
        {
            if (reaction.GameStatus == GameController.GameStatus.Restart)
            {
                Restart();
            }
        }

        private void ScoreAndMoveOnReaction(ScoreAndMoveReaction reaction)
        {
            switch (reaction.ScoreAndMove)
            {
                case GameController.ScoreAndMove.Move:
                    _move--;
                    break;
                case GameController.ScoreAndMove.Score:
                    _score++;
                    if (_score >= 100)
                    {
                        _score = 100;
                        _gameController.Success();
                    }

                    break;
            }
        }

        private void Restart()
        {
            _score = 0;
            _move = 15;
        }
    }
}