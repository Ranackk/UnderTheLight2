using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	public abstract class IMonoBehaviourWithAdditionalSerialize : MonoBehaviour, ISerializationCallbackReceiver
	{
		/*[HideInInspector] */ [SerializeField] private byte[] m_SerializedBlob;

		public void OnBeforeSerialize()
		{
			Serializer serializer = new Serializer();
			serializer.BeginSaving();
			AdditionalSerialize(serializer);
			m_SerializedBlob = serializer.EndSaving();
		}

		public void OnAfterDeserialize()
		{
			Serializer serializer = new Serializer();
			serializer.BeginLoading(m_SerializedBlob);
			AdditionalSerialize(serializer);
			serializer.EndLoading();
		}

		public byte[] GetSerializedBlob()
		{
			return m_SerializedBlob;
		}

		public string TrySaveAndGetReadableBlob(bool dumpToFile)
		{
			try
			{
				Serializer serializer = new Serializer();
				serializer.BeginSaving();
				AdditionalSerialize(serializer);
				byte[] serializedBlob = serializer.EndSaving();

				string debugStr = Serializer.ToDebugString(serializedBlob);

				if (dumpToFile)
				{
					SerializeDebugHelper.WriteToParserDebugFile(debugStr);
				}

				return debugStr;
			}
			catch (System.Exception e)
			{
				Debug.Assert(false, "Serialization Failed :(\n" + e.ToString());
			}

			return "";
		}

		abstract public void			AdditionalSerialize(Serializer serializer);
		public abstract override string ToString(); 
	}

	///////////////////////////////////////////////////////////////////////////


}