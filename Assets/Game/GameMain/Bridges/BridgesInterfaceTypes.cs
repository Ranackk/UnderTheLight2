using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using External.JEichner.ooo;

namespace Game.GameMain.Bridges
{
    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Internal settings for bridges
    /// </summary>
    public static class BridgeOptions
    {
        public const float WIDTH_WS          = 2.0f;
        public const float HEIGHT_WS         = 0.5f;
        public const float MAX_STEEP_ANGLE   = 50.0f;
        public const float MIN_TO_WALL_ANGLE = 20.0f;   // Bridges need to have at least 20° towards both end points
        public const float MAX_BRIDGE_LENGTH = 40.0f;
        public const float RAYCAST_PADDING   = 0.1f;
    }

    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Raw information about a bridge point
    /// </summary>
    public struct RawBridgePoint : Serializer.ISerializable
    {
        public static RawBridgePoint INVALID = new RawBridgePoint(Vector3.zero, Vector3.zero, false);

        public Vector3 PositionWS;
        public Vector3 WallNormalWS;
        public bool    IsMirror;

        public RawBridgePoint(Vector3 positionWS, Vector3 wallNormalWS, bool isMirror)
        {
            PositionWS   = positionWS;
            WallNormalWS = wallNormalWS;
            IsMirror     = isMirror;
        }

