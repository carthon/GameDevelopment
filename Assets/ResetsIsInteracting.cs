using UnityEngine;

public class ResetsIsInteracting : StateMachineBehaviour {
    private static readonly int IsInteracting = Animator.StringToHash("isInteracting");
    public string targetBool;
    public bool OnEnterstatus;
    public bool OnExitstatus;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(targetBool, OnEnterstatus);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(targetBool, OnExitstatus);
    }
}