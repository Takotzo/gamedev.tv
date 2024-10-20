using System;
using Unity.Netcode;
using UnityEngine;

namespace Core.Combat
{
    public class Health : NetworkBehaviour
    {
        [field: SerializeField] public int maxHealth { get; private set; } = 100;
    
        public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

        private bool isDead;

        public Action<Health> OnDie;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) {return;}

            CurrentHealth.Value = maxHealth;
        }

        public void TakeDamage(int damageValue)
        {
            ModifyHealth(-damageValue);
        }

        public void RestoreHealth(int healValue)
        {
            ModifyHealth(healValue);
        }

        private void ModifyHealth(int value)
        {
            if (isDead) { return; }

            CurrentHealth.Value += value;

            CurrentHealth.Value = Mathf.Clamp(CurrentHealth.Value, 0, maxHealth);

            if (CurrentHealth.Value == 0)
            {
                OnDie?.Invoke(this);
                isDead = true;
            }
        }
    }
}
