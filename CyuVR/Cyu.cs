// Decompiled with JetBrains decompiler
// Type: Bero.CyuVR.Cyu
// Assembly: CyuVR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E19B191-388D-44D8-A057-D416187C1E85
// Assembly location: C:\Users\MK\Desktop\KK\CyuVR\Source Dll\CyuVR.dll

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
    private float toEyeValue = 75f;
    private float toKissValue = 75f;
    private float tangAcc = 15f;
    private float toTangSpeed = 35f;
    private List<Cyu.BlendValue> bvs = new List<Cyu.BlendValue>();
    private List<Cyu.BlendValue> bvsShow = new List<Cyu.BlendValue>();
    private string filterText = "";
    public float eyesOpenValue = 100f;
    private bool berocyuOverride = true;
    private bool faceOverride = true;
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
        return this.kissFlag;
      }
    }

    private void Awake()
    {
      this.faceOverride = true;
      this.cameraControl = UnityEngine.Object.FindObjectOfType<CameraControl_Ver2>();
      this.female = this.GetComponent<ChaControl>();
      this.male = ((IEnumerable<ChaControl>) UnityEngine.Object.FindObjectsOfType<ChaControl>()).Where<ChaControl>((Func<ChaControl, bool>) (x => x.name.IndexOf("chaM") >= 0)).FirstOrDefault<ChaControl>();
      this.ReloadBlendValues();
      Transform transform = ((IEnumerable<Transform>) this.female.GetComponentsInChildren<Transform>()).ToList<Transform>().Where<Transform>((Func<Transform, bool>) (x => x.name == "o_tang")).FirstOrDefault<Transform>();
      if ((UnityEngine.Object) transform == (UnityEngine.Object) null)
      {
        Console.WriteLine("No tang founded.");
        UnityEngine.Object.Destroy((UnityEngine.Object) this);
      }
      else
      {
        this.tang = transform.gameObject;
        this.siru = this.gameObject.AddComponent<Siru>();
        this.siru.female = this.female;
        this.tangRenderer = this.siru.tangRenderer = this.tang.GetComponent<SkinnedMeshRenderer>();
        this.kissEyeTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
        this.kissEyeTarget.SetActive(false);
        this.kissEyeTarget.AddComponent<DragMove>();
        this.kissEyeTarget.transform.localScale = Vector3.one * 0.05f;
        this.kissEyeTarget.transform.SetParent(this.female.objHead.transform);
        this.kissEyeTarget.transform.localPosition = new Vector3(0.0f, 0.0f, 0.5f);
        this.neckLookController = this.female.neckLookCtrl;
        this.camera = ((IEnumerable<SteamVR_Camera>) Resources.FindObjectsOfTypeAll<SteamVR_Camera>()).FirstOrDefault<SteamVR_Camera>().GetComponent<Camera>().gameObject;
        this.myMouth = new GameObject("MyMouth");
        this.myMouth.transform.SetParent(this.camera.transform);
        this.myMouth.transform.localPosition = new Vector3(0.0f, -Config.mouthOffset, 0.0f);
        this.siru.siruTarget = this.myMouth;
        this.kissNeckTarget = new GameObject("KissNeckTarget");
        this.kissNeckTarget.transform.SetParent(this.camera.transform, false);
        this.kissNeckTarget.transform.localPosition = new Vector3(0.0f, Config.kissNeckAngle, -0.5f);
      }
    }

    private void Start()
    {
      this.scene = UnityEngine.Object.FindObjectOfType<VRHScene>();
      this.initTangBonePos = this.tangRenderer.bones[0].localPosition;
      this.initTangBoneRot = this.tangRenderer.bones[0].localRotation;
    }

    private void OnDestroy()
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.siru);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.kissEyeTarget);
    }

    public void ReloadBlendValues()
    {
      SkinnedMeshRenderer[] componentsInChildren = this.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
      this.bvs.Clear();
      this.bvsShow.Clear();
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
          this.bvs.Add(blendValue);
        }
      }
    }

    public void ChangeMouthExpression(int index)
    {
      if (index >= Singleton<CustomBase>.Instance.lstMouth.Count)
        return;
      this.female.ChangeMouthPtn(index, true);
      Console.WriteLine("Mouth:{0} {1}", (object) index, (object) Singleton<CustomBase>.Instance.lstMouth[index].list[1]);
    }

    public void ChangeEyesExpression(int index)
    {
      if (index >= Singleton<CustomBase>.Instance.lstEye.Count)
        return;
      this.female.ChangeEyesPtn(index, true);
      Console.WriteLine("Eyes:{0} {1}", (object) index, (object) Singleton<CustomBase>.Instance.lstEye[index].list[1]);
    }

    public void ChangeEyeBlowExpression(int index)
    {
      if (index >= Singleton<CustomBase>.Instance.lstEyebrow.Count)
        return;
      this.female.ChangeEyebrowPtn(index, true);
      Console.WriteLine("Eye Blow:{0} {1}", (object) index, (object) Singleton<CustomBase>.Instance.lstEyebrow[index].list[1]);
    }

    public void Kiss(bool active)
    {
      if (!active)
      {
        this.kissFlag = false;
      }
      else
      {
        this.kissFlag = true;
        if (this.kissing)
          return;
        this.DoKiss();
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
      if ((double) cur < (double) to)
      {
        cur = Mathf.SmoothDamp(cur, to, ref speed, time);
        if ((double) cur + (double) num < (double) to)
          return;
        to = UnityEngine.Random.Range(min, max);
        time = UnityEngine.Random.Range(timeMin, timeMax);
      }
      else
      {
        speed = -speed;
        cur = Mathf.SmoothDamp(cur, to, ref speed, time);
        if ((double) cur - (double) num > (double) to)
          return;
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
      if ((double) cur < (double) to)
      {
        cur = Mathf.SmoothDamp(cur, to, ref speed, time, 100f);
        if ((double) cur + (double) num < (double) to)
          return;
        to = UnityEngine.Random.Range(min, max);
        time = UnityEngine.Random.Range(timeMin, timeMax);
      }
      else
      {
        cur = Mathf.SmoothDamp(cur, to, ref speed, time, 100f);
        if ((double) cur - (double) num > (double) to)
          return;
        to = UnityEngine.Random.Range(min, max);
        time = UnityEngine.Random.Range(timeMin, timeMax);
      }
    }

    private bool IsSiruActive()
    {
      if ((UnityEngine.Object) this.siru == (UnityEngine.Object) null)
        return false;
      return (double) this.siru.siruAmount > 0.0;
    }

    public IEnumerator BeroKiss()
    {
      this.kissing = true;
      this.voiceCtrl = UnityEngine.Object.FindObjectOfType<HVoiceCtrl>();
      this.curEyeValue = 100f;
      this.tangSpeed = UnityEngine.Random.Range(10f, 20f);
      while (this.kissFlag)
      {
        this.curKissValue += (float) ((double) Time.deltaTime * (double) this.tangSpeed * 5.0);
        this.curMouthValue += (float) ((double) Time.deltaTime * (double) this.tangSpeed * 10.0);
        this.curEyeValue -= (float) ((double) Time.deltaTime * (double) this.tangSpeed * 3.0);
        this.curMouthValue = Mathf.Clamp(this.curMouthValue, 0.0f, 100f);
        this.curKissValue = Mathf.Clamp(this.curKissValue, 0.0f, 100f);
        this.curEyeValue = Mathf.Clamp(this.curEyeValue, 0.0f, 100f);
        if ((double) this.curKissValue < 100.0 || (double) this.curMouthValue < 100.0)
          yield return (object) null;
        else
          break;
      }
      while (true)
      {
        while (!this.kissFlag)
        {
          if (this.IsSiruActive())
          {
            this.RandomMoveFloatTest(ref this.curEyeValue, ref this.toEyeValue, ref this.eyeOpenSpeed, 0.0f, 75f, ref this.eyeOpenTime, 1f, 1.2f);
            this.eyesOpenValue = this.curEyeValue / 100f;
            this.curMouthValue += (float) ((double) Time.deltaTime * (double) Mathf.Abs(this.tangSpeed) * 5.0 * 0.5);
            this.curMouthValue = Mathf.Clamp(this.curMouthValue, 0.0f, 100f);
            yield return (object) null;
          }
          else
          {
            Traverse.Create((object) this.hand0).Field("isKiss").SetValue((object) false);
            while (true)
            {
              float num = Mathf.Max(25f, Mathf.Abs(this.tangSpeed));
              this.curMouthValue -= (float) ((double) Time.deltaTime * (double) num * 2.0 * 0.5);
              this.curMouthValue = Mathf.Clamp(this.curMouthValue, 0.0f, 100f);
              this.curKissValue -= (float) ((double) Time.deltaTime * (double) num * 1.0);
              this.curKissValue = Mathf.Clamp(this.curKissValue, 0.0f, 100f);
              this.curEyeValue += (float) ((double) Time.deltaTime * (double) num * 1.0 * 0.5);
              this.curEyeValue = Mathf.Clamp(this.curEyeValue, 0.0f, 100f);
              this.eyesOpenValue = this.curEyeValue / 100f;
              if ((double) this.curMouthValue > 10.0 || (double) this.curKissValue > 10.0)
                yield return (object) null;
              else
                break;
            }
            this.tangBonePos = Vector3.zero;
            this.tangBoneRot = Quaternion.identity;
            yield return (object) null;
            this.kissing = false;
            yield break;
          }
        }
        this.RandomMoveFloatTest(ref this.npWeight, ref this.npWeightTo, ref this.npWeightSpeed, 0.0f, 1f, ref this.npWeightTime, 0.1f, 0.5f);
        this.RandomMoveFloatTest(ref this.npWeight2, ref this.npWeight2To, ref this.npWeightSpeed2, 0.0f, 1f, ref this.npWeightTime2, 0.1f, 0.5f);
        this.RandomMoveFloatTest(ref this.npWeight3, ref this.npWeight3To, ref this.npWeightSpeed3, 0.0f, 1f, ref this.npWeightTime3, 0.1f, 0.5f);
        this.RandomMoveFloat(ref this.nnKutiOpenWeight, ref this.nnKutiOpenWeightTo, 0.1f, 0.0f, 1f, ref this.nnKutiOpenWeightTime, 1f, 5f);
        this.RandomMoveFloatTest(ref this.tangBonePos.x, ref this.tangBonePosTarget.x, ref this.tangBonePosSpeed.x, -1f / 500f, 1f / 500f, ref this.tangBoneTime.x, 0.1f, 2f);
        this.RandomMoveFloatTest(ref this.tangBonePos.y, ref this.tangBonePosTarget.y, ref this.tangBonePosSpeed.y, -1f / 1000f, 1f / 1000f, ref this.tangBoneTime.y, 0.1f, 2f);
        this.RandomMoveFloatTest(ref this.tangBonePos.z, ref this.tangBonePosTarget.z, ref this.tangBonePosSpeed.z, -1f / 500f, 1f / 500f, ref this.tangBoneTime.z, 0.1f, 2f);
        this.RandomMoveFloatTest(ref this.tangBoneRot.y, ref this.tangBoneRotTarget.y, ref this.tangBoneRotSpeed.y, -5f, 5f, ref this.tangBoneTime.y, 0.1f, 2f);
        this.RandomMoveFloatTest(ref this.tangBoneRot.x, ref this.tangBoneRotTarget.x, ref this.tangBoneRotSpeed.x, -5f, 2.5f, ref this.tangBoneTime.x, 0.1f, 2f);
        this.RandomMoveFloatTest(ref this.tangBoneRot.z, ref this.tangBoneRotTarget.z, ref this.tangBoneRotSpeed.z, -3.5f, 3.5f, ref this.tangBoneTime.z, 0.1f, 2f);
        this.RandomMoveFloatTest(ref this.curMouthValue, ref this.toMouthValue, ref this.mouthSpeed, 97f, 100f, ref this.mouthOpenTime, 10f, 12f);
        this.RandomMoveFloatTest(ref this.curEyeValue, ref this.toEyeValue, ref this.eyeOpenSpeed, 0.0f, 75f, ref this.eyeOpenTime, 0.01f, 1.2f);
        this.eyesOpenValue = this.curEyeValue / 100f;
        this.RandomMoveFloatTest(ref this.curKissValue, ref this.toKissValue, ref this.tangSpeed, 25f, 100f, ref this.tangTime, 0.01f, 0.1f);
        yield return (object) null;
      }
    }

    public void OnDisable()
    {
      this.kissFlag = false;
      this.kissing = false;
    }

    public void DoKiss()
    {
      if ((UnityEngine.Object) this.voiceCtrl == (UnityEngine.Object) null)
        this.voiceCtrl = UnityEngine.Object.FindObjectOfType<HVoiceCtrl>();
      if (this.voiceCtrl.nowVoices[0].state == HVoiceCtrl.VoiceKind.voice)
        this.kissFlag = false;
      else if (!this.flags.lstHeroine[0].isKiss && !this.flags.lstHeroine[0].denial.kiss)
      {
        this.flags.AddNotKiss();
        if (this.flags.mode == HFlag.EMode.aibu)
          this.flags.voice.playVoices[0] = 103;
        this.kissFlag = false;
      }
      else
      {
        UnityEngine.Object.FindObjectOfType<VRHScene>();
        if ((UnityEngine.Object) this.hand0 == (UnityEngine.Object) null)
          this.hand0 = Traverse.Create((object) this.scene).Field("vrHands").GetValue<VRHandCtrl[]>()[0];
        if (this.flags.mode == HFlag.EMode.aibu)
        {
          this.flags.click = HFlag.ClickKind.mouth;
          Traverse.Create((object) this.hand0).Field("isKiss").SetValue((object) true);
          this.berocyuOverride = true;
          if (this.flags.lstHeroine[0].HExperience == SaveData.Heroine.HExperienceKind.初めて && this.firstKiss)
            this.berocyuOverride = true;
        }
        else
          this.berocyuOverride = true;
        this.flags.AddKiss();
        this.firstKiss = false;
        this.StartCoroutine(this.BeroKiss());
      }
    }

    private void OnGUI()
    {
      if (!this.gui)
        return;
      float width = 400f;
      if ((double) this.rectWin.width < 1.0)
        this.rectWin.Set((float) Screen.width - width, 0.0f, width, (float) Screen.height);
      this.rectWin = GUI.Window(832, this.rectWin, new GUI.WindowFunction(this.WinFunc), "Control Face");
      UnityEngine.Event current = UnityEngine.Event.current;
      if (current.button == 0 && current.isMouse && current.type == EventType.MouseDrag)
        this.cameraControl.enabled = false;
      else
        this.cameraControl.enabled = true;
    }

    private void WinFunc(int id)
    {
      float num = 5f;
      GUILayout.BeginArea(new Rect(0.0f + num, 0.0f + num, this.rectWin.width - 2f * num, this.rectWin.height - 2f * num));
      this.filterText = GUILayout.TextField(this.filterText);
      GUILayout.Label("HohoAka");
      this.hohoAka = GUILayout.HorizontalSlider(this.hohoAka, 0.0f, 100f);
      GUILayout.Label("Tear");
      this.tearLv = GUILayout.Toolbar(this.tearLv, new string[5]
      {
        "0",
        "1",
        "2",
        "3",
        "4"
      });
      this.bvsShow.Clear();
      foreach (Cyu.BlendValue bv in this.bvs)
      {
        if (this.filterText.Length <= 0 || bv.name.IndexOf(this.filterText) != -1)
        {
          bv.active = GUILayout.Toggle(bv.active, bv.name);
          bv.value = GUILayout.HorizontalSlider(bv.value, 0.0f, 100f, (GUILayoutOption[]) null);
          this.bvsShow.Add(bv);
        }
      }
      if (GUI.changed)
        this.moveAuto = false;
      this.moveAuto = GUILayout.Toggle(this.moveAuto, "Move auto");
      GUILayout.EndArea();
      GUI.DragWindow();
    }

    private void Update()
    {
      if ((UnityEngine.Object) this.voiceCtrl == (UnityEngine.Object) null)
      {
        this.voiceCtrl = UnityEngine.Object.FindObjectOfType<HVoiceCtrl>();
        if ((UnityEngine.Object) this.voiceCtrl == (UnityEngine.Object) null)
          return;
      }
      if ((UnityEngine.Object) this.flags == (UnityEngine.Object) null)
      {
        this.flags = UnityEngine.Object.FindObjectOfType<HFlag>();
        if ((UnityEngine.Object) this.flags == (UnityEngine.Object) null)
          return;
      }
      if (this.bvs.Count<Cyu.BlendValue>() == 0)
        this.ReloadBlendValues();
      if ((double) Vector3.Distance(this.myMouth.transform.position, this.tang.transform.position) < (this.flags.mode == HFlag.EMode.aibu ? (double) Config.kissDistanceAibu : (double) Config.kissDistance))
        this.Kiss(true);
      else
        this.Kiss(false);
      if (this.kissing || this.IsSiruActive())
      {
        this.voiceCtrl.isPrcoStop = true;
        Traverse.Create((object) this.voiceCtrl).Method("BreathProc", new System.Type[3]
        {
          typeof (AnimatorStateInfo),
          typeof (ChaControl),
          typeof (int)
        }, (object[]) null).GetValue<bool>((object) this.female.animBody.GetCurrentAnimatorStateInfo(0), (object) this.female, (object) 0);
      }
      else
        this.voiceCtrl.isPrcoStop = false;
      if (this.kissing)
      {
        if (this.flags.mode == HFlag.EMode.aibu)
          this.flags.SpeedUpClickAibu(1f * this.flags.rateDragSpeedUp, this.flags.speedMaxAibuBody, true);
        this.flags.DragStart();
        if (!Config.faceOverride)
          return;
        this.female.ChangeEyesBlinkFlag(false);
        this.female.eyesCtrl.ChangePtn(0, true);
        this.female.eyebrowCtrl.ChangePtn(0, true);
        this.female.ChangeEyesOpenMax(this.eyesOpenValue);
        this.female.ChangeEyebrowOpenMax(this.eyesOpenValue);
        this.female.eyesCtrl.OpenMin = 0.0f;
        this.female.eyebrowCtrl.OpenMin = 0.0f;
        float num = Vector3.Angle(this.female.objHead.transform.forward, this.kissEyeTarget.transform.position - this.female.objHead.transform.position);
        this.eyeLookChangeTimer -= Time.deltaTime;
        if ((double) this.eyeLookChangeTimer < 0.0 || (double) num > 45.0)
        {
          this.eyeLookChangeTimer = UnityEngine.Random.Range(10f, 20f);
          this.eyeLookX = UnityEngine.Random.Range(-70f, 70f);
          this.eyeLookY = UnityEngine.Random.Range(-45f, 45f);
        }
        this.kissEyeTarget.transform.RotateAround(this.kissEyeTarget.transform.parent.position, this.female.objHead.transform.up, (float) ((double) this.eyeLookX * (double) Time.deltaTime * 0.200000002980232));
        this.kissEyeTarget.transform.RotateAround(this.kissEyeTarget.transform.parent.position, this.female.objHead.transform.right, (float) ((double) this.eyeLookY * (double) Time.deltaTime * 0.200000002980232));
      }
      else
        this.female.ChangeEyesBlinkFlag(true);
    }

    public void SetBlendShapeWeight()
    {
      try
      {
        foreach (Cyu.BlendValue bv in this.bvs)
        {
          if (bv.active)
          {
            float num1 = 1f;
            float num2 = 1.5f;
            if (bv.name.IndexOf("name02_") >= 0)
              num1 = (float) ((double) num2 * (double) this.npWeight / ((double) this.npWeight + (double) this.npWeight2 + (double) this.npWeight3));
            else if (bv.name.IndexOf("pero_") >= 0)
              num1 = (float) ((double) num2 * (double) this.npWeight2 / ((double) this.npWeight + (double) this.npWeight2 + (double) this.npWeight3));
            else if (bv.name.IndexOf("name_") >= 0)
              num1 = (float) ((double) num2 * (double) this.npWeight3 / ((double) this.npWeight + (double) this.npWeight2 + (double) this.npWeight3));
            bv.value = this.curKissValue * num1;
            if (this.kissing && bv.name.IndexOf("kuti_face.f00_name02_op") >= 0)
              bv.value = this.curMouthValue * (1f - this.nnKutiOpenWeight);
            else if (this.kissing && bv.name.IndexOf("kuti_face.f00_name_op") >= 0)
              bv.value = this.curMouthValue * this.nnKutiOpenWeight;
            if (this.kissing && bv.name.IndexOf("kuti_ha.ha00_name02_op") >= 0)
              bv.value = this.curMouthValue * (1f - this.nnKutiOpenWeight);
            else if (this.kissing && bv.name.IndexOf("kuti_ha.ha00_name_op") >= 0)
              bv.value = this.curMouthValue * this.nnKutiOpenWeight;
            bv.renderer.SetBlendShapeWeight(bv.index, bv.value);
          }
          else if (bv.name.IndexOf("kuti") >= 0)
            bv.renderer.SetBlendShapeWeight(bv.index, 0.0f);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        UnityEngine.Object.Destroy((UnityEngine.Object) this);
      }
    }

    public void LateUpdateHook()
    {
      if (!this.kissing)
        return;
      if (Config.faceOverride)
      {
        this.SetBlendShapeWeight();
        Vector3 localPosition = this.tangRenderer.bones[0].transform.localPosition;
        this.tangRenderer.bones[0].transform.localPosition = new Vector3(localPosition.x, this.tangBonePos.y + this.initTangBonePos.y, localPosition.z);
        this.tangRenderer.bones[0].transform.localRotation = this.initTangBoneRot * Quaternion.Euler(this.tangBoneRot.x, this.tangBoneRot.y, this.tangBoneRot.z);
      }
      this.female.ChangeLookNeckPtn(1, 1f);
      this.female.neckLookCtrl.target = this.kissNeckTarget.transform;
      this.female.ChangeLookEyesPtn(1);
      this.female.eyeLookCtrl.target = this.kissEyeTarget.transform;
    }

    internal void ToggleClothesStateAll()
    {
      ++this.clothesState;
      if (this.clothesState > 2)
        this.clothesState = 0;
      this.female.SetClothesStateAll((byte) this.clothesState);
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
