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
		private float winHeight = 300f;
		private Rect rectWin = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
		private float curMouthValue = 100f;
		private float toMouthValue = 95f;
		private float toEyeValue = 55f;
		private float toKissValue = 75f;
		private float tangAcc = 15f;
		private float toTangSpeed = 35f;
		private List<Cyu.BlendValue> bvs = new List<Cyu.BlendValue>();
		private List<Cyu.BlendValue> bvsShow = new List<Cyu.BlendValue>();
		private string filterText = "";
		public float eyesOpenValue = 100f;
		private bool firstKiss = true;
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
		private float tangWeight;
		private HVoiceCtrl voiceCtrl;
		private float curEyeValue;
		private float curKissValue;
		private bool moveAuto;
		private bool kissFlag;
		private ChaControl female;
		private ChaControl male;
		public bool gui;
		public float hohoAka;
		public int tearLv;
		public int clothState;
		internal int clothesState;
		public bool kissing;
		public GameObject maleTang;
		private bool kissManual;
		private HFlag flags;
		private NeckLookControllerVer2 neckLookController;
		public GameObject camera;
		public GameObject kissNeckTarget;
		private VRHScene scene;
		private VRHandCtrl hand0;
		private int layerInc;
		private GameObject tang;
		private SkinnedMeshRenderer tangRenderer;
		private Siru siru;
		private GameObject myMouth;
		private GameObject kissEyeTarget;
		private float eyeLookX;
		private float eyeLookXTo;
		private float eyeLookY;
		private float eyeLookYTo;
		private float eyeLookChangeTimer;
		private CameraControl_Ver2 cameraControl;
		private Vector3 initTangBonePos;
		private Quaternion initTangBoneRot;
		private float tangBoneRotYTo;
		private float tangBoneRotSpeedTo;
		private float npWeightSpeed;
		private float npWeightSpeed2;
		private float npWeightSpeed3;

		public bool IsKiss
		{
			get
			{
				return kissFlag;
			}
		}

		private void Awake()
		{
			cameraControl = FindObjectOfType<CameraControl_Ver2>();
			female = GetComponent<ChaControl>();
			male = FindObjectsOfType<ChaControl>().Where<ChaControl>(x => x.name.IndexOf("chaM") >= 0).FirstOrDefault<ChaControl>();
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
				tangRenderer = siru.tangRenderer = tang.GetComponent<SkinnedMeshRenderer>();
				kissEyeTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
				kissEyeTarget.SetActive(false);
				kissEyeTarget.AddComponent<DragMove>();
				kissEyeTarget.transform.localScale = Vector3.one * 0.05f;
				kissEyeTarget.transform.SetParent(female.objHead.transform);
				kissEyeTarget.transform.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
				neckLookController = female.neckLookCtrl;
				camera = Resources.FindObjectsOfTypeAll<SteamVR_Camera>().FirstOrDefault<SteamVR_Camera>().GetComponent<Camera>().gameObject;
				myMouth = new GameObject("MyMouth");
				myMouth.transform.SetParent(camera.transform);
				myMouth.transform.localPosition = new Vector3(0.0f, -Config.mouthOffset, 0.0f);
				siru.siruTarget = myMouth;
				kissNeckTarget = new GameObject("KissNeckTarget");
				kissNeckTarget.transform.SetParent(camera.transform, false);
				kissNeckTarget.transform.localPosition = new Vector3(0.0f, Config.kissNeckAngle, -0.5f);
			}
		}

		private void Start()
		{
			scene = FindObjectOfType<VRHScene>();
			initTangBonePos = tangRenderer.bones[0].localPosition;
			initTangBoneRot = tangRenderer.bones[0].localRotation;
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
				kissFlag = false;
			}
			else
			{
				kissFlag = true;
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
			voiceCtrl = FindObjectOfType<HVoiceCtrl>();
			curEyeValue = 100f;
			curMouthValue = 0f;
			tangSpeed = UnityEngine.Random.Range(10f, 20f);
			while (kissFlag)
			{
				curKissValue += Time.deltaTime * tangSpeed * 5f;
				curMouthValue += Time.deltaTime * tangSpeed * 10f;
				curEyeValue -= Time.deltaTime * tangSpeed * 3f;
				curMouthValue = Mathf.Clamp(curMouthValue, 0f, 100f);
				curKissValue = Mathf.Clamp(curKissValue, 0f, 100f);
				curEyeValue = Mathf.Clamp(curEyeValue, 0f, 100f);
				if (curKissValue >= 100f && curMouthValue >= 100f)
				{
					break;
				}
				yield return null;
			}
			while (kissFlag)
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
				RandomMoveFloatTest(ref curEyeValue, ref toEyeValue, ref eyeOpenSpeed, 0f, 55f, ref eyeOpenTime, 0.01f, 1.2f);
				eyesOpenValue = curEyeValue / 100f;
				RandomMoveFloatTest(ref curKissValue, ref toKissValue, ref tangSpeed, 25f, 100f, ref tangTime, 0.01f, 0.1f);
				yield return null;

			}
			Traverse.Create(hand0).Field("isKiss").SetValue(false);
			for (; ; )
			{
				float num = Mathf.Max(25f, Mathf.Abs(tangSpeed));
				curMouthValue -= Time.deltaTime * num * 2f * 0.8f;
				curMouthValue = Mathf.Clamp(curMouthValue, 0f, 100f);
				curKissValue -= Time.deltaTime * num * 1f;
				curKissValue = Mathf.Clamp(curKissValue, 0f, 100f);
				curEyeValue += Time.deltaTime * num * 1f * 0.8f;
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
			kissing = false;
			yield break;
		}

		public void OnDisable()
		{
			kissFlag = false;
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
				kissFlag = false;
			}
			else if (!flags.lstHeroine[0].isKiss && !flags.lstHeroine[0].denial.kiss)
			{
				flags.AddNotKiss();
				if (flags.mode == HFlag.EMode.aibu)
				{
					flags.voice.playVoices[0] = 103;
				}

				kissFlag = false;
			}
			else
			{
				FindObjectOfType<VRHScene>();
				if (hand0 == null)
				{
					hand0 = Traverse.Create(scene).Field("vrHands").GetValue<VRHandCtrl[]>()[0];
				}

				if (flags.mode == HFlag.EMode.aibu)
				{
					flags.click = HFlag.ClickKind.mouth;
					Traverse.Create(hand0).Field("isKiss").SetValue(true);
				}
				flags.AddKiss();
				firstKiss = false;
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
				{
					return;
				}
			}
			if (flags == null)
			{
				flags = FindObjectOfType<HFlag>();
				if (flags == null)
				{
					return;
				}
			}
			if (bvs.Count() == 0)
			{
				ReloadBlendValues();
			}

			float curDistance = Vector3.Distance(myMouth.transform.position, tang.transform.position);
			float threshold;
			if (flags.mode == HFlag.EMode.aibu)
				threshold = Config.kissDistanceAibu;
			else
				threshold = Config.kissDistance;

			if (curDistance < threshold)
			{
				if (!IsSiruActive() || flags.mode != HFlag.EMode.aibu)
				{
					Kiss(true);
				}
				else if (curDistance < (Config.kissDistanceAibu - 0.1f) || siru.siruAmount < 0.2f)
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
				//change parameter for SpeedUpClickAibu to use Config.kissMotionSpeed to control animation speed during kissing in Aibu
				if (flags.mode == HFlag.EMode.aibu)
				{
					flags.SpeedUpClickAibu(1f * flags.rateDragSpeedUp, Config.kissMotionSpeed, true);
				}

				flags.DragStart();
				if (!Config.eyesMovement)
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

			if (Config.tongueOverride || flags.gaugeFemale > 70 || flags.lstHeroine[0].HExperience == SaveData.Heroine.HExperienceKind.淫乱)
			{
				SetBlendShapeWeight();
				Vector3 localPosition = tangRenderer.bones[0].transform.localPosition;
				tangRenderer.bones[0].transform.localPosition = new Vector3(localPosition.x, tangBonePos.y + initTangBonePos.y, localPosition.z);
				tangRenderer.bones[0].transform.localRotation = initTangBoneRot * Quaternion.Euler(tangBoneRot.x, tangBoneRot.y, tangBoneRot.z);
			}
			if (kissFlag)
			{
				female.ChangeLookNeckPtn(1, 1f);
				female.neckLookCtrl.target = kissNeckTarget.transform;
				female.ChangeLookEyesPtn(1);
				female.eyeLookCtrl.target = kissEyeTarget.transform;
			}
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
			public float min;
			public float max;
			public bool active;
		}
	}
}
