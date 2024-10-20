using Input;
using Unity.Netcode;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Core.Player
{
    public class PlayerAiming : NetworkBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform turretTransform;

        private void LateUpdate()
        {
            if (!IsOwner) {return;}

            Vector2 aimScreenPosition = inputReader.AimPosition;
            Vector2 aimWorldPosition = Camera.main.ScreenToWorldPoint(aimScreenPosition);
            
            turretTransform.up = aimWorldPosition - (Vector2)transform.position;

        }
    }
}
