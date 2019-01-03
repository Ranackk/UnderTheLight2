using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Game.GameMain.Bridges;
using JSDK.Events;

namespace Game.Core
{
    public class BridgePointPlacedEvent : GameEvent
    {
        public RawBridgePoint PlacedPoint;

        public BridgePointPlacedEvent(RawBridgePoint placedPoint)
        {
            PlacedPoint = placedPoint;
        }
    }

    ////////////////////////////////////////////////////////////////

    public class ResetBridgePointsEvent : GameEvent
    {
        public ResetBridgePointsEvent()
        {

        }
    }
}
