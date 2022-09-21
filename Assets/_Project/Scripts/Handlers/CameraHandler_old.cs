using UnityEngine;

namespace _Project.Scripts.Handlers {
    public class CameraHandler : MonoBehaviour {

        public static CameraHandler Singleton;
        public Transform targetTransform;
        public Transform cameraTransform;
        public Transform cameraPivotTransform;
        public Transform myTransform;
        public float sensitivity = 1;
        [SerializeField] private float lookSpeed = 0.1f;
        [SerializeField] private float followSpeed = 0.1f;
        [SerializeField] private float pivotSpeed = 0.03f;
        public float minimumPivot = -35;
        public float maximumPivot = 35;

        public float cameraSphereRadius = .2f;
        public float cameraCollisionOffset = .2f;
        public float minimumCollisionOffset = .2f;
        private Vector3 cameraFollowVelocity = Vector3.zero;
        private Vector3 cameraTransformPosition;
        private float defaultPosition;
        private LayerMask ignoreLayers;

        private float lookAngle;
        private float pivotAngle;

        private float targetPosition;

        private void Awake() {
            if (Singleton != null && Singleton != this)
                Destroy(this);
            else
                Singleton = this;
            myTransform = transform;
            defaultPosition = cameraTransform.localPosition.z;
            ignoreLayers = ~((1 << 8) | (1 << 9) | (1 << 10));
        }

        public void FollowTarget(float delta) {
            var targetPosition = Vector3.SmoothDamp(myTransform.position, targetTransform.position, ref cameraFollowVelocity, delta / followSpeed);
            myTransform.position = targetPosition;
            HandleCameraCollision(delta);
        }

        public void HandleCameraRotation(float delta, float mouseXInput, float mouseYInput) {
            lookAngle += mouseXInput * lookSpeed / delta;
            pivotAngle -= mouseYInput * pivotSpeed / delta;
            pivotAngle = Mathf.Clamp(pivotAngle, minimumPivot, maximumPivot);

            var rotation = Vector3.zero;
            rotation.y = lookAngle;
            var targetRotation = Quaternion.Euler(rotation);
            myTransform.rotation = targetRotation;

            rotation = Vector3.zero;
            rotation.x = pivotAngle;

            targetRotation = Quaternion.Euler(rotation);
            cameraPivotTransform.localRotation = targetRotation;
        }
        public void HandleCameraCollision(float delta) {
            targetPosition = defaultPosition;
            RaycastHit hit;
            var direction = cameraTransform.position - cameraPivotTransform.position;
            direction.Normalize();

            if (Physics.SphereCast(cameraPivotTransform.position, cameraSphereRadius, direction, out hit, Mathf.Abs(targetPosition), ignoreLayers)) {
                var dis = Vector3.Distance(cameraPivotTransform.position, hit.point);
                targetPosition = -(dis - cameraCollisionOffset);
            }
            if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
                targetPosition = -minimumCollisionOffset;
            cameraTransformPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, delta / .2f);
            cameraTransform.localPosition = cameraTransformPosition;
        }
    }
}