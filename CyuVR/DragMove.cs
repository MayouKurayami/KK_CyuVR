using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	internal class DragMove : MonoBehaviour
	{
		public static readonly Color COLOR_GRAB = Color.black;
		public static readonly Color COLOR_HIGHLIGHT = Color.blue;
		public bool managible = true;
		private CameraControl_Ver2 cameraControl;
		private bool cameraControlOff;
		public bool dragging;
		protected Color colorOrg;
		public Color colorCreater;

		protected virtual void Awake()
		{
			this.cameraControl = UnityEngine.Object.FindObjectOfType<CameraControl_Ver2>();
		}

		private void Start()
		{
		}

		public virtual void Show(bool show)
		{
			((IEnumerable<Renderer>) this.GetComponentsInChildren<Renderer>()).ToList<Renderer>().ForEach((System.Action<Renderer>) (x => x.enabled = show));
		}

		private void SetColor(Color color)
		{
			((IEnumerable<MeshRenderer>) this.GetComponentsInChildren<MeshRenderer>()).ToList<MeshRenderer>().ForEach((System.Action<MeshRenderer>) (x => ((IEnumerable<Material>) x.materials).ToList<Material>().ForEach((System.Action<Material>) (y => y.color = color))));
		}

		protected virtual void OnMouseDown()
		{
			this.cameraControlOff = true;
			this.cameraControl.enabled = false;
			this.dragging = true;
		}

		protected virtual void OnMouseUp()
		{
			this.cameraControlOff = false;
			this.cameraControl.enabled = true;
			this.dragging = false;
		}

		protected virtual void OnMouseDrag()
		{
			this.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(this.transform.position).z));
		}
	}
}
