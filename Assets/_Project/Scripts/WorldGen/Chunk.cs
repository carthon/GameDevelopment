using UnityEngine;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour {
    public Vector3Int coord;

    [HideInInspector]
    public Mesh mesh;
    private bool generateCollider;
    private MeshCollider meshCollider;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public void DestroyOrDisable() {
        if (Application.isPlaying) {
            mesh.Clear();
            gameObject.SetActive(false);
        }
        else {
            DestroyImmediate(gameObject, false);
        }
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp(Material mat, bool generateCollider) {
        this.generateCollider = generateCollider;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        //gameObject.layer = LayerMask.NameToLayer("Ground");

        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (meshCollider == null && generateCollider) meshCollider = gameObject.AddComponent<MeshCollider>();
        if (meshCollider != null && !generateCollider) DestroyImmediate(meshCollider);

        mesh = meshFilter.sharedMesh;
        if (mesh == null) {
            mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            meshFilter.sharedMesh = mesh;
        }

        if (generateCollider) {
            if (meshCollider.sharedMesh == null) meshCollider.sharedMesh = mesh;
            //meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation;
            // force update
            UpdateCollider();
        }

        meshRenderer.material = mat;
    }
    public void UpdateCollider() {
        // force update
        meshCollider.enabled = false;
        meshCollider.enabled = true;
    }
}