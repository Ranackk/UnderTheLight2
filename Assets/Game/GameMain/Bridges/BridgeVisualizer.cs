using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using Game.Core;

using JSDK.Events;
using JSDK.Misc;

namespace Game.GameMain.Bridges
{

	/*
	 * Component representation of the light bridge.
	 * This is used to render the bridge.
	 */
     
     [RequireComponent(typeof(MeshFilter))]
     [RequireComponent(typeof(MeshRenderer))]
	public class BridgeVisualizer : MonoBehaviour 
	{
		BridgeMesh  m_BridgeMeshData;
		Mesh        m_Mesh;
        
        MeshRenderer    m_MeshRenderer;
        MeshFilter      m_MeshFilter;

        [SerializeField] bool m_DebugGizmos = false;
		
		void Start()
		{   
			m_BridgeMeshData                        = BridgeMesh.INVALID;
			m_Mesh                                  = null;

            m_MeshRenderer                          = GetComponent<MeshRenderer>();
            m_MeshFilter                            = GetComponent<MeshFilter>();
            
            Debug.Assert(m_MeshRenderer != null);
            Debug.Assert(m_MeshFilter != null);
            
            EventManager.Instance.AddListener<CreateBridgeEvent>(OnNewBridgeMesh);
		}
		
		////////////////////////////////////////////////////////////////

		private void OnDestroy()
		{
            EventManager.Instance.RemoveListener<CreateBridgeEvent>(OnNewBridgeMesh);
		}

		////////////////////////////////////////////////////////////////

		void OnNewBridgeMesh (CreateBridgeEvent bridgeEvent) 
		{
			// TODO: Blending & stuff.
			m_BridgeMeshData = bridgeEvent.BridgeMeshData;
            BuildMeshFromBridgeMeshData();


			// Save, for each bridge vertex, the vector towards the according bridge point inside its color (can it overflow 1? I twould need to).
			// Then, inside the bridges shader, use the vertex color and a slider 0 to 1 to fade in and out the bridge towards its bridge points.
			// Also use transparency if possible.
		}

		////////////////////////////////////////////////////////////////
		
		void BuildMeshFromBridgeMeshData()
		{
			Debug.Assert(m_BridgeMeshData.IsValid());

			m_Mesh = new Mesh();
            
			Color[] colorsWS = new Color[m_BridgeMeshData.InnerPositions.Count];
			for (int i = 0; i < colorsWS.Length; i++)
			{
                Vector3 innerPosition   = m_BridgeMeshData.InnerPositions[i];
                Vector3 offsetVector    = m_BridgeMeshData.Positions[i] - innerPosition;
				colorsWS[i]             = new Color(offsetVector.x, offsetVector.y, offsetVector.z);
			}

			int[] indicies              = m_BridgeMeshData.Indicies.ToArray();
			Vector3[] positionsWS       = m_BridgeMeshData.Positions.ToArray();
			Vector3[] normalsWS         = m_BridgeMeshData.Normals.ToArray();
			Vector2[] uvs               = m_BridgeMeshData.UVs.ToArray();

			m_Mesh.vertices   = positionsWS;
			m_Mesh.normals    = normalsWS;
			m_Mesh.uv         = uvs;
			m_Mesh.colors     = colorsWS;

			m_Mesh.triangles  = indicies;

            m_MeshFilter.sharedMesh = m_Mesh;
		}

        ////////////////////////////////////////////////////////////////

        private void Update()
        {
        }

        private void OnDrawGizmos()
		{
            if (!m_DebugGizmos)
            {
                return;
            }

            BridgeManager.Instance.DEBUG_DrawGizmos();

            if (!(m_BridgeMeshData.IsValid() && m_Mesh != null))
            {
                return;
            }
            
			for (int i = 0; i < m_BridgeMeshData.Positions.Count; i++)
			{
				bool isLeftPoint = i % 2 == 0;

				if (isLeftPoint)
				{
					Gizmos.color = new Color (0.4f, 0.4f, 1.0f, 0.5f);
				}
				else
				{
					Gizmos.color = new Color (0.4f, 1.0f, 0.4f, 0.5f);
				}

				Vector3 position = m_BridgeMeshData.Positions[i];

				Gizmos.DrawWireCube(position, Vector3.one * 0.1f);
			}

			////////////////////////////////////////////////////////////////
				
			Gizmos.color = Color.white;
			Gizmos.DrawWireMesh(m_Mesh);
		}
	}
}
