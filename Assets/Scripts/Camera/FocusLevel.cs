using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusLevel : MonoBehaviour
{
    public float HalfXBounds = 20f;
    public float HalfYBounds = 15f;
    public float HalfZBounds = 15f;

    public Vector3 Offset; 

    public Bounds FocusBounds;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = gameObject.transform.position;
        Bounds bounds = new Bounds();
        bounds.Encapsulate(new Vector3(pos.x - HalfXBounds, pos.y - HalfYBounds, pos.z - HalfZBounds));
        bounds.Encapsulate(new Vector3(pos.x + HalfXBounds, pos.y + HalfYBounds, pos.z + HalfZBounds));
        bounds.Encapsulate(bounds.center + Offset);
        FocusBounds = bounds;
    }
}
