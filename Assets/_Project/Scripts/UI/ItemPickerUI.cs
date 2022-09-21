using UnityEngine;

public class ItemPickerUI : MonoBehaviour {
    [SerializeField]
    private Vector3 offset;

    public void HandlePickUpUI(Camera camera, Transform lookAt) {
        //Vector3 pos = camera.WorldToScreenPoint(lookAt.position + offset);
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        if (transform.position != lookAt.position) transform.position = lookAt.position + offset;
        transform.rotation = camera.transform.rotation;
    }
}