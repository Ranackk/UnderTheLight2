using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using TNodeID	= System.Int16;
using TNodeSize	= System.Int32;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	public partial class Serializer
	{
		
		///////////////////////////////////////////////////////////////////////////

		public static string ToDebugString(byte[] binarySave)
		{
			string nonHumanReadable = "";
			string humanReadable	= "";

			try
			{
				// 1) Try Non human readable as fallback
				TryDebugReadFile(ref nonHumanReadable, binarySave, false);

				// 2) Try Human redable
				TryDebugReadFile(ref humanReadable, binarySave, true);
			}
			catch (System.Exception e)
			{
				string debugString = "!!! " + e.Message + "!!!\n\n";
				
				if (humanReadable.Length > 0)
				{
					debugString += humanReadable + "\n\n";
				}

				debugString += nonHumanReadable;
				return debugString;
			}

			return humanReadable;
		}

		///////////////////////////////////////////////////////////////////////////

		static void TryDebugReadFile(ref string debugString, byte[] binarySave, bool withDictionary)
		{
			Serializer serializer = new Serializer();
			serializer.BeginLoading(binarySave, !withDictionary);
			
			while (serializer.m_LevelPositionStack.Count > 1)
			{
				serializer.m_LevelPositionStack.Pop();
			}
			serializer.GotoStreamPos(0, false);

			Stack<int> removeIndentAt = new Stack<int>();
			int indentLevel = 0;

			while (true)
			{
				TNodeID			nodeID;
				TNodeSize		size;
				NodeType		nodeType = NodeType.Parent;

				int nodeStartPos = serializer.GetCurrentStreamPos();

				serializer.ReadNextNode(out nodeID, out size, out nodeType);

				debugString += "(" + nodeStartPos.ToString("0000") + ")    ";

				for (int i = 0; i < indentLevel; ++i)
				{
					debugString += "  ";
				}

				string nodeIDString = nodeID.ToString();

				switch (nodeID)
				{
					case (TNodeID)SpecialNodeIDs.DataSection:			nodeIDString = "Data";				break;
					case (TNodeID)SpecialNodeIDs.Dictionary:			nodeIDString = "Dictionary";		break;
					case (TNodeID)SpecialNodeIDs.DictionaryKey:			nodeIDString = "Key";				break;
					case (TNodeID)SpecialNodeIDs.DictionaryValue:		nodeIDString = "Value";				break;
					case (TNodeID)SpecialNodeIDs.DocumentRoot:			nodeIDString = "Root";				break;
					case (TNodeID)SpecialNodeIDs.FileVersion:			nodeIDString = "FileVersion";		break;
					case (TNodeID)SpecialNodeIDs.MetaSection:			nodeIDString = "Meta";				break;
					case (TNodeID)SpecialNodeIDs.IsNull:				nodeIDString = "IsNull";			break;
					default:
						if (withDictionary)
						{
							string key = serializer.m_NodeNameToIds.FirstOrDefault(x => x.Value == nodeID).Key;
							if (key == "")
							{
								if (serializer.m_NodeNameToIds.ContainsValue(nodeID))
								{
									key = "\"\"";
								}
								else
								{
									key = nodeID.ToString() + " (not found)";
								}
							}

							nodeIDString = key;
						}			
						break;
				}

				debugString += "[ " + nodeIDString + " , " + size + " ]";

				if (nodeType == NodeType.Parent)
				{
					indentLevel++;
					removeIndentAt.Push(nodeStartPos + size);
				}

				if (nodeType == NodeType.LeafNode)
				{
					int dataSize = size - TAG_SIZE_TOTAL;

					debugString += ": ";

					bool printFallbackBytesOrString = false;

					if (dataSize == 4)
					{
						// int / float
						int pos = serializer.GetCurrentStreamPos();
						debugString += serializer.m_BinaryReader.ReadInt32();
						serializer.GotoStreamPos(pos, false);
						debugString += " / " + serializer.m_BinaryReader.ReadSingle().ToString("0.00");
					}
					else if (dataSize == 2)
					{
						debugString += serializer.m_BinaryReader.ReadInt16();
					}
					else if (dataSize == 8)
					{
						// double
						debugString += serializer.m_BinaryReader.ReadDouble();
					}
					else if (dataSize == 0)
					{
						debugString += "[null]";
					}
					else if (dataSize < 0)
					{
						debugString += "[ERROR]";
					}
					else if (dataSize == 1)
					{
						debugString += serializer.m_BinaryReader.ReadByte();
					}
					else
					{
						printFallbackBytesOrString = true;
					}

					if (printFallbackBytesOrString)
					{
						// string?
						int oldPos = serializer.GetCurrentStreamPos();

						bool success = false;
						
						try
						{
							string str = serializer.m_BinaryReader.ReadString();

							int newPos = serializer.GetCurrentStreamPos();

							if (System.Text.ASCIIEncoding.Unicode.GetByteCount(str) + 1 == dataSize)
							{
								if (newPos - oldPos == dataSize) //< can happen when string consists of 0x1 char
								{
									success = true;
									debugString += "\"" + str + "\"";
								}
							}
						}
						catch (System.Exception) { }
						
						if (!success)
						{
							serializer.GotoStreamPos(oldPos, false);

							// fallback: bytes
							for (int j = 0; j < dataSize; ++j)
							{
								debugString += serializer.m_BinaryReader.ReadByte() + " ";
							}
						}


					}
				}

				int currentPos = serializer.GetCurrentStreamPos();

				while (removeIndentAt.Count > 0 && currentPos >= removeIndentAt.Peek())
				{
					Debug.Assert(currentPos == removeIndentAt.Peek());
					indentLevel--;
					removeIndentAt.Pop();
				}

				if (currentPos == serializer.m_BinaryStream.Length)
				{
					Debug.Assert(indentLevel == 0);
					Debug.Assert(removeIndentAt.Count == 0);
					break;
				}

				debugString += "\n";
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}





}