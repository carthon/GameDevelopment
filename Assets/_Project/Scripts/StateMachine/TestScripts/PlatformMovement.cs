using UnityEngine;

namespace _Project.Scripts.StateMachine.TestScripts {
    public class PlatformMovement : MonoBehaviour {
        public Transform TransformToTravel;
        public float speed;
        public bool goingBack;
        private Vector3 _endPoint;
        private Vector3 _pointToTravel;
        private Vector3 _startPoint;
        // Start is called before the first frame update
        private void Start() {
            _startPoint = transform.position;
            _endPoint = TransformToTravel.position;
        }

        // Update is called once per frame
        private void FixedUpdate() {
            if (!goingBack)
                _pointToTravel = _endPoint;
            else if (goingBack)
                _pointToTravel = _startPoint;
            transform.position = Vector3.Lerp(transform.position, _pointToTravel, speed * Time.fixedDeltaTime);
        }
    }
}