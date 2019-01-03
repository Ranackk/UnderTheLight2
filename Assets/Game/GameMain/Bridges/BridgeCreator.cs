﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
namespace Game.GameMain.Bridges
{
    public static class BridgeCreator 
    {
        public static BridgeMesh CreateBridgeMesh(Bridge bridge)
        {
            Debug.Assert(bridge.IsValid());

            // For each bridgePoint [e.g, 0 or 1], we want to add the two bridge vertices [L, R]
            //
            // L0------------------L1 -
            // |                   |  |
            // |                   |  < BridgeWidth / 2
            // |                   |  | 
            // 0-------------------1  -
            // |                   |
            // |                   |
            // |                   |
            // R0------------------R1

            ////////////////////////////////////////////////////////////////

            int bridgePointCount = bridge.Points.Length;

            List<BridgeMeshVertex> vertices = new List<BridgeMeshVertex>();

            for (int i = 0; i < bridgePointCount; i++)
            {

                ValidBridgePoint bridgePoint = bridge.Points[i];

                ////////////////////////////////////////////////////////////////

                bool isEvenPoint = (i % 2 == 0);
                AppendBridgeMeshVertices(ref vertices, bridgePoint, isEvenPoint);
            }

            const int VERTS_PER_BRIDGEPOINT = 2;

            int triangleCount   = VERTS_PER_BRIDGEPOINT * bridgePointCount;
            List<int> indicies  = new List<int>();

            for (int segementID = 0; segementID < bridgePointCount - 1; segementID++)
            {
                int nextSegmentID = segementID + 1;

                int vertexIDThisSegmentLeft   = VERTS_PER_BRIDGEPOINT * segementID + 0;
                int vertexIDThisSegmentRight  = VERTS_PER_BRIDGEPOINT * segementID + 1;
                int vertexIDNextSegmentLeft   = VERTS_PER_BRIDGEPOINT * nextSegmentID + 0;
                int vertexIDNextSegmentRight  = VERTS_PER_BRIDGEPOINT * nextSegmentID + 1;
                
                // Triangle 1: R0, L0, L1
                indicies.Add(vertexIDThisSegmentRight);
                indicies.Add(vertexIDThisSegmentLeft);
                indicies.Add(vertexIDNextSegmentLeft);
                
                // Triangle 2: R0, L1, R1
                indicies.Add(vertexIDThisSegmentRight);
                indicies.Add(vertexIDNextSegmentLeft);
                indicies.Add(vertexIDNextSegmentRight);
            }

            BridgeMesh bridgeMesh = new BridgeMesh(vertices, indicies);

            return bridgeMesh;
        }
        
        ////////////////////////////////////////////////////////////////
        
        static void AppendBridgeMeshVertices(ref List<BridgeMeshVertex> inOutBridgeMeshVertices, ValidBridgePoint bridgePoint, bool isEvenPoint)
        {
            BridgeMeshVertex leftVertex     = new BridgeMeshVertex();
            BridgeMeshVertex rightVertex    = new BridgeMeshVertex();

            ////////////////////////////////////////////////////////////////
            // General
            
            // Depending on if we are an even or uneven point, we need to invert our "right" to make sure it is also the bridges "right"
            float facingFactor              = isEvenPoint ? 1.0f : -1.0f; 

            ////////////////////////////////////////////////////////////////
            // Position

            float halfBridgeWidth           = BridgeOptions.WIDTH_WS / 2.0f;
            leftVertex.PositionWS           = bridgePoint.PositionWS + facingFactor * bridgePoint.RightTangentWS * halfBridgeWidth;
            rightVertex.PositionWS          = bridgePoint.PositionWS - facingFactor * bridgePoint.RightTangentWS * halfBridgeWidth;

            ////////////////////////////////////////////////////////////////
            // Normal
            
            leftVertex.NormalWS             = Vector3.up;   // TODO: Calculate this from the positions themselves to make it more accurate.
            rightVertex.NormalWS            = Vector3.up;

            ////////////////////////////////////////////////////////////////
            // UV
            
            leftVertex.UV                   = new Vector2(0.0f, bridgePoint.UV.y);
            rightVertex.UV                  = new Vector2(1.0f, bridgePoint.UV.y);

            ////////////////////////////////////////////////////////////////
            
            inOutBridgeMeshVertices.Add(leftVertex);
            inOutBridgeMeshVertices.Add(rightVertex);
        }
    
        ////////////////////////////////////////////////////////////////
        
        static Vector3 GetFacingDirection(Bridge bridge, int index)
        {
            Debug.Assert(bridge.IsValid());

            ////////////////////////////////////////////////////////////////
            
            ValidBridgePoint bridgePoint = bridge.Points[index];

            Vector3 toNextVector = Vector3.zero;
            if (index != bridge.Points.Length - 1)
            {
                ValidBridgePoint previousBridgePoint     = bridge.Points[index + 1];
                toNextVector                           = - bridgePoint.PositionWS + previousBridgePoint.PositionWS;
            }
            else
            {
                ValidBridgePoint nextBridgePoint         = bridge.Points[index - 1];
                toNextVector                           = - nextBridgePoint.PositionWS + bridgePoint.PositionWS;
            }

            return toNextVector;
        }
    }
}