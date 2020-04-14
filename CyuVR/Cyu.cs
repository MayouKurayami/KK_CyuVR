using ChaCustom;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	public class Cyu : MonoBehaviour
	{
		private float tangSpeed = 35f;
		private Rect rectWin = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
		private float curMouthValue = 100f;
		private float toMouthValue = 95f;
		private float toEyeValue = CyuLoaderVR.EyesMovement.Value;
		private float toKissValue = 75f;
		private List<Cyu.BlendValue> bvs = new List<Cyu.BlendValue>();
		private List<Cyu.BlendValue> bvsShow = new List<Cyu.BlendValue>();
		private string filterText = "";
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
		private bool moveAuto;
		private ChaControl female;
		public bool gui;
		public float hohoAka;
		public int tearLv;
		public int clothState;
		internal int clothesState;
		public bool kissing;
		internal bool isTouching;
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
		private CameraControl_Ver2 cameraControl;
		private Vector3 initTangBonePos;
		private Quaternion initTangBoneRot;
		private float npWeightSpeed;
		private float npWeightSpeed2;
		private float npWeightSpeed3;
		internal static float dragSpeed = 0.001f;
		private const float exitKissDistance = 0.16f;
		internal HAibu aibu;

		public bool IsKiss { get; private set; }

		private void Awake()
		{
			cameraControl = FindObjectOfType<CameraControl_Ver2>();
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
		}

		private void OnDestroy()
		{
			Destroy(siru);
			Destroy(kissEyeTarget);
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

		public void ChangeMouthExpression(int index)
		{
			if (index >= Singleton<CustomBase>.Instance.lstMouth.Count)
			{
				return;
			}

			female.ChangeMouthPtn(index, true);
			Console.WriteLine("Mouth:{0} {1}", index, Singleton<CustomBase>.Instance.lstMouth[index].list[1]);
		}

		public void ChangeEyesExpression(int index)
		{
			if (index >= Singleton<CustomBase>.Instance.lstEye.Count)
			{
				return;
			}

			female.ChangeEyesPtn(index, true);
			Console.WriteLine("Eyes:{0} {1}", index, Singleton<CustomBase>.Instance.lstEye[index].list[1]);
		}

		public void ChangeEyeBlowExpression(int index)
		{
			if (index >= Singleton<CustomBase>.Instance.lstEyebrow.Count)
			{
				return;
			}

			female.ChangeEyebrowPtn(index, true);
			Console.WriteLine("Eye Blow:{0} {1}", index, Singleton<CustomBase>.Instance.lstEyebrow[index].list[1]);
		}

		public void Kiss(bool active)
		{
			if (!active)
			{
				IsKiss = false;
			}
			else
			{
				IsKiss = true;
				if (kissing)
				{
					return;
				}

				DoKiss();
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
			voiceCtrl = FindObjectOfType<HVoiceCtrl>();
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
				EyeAnimate(CyuLoaderVR.EyesMovement.Value > 0);
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
				EyeAnimate(CyuLoaderVR.EyesMovement.Value > 0);	
				yield return null;

			}

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
			IsKiss = false;
			kissing = false;
		}

		public void DoKiss()
		{
			if (voiceCtrl == null)
			{
				voiceCtrl = FindObjectOfType<HVoiceCtrl>();
			}

			if (voiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
			{
				IsKiss = false;
			}
			else if (!flags.lstHeroine[0].isKiss && !flags.lstHeroine[0].denial.kiss)
			{
				flags.AddNotKiss();
				if (flags.mode == HFlag.EMode.aibu)
				{
					flags.voice.playVoices[0] = 103;
				}

				IsKiss = false;
			}
			else
			{
				if (flags.mode == HFlag.EMode.aibu)
				{
					int backIdle = 0;

					if (isTouching)
					{
						switch (flags.nowAnimStateName)
						{
							case "M_Idle":
							case "M_Touch":
								backIdle = 1;
								break;

							case "A_Idle":
							case "A_Touch":
								backIdle = 2;
								break;

							case "S_Idle":
							case "S_Touch":
								backIdle = 3;
								break;

							default:
								backIdle = 0;
								break;
						}
					}

					Traverse.Create(aibu).Field("backIdle").SetValue(backIdle);
					aibu.SetPlay("K_Touch");
				}
				flags.AddKiss();
				StartCoroutine(BeroKiss());
			}
		}

		private void OnGUI()
		{
			if (!gui)
			{
				return;
			}

			float width = 400f;
			if (rectWin.width < 1f)
			{
				rectWin.Set(Screen.width - width, 0.0f, width, Screen.height);
			}

			rectWin = GUI.Window(832, rectWin, new GUI.WindowFunction(WinFunc), "Control Face");
			Event current = Event.current;
			if (current.button == 0 && current.isMouse && current.type == EventType.MouseDrag)
			{
				cameraControl.enabled = false;
			}
			else
			{
				cameraControl.enabled = true;
			}
		}

		private void WinFunc(int id)
		{
			float num = 5f;
			GUILayout.BeginArea(new Rect(0.0f + num, 0.0f + num, rectWin.width - 2f * num, rectWin.height - 2f * num));
			filterText = GUILayout.TextField(filterText);
			GUILayout.Label("HohoAka");
			hohoAka = GUILayout.HorizontalSlider(hohoAka, 0.0f, 100f);
			GUILayout.Label("Tear");
			tearLv = GUILayout.Toolbar(tearLv, new string[5]
			{
				"0",
				"1",
				"2",
				"3",
				"4"
			});
			bvsShow.Clear();
			foreach (Cyu.BlendValue bv in bvs)
			{
				if (filterText.Length <= 0 || bv.name.IndexOf(filterText) != -1)
				{
					bv.active = GUILayout.Toggle(bv.active, bv.name);
					bv.value = GUILayout.HorizontalSlider(bv.value, 0.0f, 100f, null);
					bvsShow.Add(bv);
				}
			}
			if (GUI.changed)
			{
				moveAuto = false;
			}

			moveAuto = GUILayout.Toggle(moveAuto, "Move auto");
			GUILayout.EndArea();
			GUI.DragWindow();
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
					isTouching = true;
				else
					isTouching = false;
			}
				
			else
				threshold = CyuLoaderVR.KissDistance.Value;

			if (curDistance < threshold)
			{
				if (flags.mode != HFlag.EMode.aibu || (!kissing && !IsSiruActive()) )
				{
					Kiss(true);
				}
				else if (curDistance < (isTouching ? (exitKissDistance + 0.04f) : exitKissDistance) || kissPhase == Phase.Engaging)
				{
					Kiss(true);
				}
				else
				{
					Kiss(false);
				}
			}
			else
			{
				Kiss(false);
			}

			if (kissing || IsSiruActive())
			{
				voiceCtrl.isPrcoStop = true;
				Traverse.Create(voiceCtrl).Method("BreathProc", new System.Type[3]
				{
					typeof (AnimatorStateInfo),
					typeof (ChaControl),
					typeof (int)
				}, null).GetValue<bool>(female.animBody.GetCurrentAnimatorStateInfo(0), female, 0);
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
					if (CyuLoaderVR.GropeOverride.Value && kissPhase == Phase.InAction)
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

				flags.DragStart();
			}
			else
			{
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
			{
				return;
			}

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
			if (IsKiss)
			{
				female.ChangeLookNeckPtn(1, 1f);
				female.neckLookCtrl.target = kissNeckTarget.transform;
				female.ChangeLookEyesPtn(1);
				female.eyeLookCtrl.target = kissEyeTarget.transform;
			}
		}

		/// <summary>
		/// Execute female eye movement
		/// </summary>
		/// <param name="condition">Condition to determine whether the function runs or not</param>
		private void EyeAnimate(bool condition)
		{
			if (!condition)
			{
				return;
			}	
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

		internal void ToggleClothesStateAll()
		{
			++clothesState;
			if (clothesState > 2)
			{
				clothesState = 0;
			}

			female.SetClothesStateAll((byte)clothesState);
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
