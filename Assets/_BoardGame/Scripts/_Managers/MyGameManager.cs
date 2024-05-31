using UnityEngine;
using System;

namespace BoardGame
{
    public class MyGameManager
    {
        public EventsManager EventsManager;
        
        public static MyGameManager Instance;
        private readonly Action _onCompleteAction;

        public MyGameManager(Action onComplete)
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError($"Two {typeof(MyGameManager)} instances exist, didn't create new one");
                return;
            }

            _playerObjectsData = Resources.Load<PlayerObjectsData>("PlayerObjectsData");
            
            _onCompleteAction = onComplete;
            InitManagers();
        }
        
        private void InitManagers()
        {
            new EventsManager(result =>
            {
                EventsManager = (EventsManager)result;
                _onCompleteAction.Invoke();
                // new PlayerManager(result =>
                // {
                //     PlayerManager = (PlayerManager)result;
                //     _onCompleteAction.Invoke();
                // });
            });
        }

        public int Round;
        public MergeGameState CurrentState;
        private PlayerObjectsData _playerObjectsData;

        public void ChangeState(MergeGameState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case MergeGameState.GenerateLevel:
                   EventsManager.InvokeEvent(EventsType.GenerateGrid,null);
                    break;
                case MergeGameState.SpawningBlocks:
                    var temp = Round++ == 0 ? 2 : 1;
                    EventsManager.InvokeEvent(EventsType.SpawnBlocks,temp);
                    break;
                case MergeGameState.WaitingInput:
                    break;
                case MergeGameState.Moving:
                    break;
                case MergeGameState.Win:
                    _playerObjectsData._winScreen.SetActive(true);
                    break;
                case MergeGameState.Lose:
                    _playerObjectsData._loseScreen.SetActive(true);
                    break;
            }
        }
    }
    
    public enum MergeGameState
    {
        GenerateLevel,
        SpawningBlocks,
        WaitingInput,
        Moving,
        Win,
        Lose
    }
}