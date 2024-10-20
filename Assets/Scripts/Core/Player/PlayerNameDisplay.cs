using TMPro;
using Unity.Collections;
using UnityEngine;

namespace Core.Player
{
    public class PlayerNameDisplay : MonoBehaviour
    {
        [SerializeField] private TankPlayer player;
    
        [SerializeField] private TMP_Text playerName;
    
        private void Start()
        {
            HandlePlayerNameChanged(string.Empty, player.playerName.Value);
        
            player.playerName.OnValueChanged += HandlePlayerNameChanged;
        }

        private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
        {
            playerName.text = newName.ToString();
        }

        private void OnDestroy()
        {
            player.playerName.OnValueChanged -= HandlePlayerNameChanged;
        }
    }
}
