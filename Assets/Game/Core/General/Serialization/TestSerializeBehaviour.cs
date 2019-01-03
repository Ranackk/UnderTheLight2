using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	public class TestSerializeBehaviour : IMonoBehaviourWithAdditionalSerialize
	{
		UT_HugeCompoundClass m_CompoundClass = null;

		///////////////////////////////////////////////////////////////////////////

		public override void AdditionalSerialize(Serializer serializer)
		{
			bool foundTag = serializer.Serialize("CompoundClass", ref m_CompoundClass, false);

			if (!foundTag)
			{
				Debug.LogError("Did not find Tag [CompoundClass]");
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void CreateNewData()
		{
			UT_HugeCompoundClass.m_StructureSeed	= UnityEngine.Random.Range(0, 100000);
			UT_HugeCompoundClass.m_ValueSeed		= UnityEngine.Random.Range(0, 100000);
			m_CompoundClass = new UT_HugeCompoundClass();

			Debug.Log("Created TestSerializeBehaviour-Data (" + UT_HugeCompoundClass.m_StructureSeed + ", " + UT_HugeCompoundClass.m_ValueSeed + ")");
			UT_HugeCompoundClass.m_ValueSeed		= UnityEngine.Random.Range(0, 100000);
		}

		///////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			string outDebugString = "";

			outDebugString += SerializeDebugHelper.AddToSerializationDebugString(m_CompoundClass);

			return outDebugString;
		}
	}

	///////////////////////////////////////////////////////////////////////////

	public class UT_SimpleSerializable : Serializer.ISerializable
	{
		private int m_MyInt = 1;

		public UT_SimpleSerializable() { }
		public UT_SimpleSerializable(int value) { m_MyInt = value; }

		public void Serialize(Serializer serializer)
		{
			serializer.Serialize("m_MyInt", ref m_MyInt, 2);
		}

		public override string ToString()			{	return "SimpleSerializable(" + m_MyInt.ToString() + ")"; }
	}

	///////////////////////////////////////////////////////////////////////////

	public class UT_PolyBaseFactory : Serializer.ISerializablePolymorphicFactory
	{
		static UT_PolyBaseFactory s_Instance = null;

		public static UT_PolyBaseFactory GetInstance()
		{
			if (s_Instance == null)
			{
				s_Instance = new UT_PolyBaseFactory();
			}

			return s_Instance;
		}

		public Serializer.ISerializablePolymorphic CreateSerializable(int typeEnum)
		{
			if (typeEnum == (int) UT_PolyType.UT_PolyTypeSub1)
			{
				return new UT_PolySub1();
			}
			else if (typeEnum == (int) UT_PolyType.UT_PolyTypeSub2)
			{
				return new UT_PolySub2();
			}
				
			Debug.LogError("Unknown UT_PolyType: " + (int)typeEnum);
			return null;
		}

		public static UT_IPolyBase GetRandomPolyBase(int typeSeed, ref int valueSeed)
		{
			int desiredType = (typeSeed % 3);

			if (desiredType == 0)
			{
				return null;
			}
			else if (desiredType == 1)
			{
				return new UT_PolySub1(valueSeed++);
			}
			else
			{
				return new UT_PolySub2(valueSeed++, valueSeed++);
			}
		}
	}

	///////////////////////////////////////////////////////////////////////////

	public enum UT_PolyType
	{
		UT_PolyTypeSub1,
		UT_PolyTypeSub2
	}

	public abstract class UT_IPolyBase : Serializer.ISerializablePolymorphic
	{
		public abstract UT_PolyType GetPolyType();
		public int GetTypeEnum() { return (int) GetPolyType(); }
        public abstract void Serialize(Serializer serializer);
    }

	///////////////////////////////////////////////////////////////////////////

	public class UT_PolySub1 : UT_IPolyBase
	{
		private int m_MyInt = 3;

		public UT_PolySub1() { }
		public UT_PolySub1(int value) {m_MyInt = value; }

		public override UT_PolyType GetPolyType() { return UT_PolyType.UT_PolyTypeSub1; }

		public override void Serialize(Serializer serializer)
		{
			serializer.Serialize("m_MyInt", ref m_MyInt, 4);
		}

		public override string ToString() { return "Sub1(" + m_MyInt.ToString() + ")"; }
	}

	///////////////////////////////////////////////////////////////////////////

	public class UT_PolySub2 : UT_IPolyBase
	{
		private int m_MyIntA = 5;
		private int m_MyIntB = 6;

		public UT_PolySub2() { }
		public UT_PolySub2(int valueA, int valueB) { m_MyIntA = valueA; m_MyIntB = valueB; }

		public override UT_PolyType GetPolyType() { return UT_PolyType.UT_PolyTypeSub2; }

		public override void Serialize(Serializer serializer)
		{
			serializer.Serialize("m_MyIntA", ref m_MyIntA, 7);
			serializer.Serialize("m_MyIntB", ref m_MyIntB, 8);
		}

		public override string ToString() { return "Sub2(" + m_MyIntA.ToString() + " " + m_MyIntB.ToString() + ")"; }
	}

	///////////////////////////////////////////////////////////////////////////

	enum UT_TestEnum
	{
		TestEnumValueA,
		TestEnumValueB,

		Count
	}

	public class UT_HugeCompoundClass : Serializer.ISerializable
	{
		public static int	m_StructureSeed	= -1;
		public static int   m_ValueSeed		= -1;

		private int[]			m_TestInts		= new int[2];
		private float[]			m_TestFloats	= new float[2];
		private double[]		m_TestDoubles	= new double[2];
		private string[]		m_TestStrings	= new string[3];
		private bool[]			m_TestBools		= new bool[2];
		private char[]			m_TestChars		= new char[2];
		private UT_TestEnum[]	m_TestEnums		= new UT_TestEnum[2];

		UT_SimpleSerializable[]	m_SimpleSerializables	= new UT_SimpleSerializable[3];
		UT_PolySub1[]			m_PolySub1s				= new UT_PolySub1[3];
		UT_PolySub2[]			m_PolySub2s				= new UT_PolySub2[3];
		UT_IPolyBase[]			m_PolyBases				= new UT_IPolyBase[3];

		List<int>					m_ListIntNull					= null;
		List<int>					m_ListInt						= new List<int>();
		List<UT_SimpleSerializable> m_ListSimpleSerializablesNull	= null;
		List<UT_SimpleSerializable> m_ListSimpleSerializables		= new List<UT_SimpleSerializable>();
		List<UT_IPolyBase>			m_ListPolyBase					= new List<UT_IPolyBase>();
		List<UT_PolySub2>			m_ListPolySub					= new List<UT_PolySub2>();

		int m_ManualStructureDepth			= 0;
		System.Random m_StructureRand		= new System.Random();

		public UT_HugeCompoundClass() 
		{ 
			int initSeed  = m_ValueSeed;
			int valueSeed = m_ValueSeed;

			// ints
			for (int i = 0; i < m_TestInts.Length; ++i)
			{
				m_TestInts[i] = valueSeed++;
			}

			// floats
			for (int i = 0; i < m_TestFloats.Length; ++i)
			{
				m_TestFloats[i] = (valueSeed++) * 0.31f;
			}

			// double
			for (int i = 0; i < m_TestDoubles.Length; ++i)
			{
				m_TestDoubles[i] = (valueSeed++) * 0.13;
			}

			// string
			for (int i = 0; i < m_TestStrings.Length; ++i)
			{
				int curSeed = valueSeed++;

				if (curSeed % 3 == 0)
				{
					m_TestStrings[i] = (curSeed) + "äüösChina";

					for (int j = 0; j < 10; ++j)
					{
						m_TestStrings[i] += GetRandomUnicodeChar(curSeed++);
					}
				}
				else if (curSeed % 3 == 1)
				{
					m_TestStrings[i] = "";
				}
				else
				{
					m_TestStrings[i] = null;
				}
			}

			// bool
			for (int i = 0; i < m_TestBools.Length; ++i)
			{
				m_TestBools[i] = (valueSeed++) % 2 == 0;
			}

			// char
			for (int i = 0; i < m_TestChars.Length; ++i)
			{
				m_TestChars[i] = GetRandomUnicodeChar(valueSeed++);
			}

			// enum
			for (int i = 0; i < m_TestEnums.Length; ++i)
			{
				m_TestEnums[i] = (UT_TestEnum) ((valueSeed++) % (int)UT_TestEnum.Count);
			}

			///////////////////////////////////////////////////////////////////////////

			// simple serializables
			for (int i = 0; i < m_SimpleSerializables.Length; ++i)
			{
				m_SimpleSerializables[i] = ((initSeed + i) % 2 == 0) ? null : new UT_SimpleSerializable(valueSeed++);
			}

			// Sub 1
			for (int i = 0; i < m_PolySub1s.Length; ++i)
			{
				m_PolySub1s[i] = ((initSeed + i) % 2 == 0) ? null : new UT_PolySub1(valueSeed++);
			}

			// Sub 2
			for (int i = 0; i < m_PolySub2s.Length; ++i)
			{
				m_PolySub2s[i] = ((initSeed + i) % 2 == 0) ? null : new UT_PolySub2(valueSeed++, valueSeed++);
			}

			// Mixed
			for (int i = 0; i < m_PolyBases.Length; ++i)
			{
				m_PolyBases[i] = UT_PolyBaseFactory.GetRandomPolyBase(initSeed + i, ref valueSeed);
			}

			///////////////////////////////////////////////////////////////////////////

			// Lists
			m_ListInt					= new List<int>						{ valueSeed++, valueSeed++, valueSeed++ };
			m_ListSimpleSerializables	= new List<UT_SimpleSerializable>	{ new UT_SimpleSerializable(valueSeed++), new UT_SimpleSerializable(valueSeed++)};
			m_ListPolyBase				= new List<UT_IPolyBase>			{ UT_PolyBaseFactory.GetRandomPolyBase(initSeed, ref valueSeed), UT_PolyBaseFactory.GetRandomPolyBase(initSeed + 1, ref valueSeed), UT_PolyBaseFactory.GetRandomPolyBase(initSeed + 1, ref valueSeed) };
			m_ListPolySub				= new List<UT_PolySub2>				{ new UT_PolySub2(valueSeed++, valueSeed++), new UT_PolySub2(valueSeed++, valueSeed++) };
		}

		///////////////////////////////////////////////////////////////////////////

		char GetRandomUnicodeChar(int seed)
		{
			int seedStart = seed;

			const int minValidUnicodeValue = 0x0020;
			const int maxValidUnicodeValue = 0x21FF;
			const int validUnicodeValueCount = maxValidUnicodeValue - minValidUnicodeValue + 1;

			int randomInt = (seedStart++) * 3111;
			randomInt = minValidUnicodeValue + (randomInt % validUnicodeValueCount);

			char randomChar = (char) randomInt;

			return randomChar;
		}

		///////////////////////////////////////////////////////////////////////////

		void AddRandomTreeStructure(Serializer serializer)
		{
			bool allowOpen	= m_ManualStructureDepth < 4;
			bool allowClose = m_ManualStructureDepth > 0;

			float randValue = (float) m_StructureRand.NextDouble();

			if (randValue > 0.333f)
			{
				if (allowOpen)
				{
					bool success = serializer.OpenParentNode("_RandomStructure");
					Debug.Assert(success);

					m_ManualStructureDepth++;
				}
			}
			else if (randValue > 0.666f)
			{
				if (allowClose)
				{
					serializer.CloseParentNode();

					m_ManualStructureDepth--;
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void FinalizeManualStructureDepths(Serializer serializer)
		{
			for (int i = 0; i < m_ManualStructureDepth; ++i)
			{
				serializer.CloseParentNode();
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void Serialize(Serializer serializer)
		{
			bool foundStructureSeed = serializer.Serialize("structureSeed", ref m_StructureSeed, -1);
			if (!foundStructureSeed)
			{
				Debug.LogError("Did not found [structureSeed]");
			}

			m_StructureRand = new System.Random(m_StructureSeed);
			m_ManualStructureDepth = 0;

			// ints
			AddRandomTreeStructure(serializer);
			bool foundAllTags = serializer.Serialize("testInt01", ref m_TestInts[0], -1);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testInt01", ref m_TestInts[1], -1);
			AddRandomTreeStructure(serializer);

			// floats
		 	AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testFoat01", ref m_TestFloats[0], -1);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testFoat01", ref m_TestFloats[1], -1);
			AddRandomTreeStructure(serializer);

			// double
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testDouble01", ref m_TestDoubles[0], -1.0);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testDouble01", ref m_TestDoubles[1], -1.0);
			AddRandomTreeStructure(serializer);

			// string
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testString01", ref m_TestStrings[0], "");
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testString01", ref m_TestStrings[1], "");
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testString01", ref m_TestStrings[2], "");
			AddRandomTreeStructure(serializer);

			// bool
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testBool01", ref m_TestBools[0], true);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testBool01", ref m_TestBools[1], false);
			AddRandomTreeStructure(serializer);

			// char
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testchar01", ref m_TestChars[0], 'a');
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testchar01", ref m_TestChars[1], 'b');
			AddRandomTreeStructure(serializer);
			
			// enum
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("enum1", ref m_TestEnums[0], UT_TestEnum.Count);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("enum2", ref m_TestEnums[1], UT_TestEnum.Count);
			AddRandomTreeStructure(serializer);

			///////////////////////////////////////////////////////////////////////////

			// simple serializables
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer0", ref m_SimpleSerializables[0],	false);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer1", ref m_SimpleSerializables[1],	false);
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer2", ref m_SimpleSerializables[2],	false);
			AddRandomTreeStructure(serializer);

			// Sub 1
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolySub1s[0],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolySub1s[1],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolySub1s[2],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);

			// Sub 2
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolySub2s[0],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolySub2s[1],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolySub2s[2],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);

			// Mixed
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolyBases[0],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolyBases[1],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);
			foundAllTags &= serializer.Serialize("testSer", ref m_PolyBases[2],	UT_PolyBaseFactory.GetInstance());
			AddRandomTreeStructure(serializer);

			///////////////////////////////////////////////////////////////////////////

			// Lists
			foundAllTags &= serializer.Serialize("laLista", ref m_ListIntNull);
			foundAllTags &= serializer.Serialize("laLista", ref m_ListInt);
			foundAllTags &= serializer.Serialize("laLista", ref m_ListSimpleSerializablesNull,	false);
			foundAllTags &= serializer.Serialize("laLista", ref m_ListSimpleSerializables,		false);
			foundAllTags &= serializer.Serialize("laLista", ref m_ListPolyBase,					UT_PolyBaseFactory.GetInstance());
			foundAllTags &= serializer.Serialize("laLista", ref m_ListPolySub,					UT_PolyBaseFactory.GetInstance());

			if (!foundAllTags)
			{
				Debug.LogError("Did not find all tags :(");
			}

			FinalizeManualStructureDepths(serializer);
		}

		///////////////////////////////////////////////////////////////////////////

		public override string ToString()
		{
			string debugString = "";

			// ints
			for (int i = 0; i < m_TestInts.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_Builtin(m_TestInts[i]);
			}

			// floats
			for (int i = 0; i < m_TestFloats.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_Builtin(m_TestFloats[i]);
			}

			// double
			for (int i = 0; i < m_TestDoubles.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_Builtin(m_TestDoubles[i]);
			}

			// string
			for (int i = 0; i < m_TestStrings.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_String(m_TestStrings[i]);
			}

			// bool
			for (int i = 0; i < m_TestBools.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_Builtin(m_TestBools[i]);
			}

			// char
			for (int i = 0; i < m_TestChars.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_Builtin(m_TestChars[i]);
			}

			// enum
			for (int i = 0; i < m_TestEnums.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString_Builtin(m_TestEnums[i]);
			}

			///////////////////////////////////////////////////////////////////////////

			// simple serializables
			for (int i = 0; i < m_SimpleSerializables.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString(m_SimpleSerializables[i]);
			}

			// Sub 1
			for (int i = 0; i < m_PolySub1s.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString(m_PolySub1s[i]);
			}

			// Sub 2
			for (int i = 0; i < m_PolySub2s.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString(m_PolySub2s[i]);
			}

			// Mixed
			for (int i = 0; i < m_PolyBases.Length; ++i)
			{
				debugString += SerializeDebugHelper.AddToSerializationDebugString(m_PolyBases[i]);
			}

			///////////////////////////////////////////////////////////////////////////

			// Lists
			debugString += SerializeDebugHelper.AddToSerializationDebugString_BuiltinList(m_ListIntNull);
			debugString += SerializeDebugHelper.AddToSerializationDebugString_BuiltinList(m_ListInt);
			debugString += SerializeDebugHelper.AddToSerializationDebugString_List(m_ListSimpleSerializablesNull);
			debugString += SerializeDebugHelper.AddToSerializationDebugString_List(m_ListSimpleSerializables);
			debugString += SerializeDebugHelper.AddToSerializationDebugString_List(m_ListPolyBase);
			debugString += SerializeDebugHelper.AddToSerializationDebugString_List(m_ListPolySub);

			return debugString;
		}
	}
	
	///////////////////////////////////////////////////////////////////////////

	///////////////////////////////////////////////////////////////////////////
	
}