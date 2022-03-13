using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace _Project.Scripts.Components {
    [RequireComponent(typeof(Rigidbody))]
    public class Locomotion : MonoBehaviour {
        [Range(0,100)]
        [SerializeField] float maxSpeed = 5f;
        [Range(0,100)]
        [SerializeField] float maxRunSpeed = 10f;
        [Range(0,100)]
        [SerializeField] float maxAcceleration = 4f;
        [Range(0,100)]
        [SerializeField] float jumpStrength = 4f;
        Vector2 direction;
        private Vector3 desiredVelocity;
        private bool desiredJump;
        private Vector3 velocity;
        private Rigidbody body;

        private bool _isGrounded;
        // Start is called before the first frame update
        void Awake() {
            body = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void FixedUpdate() {
            MoveBody();
            if (desiredJump && _isGrounded) {
                desiredJump = false;
                Jump();
            }
        }
        void MoveBody() {
            velocity = body.velocity;
            float maxSpeedChange = maxAcceleration * Time.fixedDeltaTime;
            Vector3 desiredVelocity = new Vector3(direction.x, 0f, direction.y) * maxSpeed;
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
            body.velocity = velocity;
        }

        void Jump() {
            float jumpValue = jumpStrength;
            velocity.y += jumpValue;
            body.velocity = velocity;
        }
        public void SetGrounded(bool grounded) => _isGrounded = grounded;
        public void TriggerJump(bool jump) => desiredJump |= jump;
        public void SetDirection(Vector2 dir) => direction = dir;
    }
}