using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using Harmony;
using BepInEx4;

namespace Bero.CyuVR
{
	[BepInPlugin("bero.cyu.cyuvr", PluginName, Version)]
	[BepInProcess("Koikatu")]
	[BepInProcess("KoikatuVR")]
	[BepInProcess("Koikatsu Party")]
	[BepInProcess("Koikatsu Party VR")]
	public class CyuLoaderVR : BaseUnityPlugin
	{
		public const string PluginName = "CyuVR";
		public const string AssembName = "KK_CyuVR";
		public const string Version = "1.1.2";
		public static List<ChaControl> lstFemale = new List<ChaControl>();
		private static string animationName = "";
		internal static HFlag hFlag;
		private bool isVR;
		internal static Cyu mainCyu;


		[DisplayName("Eyes Animation Openness")]
		[Description("Maximum openness of eyes and eyelids during kissing. Set to 0 to keep eyes closed during kiss")]
		[AcceptableValueRange(0f, 100f, true)]
		public static ConfigWrapper<float> EyesMovement { get; private set; }

		[DisplayName("Force Allow Kiss")]
		[Description("Allow kissing even if the girl is set to refuse kiss")]
		public static ConfigWrapper<bool> ForceKiss { get; private set; }

		[DisplayName("Girl Neck Elevation")]
		[Description("How much the girl raises her head during kiss")]
		public static ConfigWrapper<float> KissNeckAngle { get; private set; }

		[DisplayName("Increase Kiss Intensity by Groping")]
		[Description("Override kissing motion speed in caress mode by groping")]
		public static ConfigWrapper<bool> GropeOverride { get; private set; }

		[DisplayName("Kiss Activation Distance")]
		[Description("When not in caress mode, kissing will start when the headset is within this range to the girl's head")]
		public static ConfigWrapper<float> KissDistance { get; private set; }

		[DisplayName("Kiss Activation Distance in Caress Mode")]
		[Description("In caress mode, kissing will start when the headset is within this range to the girl's head")]
		public static ConfigWrapper<float> KissDistanceAibu { get; private set; }

		[DisplayName("Kiss Intensity in Caress Mode")]
		[Description("Speed of kissing motion in caress mode")]
		[AcceptableValueRange(0.1f, 1.5f, true)]
		public static ConfigWrapper<float> KissMotionSpeed { get; private set; }

		[DisplayName("Mode of Tongue and Mouth Movement")]
		[Description("Set when to enable/disable tongue and mouth movement (french kiss) in kissing")]
		public static ConfigWrapper<Cyu.FrenchMode> MouthMovement { get; private set; }

		[DisplayName("Player Mouth Offset")]
		[Description("Negative vertical offset to player's mouth (increase this value to make your own mouth lower)")]
		public static ConfigWrapper<float> MouthOffset { get; private set; }	

		[DisplayName("Sustain Orgasm After Kiss")]
		[Description("If enabled, the girl will remain in orgasm after kissing. Groping the girl will end her orgasm.")]
		public static ConfigWrapper<bool> OrgasmAfterKiss { get; private set; }
	
		///
		//////////////////// Keyboard Shortcuts /////////////////////////// 
		///

		[DisplayName("Enable/Disable CyuVR")]
		[Description("Press this key to temporarily enable/disable the plugin. \nPlugin will always re-enable after changing position.")]
		public static SavedKeyboardShortcut PluginToggleKey { get; private set; }



		private void HandleLog(string condition, string stackTrace, LogType type)
		{
			Console.WriteLine(condition + "\n" + stackTrace);
		}

		private void Awake()
		{
			Application.logMessageReceived += new Application.LogCallback(HandleLog);

			EyesMovement = new ConfigWrapper<float>(nameof(EyesMovement), this, 50f);
			ForceKiss = new ConfigWrapper<bool>(nameof(ForceKiss), this, false);
			KissNeckAngle = new ConfigWrapper<float>(nameof(KissNeckAngle), this, 0.2f);
			GropeOverride = new ConfigWrapper<bool>(nameof(GropeOverride), this, true);
			KissDistance = new ConfigWrapper<float>(nameof(KissDistance), this, 0.18f);
			KissDistanceAibu = new ConfigWrapper<float>(nameof(KissDistanceAibu), this, 0.28f);
			KissMotionSpeed = new ConfigWrapper<float>(nameof(KissMotionSpeed), this, 0.1f);
			MouthMovement = new ConfigWrapper<Cyu.FrenchMode>(nameof(MouthMovement), this, Cyu.FrenchMode.Auto);
			MouthOffset = new ConfigWrapper<float>(nameof(MouthOffset), this, 0.12f);
			OrgasmAfterKiss = new ConfigWrapper<bool>(nameof(OrgasmAfterKiss), this, false);
			
			PluginToggleKey = new SavedKeyboardShortcut(nameof(PluginToggleKey), this, new KeyboardShortcut(KeyCode.None));

			if (!(isVR = Type.GetType("VRHScene, Assembly-CSharp") != null))
				return;
			try
			{
				HarmonyInstance.Create("bero.cyuvr").PatchAll(typeof(Hooks));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private void Update()
		{
			if (!isVR || hFlag == null)
				return;

			if (animationName != hFlag.nowAnimationInfo.nameAnimation)
				InitCyu(hFlag);

			if (Input.GetKeyDown(PluginToggleKey.Value.MainKey) && PluginToggleKey.Value.Modifiers.All(x => Input.GetKey(x)))
			{
				foreach (ChaControl female in lstFemale)
					female.GetComponent<Cyu>().enabled = !female.GetComponent<Cyu>().enabled;
			}
		}

		internal static void InitCyu(HFlag flags)
		{
			hFlag = flags;
			
			lstFemale.Clear();
			FindObjectsOfType<ChaControl>().ToList<ChaControl>().Where<ChaControl>(x => x.chaFile.parameter.sex == 1).ToList<ChaControl>().ForEach(female =>
			{
				Cyu oldCyu = female.GetComponent<Cyu>();
				if (oldCyu != null)
					Destroy(oldCyu);

				Cyu newCyu = female.gameObject.AddComponent<Cyu>();
				newCyu.flags = flags;
				lstFemale.Add(female);
				if (lstFemale.Count < 2)
					mainCyu = newCyu;
			});
			animationName = flags.nowAnimationInfo.nameAnimation;
		}
	}
}
