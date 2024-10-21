using UnityEngine;

namespace Core.Player
{
    [CreateAssetMenu(fileName = "NewTeamColorLookUp", menuName = "Team Color Look Up")]
    public class TeamColorLookUp : ScriptableObject
    {
        [SerializeField] private Color[] teamColors;

        public Color GetTeamColor(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= teamColors.Length)
            {
                return Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            }
            else
            {
                return teamColors[teamIndex];
            }
        }
    }
}
