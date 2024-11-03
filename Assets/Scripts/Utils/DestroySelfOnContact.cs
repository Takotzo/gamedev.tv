using Core.Player;
using UnityEngine;

namespace Utils
{
    public class DestroySelfOnContact : MonoBehaviour
    {
        [SerializeField] private Projectile projectile;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (projectile.teamIndex != -1)
            {
                if (other.attachedRigidbody == null)
                {
                    if (other.attachedRigidbody.TryGetComponent(out TankPlayer player))
                    {
                        if (player.teamIndex.Value == projectile.teamIndex) return;
                    }
                }
            }
          
            Destroy(gameObject);
        }
    }
}
