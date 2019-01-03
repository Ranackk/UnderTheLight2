using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

using TNodeID	= System.Int16;
using TNodeSize	= System.Int32;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	// [Start, 40]
	//		[Meta, 10]
	//			[FileVersion, 1, Leaf]		0
	//		[Data, 20]
	//			["Monkey", 10]
	//				["Height", 1, Leaf]		6
	//				["Width",  4, Leaf]		2.3
	//			["Peter", 10]
	//				["Height", 1, Leaf]		6
	//			["Monkey", 10]
	//				[IsNull, 1]				true
	//		[Dictionary, 10]
	//			[Key, 6, Leaf]		"Monkey"
	//			[Value, 1, Leaf]	0
	//			[Key, 5, Leaf]		"Height"
	//			[Value, 1, Leaf]	1
	// 
	//
	//
	// Invariants: 
	// 1) When writing, we always write to the end of the stream and leave the streamPos at the end for other functions

	public partial class Serializer 
	{
		const int TAG_SIZE_NODE_ID		= sizeof(TNodeID);
		const int TAG_SIZE_DATA_SIZE	= sizeof(TNodeSize);
		const int TAG_SIZE_NODE_TYPE	= sizeof(Char);
		const int TAG_SIZE_TOTAL		= TAG_SIZE_NODE_ID + TAG_SIZE_DATA_SIZE + TAG_SIZE_NODE_TYPE;
		const int HIGHEST_FILE_VERSION	= 0;
		Encoding STRING_ENCODING		= Encoding.Unicode;

		///////////////////////////////////////////////////////////////////////////

		public enum State
		{
			None,
			Loading,
			Saving,
		}

		///////////////////////////////////////////////////////////////////////////

		enum NodeType
		{
			Parent		= 0,
			LeafNode	= 1,
		}

		///////////////////////////////////////////////////////////////////////////

		enum SpecialNodeIDs
		{
			Invalid				= Int16.MaxValue,

			DocumentRoot		= Int16.MaxValue - 1,
			MetaSection			= Int16.MaxValue - 2,
				FileVersion		= Int16.MaxValue - 3,	
			DataSection			= Int16.MaxValue - 4,
			Dictionary			= Int16.MaxValue - 5,
				DictionaryKey	= Int16.MaxValue - 6,
				DictionaryValue	= Int16.MaxValue - 7,

			IsNull				= Int16.MaxValue - 20,

			BeginSpecialIDs		= Int16.MaxValue - 100,
		}

		///////////////////////////////////////////////////////////////////////////

		struct LevelPosition
		{
			public int LevelStartPosition;			// before the first Node within this level
			public int LevelEndPosition_IfLoading;	// position after the last position of this node
			public int CurrentPosition;				// before the last not-completely-read-Node
		}

		///////////////////////////////////////////////////////////////////////////

		private MemoryStream				m_BinaryStream	= null;
		private BinaryWriter				m_BinaryWriter  = null;
		private BinaryReader				m_BinaryReader	= null;

		private Stack<LevelPosition>		m_LevelPositionStack	= null;
		private Dictionary<string, TNodeID>	m_NodeNameToIds			= null;

		private int							m_Version = -1;

		///////////////////////////////////////////////////////////////////////////
		// Init/End Save/Load
		///////////////////////////////////////////////////////////////////////////

		public void BeginSaving()
		{
			m_Version = HIGHEST_FILE_VERSION;

			Debug.Assert(GetState() == State.None);
			m_BinaryStream = new MemoryStream();
			m_BinaryWriter = new BinaryWriter(m_BinaryStream, STRING_ENCODING);

			InitOnNewRun();

			PushCurrentPositionToStack(-1);

			WriteBeginParent((int) SpecialNodeIDs.DocumentRoot);
				WriteBeginParent((int) SpecialNodeIDs.MetaSection);
					SerializeMetaData();
				WriteEndParent();	// MetaSection
				WriteBeginParent((int) SpecialNodeIDs.DataSection);
		}

		///////////////////////////////////////////////////////////////////////////

		public byte[] EndSaving()
		{
			Debug.Assert(GetState() == State.Saving);
			Debug.Assert(GetCurrentStreamPos() == m_BinaryStream.Length);

				WriteEndParent();	// DataSection
				WriteBeginParent((int) SpecialNodeIDs.Dictionary);
					WriteDictionaryData();
				WriteEndParent(); // Dictionary
			WriteEndParent(); // DocumentRoot

			byte[] binaryDataWithPadding	= m_BinaryStream.GetBuffer();
			byte[] binaryData				= new byte[m_BinaryStream.Length];

			Array.Copy(binaryDataWithPadding, binaryData, binaryData.Length);

			m_BinaryWriter.Close();
			m_BinaryWriter = null;

			m_BinaryStream.Close();
			m_BinaryStream = null;

			return binaryData;
		}

		///////////////////////////////////////////////////////////////////////////

		int GetCurrentStreamPos()
		{
			return (int) m_BinaryStream.Position;
		}

		///////////////////////////////////////////////////////////////////////////

		void AssertWithinCurrentLevel(bool allowAtEnd)
		{
			int currentPos = GetCurrentStreamPos(); 

			LevelPosition levelPosition = m_LevelPositionStack.Peek();

			if (currentPos >= levelPosition.LevelStartPosition &&																						// after start
				((currentPos < levelPosition.LevelEndPosition_IfLoading || (currentPos <= levelPosition.LevelEndPosition_IfLoading && allowAtEnd))		// before/at end
				|| (levelPosition.LevelEndPosition_IfLoading == -1)))																					// saving
			{
				return;
			}

			Debug.Assert(false);
		}

		///////////////////////////////////////////////////////////////////////////

		void GotoStreamPos(int pos, bool updateCurrentLevelPos)
		{
			m_BinaryStream.Seek(pos, SeekOrigin.Begin);

			if (updateCurrentLevelPos)
			{
				LevelPosition topPosition	= m_LevelPositionStack.Pop();
				topPosition.CurrentPosition = pos;

				m_LevelPositionStack.Push(topPosition);
			}

			AssertWithinCurrentLevel(true);
		}

		///////////////////////////////////////////////////////////////////////////

		public void BeginLoading(byte[] binaryData, bool skipAllPredefinedNodesForDebugging = false)
		{
			if (binaryData == null)
			{
				binaryData = new byte[0];
			}

			Debug.Assert(GetState() == State.None);
			m_BinaryStream			= new MemoryStream(binaryData);
			m_BinaryReader			= new BinaryReader(m_BinaryStream, STRING_ENCODING);
			
			InitOnNewRun();

			PushCurrentPositionToStack(binaryData.Length);

			if (skipAllPredefinedNodesForDebugging)
			{
				return;
			}

			NodeType nodeType;

			ReadBeginParent((int) SpecialNodeIDs.DocumentRoot, out nodeType);
				ReadBeginParent((int) SpecialNodeIDs.MetaSection, out nodeType);
					SerializeMetaData();
				ReadEndParent(); //< meta section

				ReadBeginParent((int) SpecialNodeIDs.Dictionary, out nodeType);
					ReadDictionaryData();
				ReadEndParent(); //< Dictionary

				ReadBeginParent((int) SpecialNodeIDs.DataSection, out nodeType);
		}

		///////////////////////////////////////////////////////////////////////////

		void SerializeMetaData()
		{
			bool foundVersionTag = Serialize("FileVersion", ref m_Version, -1);

			if (!foundVersionTag)
			{
				Debug.LogWarning("Did not find VersionTag in Serialized Data");
			}
			else if (m_Version != HIGHEST_FILE_VERSION)
			{
				Debug.LogWarning("Serializing old file Version");
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void EndLoading()
		{
			Debug.Assert(GetState() == State.Loading);

				ReadEndParent();		//< data section
			ReadEndParent();			//< document root

			Debug.Assert(m_LevelPositionStack.Count == 1);

			m_BinaryReader.Close();
			m_BinaryReader = null;

			m_BinaryStream.Close();
			m_BinaryStream = null;
		}

		///////////////////////////////////////////////////////////////////////////

		void WriteNode(TNodeID nodeID, TNodeSize size, NodeType nodeType)
		{
			m_BinaryWriter.Write(nodeID);
			m_BinaryWriter.Write(size);
			m_BinaryWriter.Write((char)nodeType);
		}

		///////////////////////////////////////////////////////////////////////////

		void WriteBeginParent(TNodeID nodeID)
		{
			GotoStreamPos(GetCurrentStreamPos(), true);

			WriteNode(nodeID, -1, NodeType.Parent);

			PushCurrentPositionToStack(-1);
		}
	
		///////////////////////////////////////////////////////////////////////////

		//	A1
		//		*B1
		//			C1
		//			C2
		//		#B2
		//			C1
		//			C2
		//		$B3
		//			C1
		//			C2
		// 	
		// 	
		// 	
		// * == Level.currentLevelStartPos
		// # == Level.currentPos
		// $ == StreamPos
		void WriteEndParent()
		{
			// We finished this level
			m_LevelPositionStack.Pop();

			LevelPosition levelPosition = m_LevelPositionStack.Peek();
			int nextStreamPos = GetCurrentStreamPos();

			Debug.Assert(nextStreamPos == m_BinaryStream.Length, "Shouldn't we always write to the end of the stream?");

			// Overwrite size (2nd param)
			// SIZE means "From the beginning of the current node to the beginning of the next node"
			GotoStreamPos(levelPosition.CurrentPosition + TAG_SIZE_NODE_ID, false);
			m_BinaryWriter.Write(nextStreamPos - levelPosition.CurrentPosition);

			GotoStreamPos(nextStreamPos, true);
		}

		///////////////////////////////////////////////////////////////////////////

		int FindNodePos(TNodeID nodeID, out TNodeSize nodeSize, out NodeType nodeType)
		{
			nodeSize = -1;
			nodeType = NodeType.LeafNode;

			int oldPos = GetCurrentStreamPos();

			LevelPosition levelPosition = m_LevelPositionStack.Peek();

			// At least one node within this hierarchy?
			if (levelPosition.LevelStartPosition == levelPosition.LevelEndPosition_IfLoading)
			{
				return -1;
			}

			int nextStreamTestPos = levelPosition.CurrentPosition;
			ForceLevelEndToLevelStart(ref nextStreamTestPos);

			int firstTestPos = nextStreamTestPos;

			while (true)
			{
				TNodeID nextNodeID		= 0;
				TNodeSize nextNodeSize	= 0;
				NodeType nextNodeType	= NodeType.Parent;

				GotoStreamPos(nextStreamTestPos, false);
				ReadNextNode(out nextNodeID, out nextNodeSize, out nextNodeType);

				if (nextNodeID == nodeID)
				{
					// found it!
					GotoStreamPos(oldPos, false);
					nodeSize = nextNodeSize;
					nodeType = nextNodeType;
					return nextStreamTestPos;
				}
				
				nextStreamTestPos = nextStreamTestPos + nextNodeSize;
				ForceLevelEndToLevelStart(ref nextStreamTestPos);

				if (firstTestPos == nextStreamTestPos)
				{
					// nothing found :(
					GotoStreamPos(oldPos, false);
					return -1;
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

		bool ReadBeginParent(TNodeID nodeID, out NodeType nodeType)
		{
			TNodeSize nodeSize;
			int nodeStreamPos = FindNodePos(nodeID, out nodeSize, out nodeType);

			if (nodeStreamPos == -1)
			{
				return false;
			}

			Debug.Assert(nodeType == NodeType.Parent);
			ReadBeginParentAtPos(nodeStreamPos, nodeSize);
			return true;
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadBeginParentAtPos(int pos, int size)
		{
			GotoStreamPos(pos, true);

			GotoStreamPos(pos + TAG_SIZE_TOTAL, false);
			PushCurrentPositionToStack(pos + size);
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadEndParent()
		{
			// old level is finished
			m_LevelPositionStack.Pop();
			
			LevelPosition oldLevelPos = m_LevelPositionStack.Peek();

			GotoStreamPos(oldLevelPos.CurrentPosition, false);

			TNodeID nextNodeID		= 0;
			TNodeSize nextNodeSize	= 0;
			NodeType nodeType		= NodeType.Parent;

			ReadNextNode(out nextNodeID, out nextNodeSize, out nodeType);

			int nextPos = oldLevelPos.CurrentPosition + nextNodeSize;
			GotoStreamPos(nextPos, true);
		}

		///////////////////////////////////////////////////////////////////////////

		void ForceLevelEndToLevelStart(ref int pos)
		{
			LevelPosition levelPosition = m_LevelPositionStack.Peek();

			// At least one node within this hierarchy?
			if (pos >= levelPosition.LevelEndPosition_IfLoading)
			{
				bool posValid = (pos == levelPosition.LevelEndPosition_IfLoading) && (pos >= levelPosition.LevelStartPosition) && (levelPosition.LevelStartPosition != levelPosition.LevelEndPosition_IfLoading);

				Debug.Assert(posValid, "Setting ReadPos to position out of Level");
				pos = levelPosition.LevelStartPosition;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void WriteDictionaryData()
		{
			foreach (var nameToID in m_NodeNameToIds)
			{
				if (nameToID.Value == (TNodeID)SpecialNodeIDs.FileVersion)
				{
					continue;
				}

				WriteLeaf((TNodeID) SpecialNodeIDs.DictionaryKey,	nameToID.Key);
				WriteLeaf((TNodeID) SpecialNodeIDs.DictionaryValue,	nameToID.Value);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadDictionaryData()
		{
			string key;
			TNodeID value;

			while (true)
			{
				TNodeID nextNodeID		= 0;
				TNodeSize nextNodeSize	= 0;
				NodeType nextNodeType	= NodeType.Parent;

				int currentPos = GetCurrentStreamPos();
				LevelPosition levelPosition = m_LevelPositionStack.Peek();
				int levelEndPos = levelPosition.LevelEndPosition_IfLoading;

				if (currentPos >= levelEndPos)
				{
					Debug.Assert(currentPos == levelEndPos);
					GotoStreamPos(levelPosition.LevelStartPosition, true);
					return;
				}

				ReadNextNode(out nextNodeID, out nextNodeSize, out nextNodeType);

				Debug.Assert(nextNodeID == (TNodeID) SpecialNodeIDs.DictionaryKey);

				ReadLeafData(out key, nextNodeSize);

				ReadNextNode(out nextNodeID, out nextNodeSize, out nextNodeType);

				Debug.Assert(nextNodeID == (TNodeID) SpecialNodeIDs.DictionaryValue);

				ReadLeafData(out value, nextNodeSize);

				m_NodeNameToIds.Add(key, value);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void InitOnNewRun()
		{
			m_LevelPositionStack	= new Stack<LevelPosition>();
			m_NodeNameToIds			= new Dictionary<string, TNodeID>();

			m_NodeNameToIds.Add("FileVersion", (TNodeID) SpecialNodeIDs.FileVersion);
		}

		///////////////////////////////////////////////////////////////////////////

		void PushCurrentPositionToStack(int endPosition_IfLoading)
		{
			LevelPosition levelPosition;
			levelPosition.CurrentPosition				= GetCurrentStreamPos();
			levelPosition.LevelStartPosition			= GetCurrentStreamPos();
			levelPosition.LevelEndPosition_IfLoading	= endPosition_IfLoading;

			m_LevelPositionStack.Push(levelPosition);
		}

		///////////////////////////////////////////////////////////////////////////

		public State GetState()
		{
			if (m_BinaryReader != null)
			{
				return State.Loading;
			}
			else if (m_BinaryWriter != null)
			{
				return State.Saving;
			}

			return State.None;
		}

		///////////////////////////////////////////////////////////////////////////

		void ReadNextNode(out TNodeID nodeID, out TNodeSize size, out NodeType nodeType)
		{
			AssertWithinCurrentLevel(false);

			nodeID		= m_BinaryReader.ReadInt16();
			size		= m_BinaryReader.ReadInt32();
			nodeType	= (NodeType) m_BinaryReader.ReadChar();
		}

		///////////////////////////////////////////////////////////////////////////

		TNodeID TagNameToNodeID(string tagName)
		{
			TNodeID id;
			bool foundName = m_NodeNameToIds.TryGetValue(tagName, out id);

			if (foundName)
			{
				return id;
			}

			if (GetState() == State.Saving)
			{
				TNodeID newID = (TNodeID) m_NodeNameToIds.Count;
				m_NodeNameToIds[tagName] = newID;

				return newID;
			}
			else
			{
				return (int) SpecialNodeIDs.Invalid;
			}
		}

		
	}

	///////////////////////////////////////////////////////////////////////////
	
}