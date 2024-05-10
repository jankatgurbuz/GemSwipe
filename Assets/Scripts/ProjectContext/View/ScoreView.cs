using DG.Tweening;
using ProjectContext.Controller;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace ProjectContext.View
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tmpScore, _tmpMove;
        [SerializeField] private GameObject _restartPanel;
        [SerializeField] private GameObject _successPanel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _successButton;

        [Inject] private SignalBus _signalBus;
        [Inject] private ScoreController _scoreController;
        [Inject] private GameController _gameController;

        private void Start()
        {
            _signalBus.Subscribe<ScoreAndMoveReaction>(ScoreAndMoveOnReaction);
            _signalBus.Subscribe<GameStateReaction>(GameStateOnReaction);
            _restartButton.onClick.AddListener(OnRestart);
            _successButton.onClick.AddListener(OnRestart);
        }

        private void OnRestart()
        {
            _gameController.RestartGame();
        }

        private void GameStateOnReaction(GameStateReaction reaction)
        {
            switch (reaction.GameStatus)
            {
                case GameController.GameStatus.Restart:
                    _restartPanel.SetActive(false);
                    _successPanel.SetActive(false);
                    break;
                case GameController.GameStatus.FailPanel:
                    _restartPanel.transform.localScale = Vector3.zero;
                    _restartPanel.SetActive(true);
                    _restartPanel.transform.DOScale(Vector3.one, 0.25f);
                    break;
                case GameController.GameStatus.SuccessPanel:
                    _successPanel.transform.localScale = Vector3.zero;
                    _successPanel.SetActive(true);
                    _successPanel.transform.DOScale(Vector3.one, 0.25f);
                    break;
                case GameController.GameStatus.StartGame:
                    UpdateText();
                    break;
            }
        }

        private void ScoreAndMoveOnReaction(ScoreAndMoveReaction reaction)
        {
            switch (reaction.ScoreAndMove)
            {
                case GameController.ScoreAndMove.Move:
                    UpdateText();
                    if (_scoreController.Move == 0)
                        _gameController.Fail();
                    break;
                case GameController.ScoreAndMove.Score:
                    UpdateText();
                    break;
            }
        }

        private void UpdateText()
        {
            _tmpMove.text = _scoreController.Move.ToString();
            _tmpScore.text = _scoreController.Score.ToString();
        }
    }
}