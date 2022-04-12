using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using BepInEx.Bootstrap;
using Studio;
using ToolBox.Extensions;

namespace KK_AdditionalFKNodes
{
    [BepInPlugin(GUID, PluginName, VERSION)]
    [BepInDependency("com.deathweasel.bepinex.uncensorselector", "3.11.1")]
    [BepInProcess("CharaStudio")]

    public class KK_AdditionalFKNodes : BaseUnityPlugin
    {
        internal const string GUID = "com.animal42069.additionalfknodes";
        internal const string PluginName = "KK Additional FK Nodes";
        internal const string VERSION = "1.2.1.0";

        private static Type _uncensorSelectorType;

        internal void Main()
        {
            UnityEngine.Debug.Log("AdditionalFKNodes: Trying to patch methods...");

            var harmony = new Harmony(GetType().Name);
            harmony.PatchAll(GetType());

            Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.uncensorselector", out PluginInfo info);
            if (info == null || info.Instance == null)
                return;

            _uncensorSelectorType = info.Instance.GetType();
            Type uncensorSelectorControllerType = _uncensorSelectorType.GetNestedType("UncensorSelectorController", AccessTools.all);
            if (uncensorSelectorControllerType == null)
                return;

            UnityEngine.Debug.Log("AdditionalFKNodes: UncensorSelector found, trying to patch");
            MethodInfo uncensorSelectorReloadCharacterBody = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterBody");
            if (uncensorSelectorReloadCharacterBody != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterBody, postfix: new HarmonyMethod(GetType(), nameof(AddAdditionalBodyNodes)));
                UnityEngine.Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterBody correctly");
            }

            MethodInfo uncensorSelectorReloadCharacterPenis = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterPenis");
            if (uncensorSelectorReloadCharacterPenis != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterPenis, postfix: new HarmonyMethod(GetType(), nameof(AddAdditionalPenisNodes)));
                UnityEngine.Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterPenis correctly");
            }

            MethodInfo uncensorSelectorReloadCharacterBalls = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterBalls");
            if (uncensorSelectorReloadCharacterBalls != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterBalls, postfix: new HarmonyMethod(GetType(), nameof(AddAdditionalBallNodes)));
                UnityEngine.Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterBalls correctly");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(FKCtrl), "InitBones")]
        public static void FKCtrlInitBones(FKCtrl __instance, OICharInfo _info, ChaReference _charReference)
        {
            AdditionalFKNodes.AdditionalFKNodes.AddFKCtrlInfo(__instance, _info, __instance.transform.gameObject.GetComponent<ChaControl>(), _charReference);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AddObjectAssist), "InitBone")]
        public static void AddObjectAssistInitBone(OCIChar _ociChar)
        {
            AdditionalFKNodes.AdditionalFKNodes.AddFKCtrlInfo(_ociChar);
        }
		
        [HarmonyPostfix, HarmonyPatch(typeof(OCIChar), "ActiveFK")]
        internal static void OCICharActiveFK(OCIChar __instance, OIBoneInfo.BoneGroup _group, bool _active)
        {
            if (_active)
                return;

            AdditionalFKNodes.AdditionalFKNodes.ResetFKNodes(__instance.charInfo, _group);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(FKCtrl), "CopyBone", typeof(OIBoneInfo.BoneGroup))]
        internal static void TargetInfoCopyBone(FKCtrl __instance, OIBoneInfo.BoneGroup _target)
        {
            AdditionalFKNodes.AdditionalFKNodes.ResetFKNodes(__instance.m_Transform.gameObject.GetComponent<ChaControl>(), _target);

            ChaControl chaControl = __instance.m_Transform.gameObject.GetComponent<ChaControl>();
            if (chaControl == null)
                return;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GuideObject), "LateUpdate")]
        internal static bool GuideObjectLateUpdate(GuideObject __instance)
        {
            if (__instance.transformTarget == null)
                return false;

            return true;
        }

        internal static void AddAdditionalBodyNodes(object __instance)
        {
            ChaControl chaControl = (ChaControl)__instance.GetPrivateProperty("ChaControl");
            if (chaControl == null)
                return;

            string assetBundleName;
            string assetName;

            object bodyData = __instance.GetPrivateProperty("BodyData");
            if (bodyData != null)
            {
                assetBundleName = (string)bodyData.GetPrivate("OOBase");
                assetName = (string)bodyData.GetPrivate("Asset");
            }
            else
            {
                assetBundleName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("OOBase");
                if (chaControl.sex == 0)
                    assetName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("AssetMale");
                else
                    assetName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("AssetFemale");
            }

            AdditionalFKNodes.AdditionalFKNodes.AddAdditionalNodes(chaControl, assetBundleName, assetName);
        }

        internal static void AddAdditionalPenisNodes(object __instance)
        {
            ChaControl chaControl = (ChaControl)__instance.GetPrivateProperty("ChaControl");
            if (chaControl == null)
                return;

            string assetBundleName;
            string assetName;
            object penisData = __instance.GetPrivateProperty("PenisData");
            if (penisData != null)
            {
                assetBundleName = (string)penisData.GetPrivate("File");
                assetName = (string)penisData.GetPrivate("Asset");
            }
            else
            {
                assetBundleName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("File");
                assetName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("Asset");
            }

            AdditionalFKNodes.AdditionalFKNodes.AddAdditionalNodes(chaControl, assetBundleName, assetName);
        }

        internal static void AddAdditionalBallNodes(object __instance)
        {
            ChaControl chaControl = (ChaControl)__instance.GetPrivateProperty("ChaControl");
            if (chaControl == null)
                return;

            string assetBundleName;
            string assetName;
            object ballsData = __instance.GetPrivateProperty("BallsData");
            if (ballsData != null)
            {
                assetBundleName = (string)ballsData.GetPrivate("File");
                assetName = (string)ballsData.GetPrivate("Asset");
            }
            else
            {
                assetBundleName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("File");
                assetName = (string)_uncensorSelectorType.GetNestedType("Defaults", BindingFlags.Public | BindingFlags.Static).GetPrivate("Asset");
            }

            AdditionalFKNodes.AdditionalFKNodes.AddAdditionalNodes(chaControl, assetBundleName, assetName);
        }
    }
}
