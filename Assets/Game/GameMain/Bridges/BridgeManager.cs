using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Game.Core;

using JSDK.Misc;
using JSDK.Events;
using External.JEichner.ooo;

namespace Game.GameMain.Bridges
{

    /*
     * Handles bridge points, the current bridge and the bridge mesh.
     * 
     */
    public class BridgeManager : Singleton<BridgeManager>, IBaseManager
    {
        RawBridgePoint  m_FirstBridgePoint;
        RawBridgePoint  m_SecondBridgePoint;

        Bridge          m_Bridge;

        BridgeMesh      m_BridgeMesh; // TODO: Move to bridge visualizer

        public BridgeManager()
        {
            m_FirstBridgePoint  = RawBridgePoint.INVALID;
            m_SecondBridgePoint = RawBridgePoint.INVALID;
            
            m_Bridge            = Bridge.INVALID;
            m_BridgeMesh        = BridgeMesh.INVALID;
        }

        ////////////////////////////////////////////////////////////////
        // Events
        ////////////////////////////////////////////////////////////////

        public void OnInitialize()
        {
            EventManager.Instance.AddListener<BridgePointPlacedEvent>(OnPlacedBridgePointEvent);
            EventManager.Instance.AddListener<ResetBridgePointsEvent>(OnResetBridgePointsEvent);
            Debug.Log("BridgeManager initialized!");
        }
        
        ////////////////////////////////////////////////////////////////

        public void OnDestroy()
        {
            EventManager.Instance.RemoveListener<ResetBridgePointsEvent>(OnResetBridgePointsEvent);
            EventManager.Instance.RemoveListener<BridgePointPlacedEvent>(OnPlacedBridgePointEvent);
            Debug.Log("BridgeManager destroyed!");
        }

        ////////////////////////////////////////////////////////////////
        
        public void OnPlacedBridgePointEvent(BridgePointPlacedEvent gameEvent)
        {
            Debug.Log("OnBridgePointPlaced");

            CreateBridgeParameters bridgeParameters;
            bool bridgeNeedsUpdate = PlaceBridgePoint(gameEvent.PlacedPoint, out bridgeParameters);
            
            ////////////////////////////////////////////////////////////////

            if (bridgeNeedsUpdate)
            {
                m_Bridge = BridgePlanner.PrepareBridge(bridgeParameters);
                UpdateBridgeMesh();
            }
        }

        ////////////////////////////////////////////////////////////////

        public void OnResetBridgePointsEvent(ResetBridgePointsEvent gameEvent)
        {
            ResetBridgeAndBridgePoints();
        }

        ////////////////////////////////////////////////////////////////
        // Serialization
        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("CurrentBridge",       ref m_Bridge, false);
            io.Serialize("CurrentBridgeMesh",   ref m_BridgeMesh, false);
            io.Serialize("FirstBridgPoint",     ref m_FirstBridgePoint, false);
            io.Serialize("SecondBridgePoint",   ref m_SecondBridgePoint, false);

            UpdateBridgeMesh();
        }

        ////////////////////////////////////////////////////////////////
        // Private Helpers
        ////////////////////////////////////////////////////////////////

        void ResetBridgeAndBridgePoints()
        {
            Debug.Log("Reset Bridge Points");
            m_FirstBridgePoint  = RawBridgePoint.INVALID;
            m_SecondBridgePoint = RawBridgePoint.INVALID;
            
            m_Bridge            = Bridge.INVALID;
            m_BridgeMesh        = BridgeMesh.INVALID;
        }

