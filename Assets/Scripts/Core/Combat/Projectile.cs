using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int teamIndex { get; private set; }
    
    public void Initialise(int teamIndex)
    {
        this.teamIndex = teamIndex;
    }
}
