using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Photon Player Implementation
        [SerializeField, HideInInspector]
        private List<StarterAssets.ThirdPersonController> _allPlayersControl = new List<StarterAssets.ThirdPersonController>();
        private List<Player> _allPlayers = new List<Player>();

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            _allPlayers.Clear();
            foreach(Player player in PhotonNetwork.PlayerList)
            {
                _allPlayers.Add(player);
            }
        }

        
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _allPlayers.Remove(otherPlayer);
            UpdatePlayerCount();
        }

        public void AddPlayer(StarterAssets.ThirdPersonController thirdPersonController)
        {
            if (thirdPersonController == null)
                return;
            _allPlayersControl.Add(thirdPersonController);
            UpdatePlayerCount();
        }
        #endregion

        #region Game Control

        private WaveManager waveManager;
        private int noOfMonsters = 0;
        private int noOfPlayersInGame, noOfPlayersDead;

        public enum GameMode
        {
            PreGame, GameStarted, PostGame
        };

        private GameMode gameMode;

        public void StartGame()
        {
            gameMode = GameMode.GameStarted;
        }

        public void EndGame()
        {
            gameMode = GameMode.PostGame;
        }

        public GameMode GetGameMode() 
        {
            return gameMode;
        }



        private void Start()
        {
            waveManager = FindObjectOfType<WaveManager>();
            Debug.Log("Game Started");
            StartGame();
        }

        private void Update()
        {
            if(gameMode == GameMode.GameStarted && noOfPlayersInGame == noOfPlayersDead)
            {
                Debug.Log("ERRRORRO");
                LoseGame();
            }
        }

        public void WinGame()
        {
            //UI
            //SFX
            Debug.Log("YOU WIN!");
            EndGame();
        }

        public void LoseGame()
        {
            //UI
            //SFX
            Debug.Log("YOU LOSE!");
            EndGame();
        }

        public void UpdatePlayerCount()
        {
            noOfPlayersInGame = PhotonNetwork.CurrentRoom.PlayerCount;
        }

        public void UpdateDeadPlayers()
        {
            noOfPlayersDead++;
        }






        #endregion






    }

}