        public bool IsValid()
        {
            return WallNormalWS.sqrMagnitude != 0.0f;
        }

        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("PositionWS", ref PositionWS, Vector3.zero);
            io.Serialize("NormalWS", ref WallNormalWS, Vector3.zero);
            io.Serialize("IsMirror", ref IsMirror, false);
        }
    }

    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Use to validate bridges
    /// </summary>
    public struct ValidateBridgeInput
    {
        public RawBridgePoint PointStart;
        public RawBridgePoint PointEnd;

        public ValidateBridgeInput(RawBridgePoint startPoint, RawBridgePoint endPoint)
        {
            PointStart = startPoint;
            PointEnd = endPoint;
        }
    }

    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// A valid bridge point that can be placed & used to create a bridge
    /// </summary>l
    public struct ValidBridgePoint : Serializer.ISerializable
    {
        public static ValidBridgePoint INVALID = new ValidBridgePoint();

        public Vector3 PositionWS;
        public Vector3 WallNormalWS;
        public bool    IsMirror;

        public Vector3 RightTangentWS;
        public Vector3 UpTangentWS;

        public Vector2 UV;             // U = Absolute coordinate along bridge

        public ValidBridgePoint(Vector3 positionWS, Vector3 wallNormalWS, bool isMirror, Vector3 rightTangentWS, Vector3 upTangentWS, Vector2 uv)
        {
            PositionWS      = positionWS;
            WallNormalWS    = wallNormalWS;
            IsMirror        = isMirror;
            RightTangentWS  = rightTangentWS;
            UpTangentWS     = upTangentWS;
            UV              = uv;
        }

        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("PositionWS",      ref PositionWS,     Vector3.zero);
            io.Serialize("NormalWS",        ref WallNormalWS,   Vector3.zero);
            io.Serialize("IsMirror",        ref IsMirror,       false);
            io.Serialize("RightTangentWS",  ref RightTangentWS, Vector3.zero);
            io.Serialize("UpTangentWS",     ref UpTangentWS,    Vector3.zero);
            io.Serialize("UV",              ref UV,             Vector2.zero);
        }
    }

    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Input for bridge creation
    /// </summary>
    public struct CreateBridgeParameters
    {
        public static CreateBridgeParameters INVALIED = new CreateBridgeParameters();

        public List<ValidBridgePoint> BridgePoints;

        public CreateBridgeParameters(ValidBridgePoint startPoint, ValidBridgePoint endPoint)
        {
            BridgePoints = new List<ValidBridgePoint>()
            {
                startPoint, endPoint
            };
        }
    }

    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Holds a valid bridge that can be used to create a mesh
    /// </summary>
    public struct Bridge : Serializer.ISerializable
    {
        public static Bridge INVALID = new Bridge(null, -1.0f);

        public float Length;
        public ValidBridgePoint[] Points;

        public Bridge(ValidBridgePoint[] points, float length)
        {
            Length = length;
            Points = points;
        }

        public bool IsValid()
        {
            return Length >= 0.0f;
        }
        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("Length", ref Length, -1.0f);

            if (io.GetState() == Serializer.State.Saving)
            {
                if (Points != null)
                {
                    List<ValidBridgePoint> pointList = new List<ValidBridgePoint>(Points);
                    io.Serialize("Points", ref pointList, false);
                }
            }
            else if (io.GetState() == Serializer.State.Loading)
            {
                List<ValidBridgePoint> pointList = new List<ValidBridgePoint>();
                io.Serialize("Points", ref pointList, false);
                Points = pointList.ToArray();
            }
        }
    }
    
    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Holds a vertex of the bridge
    /// </summary>
    public struct BridgeMeshVertex : Serializer.ISerializable
    {
        public Vector3 PositionWS;      // The position of the vertex
        public Vector3 InnerPositionWS; // The position of the corresponding bridge point (mid point).
        public Vector3 NormalWS;
        public Vector2 UV;
        
        ////////////////////////////////////////////////////////////////
        
        public static BridgeMeshVertex INVALID = new BridgeMeshVertex(Vector3.zero, Vector3.zero, Vector3.zero, Vector2.zero);

        public BridgeMeshVertex(Vector3 positionWS, Vector3 innerPositionWS, Vector3 normalWS, Vector2 uv)
        {
            PositionWS      = positionWS;
            InnerPositionWS = innerPositionWS;
            NormalWS        = normalWS;
            UV              = uv;
        }

        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("PositionWS",      ref PositionWS,         Vector3.zero);
            io.Serialize("InnerPositionWS", ref InnerPositionWS,    Vector3.zero);
            io.Serialize("NormalWS",        ref NormalWS,           Vector3.zero);
            io.Serialize("UV",              ref UV,                 Vector2.zero);
        }
    }
    
    ////////////////////////////////////////////////////////////////
    /// <summary>
    /// Holds a vertex of the bridge
    /// </summary>
    public struct BridgeMesh : Serializer.ISerializable
    {
        public List<Vector3>   Positions;
        public List<Vector3>   InnerPositions;
        public List<Vector3>   Normals;
        public List<Vector2>   UVs;
        public List<int>       Indicies;
        
        ////////////////////////////////////////////////////////////////
        
        public static BridgeMesh INVALID = new BridgeMesh(new List<BridgeMeshVertex>(), new List<int>());

        public BridgeMesh(List<BridgeMeshVertex> vertices, List<int> indicies)
        {
            Indicies        = indicies;
            
            Positions       = new List<Vector3>(); 
            InnerPositions  = new List<Vector3>();
            Normals         = new List<Vector3>(); 
            UVs             = new List<Vector2>(); 

            for (int i = 0; i < vertices.Count; i++)
            {
                BridgeMeshVertex vertex = vertices[i];
                
                Positions.Add(vertex.PositionWS);
                InnerPositions.Add(vertex.InnerPositionWS);
                Normals.Add(vertex.NormalWS);
                UVs.Add(vertex.UV);
            }
        }
        
        ////////////////////////////////////////////////////////////////

        public bool IsValid()
        {
            return Positions.Count != 0 && Normals.Count != 0 && UVs.Count != 0 && Indicies.Count != 0;
        }

        ////////////////////////////////////////////////////////////////

        public void Serialize(Serializer io)
        {
            io.Serialize("Indicies", ref Indicies);

            ////////////////////////////////////////////////////////////////
            
            List<BridgeMeshVertex> vertices = new List<BridgeMeshVertex>();

            if (io.GetState() == Serializer.State.Saving)
            {
                for (int i = 0; i < Positions.Count; i++)
                {
                    vertices.Add(new BridgeMeshVertex(Positions[i], InnerPositions[i], Normals[i], UVs[i]));
                }
            }

            io.Serialize("Vertices", ref vertices, false);

            if (io.GetState() == Serializer.State.Loading)
            {
                Positions       = new List<Vector3>(); 
                InnerPositions  = new List<Vector3>(); 
                Normals         = new List<Vector3>(); 
                UVs             = new List<Vector2>(); 

                for (int i = 0; i < vertices.Count; i++)
                {
                    BridgeMeshVertex vertex = vertices[i];
                
                    Positions.Add(vertex.PositionWS);
                    InnerPositions.Add(vertex.InnerPositionWS);
                    Normals.Add(vertex.NormalWS);
                    UVs.Add(vertex.UV);
                }
            }
        }
    }

    /*//////////////////////////////////////////////////////////////
    //// Result Enums  										    ////
    //////////////////////////////////////////////////////////////*/

    /// <summary>
    /// Result of a bridge point validation
    /// </summary>
    public enum BridgePointValidationResult
    {
        Invalid             = -1,
        Success             = 0,
        NonBridgableWall    = 1,
        Count               = 2
    }

    /// <summary>
    /// Result of a bridge validation
    /// </summary>
    public enum BridgeValidationResult
    {
        Invalid                 = -1,
        Success                 = 0,
        TooNarrowToWallAngle    = 1,
        TooSteep                = 2,
        Blocked                 = 3,

        Count                   = 4
    }
}

