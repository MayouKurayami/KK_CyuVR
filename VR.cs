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
				if (steamvr_camera == null)
				{
					steamvr_camera = Object.FindObjectOfType<SteamVR_Camera>();
					if (steamvr_camera == null)
						return null;
				}
				return steamvr_camera.GetComponent<Camera>();
			}
		}

		public static SteamVR_Controller.Device RightController
		{
			get
			{
				if (rightController == null)
				{
					SteamVR_TrackedObject rightTrackedObject = RightTrackedObject;
					if (rightTrackedObject != null)
						rightController = SteamVR_Controller.Input((int)rightTrackedObject.index);
				}
				return rightController;
			}
		}

		public static SteamVR_TrackedObject RightTrackedObject
		{
			get
			{
				if (rightTrackedObject == null)
					rightTrackedObject = Object.FindObjectsOfType<SteamVR_TrackedObject>().ToList<SteamVR_TrackedObject>().Where<SteamVR_TrackedObject>(x => x.name.IndexOf("right") >= 0).FirstOrDefault<SteamVR_TrackedObject>();
				return rightTrackedObject;
			}
		}

		public static SteamVR_Controller.Device LeftController
		{
			get
			{
				if (leftController == null)
				{
					SteamVR_TrackedObject leftTrackedObject = LeftTrackedObject;
					if (leftTrackedObject != null)
						leftController = SteamVR_Controller.Input((int)leftTrackedObject.index);
				}
				return leftController;
			}
		}

		public static SteamVR_TrackedObject LeftTrackedObject
		{
			get
			{
				if (leftTrackedObject == null)
					leftTrackedObject = Object.FindObjectsOfType<SteamVR_TrackedObject>().ToList<SteamVR_TrackedObject>().Where<SteamVR_TrackedObject>(x => x.name.IndexOf("left") >= 0).FirstOrDefault<SteamVR_TrackedObject>();
				return leftTrackedObject;
			}
		}
	}
}
