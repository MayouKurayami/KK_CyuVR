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
			cameraControl = FindObjectOfType<CameraControl_Ver2>();
		}

		private void Start()
		{
		}

		public virtual void Show(bool show)
		{
			GetComponentsInChildren<Renderer>().ToList<Renderer>().ForEach(x => x.enabled = show);
		}

		private void SetColor(Color color)
		{
			GetComponentsInChildren<MeshRenderer>().ToList<MeshRenderer>().ForEach(x => x.materials.ToList<Material>().ForEach(y => y.color = color));
		}

		protected virtual void OnMouseDown()
		{
			cameraControlOff = true;
			cameraControl.enabled = false;
			dragging = true;
		}

		protected virtual void OnMouseUp()
		{
			cameraControlOff = false;
			cameraControl.enabled = true;
			dragging = false;
		}

		protected virtual void OnMouseDrag()
		{
			transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));
		}
	}
}
