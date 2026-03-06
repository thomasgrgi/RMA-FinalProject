using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class DoubleSidedMeshCollider : MonoBehaviour
{
    void Start()
    {
        Mesh originalMesh = GetComponent<MeshFilter>().mesh;

        // --- Collider 1: Normal (blocks from outside) ---
        MeshCollider outerCollider = gameObject.AddComponent<MeshCollider>();
        outerCollider.sharedMesh = originalMesh;

        // --- Collider 2: Inverted (blocks from inside) ---
        Mesh invertedMesh = InvertMesh(originalMesh);

        MeshCollider innerCollider = gameObject.AddComponent<MeshCollider>();
        innerCollider.sharedMesh = invertedMesh;
    }

    Mesh InvertMesh(Mesh original)
    {
        Mesh inverted = Mesh.Instantiate(original);

        // Flip normals
        Vector3[] normals = inverted.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];
        inverted.normals = normals;

        // Reverse triangle winding
        for (int i = 0; i < inverted.subMeshCount; i++)
        {
            int[] tris = inverted.GetTriangles(i);
            for (int j = 0; j < tris.Length; j += 3)
            {
                (tris[j], tris[j + 2]) = (tris[j + 2], tris[j]);
            }
            inverted.SetTriangles(tris, i);
        }

        return inverted;
    }
}