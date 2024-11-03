using Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class DealDamageOnContact : MonoBehaviour
    {
        [SerializeField] private Projectile projectile;
        [SerializeField] private int damage = 5;

        private ulong ownerClientId;

        public void SetOwner(ulong owner)
        {
            ownerClientId = owner;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) { return; }

            if (projectile.teamIndex != -1)
            {
                if (other.attachedRigidbody.TryGetComponent(out TankPlayer player))
                {
                    if (player.teamIndex.Value == projectile.teamIndex)
                    {
                        return;
                    }
                }
            }
            
            
        
            if (other.attachedRigidbody.TryGetComponent<Health>(out Health otherHealth))
            {
                otherHealth.TakeDamage(damage);
            }
        
        }
    }
}
