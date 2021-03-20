using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Harmony;

namespace Bero.CyuVR
{
	public class Cyu : MonoBehaviour
	{
		private const float ExitKissDistance = 0.16f;

		private float tangSpeed = 35f;
		private float curMouthValue = 100f;
		private float toMouthValue = 95f;
		private float toEyeValue = CyuLoaderVR.EyesMovement.Value;
		private float toKissValue = 75f;
		private List<Cyu.BlendValue> bvs = new List<Cyu.BlendValue>();
		private List<Cyu.BlendValue> bvsShow = new List<Cyu.BlendValue>();
		public float eyesOpenValue = 100f;
		private float mouthSpeed = 1f;
		private float npWeight = 0.33f;
		private float npWeightTo = 0.33f;
		private float npWeight2 = 0.33f;
		private float npWeight2To = 0.33f;
		private float npWeight3 = 0.33f;
		private float npWeight3To = 0.33f;
		private float nnKutiOpenWeight = 0.5f;
		private float nnKutiOpenWeightTo = 0.5f;
		private Vector3 tangBonePos = Vector3.zero;
		private Quaternion tangBoneRot = Quaternion.identity;
		private Vector3 tangBonePosTarget = Vector3.zero;
		private Quaternion tangBoneRotTarget = Quaternion.identity;
		private Vector3 tangBoneRotSpeed = Vector3.zero;
		private float eyeOpenSpeed = 1f;
		private float eyeOpenTime = 1f;
		private float npWeightTime = 1f;
		private float npWeightTime2 = 1f;
		private float npWeightTime3 = 1f;
		private float nnKutiOpenWeightTime = 1f;
		private Vector3 tangBoneTime = Vector3.zero;
		private float tangTime = 1f;
		private float mouthOpenTime = 1f;
		private Vector3 tangBonePosSpeed = Vector3.one;
		private HVoiceCtrl voiceCtrl;
		private float curEyeValue;
		private float curKissValue;
		private ChaControl female;
		public bool kissing;
		internal bool isAibuTouching;
		internal Phase kissPhase = Phase.None;
		public GameObject maleTang;
		internal HFlag flags;
		public GameObject camera;
		public GameObject kissNeckTarget;
		private object[] hands = new object[2];
		private GameObject tang;
		private SkinnedMeshRenderer tangRenderer;
		private Siru siru;
		private GameObject myMouth;
		private GameObject kissEyeTarget;
		private float eyeLookX;
		private float eyeLookY;
		private float eyeLookChangeTimer;
		private Vector3 initTangBonePos;
		private Quaternion initTangBoneRot;
		private float npWeightSpeed;
		private float npWeightSpeed2;
		private float npWeightSpeed3;
		internal float dragSpeed = 0.001f;
		internal HAibu aibu;
		private delegate bool BreathProc(HVoiceCtrl _instance, AnimatorStateInfo _ai, ChaControl _female, int _main);
		private BreathProc breathProcDelegate;
		internal object[] touchOrder = new object[2];
		internal static bool isInOrgasm;
		internal static HFlag.FinishKind origFinishFlag;
		private string prevAnimation;
		internal bool preventPrevAnimationSwap;

		public bool IsKiss { get; private set; }

		private void Awake()
		{
			female = GetComponent<ChaControl>();
			ReloadBlendValues();
			Transform transform = female.GetComponentsInChildren<Transform>().ToList<Transform>().Where<Transform>(x => x.name == "o_tang").FirstOrDefault<Transform>();
			if (transform == null)
			{
				Console.WriteLine("No tang founded.");
				Destroy(this);
			}
			else
			{
				tang = transform.gameObject;
				siru = gameObject.AddComponent<Siru>();
				siru.female = female;
				siru.cyu = this;
				tangRenderer = siru.tangRenderer = tang.GetComponent<SkinnedMeshRenderer>();
				kissEyeTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
				kissEyeTarget.SetActive(false);
				kissEyeTarget.AddComponent<DragMove>();
				kissEyeTarget.transform.localScale = Vector3.one * 0.05f;
				kissEyeTarget.transform.SetParent(female.objHead.transform);
				kissEyeTarget.transform.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
				camera = Resources.FindObjectsOfTypeAll<SteamVR_Camera>().FirstOrDefault<SteamVR_Camera>().GetComponent<Camera>().gameObject;
				myMouth = new GameObject("MyMouth");
				myMouth.transform.SetParent(camera.transform);
				myMouth.transform.localPosition = new Vector3(0.0f, -CyuLoaderVR.MouthOffset.Value, 0.0f);
				siru.siruTarget = myMouth;
				kissNeckTarget = new GameObject("KissNeckTarget");
				kissNeckTarget.transform.SetParent(camera.transform, false);
				kissNeckTarget.transform.localPosition = new Vector3(0.0f, CyuLoaderVR.KissNeckAngle.Value, -0.5f);
			}
		}

