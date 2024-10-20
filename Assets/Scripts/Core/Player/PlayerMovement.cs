using System;
using Input;
using Unity.Netcode;
using UnityEngine;

namespace Core.Player
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Transform bodyTransform;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private ParticleSystem dustCloud;
    
        [Header("Settings")]
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private float turningRate = 30f;
        [SerializeField] private float particleEmissionValue = 10;
        private const float PARTICLE_STOP_THRESHOLD = 0.01f;


        private ParticleSystem.EmissionModule emissionModule;
        private Vector2 previousMovementInput;
        private Vector3 previousPos;

        private void Awake()
        {
            emissionModule = dustCloud.emission;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) {return;}

            inputReader.MoveEvent += HandleMove;

        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) {return;}
        
            inputReader.MoveEvent -= HandleMove;

        }

        private void HandleMove(Vector2 movementInput)
        {
            previousMovementInput = movementInput;

        }
        void Update()
        {
            if (!IsOwner) { return; }

            float zRotation = previousMovementInput.x * -turningRate * Time.deltaTime;
            bodyTransform.Rotate(0f, 0f, zRotation);
        }

        private void FixedUpdate()
        {
            if ((transform.position - previousPos).sqrMagnitude > PARTICLE_STOP_THRESHOLD)
            {
                emissionModule.rateOverTime = particleEmissionValue;
            }
            else
            {
                emissionModule.rateOverTime = 0f;
            }
            previousPos = bodyTransform.position;
            
            if (!IsOwner) {return;}

            rb.velocity = (Vector2)bodyTransform.up * (previousMovementInput.y * movementSpeed);
        }
    }
}
