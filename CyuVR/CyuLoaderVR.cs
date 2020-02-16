using BepInEx;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bero.CyuVR
{
	[BepInPlugin("bero.cyu.cyuvr", "CyuVR", Version)]
	[BepInProcess("KoikatuVR")]
	[BepInProcess("Koikatsu Party VR")]
	public class CyuLoaderVR : BaseUnityPlugin
	{
		public const string Version = "0.1.1";
		public static List<ChaControl> lstFemale = new List<ChaControl>();
		private string animationName = "";
		private HFlag hFlag;

		private void HandleLog(string condition, string stackTrace, LogType type)
		{
			Console.WriteLine(condition + "\n" + stackTrace);
		}

		private void Awake()
		{
			Application.logMessageReceived += new Application.LogCallback(HandleLog);
			DontDestroyOnLoad(new GameObject("BeroConfig").AddComponent<Config>());
			try
			{
				HarmonyInstance.Create("bero.cyuvr").PatchAll(typeof(Hooks));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private void Update()
		{
			if (hFlag == null)
			{
				hFlag = FindObjectOfType<HFlag>();
				if (hFlag == null)
					return;
			}
			if (!(animationName != hFlag.nowAnimationInfo.nameAnimation))
				return;
			lstFemale.Clear();
			FindObjectsOfType<ChaControl>().ToList<ChaControl>().Where<ChaControl>(x => x.chaFile.parameter.sex == 1).ToList<ChaControl>().ForEach(x =>
		  {
			  Cyu component = x.GetComponent<Cyu>();
			  if (component != null)
				  Destroy(component);
			  x.gameObject.AddComponent<Cyu>();
			  lstFemale.Add(x);
		  });
			animationName = hFlag.nowAnimationInfo.nameAnimation;
			Console.WriteLine(animationName);
		}
	}
}
