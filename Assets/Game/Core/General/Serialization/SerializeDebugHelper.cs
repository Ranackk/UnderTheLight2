
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace External.JEichner.ooo
{

	///////////////////////////////////////////////////////////////////////////

	public class SerializeDebugHelper : MonoBehaviour 
	{
		///////////////////////////////////////////////////////////////////////////

		public enum LogToFileMode
		{
			DoNotLog,
			AlwaysLog,
			LogIfUnsuccessfull,
		}

		///////////////////////////////////////////////////////////////////////////

		public static void CompareDebugSerializations(string stringBefore, string stringAfter, byte[] savedBytes, LogToFileMode logMode = LogToFileMode.LogIfUnsuccessfull)
		{
			if (stringBefore == null)
			{
				Debug.Assert(false, "Invalid String input");
				stringBefore = "";
			}
			if (stringAfter == null)
			{
				Debug.Assert(false, "Invalid String input");
				stringAfter = "";
			}

			int lengthBefore	= stringBefore.Length;
			int lengthAfter		= stringAfter.Length;

			int foundErrorAt = -1;
			if (lengthBefore != lengthAfter)
			{
				foundErrorAt = System.Math.Max(lengthBefore, lengthAfter) - 1;
			}

			for (int i = 0; i < System.Math.Min(lengthBefore, lengthAfter); ++i)
			{
				if (stringBefore[i] != stringAfter[i])
				{
					foundErrorAt = i;
					break;
				}
			}

			string deviationString = "";

			if (foundErrorAt != -1)
			{
				string equalUntil		= (lengthBefore > lengthAfter ? stringBefore : stringAfter).Substring(0, foundErrorAt);
				string deviationSince	= (lengthBefore > lengthAfter ? stringBefore : stringAfter).Substring(foundErrorAt);

				if (deviationSince.Length > 100)
				{
					deviationSince = deviationSince.Substring(0, 100);
				}
				if (equalUntil.Length > 100)
				{
					equalUntil = equalUntil.Substring(equalUntil.Length - 100, 100);
				}

				deviationString = "Error at DebugStr char " + foundErrorAt + ". First Deviation at: \n\n" + equalUntil + " >>>>>>>>> " + deviationSince;

				string debugString = "";

				debugString += "Serialization DebugString Mismatch:\n";
				debugString += deviationString;
				debugString += "\n\nBefore Serialization:\n\n" + stringBefore;
				debugString += "\n\nAfter  Serialization:\n\n" + stringAfter;	

				Debug.LogError(debugString);
			}
			else
			{
				Debug.Log("Serialization Verified:\n\n" + stringBefore + "\n\n" + stringAfter);
			}

			if ((logMode == LogToFileMode.AlwaysLog) || (logMode == LogToFileMode.LogIfUnsuccessfull && foundErrorAt != -1))
			{
				string debugStr = Serializer.ToDebugString(savedBytes);
				
				debugStr += "\n\n------------------------------------\n\n";
				debugStr += deviationString;
				debugStr += "\n\n------------------------------------\n\n";
				debugStr += stringBefore;
				debugStr += "\n\n------------------------------------\n\n";
				debugStr += stringAfter;

				WriteToParserDebugFile(debugStr);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public static void VerifyCorrectSerialization_Serializable<T>(T serializable, LogToFileMode logMode = LogToFileMode.LogIfUnsuccessfull) where T : Serializer.ISerializable, new()
		{
			string debugStringBefore = serializable.ToString();
			
			///////////////////////////////////////////////////////////////////////////
			// Save
			///////////////////////////////////////////////////////////////////////////

			Serializer serializer = new Serializer();
			serializer.BeginSaving();
			serializer.Serialize("Test", ref serializable, false);

			byte[] savedBytes = serializer.EndSaving();

			try
			{
				///////////////////////////////////////////////////////////////////////////
				// Load
				///////////////////////////////////////////////////////////////////////////

				T loadedSerializable = default(T);
				serializer.BeginLoading(savedBytes);
				serializer.Serialize("Test", ref loadedSerializable, false);
				serializer.EndLoading();

				string debugStringAfter = loadedSerializable.ToString();	

				CompareDebugSerializations(debugStringBefore, debugStringAfter, savedBytes, logMode);
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception happened: " + e.ToString());

				if (logMode == LogToFileMode.AlwaysLog || logMode == LogToFileMode.LogIfUnsuccessfull)
				{
					string debugStr = Serializer.ToDebugString(savedBytes);
					WriteToParserDebugFile(debugStr);
				}
			}

			///////////////////////////////////////////////////////////////////////////
		}

		///////////////////////////////////////////////////////////////////////////

		public static void VerifyCorrectSerialization<T>(T originalBehaviour, LogToFileMode logMode = LogToFileMode.LogIfUnsuccessfull) where T : IMonoBehaviourWithAdditionalSerialize
		{
			try
			{
				GameObject gameObjectCopy = GameObject.Instantiate(originalBehaviour.gameObject);
				gameObjectCopy.name = "REMOVE_ME_UnitTestObject";
				T behaviourCopy = gameObjectCopy.GetComponent<T>();
			
				string debugStr1 = originalBehaviour.ToString();
				string debugStr2 = behaviourCopy.ToString();

				CompareDebugSerializations(debugStr1, debugStr2, behaviourCopy.GetSerializedBlob(), logMode);

				if (Application.isEditor)
				{
					GameObject.DestroyImmediate(gameObjectCopy);
				}
				else
				{
					GameObject.Destroy(gameObjectCopy);
				}

				return;
			}
			catch (System.Exception e)
			{
				Debug.LogError("Exception happened during ComponentCopy or ComponentCopyVerification: " + e.ToString());
			}

			if (logMode == LogToFileMode.AlwaysLog || logMode == LogToFileMode.LogIfUnsuccessfull)
			{
				originalBehaviour.TrySaveAndGetReadableBlob(true);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		static public void WriteToParserDebugFile(string debugString)
		{
			string parserFilePath = @"C:\Users\Wurstmann\Desktop\ParserDebug.txt";

			Debug.Log("Writing Parser Debug to " + parserFilePath);
			System.IO.File.WriteAllText(parserFilePath, debugString);
		}

		///////////////////////////////////////////////////////////////////////////
		// Serialization Debug String Buildup
		///////////////////////////////////////////////////////////////////////////

		static public string AddToSerializationDebugString(Serializer.ISerializable serializable)
		{
			if (serializable == null)
			{
				return "[null]" + ", ";
			}
			else
			{
				return serializable.ToString()  + ", ";
			}
		}

		///////////////////////////////////////////////////////////////////////////

		static public string AddToSerializationDebugString_Builtin<T>(T serializable) where T : struct
		{
			return serializable.ToString()  + ", ";
		}

		///////////////////////////////////////////////////////////////////////////

		static public string AddToSerializationDebugString_String(string serializable)
		{
			return ((serializable == null || serializable == "") ? "[NullOrEmpty}" : serializable) + ", ";
		}

		///////////////////////////////////////////////////////////////////////////

		static public string AddToSerializationDebugString_BuiltinList<T>(List<T> list)
		{
			if (list == null || list.Count == 0)
			{
				return "[NullOrEmpty], ";
			}

			string outString = "List{";

			for (int i = 0; i < list.Count; ++i)
			{
				outString += (list[i] != null ? list[i].ToString() : "[null]");

				if (i != list.Count - 1)
				{
					outString += ", ";
				}
			}		

			return outString + "}, ";
		}

		///////////////////////////////////////////////////////////////////////////

		static public string AddToSerializationDebugString_List<T>(List<T> list) where T : Serializer.ISerializable
		{
			if (list == null || list.Count == 0)
			{
				return "[NullOrEmpty], ";
			}

			string outString = "List{";

			for (int i = 0; i < list.Count; ++i)
			{
				outString += (list[i] != null ? list[i].ToString() : "[null]") + " ";
			}		

			return outString + "}, ";
		}
	}

	///////////////////////////////////////////////////////////////////////////
	
}