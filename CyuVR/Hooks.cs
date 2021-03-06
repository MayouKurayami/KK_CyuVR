﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	public static class Hooks
	{
		/// <summary>
		/// Strings that are part of the names of animations played during orgasm
		/// </summary>
		private readonly static string[] OrgAnims = new string[] { "_Start", "IN_Loop", "OUT_Loop", "Orgasm_Loop" };
		/// <summary>
		/// Strings that are part of the names of animations played when sex is resumed after orgasm
		/// </summary>
		private readonly static string[] ResumeAnims = new string[] { "WLoop", "Insert", "Idle" };

		//This should hook to a method that loads as late as possible in the loading phase
		//Hooking method "MapSameObjectDisable" because: "Something that happens at the end of H scene loading, good enough place to hook" - DeathWeasel1337/Anon11
		//https://github.com/DeathWeasel1337/KK_Plugins/blob/master/KK_EyeShaking/KK.EyeShaking.Hooks.cs#L20
		[HarmonyPostfix]
		[HarmonyPatch(typeof(VRHScene), "MapSameObjectDisable")]
		public static void VRHSceneLoadPostfix(VRHScene __instance)
		{
			CyuLoaderVR.InitCyu(__instance.flags);
		}

		[HarmonyPatch(typeof(FaceBlendShape), "LateUpdate", null, null)]
		[HarmonyPostfix]
		public static void FaceBlendShapeLateUpdateHook()
		{
			Cyu cyu = CyuLoaderVR.mainCyu;
			if (cyu == null)
				return;
			cyu.LateUpdateHook();
		}

		[HarmonyPatch(typeof(VRHandCtrl), "IsKissAction", null, null)]
		[HarmonyPrefix]
		public static bool IsKissActionHook(ref bool __result, VRHandCtrl __instance)
		{
			try
			{
				//IsKissAction is only called in caress mode, which only exists when there is a single girl in H
				//Therefore we only need the first Cyu object
				__result = CyuLoaderVR.mainCyu.IsKiss;
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
			//This patch is only designed for caress mode, which only exists when there is a single girl in H
			//Therefore we only need the first Cyu object
			Cyu cyu = CyuLoaderVR.mainCyu;
			if (!CyuLoaderVR.GropeOverride.Value || !cyu || cyu.flags.mode != HFlag.EMode.aibu  || !cyu.IsKiss)
				return;

			if (_rateSpeedUp > 1f)
			{
				_rateSpeedUp = 0.7f;
				return;
			}
			else
				cyu.dragSpeed = Math.Max(cyu.dragSpeed, Mathf.Clamp(0.1f + (214.444f * _rateSpeedUp) - (7222.222f * _rateSpeedUp * _rateSpeedUp), 0.1f, 1.5f));
		}

		//This patch happens when a body part is touched.
		//If groping is initiated during kissing, the game does not continue the groping animation after disengaged from kissing.
		//This patch updates the touched body parts array and sets the backIdle field (used to return animation to the previous state) to the right value based on flags.click 
		//before flags.click gets overridden to ClickKind.none later in the frame
		[HarmonyPatch(typeof(VRHandCtrl), "JudgeProc")]
		[HarmonyPrefix]
		public static void JudgeProcPre()
		{
			//This patch is only designed for caress mode, which only exists when there is a single girl in H
			//Therefore we only need the first Cyu object
			Cyu cyu = CyuLoaderVR.mainCyu;
			if (!cyu || cyu.flags.mode != HFlag.EMode.aibu)
				return;

			//If the last position in the array is filled, move it back one position and fill the last position with the current action
			//This ensures the last touched body part is always tracked in the last position of the array, 
			//while the first position tracks any other body part that is being touched concurrently 
			if (cyu.touchOrder.Last() != null)
				cyu.touchOrder[cyu.touchOrder.Length - 2] = cyu.touchOrder[cyu.touchOrder.Length - 1];

			cyu.touchOrder[cyu.touchOrder.Length - 1] = cyu.flags.click;

			//If currently kissing, update back-to-idle animation to the current touched body part
			if (cyu.IsKiss)
			{
				switch (cyu.touchOrder.Last())
				{
					case HFlag.ClickKind.muneL:
					case HFlag.ClickKind.muneR:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(1);
						break;

					case HFlag.ClickKind.kokan:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(2);
						break;

					case HFlag.ClickKind.siriL:
					case HFlag.ClickKind.siriR:
					case HFlag.ClickKind.anal:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(3);
						break;
				}
			}
		}

		//This patch happens when a body part is released.
		//If kissing is initiated while groping and groping stops in the middle of kissing, this makes sure animation will return to normal idle (non-groping)
		// after kissing is disengaged, by setting the backIdle field (used to return animation to the previous state) to the right value
		[HarmonyPatch(typeof(VRHandCtrl), "FinishAction")]
		[HarmonyPostfix]
		public static void FinishActionPost()
		{
			//This patch is only designed for caress mode, which only exists when there is a single girl in H
			//Therefore we only need the first Cyu object
			Cyu cyu = CyuLoaderVR.mainCyu;
			if (!cyu || cyu.flags.mode != HFlag.EMode.aibu)
				return;

			//Find any entry in the touched body parts array that matches with the body part that was just released, and clear it
			int index = Array.LastIndexOf(cyu.touchOrder, cyu.flags.click - 6);
			if (index > -1)
				Array.Clear(cyu.touchOrder, index, 1);

			//While kissing and not in disengagement transition, update the back-to-idle animation to the last holding body part,
			//or set it to idle if no body part is currently being held.
			//It's too late to change the back-to-idle animation during disengagement transition, as the crossfade to the next animation has already begun
			//Changing the backIdle variable in this case would result in it being the wrong value and causing undesired behaviors
			if (cyu.IsKiss && cyu.kissPhase != Cyu.Phase.Disengaging)
			{
				switch (cyu.touchOrder.LastOrDefault(x => x != null))
				{
					case HFlag.ClickKind.muneL:
					case HFlag.ClickKind.muneR:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(1);
						break;

					case HFlag.ClickKind.kokan:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(2);
						break;

					case HFlag.ClickKind.siriL:
					case HFlag.ClickKind.siriR:
					case HFlag.ClickKind.anal:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(3);
						break;

					default:
						Traverse.Create(cyu.aibu).Field("backIdle").SetValue(0);
						break;
				}	
			}
		}

		//Make sure kissing engagement transition always last one second, to avoid girl from jumping forward too fast
		[HarmonyPatch(typeof(Animator), "CrossFadeInFixedTime",  new Type[] {typeof(string), typeof(float), typeof(int)} )]
		[HarmonyPrefix]
		public static void CrossFadeInFixedTimePre(string stateName, ref float transitionDuration)
		{
			if (stateName == "K_Touch")
				transitionDuration = 1f;
		}

		[HarmonyPatch(typeof(HActionBase), "SetPlay")]
		[HarmonyPrefix]
		public static void SetPlayPre(string _nextAnimation)
		{
			//Set flag to prevent entering kiss when entering orgasm animation, 
			//or when entering precum loop with valid finish flag, which indicates the intent to enter orgasm instead of staying in precum loop
			if (OrgAnims.Any(str => _nextAnimation.Contains(str)) || (_nextAnimation.Contains("OLoop") && CyuLoaderVR.hFlag?.finish != HFlag.FinishKind.none))
			{
				Cyu.isInOrgasm = true;
			}		
			else
			{
				Cyu.isInOrgasm = false;

				//Update the stored finish flag in Cyu to none when motion restarts right after orgasm
				//This is when the vanilla game would reset the finish flag to none, so this ensures that Cyu doesn't return an incorrect finish flag after kissing is done
				if (CyuLoaderVR.hFlag?.nowAnimStateName.Contains("_A") ?? false && ResumeAnims.Any(str => _nextAnimation.Contains(str)))
					Cyu.origFinishFlag = HFlag.FinishKind.none;
			}
		}
	}
}
