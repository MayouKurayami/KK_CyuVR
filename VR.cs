using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero
{
	public class VR
	{
		private static SteamVR_Camera steamvr_camera;
		private static SteamVR_Controller.Device rightController;
		private static SteamVR_TrackedObject rightTrackedObject;
		private static SteamVR_Controller.Device leftController;
		private static SteamVR_TrackedObject leftTrackedObject;

		public static bool Active
		{
			get
			{
				return true;
			}
		}

		public static Camera Hmd
		{
			get
			{
				if ((UnityEngine.Object) VR.steamvr_camera == (UnityEngine.Object) null)
				{
					VR.steamvr_camera = UnityEngine.Object.FindObjectOfType<SteamVR_Camera>();
					if ((UnityEngine.Object) VR.steamvr_camera == (UnityEngine.Object) null)
						return (Camera) null;
				}
				return VR.steamvr_camera.GetComponent<Camera>();
			}
		}

		public static SteamVR_Controller.Device RightController
		{
			get
			{
				if (VR.rightController == null)
				{
					SteamVR_TrackedObject rightTrackedObject = VR.RightTrackedObject;
					if ((UnityEngine.Object) rightTrackedObject != (UnityEngine.Object) null)
						VR.rightController = SteamVR_Controller.Input((int) rightTrackedObject.index);
				}
				return VR.rightController;
			}
		}

		public static SteamVR_TrackedObject RightTrackedObject
		{
			get
			{
				if ((UnityEngine.Object) VR.rightTrackedObject == (UnityEngine.Object) null)
					VR.rightTrackedObject = ((IEnumerable<SteamVR_TrackedObject>) UnityEngine.Object.FindObjectsOfType<SteamVR_TrackedObject>()).ToList<SteamVR_TrackedObject>().Where<SteamVR_TrackedObject>((Func<SteamVR_TrackedObject, bool>) (x => x.name.IndexOf("right") >= 0)).FirstOrDefault<SteamVR_TrackedObject>();
				return VR.rightTrackedObject;
			}
		}

		public static SteamVR_Controller.Device LeftController
		{
			get
			{
				if (VR.leftController == null)
				{
					SteamVR_TrackedObject leftTrackedObject = VR.LeftTrackedObject;
					if ((UnityEngine.Object) leftTrackedObject != (UnityEngine.Object) null)
						VR.leftController = SteamVR_Controller.Input((int) leftTrackedObject.index);
				}
				return VR.leftController;
			}
		}

		public static SteamVR_TrackedObject LeftTrackedObject
		{
			get
			{
				if ((UnityEngine.Object) VR.leftTrackedObject == (UnityEngine.Object) null)
					VR.leftTrackedObject = ((IEnumerable<SteamVR_TrackedObject>) UnityEngine.Object.FindObjectsOfType<SteamVR_TrackedObject>()).ToList<SteamVR_TrackedObject>().Where<SteamVR_TrackedObject>((Func<SteamVR_TrackedObject, bool>) (x => x.name.IndexOf("left") >= 0)).FirstOrDefault<SteamVR_TrackedObject>();
				return VR.leftTrackedObject;
			}
		}
	}
}
