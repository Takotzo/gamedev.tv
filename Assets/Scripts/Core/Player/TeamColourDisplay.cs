using System;
using System.Collections;
using System.Collections.Generic;
using Core.Player;
using UnityEngine;

public class TeamColourDisplay : MonoBehaviour
{
    [SerializeField] private TeamColorLookUp teamColorLookUp;
    [SerializeField] private TankPlayer player;
    [SerializeField] private SpriteRenderer[] spriteRenderer;

    private void Start()
    {
        HandleTeamChanged(-1, player.teamIndex.Value);
        
        player.teamIndex.OnValueChanged += HandleTeamChanged;
    }

    private void HandleTeamChanged(int oldTeamIndex, int newTeamIndex)
    {
        Color teamColor = teamColorLookUp.GetTeamColor(newTeamIndex);

        foreach (var sprie in spriteRenderer)
        {
            sprie.material.color = teamColor;
        }
    }

    private void OnDestroy()
    {
        player.teamIndex.OnValueChanged -= HandleTeamChanged;
    }
}
