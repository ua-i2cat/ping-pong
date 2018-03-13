// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointConstrainer : MonoBehaviour
{
    public enum ConstrainerType { Constrainer_angle, Constrainer_plane, Constrainer_twist, Constrainer_minmaxangle, Constrainer_minmaxangle_plane };

    public ConstrainerType constrainerType;

    public bool active;
    public bool drawProjection;

    public float minAngle;
    public float maxAngle;

    new public Transform transform;
    public Transform parent;
    public Transform child;

    // Normal of the plane in which we allow rotation (up vector of the plane transform)
    public Transform plane;

    public float threshold = 0.0f;
    public float mag;

    public Constrainer_twist.ForwardDir localForward;

    public Constrainer constrainer = null;

    void OnValidate()
    {
        switch(constrainerType)
        {
            case ConstrainerType.Constrainer_angle:
                constrainer = new Constrainer_angle();
                ((Constrainer_angle)constrainer).active = active;
                ((Constrainer_angle)constrainer).maxAngle = maxAngle;
                ((Constrainer_angle)constrainer).transform = transform;
                ((Constrainer_angle)constrainer).parent = parent;
                ((Constrainer_angle)constrainer).child = child;
                break;

            case ConstrainerType.Constrainer_plane:
                constrainer = new Constrainer_plane();
                ((Constrainer_plane)constrainer).active = active;
                ((Constrainer_plane)constrainer).drawProjection = drawProjection;
                ((Constrainer_plane)constrainer).transform = transform;
                ((Constrainer_plane)constrainer).parent = parent;
                ((Constrainer_plane)constrainer).child = child;
                ((Constrainer_plane)constrainer).plane = plane;
                ((Constrainer_plane)constrainer).threshold = threshold;
                break;

            case ConstrainerType.Constrainer_twist:
                constrainer = new Constrainer_twist();
                ((Constrainer_twist)constrainer).active = active;
                ((Constrainer_twist)constrainer).minAngle = minAngle;
                ((Constrainer_twist)constrainer).maxAngle = maxAngle;
                ((Constrainer_twist)constrainer).transform = transform;
                ((Constrainer_twist)constrainer).localForward = localForward;
                break;

            case ConstrainerType.Constrainer_minmaxangle:
                constrainer = new Constrainer_minmaxangle();
                ((Constrainer_minmaxangle)constrainer).active = active;
                ((Constrainer_minmaxangle)constrainer).minAngle = minAngle;
                ((Constrainer_minmaxangle)constrainer).maxAngle = maxAngle;
                ((Constrainer_minmaxangle)constrainer).transform = transform;
                ((Constrainer_minmaxangle)constrainer).parent = parent;
                ((Constrainer_minmaxangle)constrainer).child = child;
                break;

            case ConstrainerType.Constrainer_minmaxangle_plane:
                constrainer = new Constrainer_minmaxangle_plane();
                ((Constrainer_minmaxangle_plane)constrainer).active = active;
                ((Constrainer_minmaxangle_plane)constrainer).drawProjection = drawProjection;
                ((Constrainer_minmaxangle_plane)constrainer).minAngle = minAngle;
                ((Constrainer_minmaxangle_plane)constrainer).maxAngle = maxAngle;
                ((Constrainer_minmaxangle_plane)constrainer).transform = transform;
                ((Constrainer_minmaxangle_plane)constrainer).parent = parent;
                ((Constrainer_minmaxangle_plane)constrainer).child = child;
                ((Constrainer_minmaxangle_plane)constrainer).plane = plane;
                ((Constrainer_minmaxangle_plane)constrainer).threshold = threshold;
                break;
        }

        Debug.Assert(constrainer != null);
    }
}
