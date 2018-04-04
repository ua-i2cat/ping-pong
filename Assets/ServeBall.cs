using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedController))]
public class ServeBall : MonoBehaviour
{
    private SteamVR_TrackedController inputController;

    private void Start()
    {
        inputController = GetComponent<SteamVR_TrackedController>();
        inputController.TriggerClicked += OnTriggerClicked;
    }

    private void OnTriggerClicked(object sender, ClickedEventArgs e)
    {
        //Debug.Log("Trigger Pressed");
        BallController ballController = GameObject.Find("Ball").GetComponent<BallController>();
        ballController.paddle = transform.Find("attach").Find("Collider").gameObject;
        ballController.serve = true;
    }

    private void FixedUpdate()
    {
        //var rb = this.gameObject.transform.Find("attach").Find("Collider").GetComponent<Rigidbody>();
        //Debug.Log(rb.velocity);
    }
}
