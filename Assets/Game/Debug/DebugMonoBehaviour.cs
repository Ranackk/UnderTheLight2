using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.Core;
using Game.GameMain.Bridges;
using JSDK.Events;
using JSDK.Misc;

public class DebugMonoBehaviour : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.B))
        {
            TryPlaceBridge();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameManager.SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            GameManager.LoadGame();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
           ResetBridge();
        }
    }

    ////////////////////////////////////////////////////////////////

    void TryPlaceBridge()
    {
        ObjectPool<RaycastHit> rayCastHitPool = new ObjectPool<RaycastHit>(1, true);

        ResetBridge();

        Ray rayForward  = new Ray(Vector3.zero, Vector3.forward);
        Ray rayBackward = new Ray(Vector3.zero, Vector3.back);

        RaycastHit hit = rayCastHitPool.Get();
        if (Physics.Raycast(ray: rayForward, maxDistance: 10.0f, hitInfo: out hit))
        {
            RawBridgePoint firstPoint;
            BridgePointValidationResult result = BridgePlanner.ValidateBridgePoint(hit, out firstPoint);
            if (result == BridgePointValidationResult.Success)
            {
                EventManager.Instance.FireEvent<BridgePointPlacedEvent>(new BridgePointPlacedEvent(firstPoint));
            }
            else
            {
                Debug.Log("BridgePointPlacement failed due to " + result.ToString());
            }
        }

        if (Physics.Raycast(ray: rayBackward, maxDistance: 10.0f, hitInfo: out hit))
        {
            RawBridgePoint secondPoint;
            BridgePointValidationResult result = BridgePlanner.ValidateBridgePoint(hit, out secondPoint);
            if (result == BridgePointValidationResult.Success)
            {
                EventManager.Instance.FireEvent<BridgePointPlacedEvent>(new BridgePointPlacedEvent(secondPoint));
            }
            else
            {
                Debug.Log("BridgePointPlacement failed due to " + result.ToString());
            }
        }

        rayCastHitPool.Release(hit);
    }

    ////////////////////////////////////////////////////////////////

    void ResetBridge()
    {
        EventManager.Instance.FireEvent<ResetBridgePointsEvent>(new ResetBridgePointsEvent());
    }
}
