using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.PlayerLoop;

public class IKFootSolver : MonoBehaviour {
    public float footSpacing;
    public Transform body;
    [SerializeField] Animator anim;
    [SerializeField] TwoBoneIKConstraint footIK;
    [SerializeField] private string animatorParam;
    private Vector3 centerOfMass;
    public float predictionThreshold;
    public float predictedFootForward = 5f;
    public float floorDistance = 5f;
    private int FootIKFloat;

    private void Awake() {
        FootIKFloat = Animator.StringToHash(animatorParam);
        centerOfMass = body.position;
    }
    private void FixedUpdate() {
        footIK.weight = anim.GetFloat(FootIKFloat);

        if (Vector3.SqrMagnitude(body.position - centerOfMass) > predictionThreshold) {
            centerOfMass = body.position;
        }
        
        Ray ray = new Ray(body.position + (transform.right * footSpacing) + (transform.forward * predictedFootForward), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit info, floorDistance)) {
            var self = transform;
            Vector3 actualPosition = self.position;
            self.position = new Vector3(actualPosition.x, info.point.y, actualPosition.z);
            transform.rotation = Quaternion.LookRotation(self.forward, info.normal);
        }
        Debug.DrawRay(body.position + (transform.right * footSpacing) + (transform.forward * predictedFootForward), Vector3.down);
    }
}