		private void Start()
		{
			var scene = FindObjectOfType<VRHScene>();
			initTangBonePos = tangRenderer.bones[0].localPosition;
			initTangBoneRot = tangRenderer.bones[0].localRotation;

			aibu = Traverse.Create(scene).Field("lstProc").GetValue<List<HActionBase>>().OfType<HAibu>().FirstOrDefault();
			hands = Traverse.Create(scene).Field("vrHands").GetValue<VRHandCtrl[]>();

			MethodInfo breathProcInfo = AccessTools.Method(typeof(HVoiceCtrl), "BreathProc", new Type[3]
				{
					typeof (AnimatorStateInfo),
					typeof (ChaControl),
					typeof (int)
				});
			breathProcDelegate = (BreathProc)Delegate.CreateDelegate(typeof(BreathProc), breathProcInfo);

		}

		private void OnDestroy()
		{
			Destroy(siru);
			Destroy(kissEyeTarget);
			Destroy(myMouth);
			Destroy(kissNeckTarget);
		}

		public void ReloadBlendValues()
		{
			SkinnedMeshRenderer[] componentsInChildren = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
			bvs.Clear();
			bvsShow.Clear();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
			{
				int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
				for (int index = 0; index < blendShapeCount; ++index)
				{
					Cyu.BlendValue blendValue = new Cyu.BlendValue();
					blendValue.index = index;
					blendValue.name = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(index);
					blendValue.value = skinnedMeshRenderer.GetBlendShapeWeight(index);
					blendValue.renderer = skinnedMeshRenderer;
					if (blendValue.name.IndexOf("name_") >= 0 || blendValue.name.IndexOf("name02_") >= 0 || blendValue.name.IndexOf("pero_") >= 0)
					{
						blendValue.active = true;
						blendValue.weight = 0.5f;
					}
					bvs.Add(blendValue);
				}
			}
		}