        /// <summary>
        /// Places the given bridge point
        /// </summary>
        /// <param name="placePoint"></param>
        /// <returns>True, if the bridge needs to be updated now</returns>
        bool PlaceBridgePoint(RawBridgePoint placePoint, out CreateBridgeParameters outBridgeParamters)
        {
            outBridgeParamters      = new CreateBridgeParameters();
            bool firstPointIsValid  = m_FirstBridgePoint.IsValid();
            bool secondPointIsValid = m_SecondBridgePoint.IsValid();
            

            ////////////////////////////////////////////////////////////////

            if (!firstPointIsValid && !secondPointIsValid)
            {
                // No point set? Set the first point
                m_FirstBridgePoint = placePoint;
            }
            else if (firstPointIsValid && !secondPointIsValid)
            {
                // Does this point work to create a bridge?
                CreateBridgeParameters createBridgeParamters;
                BridgeValidationResult result = BridgePlanner.ValidateBridgePlacement(new ValidateBridgeInput(m_FirstBridgePoint, placePoint), out createBridgeParamters);
                if (result == BridgeValidationResult.Success)
                {
                    // Yes! Lets prepare everything for updating the bridge
                    m_SecondBridgePoint = placePoint;
                    outBridgeParamters  = createBridgeParamters;
                    return true;
                }
                else
                {
                    // No. Instead, override the first point!
                    m_FirstBridgePoint = placePoint;
                    return false;
                } 
            }
            else if (firstPointIsValid && secondPointIsValid)
            {
                // First try to override second point, if that fails, try to override first point.

                // Try first point.
                CreateBridgeParameters createBridgeParamters;
                BridgeValidationResult result = BridgePlanner.ValidateBridgePlacement(new ValidateBridgeInput(m_FirstBridgePoint, placePoint), out createBridgeParamters);
                if (result == BridgeValidationResult.Success)
                {
                    // Make the new point the second point
                    m_SecondBridgePoint = placePoint;
                    outBridgeParamters  = createBridgeParamters;
                    return true;
                }
                else
                {
                    result = BridgePlanner.ValidateBridgePlacement(new ValidateBridgeInput(placePoint, m_SecondBridgePoint), out createBridgeParamters);
                    if (result == BridgeValidationResult.Success)
                    {
                        // Make the previous second the first point & the new point the second point
                        m_FirstBridgePoint  = m_SecondBridgePoint;
                        m_SecondBridgePoint = placePoint;
                        outBridgeParamters  = createBridgeParamters;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                Debug.LogError("BridgePlanner: Second point is valid but first is not?!");
            }

            return false;
        }
    
        ////////////////////////////////////////////////////////////////

        void UpdateBridgeMesh()
        {
            BridgeMesh bridgeMesh = BridgeCreator.CreateBridgeMesh(m_Bridge);
            EventManager.Instance.FireEvent<CreateBridgeEvent>(new CreateBridgeEvent(bridgeMesh));
        }

        ////////////////////////////////////////////////////////////////

        public void DEBUG_DrawGizmos()
        {
            if (m_Bridge.IsValid())
            {
                Gizmos.color = Color.white;    
                for (int i = 0; i < m_Bridge.Points.Length; i++)
                {
                    bool isFirst = i == 0;
                    bool isLast = i == m_Bridge.Points.Length - 1;
                    
                    if (isFirst)
                    {
                        Gizmos.color = new Color (0.8f, 0.8f, 1.0f);
                    }
                    else if (isLast)
                    {
                        Gizmos.color = new Color (0.8f, 1.0f, 0.8f);
                    }
                    else
                    {
                        Gizmos.color = new Color (0.8f, 0.8f, 0.8f);
                    }

                    ValidBridgePoint bridgePoint = m_Bridge.Points[i];
                    
                    Gizmos.DrawWireSphere(bridgePoint.PositionWS, 0.5f);
                    if (!isLast)
                    {
                        Gizmos.DrawLine(m_Bridge.Points[i].PositionWS, m_Bridge.Points[i+1].PositionWS);
                    }

                    ////////////////////////////////////////////////////////////////
                    
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(bridgePoint.PositionWS, bridgePoint.PositionWS + bridgePoint.WallNormalWS * 0.65f);
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(bridgePoint.PositionWS, bridgePoint.PositionWS + bridgePoint.RightTangentWS * 0.65f);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(bridgePoint.PositionWS, bridgePoint.PositionWS + bridgePoint.UpTangentWS * 0.65f);
                }
            }
    
            ////////////////////////////////////////////////////////////////

            if (m_FirstBridgePoint.IsValid())
            {
                Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                Gizmos.DrawWireSphere(m_FirstBridgePoint.PositionWS, 1.0f);
            }

            if (m_SecondBridgePoint.IsValid())
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawWireSphere(m_SecondBridgePoint.PositionWS, 1.0f);
            }

            ////////////////////////////////////////////////////////////////
            
        }
    }
}
