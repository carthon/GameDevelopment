using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Cinemachine;
using Cinemachine.Utility;
using StarterAssets;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SearchService;
using Color = UnityEngine.Color;

public class OrbitalMovement : MonoBehaviour {
    [SerializeField] private Transform origin;
    [SerializeField] private Transform target;
    private Vector3 pointPos;
    [SerializeField] private float maxAngleRotation = 90f;
    [SerializeField] private float minAngleRotation = -90f;
    [SerializeField] private float speed = 3f;
    private float angle;
    // Start is called before the first frame update
    void Awake() {
        if (origin == null)
            origin = GetComponent<Transform>();
        if (target == null)
            target = GetComponent<Transform>();
    }
    // Update is called once per frame
    void Update() {
        Vector3 dir = (transform.position - origin.transform.position).normalized;
        angle = Vector3.Angle(dir, transform.forward);
        if (angle < maxAngleRotation && angle > minAngleRotation) {
            if (Physics.Raycast(origin.transform.position, dir, out var hit) && hit.collider.isTrigger) {
                pointPos = hit.point;
                float x = Mathf.Lerp(target.transform.position.x, pointPos.x, speed * Time.deltaTime);
                float y = Mathf.Lerp(target.transform.position.y, pointPos.y, speed * Time.deltaTime);
                float z = Mathf.Lerp(target.transform.position.z, pointPos.z, speed * Time.deltaTime);
                target.transform.position = new Vector3(x,y,z);
            }
        }
    }
}
