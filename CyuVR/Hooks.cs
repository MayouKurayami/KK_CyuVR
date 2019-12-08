// Decompiled with JetBrains decompiler
// Type: Bero.CyuVR.Hooks
// Assembly: CyuVR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E19B191-388D-44D8-A057-D416187C1E85
// Assembly location: C:\Users\MK\Desktop\KK\CyuVR\Source Dll\CyuVR.dll

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
						if ((double) __instance.flags.gaugeFemale < 50.0)
						{
							__result = false;
							return false;
						}
						continue;
					case 1:
						if ((double) __instance.flags.gaugeFemale >= 50.0)
						{
							__result = false;
							return false;
						}
						continue;
					case 2:
						__result = false;
						return false;
					case 3:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							flag = true;
							continue;
						}
						continue;
					case 4:
						if (__instance.flags.voice.speedMotion)
						{
							__result = false;
							return false;
						}
						continue;
					case 5:
						if (__instance.flags.mode != HFlag.EMode.aibu && !__instance.flags.voice.speedMotion)
						{
							__result = false;
							return false;
						}
						continue;
					case 6:
						if (__instance.flags.voice.speedItem)
						{
							__result = false;
							return false;
						}
						continue;
					case 7:
						if (!__instance.flags.voice.speedItem)
						{
							__result = false;
							return false;
						}
						continue;
					case 8:
						flag = true;
						continue;
					case 9:
						__result = false;
						return false;
					case 12:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							if ((double) __instance.flags.speed < 0.00999999977648258)
							{
								__result = false;
								return false;
							}
							continue;
						}
						if ((double) __instance.flags.speedCalc < 0.00999999977648258)
						{
							__result = false;
							return false;
						}
						continue;
					case 13:
						if (__instance.flags.mode == HFlag.EMode.aibu)
						{
							if ((double) __instance.flags.speed >= 0.00999999977648258)
							{
								__result = false;
								return false;
							}
							continue;
						}
						if ((double) __instance.flags.speedCalc >= 0.00999999977648258)
						{
							__result = false;
							return false;
						}
						continue;
					case 14:
						if (__instance.flags.mode == HFlag.EMode.aibu)
							flag = true;
						if (!((IEnumerable<VRHandCtrl>) __instance.hands).Any<VRHandCtrl>((Func<VRHandCtrl, bool>) (h => h.IsAction())))
						{
							__result = false;
							return false;
						}
						continue;
					case 15:
						if (__instance.flags.mode == HFlag.EMode.aibu)
							flag = true;
						if (((IEnumerable<VRHandCtrl>) __instance.hands).Any<VRHandCtrl>((Func<VRHandCtrl, bool>) (h => h.IsAction())))
						{
							__result = false;
							return false;
						}
						continue;
					default:
						continue;
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
