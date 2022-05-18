using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetsIsInteracting : StateMachineBehaviour
{
    private static readonly int IsInteracting = Animator.StringToHash("isInteracting");
    public string targetBool;
    public bool status;
    public bool OnExitstatus;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(targetBool, status);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(targetBool, OnExitstatus);
    }
}
