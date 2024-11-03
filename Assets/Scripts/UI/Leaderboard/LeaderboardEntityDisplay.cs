using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace UI.Leaderboard
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;
        
        public LeaderboardEntityState LeaderboardEntityState { get; private set; }
        
        public int TeamIndex { get; private set; }

        public void Initialise(LeaderboardEntityState leaderboardEntityState)
        {
            LeaderboardEntityState = leaderboardEntityState;
            
            UpdateCoins(leaderboardEntityState.Coins);
        }
        

        public void SetColor(Color color)
        {
            displayText.color = color;
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
