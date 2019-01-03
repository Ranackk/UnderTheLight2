using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.GameMain.Bridges
{
    static class BridgePlanner
    {
        ////////////////////////////////////////////////////////////////
        /* [USAGE]                                        
        //
        // EACH TICK: 
        // ValidateBridgePoint     for the players look at position
        // CanCreateBridge         if the player has a valid point placed and looks at another valid point
        ////////////////////////////////////////////////////////////////
        // ON PLAYER PLACE BRIDGE:
        // PrepareBridge           when the player wants to place a bridge
        //////////////////////////////////////////////////////////////*/

        /// <summary>
        /// Checks if a ray cast hit produces a valid bridge point.
        /// </summary>
        /// <param name="hit">In RaycastHit</param>
        /// <param name="outPoint">Optional out in success case</param>
        /// <returns></returns>
        public static BridgePointValidationResult ValidateBridgePoint(RaycastHit hit, out RawBridgePoint outPoint)
        {
            outPoint = RawBridgePoint.INVALID;

            ////////////////////////////////////////////////////////////////
            // Check if hit point is marked as shootable
            ////////////////////////////////////////////////////////////////

            // TODO: Research if Mesh Colliders for box shapes are bad performance wise;
            //Debug.Log("TODO: Find a better way to find out if a point is bridgeable! Resort to tag again? :/ ");
            //Color vertexColorHit    = ((MeshCollider)hit.collider).sharedMesh.colors[hit.triangleIndex];
            //bool isBridgeable       = vertexColorHit.b > 0.5f;
            //if (!isBridgeable)
            //{
            //    return BridgePointValidationResult.NonBridgableWall;
            //}
            //bool isMirror           = vertexColorHit.r > 0.5f;

            bool isMirror = true;

            Vector3 position = hit.point;
            Vector3 normal = hit.normal;

            outPoint = new RawBridgePoint(position, normal, isMirror);

            return BridgePointValidationResult.Success;
        }

        //////////////////////////////////////////////////////////////// 
        /// <summary>
        /// Checks if a valid bridge could exist between two input points
        /// </summary>
        /// <param name="bridgeInput"></param>
        /// <param name="bridgeOutput"></param>
        /// <returns></returns>
        public static BridgeValidationResult ValidateBridgePlacement(ValidateBridgeInput bridgeInput, out CreateBridgeParameters bridgeOutput)
        {
            bridgeOutput = CreateBridgeParameters.INVALIED;

            ////////////////////////////////////////////////////////////////
            // Input
            ////////////////////////////////////////////////////////////////

            Vector3 startPoint      = bridgeInput.PointStart.PositionWS;
            Vector3 endPoint        = bridgeInput.PointEnd.PositionWS;

            Vector3 startNormal     = bridgeInput.PointStart.WallNormalWS;
            Vector3 endNormal       = bridgeInput.PointEnd.WallNormalWS;

            Vector3 startToEnd      = endPoint - startPoint;
            Vector3 startToEndRight = Vector3.Cross(startToEnd, Vector3.up);

            ////////////////////////////////////////////////////////////////
            // Get right extends
            ////////////////////////////////////////////////////////////////

            Vector3 startPointWallExtendRight   = Vector3.Cross(startNormal, Vector3.up);
            Vector3 endPointWallExtendRight     = Vector3.Cross(endNormal, Vector3.up);

            // The two normals should face each other
            if (Vector3.Dot(startPointWallExtendRight, endPointWallExtendRight) > 0)
            {
                return BridgeValidationResult.TooNarrowToWallAngle;
            }

            // Start Point is facing opposite direction of End Point, so it checks for "< 0" instead of ">= 0"
            if (Vector3.Dot(startToEndRight, startPointWallExtendRight) < 0)
            {
                startPointWallExtendRight *= -1;
            }

            if (Vector3.Dot(startToEndRight, endPointWallExtendRight) >= 0)
            {
                endPointWallExtendRight *= -1;
            }

            ////////////////////////////////////////////////////////////////
            // Check both Wall Angles
            ////////////////////////////////////////////////////////////////

            float startPointWallAngle   = Vector3.Angle(-startToEnd, startNormal);
            float endPointWallAngle     = Vector3.Angle(startToEnd, endNormal);

            bool startAngleTooNarrow    = startPointWallAngle   < BridgeOptions.MIN_TO_WALL_ANGLE;
            bool endAngleTooNarrow      = endPointWallAngle     < BridgeOptions.MIN_TO_WALL_ANGLE;

            if (startAngleTooNarrow || endAngleTooNarrow)
            {
                return BridgeValidationResult.TooNarrowToWallAngle;
            }

            ////////////////////////////////////////////////////////////////
            // Check Steepness
            ////////////////////////////////////////////////////////////////
            
            float steepness = Vector3.Angle(startToEnd, new Vector3(startToEnd.x, 0, startToEnd.z));

            bool isPathTooSteep = steepness >= BridgeOptions.MAX_STEEP_ANGLE;

            if (isPathTooSteep)
            {
                return BridgeValidationResult.TooSteep;
            }


            ////////////////////////////////////////////////////////////////
            // Check Blocking
            ////////////////////////////////////////////////////////////////

            float bridgeWidthHalf = BridgeOptions.WIDTH_WS / 2.0f;

            Vector3 startLeft   = startPoint    - startPointWallExtendRight * bridgeWidthHalf;
            Vector3 startRight  = startPoint    + startPointWallExtendRight * bridgeWidthHalf;
            //Vector3 endLeft     = endPoint      - startPointWallExtendRight * bridgeWidthHalf;
            //Vector3 endRight    = endPoint      + startPointWallExtendRight * bridgeWidthHalf;
                
            float rayLength                 = startToEnd.magnitude - 0.2f;
            Vector3 startToEndNormalized    = startPoint.normalized;

            Ray rayLeft         = new Ray(startLeft  + startToEndNormalized * 0.1f, startToEndNormalized);
            Ray rayMid          = new Ray(startPoint + startToEndNormalized * 0.1f, startToEndNormalized);
            Ray rayRight        = new Ray(startRight + startToEndNormalized * 0.1f, startToEndNormalized);

            if (Physics.Raycast(rayLeft, rayLength) || Physics.Raycast(rayMid, rayLength) || Physics.Raycast(rayRight, rayLength))
            {
                return BridgeValidationResult.Blocked;
            }

            // UV not yet set, will be set when the bridge is created
            bridgeOutput = new CreateBridgeParameters(
                                            new ValidBridgePoint(bridgeInput.PointStart.PositionWS, bridgeInput.PointStart.WallNormalWS, bridgeInput.PointStart.IsMirror, startPointWallExtendRight, Vector3.up, Vector2.zero),
                                            new ValidBridgePoint(bridgeInput.PointEnd.PositionWS, bridgeInput.PointEnd.WallNormalWS, bridgeInput.PointEnd.IsMirror, endPointWallExtendRight, Vector3.up, Vector2.zero));

            return BridgeValidationResult.Success;
        }

        ////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns a valid bridge that can be used to create a mesh
        /// </summary>
        /// <param name="validBridgeInput"></param>
        /// <returns></returns>
        public static Bridge PrepareBridge(CreateBridgeParameters createBridgParamters)
        {
            FindMirrorPointsForBridge(ref createBridgParamters);
           
            float bridgeLength = 0.0f;
            FindUVsForBridge(ref createBridgParamters, out bridgeLength);

            return new Bridge(createBridgParamters.BridgePoints.ToArray(), bridgeLength);
        }
       
        /*//////////////////////////////////////////////////////////////
        //// Private Helpers    									////
        //////////////////////////////////////////////////////////////*/

        /// <summary>
        /// Finds all bridge points a bridge would have.
        /// </summary>
        /// <param name="validBridgeInput"></param>
        /// <returns></returns>
        private static void FindMirrorPointsForBridge(ref CreateBridgeParameters inOutBridgePoints)
        {
            Assert.IsTrue(inOutBridgePoints.BridgePoints.Count >= 2);

#if DEBUG 
            int debugCount = 0;
#endif
            ////////////////////////////////////////////////////////////////
            // For both start & end point of the bridge parameters:
            // If the point is a mirror, try to find the point before. Continue until no mirror is hit anymore.
            ////////////////////////////////////////////////////////////////

            int indexMirrorPoint    = 0;
            int indexOther          = 1;

            // Start Point
            while (inOutBridgePoints.BridgePoints[indexMirrorPoint].IsMirror)
            {
                Vector3 backToFront     = inOutBridgePoints.BridgePoints[indexOther].PositionWS - inOutBridgePoints.BridgePoints[indexMirrorPoint].PositionWS;
                Vector3 mirroredVector  = Vector3.Reflect(backToFront, inOutBridgePoints.BridgePoints[indexMirrorPoint].WallNormalWS);
                Vector3 mirroredVectorN = mirroredVector.normalized;
                Vector3 startPosition   = inOutBridgePoints.BridgePoints[indexMirrorPoint].PositionWS + mirroredVectorN * BridgeOptions.RAYCAST_PADDING;
                Ray ray                 = new Ray(startPosition, mirroredVector * BridgeOptions.MAX_BRIDGE_LENGTH);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    RawBridgePoint newPoint;
                    if (ValidateBridgePoint(hit, out newPoint) == BridgePointValidationResult.Success)
                    {
                        Vector3 wallExtendRight = Vector3.Cross(hit.normal, Vector3.up);
                        ValidBridgePoint newValidPoint = new ValidBridgePoint(newPoint.PositionWS, newPoint.WallNormalWS, newPoint.IsMirror, wallExtendRight, Vector3.up, Vector3.zero);

                        inOutBridgePoints.BridgePoints.Insert(indexMirrorPoint, newValidPoint);
                    }

#if DEBUG
                    Assert.IsTrue(debugCount < 20);
#endif
                    continue;
                }

                break;
            }

                // End Point
            indexMirrorPoint    = inOutBridgePoints.BridgePoints.Count - 1;
            indexOther          = inOutBridgePoints.BridgePoints.Count - 2;

            // End Point
            while (inOutBridgePoints.BridgePoints[indexMirrorPoint].IsMirror)
            {
                Vector3 backToFront     = inOutBridgePoints.BridgePoints[indexMirrorPoint].PositionWS - inOutBridgePoints.BridgePoints[indexOther].PositionWS;
                Vector3 mirroredVector  = Vector3.Reflect(backToFront, inOutBridgePoints.BridgePoints[indexMirrorPoint].WallNormalWS);
                Vector3 mirroredVectorN = mirroredVector.normalized;
                Vector3 startPosition   = inOutBridgePoints.BridgePoints[indexMirrorPoint].PositionWS + mirroredVectorN * BridgeOptions.RAYCAST_PADDING;
                Ray ray                 = new Ray(startPosition, mirroredVector * BridgeOptions.MAX_BRIDGE_LENGTH);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    RawBridgePoint newPoint;
                    if (ValidateBridgePoint(hit, out newPoint) == BridgePointValidationResult.Success)
                    {
                        Vector3 wallExtendRight = Vector3.Cross(hit.normal, Vector3.up);
                        ValidBridgePoint newValidPoint = new ValidBridgePoint(newPoint.PositionWS, newPoint.WallNormalWS, newPoint.IsMirror, wallExtendRight, Vector3.up, Vector3.zero);
                        
                        inOutBridgePoints.BridgePoints.Add(newValidPoint);
                    }

#if DEBUG
                    Assert.IsTrue(debugCount < 20);
#endif
                    indexMirrorPoint    = inOutBridgePoints.BridgePoints.Count - 1;
                    indexOther          = inOutBridgePoints.BridgePoints.Count - 2;

                    continue;
                }

                break;

            }

        }

        ////////////////////////////////////////////////////////////////
        /// <summary>
        /// Finds the UVs for all points along a bridge
        /// </summary>
        /// <param name="inOutBridgePoints"></param>
        private static void FindUVsForBridge(ref CreateBridgeParameters inOutBridgePoints, out float bridgeLength)
        {
            Assert.IsTrue(inOutBridgePoints.BridgePoints.Count >= 2);

            ////////////////////////////////////////////////////////////////

            bridgeLength                            = 0.0f;

            ValidBridgePoint startPoint    = inOutBridgePoints.BridgePoints[0];
            startPoint.UV                  = new Vector2(bridgeLength, 0.0f);

            for (int i = 0; i < inOutBridgePoints.BridgePoints.Count - 1; i++)
            {
                Vector3 pointA  = inOutBridgePoints.BridgePoints[i].PositionWS;
                Vector3 pointB  = inOutBridgePoints.BridgePoints[i + 1].PositionWS;

                bridgeLength    += Vector3.Distance(pointA, pointB);

                ValidBridgePoint bridgePoint    = inOutBridgePoints.BridgePoints[i + 1];
                bridgePoint.UV                  = new Vector2(bridgeLength, 0.0f);
            }
        }
    }
}
