// Licensed under the LGPL 3.0
// See the LICENSE file in the project root for more information.
// Author: alexandre.via@i2cat.net

using UnityEngine;

public static class Constants
{
    public static string IP = "147.83.206.67";
    public static int PORT = 33333;

    public static int BUFF_SIZE = 8192;

    public static int HEADER_SIZE = 3;

    public static string Body = "Body";
    public static string Rig = "Rig";
    public static string Eye = "Eye";
    public static string LeftHand = "LH";
    public static string RightHand = "RH";
    public static string Hip = "Hip";
    public static string LeftFoot = "LF";
    public static string RightFoot = "RF";

    public static string ServeRequest = "ServeRequest";
    public static string WelcomeMsg = "Welcome";

    public static string OnlineText = "Online";
    public static string OfflineText = "Offline";

    public static string SendInputField = "SendInputField";

    public static string Ball = "Ball";

    public static Vector3 RightHandScale = new Vector3(0.11692f, 0.11692f, 0.11692f);
    public static Vector3 ColliderCenter = new Vector3(0, -0.15f, 1);
    public static Vector3 ColliderSize = new Vector3(2, 0.5f, 3);

    public static Vector3 SphereScale = new Vector3(0.1f, 0.1f, 0.1f);
}
