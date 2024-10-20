using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace UI.Leaderboard
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;
        
        [SerializeField] private Color myColor;
        public LeaderboardEntityState LeaderboardEntityState { get; private set; }

        public void Initialise(LeaderboardEntityState leaderboardEntityState)
        {
            LeaderboardEntityState = leaderboardEntityState;

            if (LeaderboardEntityState.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                displayText.color = myColor;
            }
            
            UpdateCoins(leaderboardEntityState.Coins);
        }

        public void UpdateCoins(int coins)
        {
            LeaderboardEntityState.SetCoins(coins);
            UpdateText();
        }
        

        public void UpdateText()
        {
            displayText.text = $"{transform.GetSiblingIndex() + 1}. {LeaderboardEntityState.PlayerName} ({LeaderboardEntityState.Coins})";

        }
    }
}
