using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	[CustomEditor(typeof(TestSerializeBehaviour))]
	public class TestSerializeBehaviourEditor : Editor 
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			TestSerializeBehaviour myTarget = (TestSerializeBehaviour)target;

			if (GUILayout.Button("TestSerialize"))
			{
				Undo.RecordObject(myTarget, "TestSerialize");
				myTarget.CreateNewData();
				SerializeDebugHelper.VerifyCorrectSerialization(myTarget, SerializeDebugHelper.LogToFileMode.AlwaysLog);
			}

			if (GUILayout.Button("TestSerialize (Bulk)"))
			{
				Undo.RecordObject(myTarget, "TestSerializeBulk");
				for (int i = 0; i < 10; ++i)
				{
					myTarget.CreateNewData();
					SerializeDebugHelper.VerifyCorrectSerialization(myTarget);
				}
			}

			if (GUILayout.Button("Dump File"))
			{
				myTarget.TrySaveAndGetReadableBlob(true);
			}
		}
	}

	///////////////////////////////////////////////////////////////////////////
	
}