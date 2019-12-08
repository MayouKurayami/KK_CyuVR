using BepInEx;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	[BepInPlugin("bero.cyu.cyuvr", "CyuVR", "0.0.4")]
	[BepInProcess("KoikatuVR.exe")]
	public class CyuLoaderVR : BaseUnityPlugin
	{
		public static List<ChaControl> lstFemale = new List<ChaControl>();
		private string animationName = "";
		private HFlag hFlag;

		private void HandleLog(string condition, string stackTrace, LogType type)
		{
			Console.WriteLine(condition + "\n" + stackTrace);
		}

		private void Awake()
		{
			Application.logMessageReceived += new Application.LogCallback(this.HandleLog);
			UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) new GameObject("BeroConfig").AddComponent<Config>());
			if (!Application.dataPath.EndsWith("KoikatuVR_Data"))
				return;
			try
			{
				HarmonyInstance.Create("bero.cyuvr").PatchAll(typeof (Hooks));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private void Update()
		{
			if ((UnityEngine.Object) this.hFlag == (UnityEngine.Object) null)
			{
				this.hFlag = UnityEngine.Object.FindObjectOfType<HFlag>();
				if ((UnityEngine.Object) this.hFlag == (UnityEngine.Object) null)
					return;
			}
			if (!(this.animationName != this.hFlag.nowAnimationInfo.nameAnimation))
				return;
			CyuLoaderVR.lstFemale.Clear();
			((IEnumerable<ChaControl>) UnityEngine.Object.FindObjectsOfType<ChaControl>()).ToList<ChaControl>().Where<ChaControl>((Func<ChaControl, bool>) (x => x.chaFile.parameter.sex == (byte) 1)).ToList<ChaControl>().ForEach((System.Action<ChaControl>) (x =>
			{
				Cyu component = x.GetComponent<Cyu>();
				if ((UnityEngine.Object) component != (UnityEngine.Object) null)
					UnityEngine.Object.Destroy((UnityEngine.Object) component);
				x.gameObject.AddComponent<Cyu>();
				CyuLoaderVR.lstFemale.Add(x);
			}));
			this.animationName = this.hFlag.nowAnimationInfo.nameAnimation;
			Console.WriteLine(this.animationName);
		}
	}
}
