using UnityEngine;

namespace Bero.CyuVR
{
	public class Config : MonoBehaviour
	{
		public static float kissDistance;
		public static float kissDistanceAibu;
		public static bool eyesMovement;
		public static bool tongueOverride;
		public static float mouthOffset;
		public static float kissNeckAngle;
		public static float kissMotionSpeed;

		private void Awake()
		{
			Config.kissDistance = float.Parse(BepInEx.Config.GetEntry("KissDistance", "0.20", "Cyu"));
			Config.kissDistanceAibu = float.Parse(BepInEx.Config.GetEntry("KissDistanceAibu", "0.35", "Cyu"));
			Config.eyesMovement = bool.Parse(BepInEx.Config.GetEntry("EyesMovement", "true", "Cyu"));
			Config.tongueOverride = bool.Parse(BepInEx.Config.GetEntry("TongueOverride", "false", "Cyu"));
			Config.mouthOffset = float.Parse(BepInEx.Config.GetEntry("MouthOffset", "0.12", "Cyu"));
			Config.kissNeckAngle = float.Parse(BepInEx.Config.GetEntry("KissNeckAngle", "0.38", "Cyu"));
			Config.kissMotionSpeed = float.Parse(BepInEx.Config.GetEntry("KissMotionSpeed", "0.1", "Cyu"));
			Config.kissMotionSpeed = Mathf.Clamp(Config.kissMotionSpeed, 0.1f, 1.5f);
		}
	}
}
