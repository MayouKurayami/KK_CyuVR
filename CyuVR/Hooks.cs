using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	public static class Hooks
	{
		private static Cyu GetCyu()
		{
			ChaControl component = null;
			if (CyuLoaderVR.lstFemale.Count > 0)
				component = CyuLoaderVR.lstFemale[0];
			if (component == null)
				return null;
			return component.GetOrAddComponent<Cyu>();
		}

		[HarmonyPatch(typeof(FaceBlendShape), "LateUpdate", null, null)]
		[HarmonyPostfix]
		public static void FaceBlendShapeLateUpdateHook()
		{
			Cyu cyu = GetCyu();
			if (!(cyu != null))
				return;
			cyu.LateUpdateHook();
		}

		[HarmonyPatch(typeof(VRHandCtrl), "IsKissAction", null, null)]
		[HarmonyPrefix]
		public static bool IsKissActionHook(ref bool __result, VRHandCtrl __instance)
		{
			try
			{
				__result = CyuLoaderVR.lstFemale[0].GetComponent<Cyu>().IsKiss;
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return true;
		}

		[HarmonyPatch(typeof(HVoiceCtrl), "IsBreathPtnConditions", new System.Type[] { typeof(List<int>), typeof(int) }, null)]
		[HarmonyPrefix]
		public static bool IsBreathPtnConditionsHook(
			ref bool __result,
			HVoiceCtrl __instance,
			List<int> _lstConditions,
			int _main)
		{
			if (!CyuLoaderVR.lstFemale[_main].GetComponent<Cyu>().IsKiss)
				return true;
			bool flag = false;
			foreach (int lstCondition in _lstConditions)
			{
				if (lstCondition >= 90)
				{
					__result = false;
					return false;
				}
				switch (lstCondition)
				{
					case 0:
						if (__instance.flags.gaugeFemale < 50f)
						{
							__result = false;
							return false;
						}
						break;
					case 1:
						if (__instance.flags.gaugeFemale >= 50f)
						{
							__result = false;
							return false;
						}
						break;
					case 2:
						__result = false;
						return false;
					case 3:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							flag = true;
						}
						break;
					case 4:
						if (__instance.flags.voice.speedMotion)
						{
							__result = false;
							return false;
						}
						break;
					case 5:
						if (__instance.flags.mode != 0 && !__instance.flags.voice.speedMotion)
						{
							__result = false;
							return false;
						}
						break;
					case 6:
						if (__instance.flags.voice.speedItem)
						{
							__result = false;
							return false;
						}
						break;
					case 7:
						if (!__instance.flags.voice.speedItem)
						{
							__result = false;
							return false;
						}
						break;
					case 8:
						flag = true;
						break;
					case 9:
						__result = false;
						return false;
					case 12:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							if (__instance.flags.speed < 0.01f)
							{
								__result = false;
								return false;
							}
						}
						else if (__instance.flags.speedCalc < 0.01f)
						{
							__result = false;
							return false;
						}
						break;
					case 13:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							if (__instance.flags.speed >= 0.01f)
							{
								__result = false;
								return false;
							}
						}
						else if (__instance.flags.speedCalc >= 0.01f)
						{
							__result = false;
							return false;
						}
						break;
					case 14:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							flag = true;
						}
						if (!__instance.hands.Any((VRHandCtrl h) => h.IsAction()))
						{
							__result = false;
							return false;
						}
						break;
					case 15:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							flag = true;
						}
						if (__instance.hands.Any((VRHandCtrl h) => h.IsAction()))
						{
							__result = false;
							return false;
						}
						break;
				}
			}
			if (flag)
			{
				__result = true;
				return false;
			}
			__result = false;
			return false;
		}

		//When beginning to touch in caress mode, the game passes a really high rate value (_rateSpeedUp) to item speed and thus making it at maximum in the beginning.
		//This behavior interferes with the kissing animation speed since it's dependent on the item speed, causing kissing animation speed to also skyrocket when started touching.
		//The below patch normalizes abnormally high speed values and converts other values into ones in a polynomial function curve between 0.1 and 1.5, 
		//which is the range of the kissing animation speed, thus making them suitable to be passed onto Cyu to calculate kissing animation speed
		[HarmonyPatch(typeof(HFlag), "SpeedUpClickItemAibu")]
		[HarmonyPrefix]
		public static void SpeedUpClickItemAibuPre(ref float _rateSpeedUp)
		{
			if (_rateSpeedUp > 1f)
			{
				_rateSpeedUp = 0.7f;
				return;
			}
			else
				Cyu.dragSpeed = Mathf.Clamp(0.1f + (214.444f * _rateSpeedUp) - (7222.222f * Mathf.Pow(_rateSpeedUp,2) ), 0.1f, 1.5f);
		}
	}
}
