using TMPro;
using UnityEngine;

namespace UI.Lobby
{
    public class LobbyItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text lobbyNameText, lobbyPlayersText;

        private LobbiesList lobbiesList;
        private Unity.Services.Lobbies.Models.Lobby lobby;
        
        public void Initialise(LobbiesList lobbiesList, Unity.Services.Lobbies.Models.Lobby lobby)
        {
            this.lobbiesList = lobbiesList;
            this.lobby = lobby;
            
            lobbyNameText.text = lobby.Name;
            lobbyPlayersText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
        }

        public void Join()
        {
            lobbiesList.JoinAsync(lobby);
        }
    }
}
