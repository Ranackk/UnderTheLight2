using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TNodeID	= System.Int16;
using TNodeSize	= System.Int32;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	public partial class Serializer 
	{
		///////////////////////////////////////////////////////////////////////////
		// Internally Used
		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, int value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + sizeof(int), NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out int value, int size)
		{
			Debug.Assert(size == TAG_SIZE_TOTAL + sizeof(int));
			value = m_BinaryReader.ReadInt32();
		}

		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, short value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + sizeof(short), NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out short value, int size)
		{
			Debug.Assert(size == TAG_SIZE_TOTAL + sizeof(short));
			value = m_BinaryReader.ReadInt16();
		}
		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, float value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + sizeof(float), NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out float value, int size)
		{
			Debug.Assert(size == TAG_SIZE_TOTAL + sizeof(float));
			value = m_BinaryReader.ReadSingle();
		}		

		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, double value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + sizeof(double), NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out double value, int size)
		{
			Debug.Assert(size == TAG_SIZE_TOTAL + sizeof(double));
			value = m_BinaryReader.ReadDouble();
		}	

		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, bool value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + sizeof(bool), NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}
		
		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out bool value, int size)
		{
			Debug.Assert(size == TAG_SIZE_TOTAL + sizeof(bool));
			value = m_BinaryReader.ReadBoolean();
		}	

		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, char value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + sizeof(char), NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out char value, int size)
		{
			Debug.Assert(size == TAG_SIZE_TOTAL + sizeof(char));
			value = m_BinaryReader.ReadChar();
		}	

		///////////////////////////////////////////////////////////////////////////

		void WriteLeaf(TNodeID nodeID, string value)
		{
			WriteNode(nodeID, TAG_SIZE_TOTAL + System.Text.ASCIIEncoding.Unicode.GetByteCount(value) + 1, NodeType.LeafNode);
			m_BinaryWriter.Write(value);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadLeafData(out string value, int size)
		{
			value = m_BinaryReader.ReadString();
			Debug.Assert(size == TAG_SIZE_TOTAL + System.Text.ASCIIEncoding.Unicode.GetByteCount(value) + 1);
		}	
		
		///////////////////////////////////////////////////////////////////////////
		
		bool PrepareSerializeLeaf<T>(string tagName, TNodeSize requiredDataSize, ref T value, T fallbackValue)
		{
			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				WriteNode(nodeID, TAG_SIZE_TOTAL + requiredDataSize, NodeType.LeafNode);
				return true;
			}
			else
			{			
				TNodeSize nodeSize;
				NodeType  nodeType;
				int nodePos = FindNodePos(nodeID, out nodeSize, out nodeType);

				if (nodePos == -1)
				{
					value = fallbackValue;
					return false;
				}

				GotoStreamPos(nodePos + TAG_SIZE_TOTAL, false);

				if (nodeSize != TAG_SIZE_TOTAL + requiredDataSize)
				{
					if (requiredDataSize != -1)	//< special values for strings
					{
						Debug.LogWarning("Size of attribute " + nodeID + " changed");

						value = fallbackValue;
						GotoStreamPos(nodePos + nodeSize, true);
						return false;	
					}
				}
			
				return true;	
			}		
		}

	///////////////////////////////////////////////////////////////////////////
	// Public
	///////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////
		// Parent Node
		///////////////////////////////////////////////////////////////////////////

		public bool OpenParentNode(string tagName)
		{
			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				WriteBeginParent(nodeID);
				return true;
			}
			else
			{
				NodeType nodeType;
				return ReadBeginParent(nodeID, out nodeType);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void CloseParentNode()
		{
			if (GetState() == State.Saving)
			{
				WriteEndParent();
			}
			else
			{
				ReadEndParent();
			}
		}

		///////////////////////////////////////////////////////////////////////////
		// Builtin Types
		///////////////////////////////////////////////////////////////////////////

		// int
		public bool Serialize(string tagName, ref int value, int fallbackValue)
		{
			if (!PrepareSerializeLeaf(tagName, sizeof(int), ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				m_BinaryWriter.Write(value);
			}
			else
			{
				value = m_BinaryReader.ReadInt32();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
		}

		// short
		public bool Serialize(string tagName, ref short value, short fallbackValue)
		{
			if (!PrepareSerializeLeaf(tagName, sizeof(short), ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				m_BinaryWriter.Write(value);
			}
			else
			{
				value = m_BinaryReader.ReadByte();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
		}

		// char
		public bool Serialize(string tagName, ref char value, char fallbackValue)
		{
			if (!PrepareSerializeLeaf(tagName, sizeof(char), ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				m_BinaryWriter.Write(value);
			}
			else
			{
				value = m_BinaryReader.ReadChar();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
		}

		// bool
		public bool Serialize(string tagName, ref bool value, bool fallbackValue)
		{
			if (!PrepareSerializeLeaf(tagName, sizeof(bool), ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				m_BinaryWriter.Write(value);
			}
			else
			{
				value = m_BinaryReader.ReadBoolean();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
		}

		// float
		public bool Serialize(string tagName, ref float value, float fallbackValue)
		{
			if (!PrepareSerializeLeaf(tagName, sizeof(float), ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				m_BinaryWriter.Write(value);
			}
			else
			{
				value = m_BinaryReader.ReadSingle();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
		}

		// double
		public bool Serialize(string tagName, ref double value, double fallbackValue)
		{
			if (!PrepareSerializeLeaf(tagName, sizeof(double), ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				m_BinaryWriter.Write(value);
			}
			else
			{
				value = m_BinaryReader.ReadDouble();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
		}

		// enum
		public bool Serialize<T>(string tagName, ref T value, T fallbackValue) where T : struct, System.IConvertible
		{
			if (!typeof(T).IsEnum) 
			{
				Debug.LogError("UnknownType : " + typeof(T));
			}

			int valueAsInt		= System.Convert.ToInt32(value);
			int fallbackAsInt	= System.Convert.ToInt32(fallbackValue);
			bool success = Serialize(tagName, ref valueAsInt, fallbackAsInt);

			value = (T)(object)valueAsInt;
			return success;
		}

		// string
		public bool Serialize(string tagName, ref string value, string fallbackValue)
		{
			int saveSize = -1; //< special value for unknown size when loading
			
			if (GetState() == State.Saving)
			{
				if (value == null)
				{
					saveSize = 1;
				}
				else
				{
					saveSize = System.Text.ASCIIEncoding.Unicode.GetByteCount(value) + 1;
				}
			}		

			if (!PrepareSerializeLeaf(tagName, saveSize, ref value, fallbackValue))
			{
				return false;
			}
			
			if (GetState() == State.Saving)
			{
				int posBefore = GetCurrentStreamPos();

				if (value == null)
				{
					m_BinaryWriter.Write("");
				}
				else
				{
					m_BinaryWriter.Write(value);
				}

				Debug.Assert(GetCurrentStreamPos() - posBefore == saveSize);
			}
			else
			{
				value = m_BinaryReader.ReadString();
			}

			GotoStreamPos(GetCurrentStreamPos(), true);

			return true;
        }

        ////////////////////////////////////////////////////////////////
        // Custom
        ////////////////////////////////////////////////////////////////

        // Vector2
        public bool Serialize(string tagName, ref Vector2 value, Vector2 fallbackValue)
        {
            bool success = true;
            success &= Serialize(tagName + "_x", ref value.x, fallbackValue.x);
            success &= Serialize(tagName + "_y", ref value.y, fallbackValue.y);
            return success;
        }

        // Vector3
        public bool Serialize(string tagName, ref Vector3 value, Vector3 fallbackValue)
        {
            bool success = true;
            success &= Serialize(tagName + "_x", ref value.x, fallbackValue.x);
            success &= Serialize(tagName + "_y", ref value.y, fallbackValue.y);
            success &= Serialize(tagName + "_z", ref value.z, fallbackValue.z);
            return success;
        }

        ///////////////////////////////////////////////////////////////////////////
        // Interfaces
        ///////////////////////////////////////////////////////////////////////////

        public interface ISerializable
		{
            // This nulls everything that is not present!
			void    Serialize(Serializer io);
			string  ToString(); 
		}

		public interface ISerializablePolymorphic : ISerializable
		{
			int     GetTypeEnum();
		}

		public interface ISerializablePolymorphicFactory
		{
			ISerializablePolymorphic CreateSerializable(int typeEnum);
		}

		///////////////////////////////////////////////////////////////////////////
		// ISerializable
		///////////////////////////////////////////////////////////////////////////

		public bool Serialize<T>(string tagName, ref T value, bool fallbackIsNewObject) where T : ISerializable, new()
		{
			Debug.Assert(!(value is ISerializablePolymorphic));

			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				if (value == null)
				{
					WriteNode(nodeID, TAG_SIZE_TOTAL, NodeType.LeafNode);
				}
				else
				{
					WriteBeginParent(nodeID);
					value.Serialize(this);
					WriteEndParent();
				}

				return true;
			}
			else
			{
				TNodeSize	nodeSize;
				NodeType	nodeType;
				int nodeStreamPos = FindNodePos(nodeID, out nodeSize, out nodeType);

				bool foundNode = (nodeStreamPos != -1);
				
				if (!foundNode)
				{
					if (fallbackIsNewObject)
					{
						value = new T();
					}
					else
					{
						value = default(T);
					}

					return false;
				}
				
				if (nodeType == NodeType.LeafNode)
				{
					// null
					value = default(T);
					GotoStreamPos(nodeStreamPos + nodeSize, true);
					return true;
				}

				ReadBeginParentAtPos(nodeStreamPos, nodeSize);
				value = new T();
				value.Serialize(this);
				ReadEndParent();

				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////
		// ISerializablePolymorphic
		///////////////////////////////////////////////////////////////////////////

		public bool Serialize<T>(string tagName, ref T value, ISerializablePolymorphicFactory factory) where T : ISerializablePolymorphic
		{
			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				if (value == null)
				{
					WriteNode(nodeID, TAG_SIZE_TOTAL, NodeType.LeafNode);
				}
				else
				{
					WriteBeginParent(nodeID);

					int typeEnum = value.GetTypeEnum();
					Serialize("_TypeID", ref typeEnum, -1);

					value.Serialize(this);
					WriteEndParent();
				}

				return true;
			}
			else
			{
				TNodeSize	nodeSize;
				NodeType	nodeType;
				int nodeStreamPos = FindNodePos(nodeID, out nodeSize, out nodeType);

				bool foundNode = (nodeStreamPos != -1);
				
				if (!foundNode || nodeType == NodeType.LeafNode)
				{
					// null or not found
					value = default(T);
					GotoStreamPos(nodeStreamPos + nodeSize, true);
					return foundNode;
				}			
				
				ReadBeginParentAtPos(nodeStreamPos, nodeSize);

				int typeEnum = -1;
				bool hasValidType = Serialize("_TypeID", ref typeEnum, -1);

				if (hasValidType)
				{
					ISerializablePolymorphic newSerializable = factory.CreateSerializable(typeEnum);

					try
					{
						value = (T) newSerializable;
					}
					catch (System.InvalidCastException)
					{
						Debug.LogError("Could not cast Factories " + 
							(newSerializable == null ? "[null]" : newSerializable.GetType().ToString()) +  
							" to " + typeof(T) + ". EnumValue: " + typeEnum);
						value = default(T);
					}

					if (value == null)
					{
						Debug.Assert(false, "Could not Create " + typeof(T) + " via Factory " + factory.ToString());
						hasValidType = false;
					}
					else
					{
						value.Serialize(this);
					}

				}

				ReadEndParent();

				if (!hasValidType)
				{
					value = default(T);
					return false;
				}

				return hasValidType;
			}
		}
		
	///////////////////////////////////////////////////////////////////////////
	// List
	///////////////////////////////////////////////////////////////////////////

		///////////////////////////////////////////////////////////////////////////
		// List Serializable
		///////////////////////////////////////////////////////////////////////////

		public bool Serialize<T>(string tagName, ref List<T> value, bool fallbackIsNewObject) where T : ISerializable, new()
		{
			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				WriteBeginParent(nodeID);

				int elemCount = (value == null) ? 0 : value.Count;
				Serialize("_ElemCount", ref elemCount, 0);

				for (int e = 0; e < elemCount; ++e)
				{
					T elem = value[e];

					Serialize("_Elem", ref elem, fallbackIsNewObject);
				}

				WriteEndParent();

				return true;
			}
			else
			{
				NodeType nodeType;
				if (!ReadBeginParent(nodeID, out nodeType))
				{
					value = new List<T>();
					return false;
				}

				int elemCount = 0;
				Serialize("_ElemCount", ref elemCount, 0);

				if (value == null)
				{
					value = new List<T>();
				}
				else
				{
					value.Clear();
				}

				value.Capacity = elemCount;

				for (int e = 0; e < elemCount; ++e)
				{
					T loadedElem = default(T);

					Serialize("_Elem", ref loadedElem, fallbackIsNewObject);
					value.Add(loadedElem);
				}

				ReadEndParent();

				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public bool Serialize<T>(string tagName, ref List<T> value, ISerializablePolymorphicFactory factory) where T : ISerializablePolymorphic
		{
			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				WriteBeginParent(nodeID);

				int elemCount = (value == null) ? 0 : value.Count;
				Serialize("_ElemCount", ref elemCount, 0);

				for (int e = 0; e < elemCount; ++e)
				{
					T elem = value[e];

					Serialize("_Elem", ref elem, factory);
				}

				WriteEndParent();

				return true;
			}
			else
			{
				NodeType nodeType;
				if (!ReadBeginParent(nodeID, out nodeType))
				{
					value = new List<T>();
					return false;
				}

				int elemCount = 0;
				Serialize("_ElemCount", ref elemCount, 0);

				if (value == null)
				{
					value = new List<T>();
				}
				else
				{
					value.Clear();
				}

				value.Capacity = elemCount;

				for (int e = 0; e < elemCount; ++e)
				{
					T loadedElem = default(T);

					Serialize("_Elem", ref loadedElem, factory);
					value.Add(loadedElem);
				}

				ReadEndParent();

				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////
		// List Builtin Helper
		///////////////////////////////////////////////////////////////////////////

		public delegate bool SerializeCollectionElement<T>(string name, ref T serializable, T fallbackValue);

		bool SerializeList<T>(string tagName, ref List<T> value, SerializeCollectionElement<T> callback)
		{
			TNodeID nodeID = TagNameToNodeID(tagName);

			if (GetState() == State.Saving)
			{
				WriteBeginParent(nodeID);

				int elemCount = (value == null) ? 0 : value.Count;
				Serialize("_ElemCount", ref elemCount, 0);

				for (int e = 0; e < elemCount; ++e)
				{
					T elem = value[e];

					callback("elem", ref elem, default(T));
				}

				WriteEndParent();

				return true;
			}
			else
			{
				NodeType nodeType;
				if (!ReadBeginParent(nodeID, out nodeType))
				{
					value = new List<T>();
					return false;
				}

				int elemCount = 0;
				Serialize("_ElemCount", ref elemCount, 0);

				if (value == null)
				{
					value = new List<T>();
				}
				else
				{
					value.Clear();
				}

				value.Capacity = elemCount;

				for (int e = 0; e < elemCount; ++e)
				{
					T loadedElem = default(T);

					callback("elem", ref loadedElem, default(T));
					value.Add(loadedElem);
				}

				ReadEndParent();

				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////
		// List Builtin Specializations
		///////////////////////////////////////////////////////////////////////////

		public bool Serialize(string tagName, ref List<int> value)
		{
			return SerializeList(tagName, ref value, Serialize);
		}
   
    }
    
	///////////////////////////////////////////////////////////////////////////

}