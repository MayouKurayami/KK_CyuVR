// Decompiled with JetBrains decompiler
// Type: Bero.CyuVR.Config
// Assembly: CyuVR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E19B191-388D-44D8-A057-D416187C1E85
// Assembly location: C:\Users\MK\Desktop\KK\CyuVR\Source Dll\CyuVR.dll

using UnityEngine;

namespace Bero.CyuVR
{
	public class Config : MonoBehaviour
	{
		public static float kissDistance;
		public static float kissDistanceAibu;
		public static bool faceOverride;
		public static float mouthOffset;
		internal static float kissNeckAngle;

		private void Awake()
		{
			Config.kissDistance = float.Parse(BepInEx.Config.GetEntry("KissDistance", "0.20", "Cyu"));
			Config.kissDistanceAibu = float.Parse(BepInEx.Config.GetEntry("KissDistanceAibu", "0.35", "Cyu"));
			Config.faceOverride = bool.Parse(BepInEx.Config.GetEntry("FaceOverride", "true", "Cyu"));
			Config.mouthOffset = float.Parse(BepInEx.Config.GetEntry("MouthOffset", "0.12", "Cyu"));
			Config.kissNeckAngle = float.Parse(BepInEx.Config.GetEntry("KissNeckAngle", "0.38", "Cyu"));
		}
	}
}
