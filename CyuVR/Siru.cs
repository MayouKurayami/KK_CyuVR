// Decompiled with JetBrains decompiler
// Type: Bero.CyuVR.Siru
// Assembly: CyuVR, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E19B191-388D-44D8-A057-D416187C1E85
// Assembly location: C:\Users\MK\Desktop\KK\CyuVR\Source Dll\CyuVR.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	public class Siru : MonoBehaviour
	{
		private float itoRemainTime = 10f;
		private float itoStartTime = 10f;
		public int tangVertexIndex = 161;
		private List<Vector3> posList = new List<Vector3>();
		private int itoMatIndex = 1;
		private int siruMatIndex = 6;
		private List<Vector3>[] posListDelays = new List<Vector3>[5];
		private List<Material> orgMaterial = new List<Material>();
		private float distanceReduction = 100f;
		private float itoDistance = 0.025f;
		private float itoBreakDistance = 0.07f;
		public float t;
		public Transform head;
		public Transform top;
		public Transform tail;
		private ParticleSystem particleSystem;
		private HFlag flags;
		private GameObject itoTop;
		private GameObject itoTail;
		private GameObject itoHead;
		public bool itoOn;
		private ParticleSystem subEmitter;
		public bool itoBreaking;
		private float itoTimer;
		private float itoWaitTimer;
		private const int delay = 5;
		public GameObject siruTarget;
		private LineRenderer ito;
		public SkinnedMeshRenderer tangRenderer;
		public ChaControl female;
		private LineTextureMode lineTextureMode;
		private GameObject pTang;
		private ParticleSystemRenderer particleSystemRenderer;
		public float siruAmount;

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy((UnityEngine.Object) this.particleSystem.gameObject);
			UnityEngine.Object.Destroy((UnityEngine.Object) this.head.gameObject);
			UnityEngine.Object.Destroy((UnityEngine.Object) this.tail.gameObject);
			UnityEngine.Object.Destroy((UnityEngine.Object) this.top.gameObject);
			UnityEngine.Object.Destroy((UnityEngine.Object) this.ito.gameObject);
		}

		private void Start()
		{
			this.particleSystem = new GameObject("BeroItoEffect").AddComponent<ParticleSystem>();
			this.particleSystemRenderer = this.particleSystem.GetOrAddComponent<ParticleSystemRenderer>();
			this.particleSystemRenderer.renderMode = ParticleSystemRenderMode.Billboard;
			((IEnumerable<ParticleSystemRenderer>) ((IEnumerable<ParticleSystem>) UnityEngine.Object.FindObjectsOfType<ParticleSystem>()).Where<ParticleSystem>((Func<ParticleSystem, bool>) (x => x.name.IndexOf("LiquidSiru") >= 0)).FirstOrDefault<ParticleSystem>().GetComponentsInChildren<ParticleSystemRenderer>()).ToList<ParticleSystemRenderer>().ForEach((System.Action<ParticleSystemRenderer>) (x => this.orgMaterial.Add(x.material)));
			((IEnumerable<ParticleSystemRenderer>) ((IEnumerable<ParticleSystem>) UnityEngine.Object.FindObjectsOfType<ParticleSystem>()).Where<ParticleSystem>((Func<ParticleSystem, bool>) (x => x.name.IndexOf("LiquidSio") >= 0)).FirstOrDefault<ParticleSystem>().GetComponentsInChildren<ParticleSystemRenderer>()).ToList<ParticleSystemRenderer>().ForEach((System.Action<ParticleSystemRenderer>) (x => this.orgMaterial.Add(x.material)));
			((IEnumerable<ParticleSystemRenderer>) ((IEnumerable<ParticleSystem>) UnityEngine.Object.FindObjectsOfType<ParticleSystem>()).Where<ParticleSystem>((Func<ParticleSystem, bool>) (x => x.name.IndexOf("LiquidToilet") >= 0)).FirstOrDefault<ParticleSystem>().GetComponentsInChildren<ParticleSystemRenderer>()).ToList<ParticleSystemRenderer>().ForEach((System.Action<ParticleSystemRenderer>) (x => this.orgMaterial.Add(x.material)));
			this.particleSystemRenderer.material = this.orgMaterial[0];
			ParticleSystem.MainModule main = this.particleSystem.main;
			main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);
			main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 4f);

			//Due to Unity limitation, "rateOverTime" cannot be called directly from particleSystem.emission, so create new temporary variable "em"
			var em = this.particleSystem.emission.rateOverTime;
			em = (ParticleSystem.MinMaxCurve) 2f;

			this.top = new GameObject("ItoTop").transform;
			this.tail = new GameObject("Itotail").transform;
			this.head = new GameObject("ItoHead").transform;
			this.top.gameObject.name = "ItoTop";
			this.tail.gameObject.name = "ItoTail";
			this.head.gameObject.name = "ItoHead";
			this.tail.position = this.female.objHead.transform.TransformPoint(0.0f, 0.0f, 1f);
			this.head.transform.localScale = Vector3.one * 0.005f;
			this.tail.transform.localScale = Vector3.one * 0.03f;
			this.ito = new GameObject("BeroIto").AddComponent<LineRenderer>();
			ParticleSystem.ShapeModule shape = this.particleSystem.shape;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.angle = 10f;
			shape.radius = 0.5f;
			this.ito.numCapVertices = 40;
			this.ito.numCornerVertices = 32;
			this.ito.enabled = true;
			this.ito.useWorldSpace = true;
			this.ito.startWidth = 0.005f;
			this.ito.endWidth = 0.005f;
			this.flags = UnityEngine.Object.FindObjectOfType<HFlag>();
			this.itoWaitTimer = this.itoStartTime;
		}

		public void StartIto()
		{
			this.itoOn = true;
			this.ito.enabled = true;
		}

		public void BreakIto()
		{
			if (this.itoBreaking)
				return;
			this.itoBreaking = true;
			this.itoTimer = this.itoRemainTime;
		}

		private float GetVoiceValue()
		{
			if ((bool) ((UnityEngine.Object) this.female.asVoice) && this.female.asVoice.isPlaying && this.female.wavInfoData != null)
				return this.female.wavInfoData.GetValue(this.female.asVoice.time);
			return 0.0f;
		}

		private IEnumerator ItoBreaking()
		{
			this.itoBreaking = true;
			this.ito.enabled = false;
			this.itoOn = false;
			this.itoBreaking = false;
			yield return (object) null;
		}

		private void UpdateWidthCurve()
		{
			float b = Vector3.SqrMagnitude(this.head.transform.position - this.tail.transform.position);
			float num1 = b / 2f;
			AnimationCurve animationCurve = new AnimationCurve();
			float num2 = 0.0f;
			do
			{
				float num3 = Mathf.Lerp(0.0f, b, num2);
				float num4 = Mathf.Abs(num1 - num3);
				if (this.itoBreaking && (double) num1 - (double) num3 < 0.0)
					num4 = 0.0f;
				animationCurve.AddKey(num2, (float) ((double) this.siruAmount * (double) num4 / ((double) b * (double) this.distanceReduction)));
				num2 += 0.01f;
			}
			while ((double) num2 <= 1.0);
			this.ito.widthCurve = animationCurve;
		}

		private void Update()
		{
			this.ito.material = this.orgMaterial[this.itoMatIndex];
			this.particleSystemRenderer.material = this.orgMaterial[this.siruMatIndex];
			this.ito.textureMode = this.lineTextureMode;

			//Due to Unity limitation, "startColor" cannot be called directly from particleSystem.main, so create new temporary variable "mn"
			var mn = this.particleSystem.main;
			mn.startColor = (double) this.flags.gaugeFemale < 70.0 ? (ParticleSystem.MinMaxGradient) new Color(1f, 1f, 1f, 1f) : (ParticleSystem.MinMaxGradient) new Color(1f, 0.62f, 0.85f);
			//Due to Unity limitation, "rateOverTime" cannot be called directly from particleSystem.emission, so create new temporary variable "em"
			var em = this.particleSystem.emission;
			em.rateOverTime = (ParticleSystem.MinMaxCurve) (2f * this.GetVoiceValue());

			if ((double) Vector3.SqrMagnitude(this.tail.transform.position - this.head.transform.position) < (double) this.itoDistance && this.female.GetComponent<Cyu>().kissing)
				this.siruAmount += Time.deltaTime * 0.05f;
			else if (this.itoBreaking)
				this.siruAmount -= Time.deltaTime * 0.5f;
			else
				this.siruAmount -= Time.deltaTime * 0.05f;
			if ((double) this.siruAmount > 0.0 && (double) Vector3.SqrMagnitude(this.tail.transform.position - this.head.transform.position) > (double) this.itoBreakDistance)
				this.BreakIto();
			this.siruAmount = Mathf.Clamp(this.siruAmount, 0.0f, 1f);
			ref Vector3 local = ref this.tangRenderer.sharedMesh.vertices[this.tangVertexIndex];
			Mesh mesh = new Mesh();
			this.tangRenderer.BakeMesh(mesh);
			Vector3 vector3 = mesh.vertices[this.tangVertexIndex];
			vector3 = new Vector3(vector3.x, vector3.y, vector3.z);
			this.head.position = this.tangRenderer.transform.position + this.tangRenderer.transform.rotation * vector3;
			this.tail.position = this.siruTarget.transform.position;
			if (this.itoBreaking)
			{
				this.itoTimer -= Time.deltaTime;
				int index = this.posList.Count - 2;
				if (index > 0)
				{
					this.tail.position = this.posList[index];
					if ((double) this.itoTimer < 0.0 || (double) this.siruAmount <= 0.0)
					{
						this.itoBreaking = false;
						this.siruAmount = 0.0f;
					}
				}
				float y = this.top.position.y;
				this.top.position = Vector3.Lerp(this.head.position, this.tail.position, 10f * Time.deltaTime);
				this.top.position = new Vector3(this.top.position.x, y - Time.deltaTime * 0.3f, this.top.position.z);
			}
			else
				this.top.position = Vector3.Lerp(this.head.position, this.tail.position, 10f * Time.deltaTime) - new Vector3(0.0f, 0.5f * Vector3.SqrMagnitude(this.head.position - this.tail.position), 0.0f);
			this.OnLineDraw();
		}

		public static Vector3 BezierCurve(Vector3 pt1, Vector3 pt2, Vector3 ctrlPt, float t)
		{
			if ((double) t > 1.0)
				t = 1f;
			Vector3 vector3 = new Vector3();
			float num = 1f - t;
			vector3.x = (float) ((double) num * (double) num * (double) pt1.x + 2.0 * (double) num * (double) t * (double) ctrlPt.x + (double) t * (double) t * (double) pt2.x);
			vector3.y = (float) ((double) num * (double) num * (double) pt1.y + 2.0 * (double) num * (double) t * (double) ctrlPt.y + (double) t * (double) t * (double) pt2.y);
			vector3.z = (float) ((double) num * (double) num * (double) pt1.z + 2.0 * (double) num * (double) t * (double) ctrlPt.z + (double) t * (double) t * (double) pt2.z);
			return vector3;
		}

		public void OnLineDraw()
		{
			if (this.posListDelays[Time.frameCount % 5] == null)
				this.posListDelays[Time.frameCount % 5] = new List<Vector3>();
			this.posList.Clear();
			this.posListDelays[Time.frameCount % 5].Clear();
			this.posList.Add(this.head.position);
			this.posListDelays[Time.frameCount % 5].Add(this.head.position);
			float t = 0.0f;
			int index = 1;
			while ((double) t < 1.0)
			{
				t += 0.03f;
				Vector3 a = Siru.BezierCurve(this.head.position, this.tail.position, this.top.position, t);
				if (this.posListDelays.Length == 5 && this.posListDelays[(Time.frameCount + 1) % 5] != null && this.posListDelays[(Time.frameCount + 1) % 5].Count > index)
				{
					Vector3 vector3 = Vector3.Lerp(a, this.posListDelays[(Time.frameCount + 1) % 5][index], Mathf.Abs(0.5f - t));
					this.posList.Add(vector3);
					this.posListDelays[Time.frameCount % 5].Add(vector3);
				}
				else
				{
					this.posList.Add(a);
					this.posListDelays[Time.frameCount % 5].Add(a);
				}
				++index;
			}
			this.ito.positionCount = this.posList.Count;
			this.ito.SetPositions(this.posList.ToArray());
			this.UpdateWidthCurve();
		}
	}
}
