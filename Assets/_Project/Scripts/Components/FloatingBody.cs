using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace _Project.Scripts.Components {
    public class FloatingBody : MonoBehaviour {

        [SerializeField] float rideHeight = 10f;
        [SerializeField] float rideSpringStrenght = 4f;
        [SerializeField] float rideSpringDamper = 4f;
        [SerializeField] float groundRay = 3f;
        [SerializeField] RaycastHit surface;
        private Rigidbody _rb;
        private Locomotion _locomotion;

        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _locomotion = GetComponent<Locomotion>();
        }

        private void FixedUpdate() {
            Float();
            EquilibrateBody();
        }

        void EquilibrateBody() {
            Vector3 surfaceNormal = surface.normal;
            Quaternion surfaceRotation = Quaternion.LookRotation(Vector3.up, surfaceNormal); 
            Quaternion currentRotation = transform.rotation;
            Quaternion toGoal = Quaternion.FromToRotation(transform.rotation.eulerAngles, surfaceNormal);
            float torqueForce = 10f;
            Utils.Logger.DrawRay(transform.position, transform.rotation.eulerAngles * torqueForce, Color.white, default, default);
            //_rb.AddTorque(surfaceNormal);
        }

        void Float() {
            Vector3 downDir = Vector3.down;
            Ray floatingRay = new Ray(transform.position, downDir);
            if (Physics.Raycast(floatingRay, out RaycastHit hit, groundRay)) {
                surface = hit;
                _locomotion.SetGrounded(true);
                Vector3 vel = _rb.velocity;
                Vector3 rayDir = -transform.up;

                Vector3 otherVel = Vector3.zero;
                Rigidbody hitBody = hit.rigidbody;

                if (hitBody != null)
                    otherVel = hitBody.velocity;

                float rayDirVel = Vector3.Dot(rayDir, vel);
                float otherDirVel = Vector3.Dot(rayDir, otherVel);

                float relVel = rayDirVel - otherDirVel;
                float x = hit.distance - rideHeight;

                float springForce = (x * rideSpringStrenght) - (relVel * rideSpringDamper);
                Utils.Logger.DrawRay(transform.position, transform.position + (rayDir * springForce), Color.yellow, default,default);
                _rb.AddForce(rayDir * springForce);
            }
            else {
                _locomotion.SetGrounded(false);
            }
        }
    }
}
