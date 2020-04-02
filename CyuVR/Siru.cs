using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	public class Siru : MonoBehaviour
	{
		private const float itoRemainTime = 10f;
		public int tangVertexIndex = 161;
		private List<Vector3> posList = new List<Vector3>();
		private const int itoMatIndex = 1;
		private const int siruMatIndex = 6;
		private List<Vector3>[] posListDelays = new List<Vector3>[5];
		private List<Material> orgMaterial = new List<Material>();
		private const float distanceReduction = 100f;
		private const float itoDistance = 0.025f;
		private const float itoBreakDistance = 0.07f;
		public Transform head;
		public Transform top;
		public Transform tail;
		private ParticleSystem particleSystem;
		private HFlag flags;
		public bool itoOn;
		public bool itoBreaking;
		private float itoTimer;
		public GameObject siruTarget;
		private LineRenderer ito;
		public SkinnedMeshRenderer tangRenderer;
		public ChaControl female;
		private LineTextureMode lineTextureMode;
		private ParticleSystemRenderer particleSystemRenderer;
		public float siruAmount;

		private void OnDestroy()
		{
			Destroy(particleSystem.gameObject);
			Destroy(head.gameObject);
			Destroy(tail.gameObject);
			Destroy(top.gameObject);
			Destroy(ito.gameObject);
		}

		private void Start()
		{
			particleSystem = new GameObject("BeroItoEffect").AddComponent<ParticleSystem>();
			particleSystemRenderer = particleSystem.GetOrAddComponent<ParticleSystemRenderer>();
			particleSystemRenderer.renderMode = ParticleSystemRenderMode.Billboard;
			((IEnumerable<ParticleSystem>)FindObjectsOfType<ParticleSystem>()).Where<ParticleSystem>(x => x.name.IndexOf("LiquidSiru") >= 0).FirstOrDefault<ParticleSystem>().GetComponentsInChildren<ParticleSystemRenderer>().ToList<ParticleSystemRenderer>().ForEach(x => orgMaterial.Add(x.material));
			((IEnumerable<ParticleSystem>)FindObjectsOfType<ParticleSystem>()).Where<ParticleSystem>(x => x.name.IndexOf("LiquidSio") >= 0).FirstOrDefault<ParticleSystem>().GetComponentsInChildren<ParticleSystemRenderer>().ToList<ParticleSystemRenderer>().ForEach(x => orgMaterial.Add(x.material));
			((IEnumerable<ParticleSystem>)FindObjectsOfType<ParticleSystem>()).Where<ParticleSystem>(x => x.name.IndexOf("LiquidToilet") >= 0).FirstOrDefault<ParticleSystem>().GetComponentsInChildren<ParticleSystemRenderer>().ToList<ParticleSystemRenderer>().ForEach(x => orgMaterial.Add(x.material));
			particleSystemRenderer.material = orgMaterial[0];
			ParticleSystem.MainModule main = particleSystem.main;
			main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);
			main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 4f);

			//Due to Unity limitation, "rateOverTime" cannot be called directly from particleSystem.emission, so create new temporary variable "em"
			var em = particleSystem.emission;
			em.rateOverTime = 2f;

			top = new GameObject("ItoTop").transform;
			tail = new GameObject("Itotail").transform;
			head = new GameObject("ItoHead").transform;
			top.gameObject.name = "ItoTop";
			tail.gameObject.name = "ItoTail";
			head.gameObject.name = "ItoHead";
			tail.position = female.objHead.transform.TransformPoint(0.0f, 0.0f, 1f);
			head.transform.localScale = Vector3.one * 0.005f;
			tail.transform.localScale = Vector3.one * 0.03f;
			ito = new GameObject("BeroIto").AddComponent<LineRenderer>();
			ParticleSystem.ShapeModule shape = particleSystem.shape;
			shape.shapeType = ParticleSystemShapeType.Cone;
			shape.angle = 10f;
			shape.radius = 0.5f;
			ito.numCapVertices = 40;
			ito.numCornerVertices = 32;
			ito.enabled = true;
			ito.useWorldSpace = true;
			ito.startWidth = 0.005f;
			ito.endWidth = 0.005f;
			flags = FindObjectOfType<HFlag>();
		}

		public void StartIto()
		{
			itoOn = true;
			ito.enabled = true;
		}

		public void BreakIto()
		{
			if (itoBreaking)
				return;
			itoBreaking = true;
			itoTimer = itoRemainTime;
		}

		private float GetVoiceValue()
		{
			if (female.asVoice && female.asVoice.isPlaying && female.wavInfoData != null)
				return female.wavInfoData.GetValue(female.asVoice.time);
			return 0f;
		}

		private IEnumerator ItoBreaking()
		{
			itoBreaking = true;
			ito.enabled = false;
			itoOn = false;
			itoBreaking = false;
			yield return null;
		}

		private void UpdateWidthCurve()
		{
			float num = Vector3.SqrMagnitude(head.transform.position - tail.transform.position);
			float num2 = num / 2f;
			AnimationCurve animationCurve = new AnimationCurve();
			float num3 = 0f;
			do
			{
				float num4 = Mathf.Lerp(0f, num, num3);
				float num5 = Mathf.Abs(num2 - num4);
				if (itoBreaking && num2 - num4 < 0f)
				{
					num5 = 0f;
				}
				animationCurve.AddKey(num3, siruAmount * num5 / (num * distanceReduction));
				num3 += 0.01f;
			}
			while (!(num3 > 1f));
			ito.widthCurve = animationCurve;
		}

		private void Update()
		{
			ito.material = orgMaterial[itoMatIndex];
			particleSystemRenderer.material = orgMaterial[siruMatIndex];
			ito.textureMode = lineTextureMode;

			//Due to Unity limitation, "startColor" cannot be called directly from particleSystem.main, so create new temporary variable "mn"
			var mn = particleSystem.main;
			mn.startColor = flags.gaugeFemale < 70.0 ? new Color(1f, 1f, 1f, 1f) : (ParticleSystem.MinMaxGradient)new Color(1f, 0.62f, 0.85f);
			//Due to Unity limitation, "rateOverTime" cannot be called directly from particleSystem.emission, so create new temporary variable "em"
			var em = particleSystem.emission;
			em.rateOverTime = 2f * GetVoiceValue();

			if (Vector3.SqrMagnitude(tail.transform.position - head.transform.position) < itoDistance && female.GetComponent<Cyu>().kissing)
				siruAmount += Time.deltaTime * 0.05f;
			else if (itoBreaking)
				siruAmount -= Time.deltaTime * 0.5f;
			else
				siruAmount -= Time.deltaTime * 0.05f;
			if (siruAmount > 0f && Vector3.SqrMagnitude(tail.transform.position - head.transform.position) > itoBreakDistance)
				BreakIto();
			siruAmount = Mathf.Clamp(siruAmount, 0.0f, 1f);
			ref Vector3 local = ref tangRenderer.sharedMesh.vertices[tangVertexIndex];
			Mesh mesh = new Mesh();
			tangRenderer.BakeMesh(mesh);
			Vector3 vector3 = mesh.vertices[tangVertexIndex];
			vector3 = new Vector3(vector3.x, vector3.y, vector3.z);
			head.position = tangRenderer.transform.position + tangRenderer.transform.rotation * vector3;
			tail.position = siruTarget.transform.position;
			if (itoBreaking)
			{
				itoTimer -= Time.deltaTime;
				int index = posList.Count - 2;
				if (index > 0)
				{
					tail.position = posList[index];
					if (itoTimer < 0f || siruAmount <= 0f)
					{
						itoBreaking = false;
						siruAmount = 0.0f;
					}
				}
				float y = top.position.y;
				top.position = Vector3.Lerp(head.position, tail.position, 10f * Time.deltaTime);
				top.position = new Vector3(top.position.x, y - Time.deltaTime * 0.3f, top.position.z);
			}
			else
			{
				top.position = Vector3.Lerp(head.position, tail.position, 10f * Time.deltaTime) - new Vector3(0.0f, 0.5f * Vector3.SqrMagnitude(head.position - tail.position), 0.0f);
			}

			OnLineDraw();
		}

		public static Vector3 BezierCurve(Vector3 pt1, Vector3 pt2, Vector3 ctrlPt, float t)
		{
			if (t > 1f)
			{
				t = 1f;
			}
			Vector3 result = default(Vector3);
			float num = 1f - t;
			result.x = num * num * pt1.x + 2f * num * t * ctrlPt.x + t * t * pt2.x;
			result.y = num * num * pt1.y + 2f * num * t * ctrlPt.y + t * t * pt2.y;
			result.z = num * num * pt1.z + 2f * num * t * ctrlPt.z + t * t * pt2.z;
			return result;
		}

		public void OnLineDraw()
		{
			if (posListDelays[Time.frameCount % 5] == null)
				posListDelays[Time.frameCount % 5] = new List<Vector3>();

			posList.Clear();
			posListDelays[Time.frameCount % 5].Clear();
			posList.Add(head.position);
			posListDelays[Time.frameCount % 5].Add(head.position);
			float t = 0.0f;
			int index = 1;
			while (t < 1.0)
			{
				t += 0.03f;
				Vector3 a = BezierCurve(head.position, tail.position, top.position, t);
				if (posListDelays.Length == 5 && posListDelays[(Time.frameCount + 1) % 5] != null && posListDelays[(Time.frameCount + 1) % 5].Count > index)
				{
					Vector3 vector3 = Vector3.Lerp(a, posListDelays[(Time.frameCount + 1) % 5][index], Mathf.Abs(0.5f - t));
					posList.Add(vector3);
					posListDelays[Time.frameCount % 5].Add(vector3);
				}
				else
				{
					posList.Add(a);
					posListDelays[Time.frameCount % 5].Add(a);
				}
				++index;
			}
			ito.positionCount = posList.Count;
			ito.SetPositions(posList.ToArray());
			UpdateWidthCurve();
		}
	}
}
