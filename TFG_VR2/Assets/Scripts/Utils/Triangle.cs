// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    private void Start()
    {
        Vector3 v0 = transform.GetChild(0).position;
        Vector3 v1 = transform.GetChild(1).position;
        Vector3 v2 = transform.GetChild(2).position;

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        transform.position = v0;
        mesh.vertices = new Vector3[] { Vector3.zero, v1 - transform.position, v2 - transform.position };
        mesh.uv = new Vector2[] { Vector2.zero, Vector2.up, Vector2.one }; // (0,0) - (0,1) - (1,1)
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 1 };
        mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up };
    }
}
