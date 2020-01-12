using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bero.CyuVR
{
	public static class Hooks
	{
		private static Cyu GetCyu()
		{
			ChaControl component = (ChaControl) null;
			if (CyuLoaderVR.lstFemale.Count > 0)
				component = CyuLoaderVR.lstFemale[0];
			if ((UnityEngine.Object) component == (UnityEngine.Object) null)
				return (Cyu) null;
			return component.GetOrAddComponent<Cyu>();
		}

		[HarmonyPatch(typeof (FaceBlendShape), "LateUpdate", null, null)]
		[HarmonyPostfix]
		public static void FaceBlendShapeLateUpdateHook()
		{
			Cyu cyu = Hooks.GetCyu();
			if (!((UnityEngine.Object) cyu != (UnityEngine.Object) null))
				return;
			cyu.LateUpdateHook();
		}

		[HarmonyPatch(typeof (VRHandCtrl), "IsKissAction", null, null)]
		[HarmonyPrefix]
		public static bool IsKissActionHook(ref bool __result, VRHandCtrl __instance)
		{
			try
			{
				Console.WriteLine("iskissactionhook={0}", (object) __result);
				__result = CyuLoaderVR.lstFemale[0].GetComponent<Cyu>().IsKiss;
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			return true;
		}

		[HarmonyPatch(typeof (HVoiceCtrl), "IsBreathPtnConditions", new System.Type[] {typeof (List<int>), typeof (int)}, null)]
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
	}
}