		public void Kiss(bool active, bool immediateStop = false)
		{
			if (!active)
			{
				IsKiss = false;
				if (immediateStop)
					kissing = false;
			}
			else
			{
				IsKiss = true;
				if (kissing)
					return;

				//Stop kissing is girl is speaking
				if (voiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
				{
					IsKiss = false;
					return;
				}
				//No kissing if girl doesn't allow
				if (!CyuLoaderVR.ForceKiss.Value && !flags.lstHeroine[0].isKiss && !flags.lstHeroine[0].denial.kiss)
				{
					flags.AddNotKiss();

					if (flags.mode == HFlag.EMode.aibu)
						flags.voice.playVoices[0] = 103;

					IsKiss = false;
					return;
				}

				// Save the current animation so that we can return to it after kissing is over.
				// This allows us to retain facial expressions after going back to idle, e.g., post-orgasm expressions.
				if (CyuLoaderVR.OrgasmAfterKiss.Value)
				{
					prevAnimation = flags.nowAnimStateName;
					preventPrevAnimationSwap = false;
				}

				if (flags.mode == HFlag.EMode.aibu)
				{
					//This is to determine if character is currently in animation crossfading
					//If she is, we'd have to manually start the kissing animation and set the back-to-idle animation to the currently active one
					if (female.animBody.GetNextAnimatorClipInfoCount(0) > 0)
					{
						switch (touchOrder.LastOrDefault(x => x != null))
						{
							case HFlag.ClickKind.muneL:
							case HFlag.ClickKind.muneR:
								Traverse.Create(aibu).Field("backIdle").SetValue(1);
								break;

							case HFlag.ClickKind.kokan:
								Traverse.Create(aibu).Field("backIdle").SetValue(2);
								break;

							case HFlag.ClickKind.siriL:
							case HFlag.ClickKind.siriR:
							case HFlag.ClickKind.anal:
								Traverse.Create(aibu).Field("backIdle").SetValue(3);
								break;

							default:
								Traverse.Create(aibu).Field("backIdle").SetValue(0);
								break;
						}

						aibu.SetPlay("K_Touch");
					}
					else
					{
						flags.click = HFlag.ClickKind.mouth;
					}
				}

				flags.AddKiss();
				StartCoroutine(BeroKiss());
			}
		}

		public void RandomMoveFloat(
			ref float cur,
			ref float to,
			float speed,
			float min,
			float max,
			ref float time,
			float timeMin = 1f,
			float timeMax = 5f)
		{
			float num = 0.01f;
			if (cur < to)
			{
				cur = Mathf.SmoothDamp(cur, to, ref speed, time);
				if (cur + num >= to)
				{
					to = UnityEngine.Random.Range(min, max);
					time = UnityEngine.Random.Range(timeMin, timeMax);
				}
				return;
			}
			speed = 0f - speed;
			cur = Mathf.SmoothDamp(cur, to, ref speed, time);
			if (cur - num <= to)
			{
				to = UnityEngine.Random.Range(min, max);
				time = UnityEngine.Random.Range(timeMin, timeMax);
			}
		}

		public void RandomMoveFloatTest(
			ref float cur,
			ref float to,
			ref float speed,
			float min,
			float max,
			ref float time,
			float timeMin = 1f,
			float timeMax = 5f)
		{
			float num = 0.05f;
			if (cur < to)
			{
				cur = Mathf.SmoothDamp(cur, to, ref speed, time, 100f);
				if (cur + num >= to)
				{
					to = UnityEngine.Random.Range(min, max);
					time = UnityEngine.Random.Range(timeMin, timeMax);
				}
			}
			else
			{
				cur = Mathf.SmoothDamp(cur, to, ref speed, time, 100f);
				if (cur - num <= to)
				{
					to = UnityEngine.Random.Range(min, max);
					time = UnityEngine.Random.Range(timeMin, timeMax);
				}
			}
		}

		/// <summary>
		/// Checks whether saliva string exists and is visible
		/// </summary>
		/// <returns>Returns true if saliva string exists and is visible, and false otherwise</returns>
		private bool IsSiruActive()
		{
			if (siru == null)
			{
				return false;
			}

			return siru.siruAmount > 0f;
		}

		public IEnumerator BeroKiss()
		{
			kissing = true;
			kissPhase = Phase.Engaging;
			curEyeValue = 100f;
			curMouthValue = 0f;
			tangSpeed = UnityEngine.Random.Range(10f, 20f);
			while (IsKiss)
			{
				curKissValue += Time.deltaTime * tangSpeed * 5f;
				curMouthValue += Time.deltaTime * tangSpeed * 10f;
				curEyeValue -= Time.deltaTime * tangSpeed * 3f;
				curMouthValue = Mathf.Clamp(curMouthValue, 0f, 100f);
				curKissValue = Mathf.Clamp(curKissValue, 0f, 100f);
				curEyeValue = Mathf.Clamp(curEyeValue, 0f, 100f);
				eyesOpenValue = curEyeValue / 100f;
				EyeAnimate();
				if (curKissValue >= 100f && curMouthValue >= 100f)
				{
					break;
				}
				yield return null;
			}
			kissPhase = Phase.InAction;
			while (IsKiss)
			{
				RandomMoveFloatTest(ref npWeight, ref npWeightTo, ref npWeightSpeed, 0f, 1f, ref npWeightTime, 0.1f, 0.5f);
				RandomMoveFloatTest(ref npWeight2, ref npWeight2To, ref npWeightSpeed2, 0f, 1f, ref npWeightTime2, 0.1f, 0.5f);
				RandomMoveFloatTest(ref npWeight3, ref npWeight3To, ref npWeightSpeed3, 0f, 1f, ref npWeightTime3, 0.1f, 0.5f);
				RandomMoveFloat(ref nnKutiOpenWeight, ref nnKutiOpenWeightTo, 0.1f, 0f, 1f, ref nnKutiOpenWeightTime, 1f, 5f);
				RandomMoveFloatTest(ref tangBonePos.x, ref tangBonePosTarget.x, ref tangBonePosSpeed.x, -0.002f, 0.002f, ref tangBoneTime.x, 0.1f, 2f);
				RandomMoveFloatTest(ref tangBonePos.y, ref tangBonePosTarget.y, ref tangBonePosSpeed.y, -0.001f, 0.001f, ref tangBoneTime.y, 0.1f, 2f);
				RandomMoveFloatTest(ref tangBonePos.z, ref tangBonePosTarget.z, ref tangBonePosSpeed.z, -0.002f, 0.002f, ref tangBoneTime.z, 0.1f, 2f);
				RandomMoveFloatTest(ref tangBoneRot.y, ref tangBoneRotTarget.y, ref tangBoneRotSpeed.y, -5f, 5f, ref tangBoneTime.y, 0.1f, 2f);
				RandomMoveFloatTest(ref tangBoneRot.x, ref tangBoneRotTarget.x, ref tangBoneRotSpeed.x, -5f, 2.5f, ref tangBoneTime.x, 0.1f, 2f);
				RandomMoveFloatTest(ref tangBoneRot.z, ref tangBoneRotTarget.z, ref tangBoneRotSpeed.z, -3.5f, 3.5f, ref tangBoneTime.z, 0.1f, 2f);
				RandomMoveFloatTest(ref curMouthValue, ref toMouthValue, ref mouthSpeed, 97f, 100f, ref mouthOpenTime, 10f, 12f);
				RandomMoveFloatTest(ref curEyeValue, ref toEyeValue, ref eyeOpenSpeed, 0f, CyuLoaderVR.EyesMovement.Value, ref eyeOpenTime, 0.01f, 1.2f);
				eyesOpenValue = curEyeValue / 100f;
				RandomMoveFloatTest(ref curKissValue, ref toKissValue, ref tangSpeed, 25f, 100f, ref tangTime, 0.01f, 0.1f);
				if (CyuLoaderVR.EyesMovement.Value > 0)
					EyeAnimate();	
				yield return null;

			}

			// Return to the previous animation after the player finishes kissing.
			if(CyuLoaderVR.OrgasmAfterKiss.Value && !preventPrevAnimationSwap)
				aibu.SetPlay(prevAnimation);

			kissPhase = Phase.Disengaging;
			for (; ; )
			{
				float num = Mathf.Max(25f, Mathf.Abs(tangSpeed));
				curMouthValue -= Time.deltaTime * num;
				curMouthValue = Mathf.Clamp(curMouthValue, 0f, 100f);
				curKissValue -= Time.deltaTime * num;
				curKissValue = Mathf.Clamp(curKissValue, 0f, 100f);
				curEyeValue += Time.deltaTime * num * 0.5f;
				curEyeValue = Mathf.Clamp(curEyeValue, 0f, 100f);
				eyesOpenValue = curEyeValue / 100f;
				if (curMouthValue <= 10f && curKissValue <= 10f)
				{
					break;
				}
				yield return null;
			}
			tangBonePos = Vector3.zero;
			tangBoneRot = Quaternion.identity;
			yield return null;
			kissPhase = Phase.None;
			kissing = false;

			yield break;
		}

		public void OnDisable()
		{
			if (IsKiss || kissing)
			{
				Kiss(false, immediateStop: true);

				if (bvs.Count() == 0)
					ReloadBlendValues();

				voiceCtrl.isPrcoStop = false;

				if (origFinishFlag != HFlag.FinishKind.none)
				{
					flags.finish = origFinishFlag;
					origFinishFlag = HFlag.FinishKind.none;
				}

				female.ChangeEyesBlinkFlag(true);
			}					
		}


		private void Update()
		{
			if (voiceCtrl == null)
			{
				voiceCtrl = FindObjectOfType<HVoiceCtrl>();
				if (voiceCtrl == null)
					return;
			}
	
			if (bvs.Count() == 0)
				ReloadBlendValues();

			float curDistance = Vector3.Distance(myMouth.transform.position, tang.transform.position);
			float threshold;
			if (flags.mode == HFlag.EMode.aibu)
			{
				threshold = CyuLoaderVR.KissDistanceAibu.Value;

				if (((VRHandCtrl[])hands).Any((VRHandCtrl h) => h.IsAction()))
					isAibuTouching = true;
				else
					isAibuTouching = false;
			}	
			else
				threshold = CyuLoaderVR.KissDistance.Value;


			if (flags.click == HFlag.ClickKind.inside || flags.click == HFlag.ClickKind.outside)
				isInOrgasm = true;

			//If currently in orgasm, stop kissing immediately without disengaging transition to prevent interferring with orgasm animation or voice
			if (isInOrgasm)
			{
				Kiss(false, immediateStop: true);
			}
			else if (curDistance < threshold)
			{
				if (flags.mode != HFlag.EMode.aibu || (!kissing && !IsSiruActive()) )
					Kiss(true);
				//Continue kissing if currently transitioning into kiss, or if within distance threshold
				//The distance threshold is larger when groping to allow some eye candy
				else if (curDistance < (isAibuTouching ? (ExitKissDistance + 0.04f) : ExitKissDistance) || kissPhase == Phase.Engaging)
					Kiss(true);
				else
					Kiss(false);
			}
			else
			{
				Kiss(false);
			}

			//Stop girl from speaking lines and manually proc breath/moan sounds 
			//when she is not in orgasm and is still kissing or saliva string still visible
			if (!isInOrgasm && (kissing || IsSiruActive()))
			{
				voiceCtrl.isPrcoStop = true;

				//Manually proc breath sound every frame causes eyes flickering when initiating kissing during transition of releasing a body part in caress mode
				//This makes sure breath proc is not run when approaching to kiss in caress mode
				if (flags.mode == HFlag.EMode.aibu ? kissPhase > Phase.Engaging : true)
					breathProcDelegate.Invoke(voiceCtrl, female.animBody.GetCurrentAnimatorStateInfo(0), female, 0);
			}
			else
			{
				voiceCtrl.isPrcoStop = false;
			}

			if (kissing)
			{		
				if (flags.mode == HFlag.EMode.aibu)
				{
					//Use configured value (KissMotionSpeed) to control animation speed during kissing in caress mode
					//Increase animation speed further if GropeOverride is set to true and groping motion is larger than KissMotionSpeed
					if (CyuLoaderVR.GropeOverride.Value && kissPhase == Phase.InAction && isAibuTouching)
					{
						//Use the higher value between dragSpeed(value based on controller movement) and speedItem(game calculated value) to set kissing animation speed
						//Then make sure the speed value used for calculating animation speed is reset to a minimum
						//so that it doesn't get stuck at a high value, causing kissing animation speed to also be stuck at a high value.
						flags.SpeedUpClickAibu(flags.rateDragSpeedUp, Mathf.Clamp(Mathf.Max(dragSpeed, flags.speedItem), CyuLoaderVR.KissMotionSpeed.Value, 1.5f), true);

						dragSpeed = 0.0001f;
					}
					else
						flags.SpeedUpClickAibu(flags.rateDragSpeedUp, CyuLoaderVR.KissMotionSpeed.Value, true);
				}

				//While kissing, assigns HFlag.finish to none to prevent orgasm, while storing the finish flag in origFinishFlag to be assigned back to the game after kissing
				if (flags.finish != HFlag.FinishKind.none)
				{
					origFinishFlag = flags.finish;
					flags.finish = HFlag.FinishKind.none;
				}

				flags.DragStart();
			}
			else
			{
				//Outside of kissing, restore finish flag from origFinishFlag to resume orgasm
				if (origFinishFlag != HFlag.FinishKind.none)
				{
					flags.finish = origFinishFlag;
					origFinishFlag = HFlag.FinishKind.none;
				}

				female.ChangeEyesBlinkFlag(true);
			}
		}

		public void SetBlendShapeWeight()
		{
			try
			{
				foreach (Cyu.BlendValue bv in bvs)
				{
					if (bv.active)
					{
						float num = 1f;
						float num2 = 1.5f;
						if (bv.name.IndexOf("name02_") >= 0)
						{
							num = num2 * npWeight / (npWeight + npWeight2 + npWeight3);
						}
						else if (bv.name.IndexOf("pero_") >= 0)
						{
							num = num2 * npWeight2 / (npWeight + npWeight2 + npWeight3);
						}
						else if (bv.name.IndexOf("name_") >= 0)
						{
							num = num2 * npWeight3 / (npWeight + npWeight2 + npWeight3);
						}
						bv.value = curKissValue * num;
						if (kissing && bv.name.IndexOf("kuti_face.f00_name02_op") >= 0)
						{
							bv.value = curMouthValue * (1f - nnKutiOpenWeight);
						}
						else if (kissing && bv.name.IndexOf("kuti_face.f00_name_op") >= 0)
						{
							bv.value = curMouthValue * nnKutiOpenWeight;
						}
						if (kissing && bv.name.IndexOf("kuti_ha.ha00_name02_op") >= 0)
						{
							bv.value = curMouthValue * (1f - nnKutiOpenWeight);
						}
						else if (kissing && bv.name.IndexOf("kuti_ha.ha00_name_op") >= 0)
						{
							bv.value = curMouthValue * nnKutiOpenWeight;
						}
						bv.renderer.SetBlendShapeWeight(bv.index, bv.value);
					}
					else if (bv.name.IndexOf("kuti") >= 0)
					{
						bv.renderer.SetBlendShapeWeight(bv.index, 0f);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Destroy(this);
			}
		}

		public void LateUpdateHook()
		{
			if (!kissing)
				return;

			//Animate mouth and tongue if
			// - tongueMovement config is set to ForceOn, or
			// - tongueMovement config is set to Auto while female excite gauge is over 70 or H state is "lewd"
			if (CyuLoaderVR.MouthMovement.Value == FrenchMode.ForceOn || (CyuLoaderVR.MouthMovement.Value == FrenchMode.Auto && (flags.gaugeFemale > 70 || flags.lstHeroine[0].HExperience == SaveData.Heroine.HExperienceKind.淫乱)))
			{
				SetBlendShapeWeight();
				Vector3 localPosition = tangRenderer.bones[0].transform.localPosition;
				tangRenderer.bones[0].transform.localPosition = new Vector3(localPosition.x, tangBonePos.y + initTangBonePos.y, localPosition.z);
				tangRenderer.bones[0].transform.localRotation = initTangBoneRot * Quaternion.Euler(tangBoneRot.x, tangBoneRot.y, tangBoneRot.z);
			}

			//Keep neck elevated except during kiss disengagement to prevent girl from looking down on user as she exits kiss
			if (IsKiss)
			{
				female.ChangeLookNeckPtn(1, 1f);
				female.neckLookCtrl.target = kissNeckTarget.transform;
			}
			
			//Move eyes around only in the middle of kissing, to prevent ahegao eyes during engagement and disengagement
			if (kissPhase == Phase.InAction)
			{
				female.ChangeLookEyesPtn(1);
				female.eyeLookCtrl.target = kissEyeTarget.transform;
			}		
		}

		/// <summary>
		/// Execute female eye movement
		/// </summary>
		private void EyeAnimate()
		{
			female.ChangeEyesBlinkFlag(false);
			female.eyesCtrl.ChangePtn(0, true);
			female.eyebrowCtrl.ChangePtn(0, true);
			female.ChangeEyesOpenMax(eyesOpenValue);
			female.ChangeEyebrowOpenMax(eyesOpenValue);
			female.eyesCtrl.OpenMin = 0.0f;
			female.eyebrowCtrl.OpenMin = 0.0f;
			float num3 = Vector3.Angle(female.objHead.transform.forward, kissEyeTarget.transform.position - female.objHead.transform.position);
			eyeLookChangeTimer -= Time.deltaTime;
			if (eyeLookChangeTimer < 0f || num3 > 45f)
			{
				eyeLookChangeTimer = UnityEngine.Random.Range(10f, 20f);
				eyeLookX = UnityEngine.Random.Range(-70f, 70f);
				eyeLookY = UnityEngine.Random.Range(-45f, 45f);
			}
			
			kissEyeTarget.transform.RotateAround(kissEyeTarget.transform.parent.position, female.objHead.transform.up, eyeLookX * Time.deltaTime * 0.2f);
			kissEyeTarget.transform.RotateAround(kissEyeTarget.transform.parent.position, female.objHead.transform.right, eyeLookY * Time.deltaTime * 0.2f);
			
		}

		private class BlendValue
		{
			public float weight = 1f;
			public int index;
			public string name;
			public float value;
			public SkinnedMeshRenderer renderer;
			public bool active;
		}
		public enum FrenchMode
		{
			ForceOff,
			Auto,
			ForceOn
		}

		internal enum Phase
		{
			None,
			Engaging,
			InAction,
			Disengaging
		}
	}
}
