using UnityEngine;

public class CombineChildrenMeshes : MonoBehaviour
{
    void Start()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            combine[i].mesh = filters[i].sharedMesh;
            combine[i].transform = filters[i].transform.localToWorldMatrix;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine);

        GameObject combined = new GameObject("CombinedMesh");
        combined.AddComponent<MeshFilter>().sharedMesh = finalMesh;
        combined.AddComponent<MeshRenderer>().sharedMaterial = filters[0].GetComponent<MeshRenderer>().sharedMaterial;

        combined.transform.position = transform.position;

        // Desactivar originales
        gameObject.SetActive(false);
    }
}
