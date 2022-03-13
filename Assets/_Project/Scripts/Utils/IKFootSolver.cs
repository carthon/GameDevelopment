using UnityEngine;
using UnityEngine.Animations.Rigging;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class IKFootSolver : MonoBehaviour {
    [SerializeField] Animator anim;
    [SerializeField] private Transform[] allfootTransforms;
    [SerializeField] private Transform[] allfootTargetTransforms;
    [SerializeField] private TwoBoneIKConstraint[] allfootIKConstraints;
    [SerializeField] private string animatorParam;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] float maxHitDistance = 5f;
    [SerializeField] float addedHeight = 3f;
    [SerializeField] float yOffset = .2f;
    [SerializeField] private string[] allFootWeightsName;
    private float[] allFootWeights;
    bool[] allGroundSphereCastHit;
    private int FootIKFloat;
    private LayerMask hitLayer;
    private Vector3[] allHitNormals;
    private float angleAboutX;
    private float angleAboutZ;

    private void Awake() {
        FootIKFloat = Animator.StringToHash(animatorParam);
        allGroundSphereCastHit = new bool[allfootTransforms.Length + 1];
        allHitNormals = new Vector3[allfootTransforms.Length];
        allFootWeights = new float[allfootTransforms.Length];
    }
    private void FixedUpdate() {
        RotateCharacterFeet();
    }

    private void defaultValues(out Vector3 hitPoint, out bool gotGroundSpherecastHit, out Vector3 hitNormal,
        out LayerMask hitLayer, out float currentHitDistance, Transform objectTransform) {
        currentHitDistance = 0;
        gotGroundSpherecastHit = false;
        hitLayer = LayerMask.NameToLayer("Player");
        hitNormal = Vector3.up;
        hitPoint = objectTransform.position;
    }
    
    private void CheckGroundBelow(out Vector3 hitPoint, out bool gotGroundSpherecastHit, out Vector3 hitNormal, out LayerMask hitLayer,
        out float currentHitDistance, Transform objectTransform, int checkForLayerMask, float maxHitDistance, float addedHeight){
        RaycastHit hit;
        Vector3 startSphereCast = objectTransform.position + new Vector3(0f, addedHeight, 0f);
        if (checkForLayerMask == -1) {
            Debug.LogError("Layer does not exist!");
            defaultValues(out hitPoint, out gotGroundSpherecastHit, out hitNormal, out hitLayer, out currentHitDistance, objectTransform);
        }
        else {
            int layerMask = (checkForLayerMask);
            if (Physics.SphereCast(startSphereCast,0.2f, Vector3.down, out hit, maxHitDistance, layerMask,
                    QueryTriggerInteraction.UseGlobal)) {
                hitLayer = hit.transform.gameObject.layer;
                currentHitDistance = hit.distance - addedHeight;
                hitNormal = hit.normal;
                gotGroundSpherecastHit = true;
                hitPoint = hit.point;
            }
            else {
                defaultValues(out hitPoint, out gotGroundSpherecastHit, out hitNormal, out hitLayer, out currentHitDistance, objectTransform);
            }
        }
    }

    Vector3 ProjectOnContactPlane(Vector3 vector, Vector3 hitNormal) {
        return vector - hitNormal * Vector3.Dot(vector, hitNormal);
    }

    private void ProjectedAxisAngles(out float angleAboutX, out float angleAboutZ, Transform footTarget, Vector3 hitNormal) {
        Vector3 xAxisProjected = ProjectOnContactPlane(footTarget.forward, hitNormal).normalized;
        Vector3 zAxisProjected = ProjectOnContactPlane(footTarget.right, hitNormal).normalized;

        angleAboutX = Vector3.SignedAngle(footTarget.forward, xAxisProjected, footTarget.right);
        angleAboutZ = Vector3.SignedAngle(footTarget.right, zAxisProjected, footTarget.forward);
    }

    private void RotateCharacterFeet() {
        for (int i = 0; i < allfootTransforms.Length; i++) {
            allFootWeights[i] = anim.GetFloat(allFootWeightsName[i]);
            allfootIKConstraints[i].weight = allFootWeights[i];
            CheckGroundBelow(out Vector3 hitPoint, out allGroundSphereCastHit[i], out Vector3 hitNormal, out hitLayer, out _,
                allfootTransforms[i], groundLayerMask, maxHitDistance, addedHeight);
            allHitNormals[i] = hitNormal;

            if (allGroundSphereCastHit[i] == true) {
                ProjectedAxisAngles(out angleAboutX, out angleAboutZ, allfootTransforms[i], allHitNormals[i]);
                allfootTargetTransforms[i].position = new Vector3(allfootTransforms[i].position.x, hitPoint.y + yOffset,
                    allfootTransforms[i].position.z);
                allfootTargetTransforms[i].rotation = allfootTransforms[i].rotation;
            }
            else {
                allfootTargetTransforms[i].position = allfootTransforms[i].position;
                allfootTargetTransforms[i].rotation = allfootTransforms[i].rotation;
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.black;
        for (int i = 0; i < allfootTransforms.Length; i++) {
            Gizmos.DrawRay(allfootTransforms[i].position + new Vector3(0f, addedHeight), Vector3.down * maxHitDistance);
            Gizmos.DrawWireSphere(allfootTransforms[i].position + new Vector3(0f, addedHeight) - Vector3.up * maxHitDistance, .2f);
        }
    }
}
