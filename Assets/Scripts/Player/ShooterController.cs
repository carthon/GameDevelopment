using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class ShooterController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private StarterAssetsInputs starterAssetsInputs;
    [SerializeField] private ThirdPersonController characterController;
    public float sensitivityMult = 0.5f;
    void Awake() {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        characterController = GetComponent<ThirdPersonController>();
    }
    
    // Update is called once per frame
    void Update() {
        if (starterAssetsInputs.aim) {
            characterController.SetSensitivity(characterController.Sensitivity * sensitivityMult);
        }
        else
            characterController.SetSensitivity(characterController.Sensitivity);
        aimVirtualCamera.gameObject.SetActive(starterAssetsInputs.aim);
    }
}
