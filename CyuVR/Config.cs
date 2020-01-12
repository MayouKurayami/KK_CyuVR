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
			kissDistance = float.Parse(BepInEx.Config.GetEntry("KissDistance", "0.20", "Cyu"));
			kissDistanceAibu = float.Parse(BepInEx.Config.GetEntry("KissDistanceAibu", "0.35", "Cyu"));
			eyesMovement = bool.Parse(BepInEx.Config.GetEntry("EyesMovement", "true", "Cyu"));
			tongueOverride = bool.Parse(BepInEx.Config.GetEntry("TongueOverride", "false", "Cyu"));
			mouthOffset = float.Parse(BepInEx.Config.GetEntry("MouthOffset", "0.12", "Cyu"));
			kissNeckAngle = float.Parse(BepInEx.Config.GetEntry("KissNeckAngle", "0.38", "Cyu"));
			kissMotionSpeed = float.Parse(BepInEx.Config.GetEntry("KissMotionSpeed", "0.1", "Cyu"));
			kissMotionSpeed = Mathf.Clamp(kissMotionSpeed, 0.1f, 1.5f);
		}
	}
}
