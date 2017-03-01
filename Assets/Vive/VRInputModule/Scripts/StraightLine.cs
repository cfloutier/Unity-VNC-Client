using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class StraightLine : MonoBehaviour
{
    public Transform from;
    public Transform to;

    public Color color;

    public float sizeDot = 0.05f;
    public float sizeLine = 0.01f;

   

    private GameObject line;
    private GameObject dot1;
    private GameObject dot2;
    Material newMaterial;

    public void Rebuild()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
       

        // todo:    let the user choose a mesh for laser pointer ray and hit point
        //          or maybe abstract the whole menu control some more and make the 
        //          laser pointer a module.
        line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.transform.SetParent(transform, false);
        line.transform.localScale = new Vector3(sizeLine, sizeLine, 100.0f);
        line.transform.localPosition = new Vector3(0.0f, 0.0f, 50.0f);
        line.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;

        dot1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot1.transform.SetParent(transform, false);
        
        dot1.transform.localPosition = new Vector3(0.0f, 0.0f, 100.0f);
        dot1.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;

        dot2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot2.transform.SetParent(transform, false);
        
        dot2.transform.localPosition = new Vector3(0.0f, 0.0f, 100.0f);
        dot2.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;

        // remove the colliders on our primitives
        Object.DestroyImmediate(dot1.GetComponent<SphereCollider>());
        Object.DestroyImmediate(dot2.GetComponent<SphereCollider>());
        Object.DestroyImmediate(line.GetComponent<BoxCollider>());

        newMaterial = new Material(Shader.Find("Wacki/LaserPointer"));

        newMaterial.SetColor("_Color", color);
        line.GetComponent<MeshRenderer>().sharedMaterial = newMaterial;
        dot2.GetComponent<MeshRenderer>().sharedMaterial = newMaterial;
        dot1.GetComponent<MeshRenderer>().sharedMaterial = newMaterial;

        dot1.transform.localPosition = Vector3.zero;

    }

    public void Start()
    {
        Rebuild();
    }

    void Update()
    {
        dot2.transform.localScale = new Vector3(sizeDot, sizeDot, sizeDot);
        dot1.transform.localScale = new Vector3(sizeDot, sizeDot, sizeDot);
        newMaterial.SetColor("_Color", color);

        if (from == null || to == null) return;

        Vector3 pa = from.position;
        Vector3 pb = to.position;
        float distance = (pa - pb).magnitude;

        transform.position = pa;

        line.transform.localScale = new Vector3(sizeLine, sizeLine, distance);
        line.transform.localPosition = Vector3.forward * distance/2;

        transform.LookAt(pb);

        dot2.transform.localPosition = Vector3.forward * distance;
    }

}

