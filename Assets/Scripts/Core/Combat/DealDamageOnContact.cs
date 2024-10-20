using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class DealDamageOnContact : MonoBehaviour
    {
        [SerializeField] private int damage = 5;

        private ulong ownerClientId;

        public void SetOwner(ulong owner)
        {
            ownerClientId = owner;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) { return; }

            if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
            {
                if (ownerClientId == netObj.OwnerClientId)
                {
                    return;
                }
            }
        
            if (other.attachedRigidbody.TryGetComponent<Health>(out Health otherHealth))
            {
                otherHealth.TakeDamage(damage);
            }
        
        }
    }
}
