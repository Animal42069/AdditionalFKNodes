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
        internal const string VERSION = "1.2.0.0";

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
                harmony.Patch(uncensorSelectorReloadCharacterBody, postfix: new HarmonyMethod(GetType(), nameof(AdditionalFKNodes_AddBodyNodes)));
                UnityEngine.Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterBody correctly");
            }

            MethodInfo uncensorSelectorReloadCharacterPenis = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterPenis");
            if (uncensorSelectorReloadCharacterPenis != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterPenis, postfix: new HarmonyMethod(GetType(), nameof(AdditionalFKNodes_AddPenisNodes)));
                UnityEngine.Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterPenis correctly");
            }

            MethodInfo uncensorSelectorReloadCharacterBalls = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterBalls");
            if (uncensorSelectorReloadCharacterBalls != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterBalls, postfix: new HarmonyMethod(GetType(), nameof(AdditionalFKNodes_AddBallNodes)));
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

        [HarmonyPrefix, HarmonyPatch(typeof(GuideObject), "LateUpdate")]
        internal static bool GuideObjectLateUpdate(GuideObject __instance)
        {
            if (__instance.transformTarget == null)
                return false;

            return true;
        }

        private static void AdditionalFKNodes_AddBodyNodes(object __instance)
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

        private static void AdditionalFKNodes_AddPenisNodes(object __instance)
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

        private static void AdditionalFKNodes_AddBallNodes(object __instance)
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
        /*
                private static void AdditionalFKNodes_AddNodes(ChaControl chaControl, string assetBundleName, string assetName)
                {
                    TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(assetBundleName, "additional_fknodes", true);
                    if (textAsset == null)
                        return;

                    UnityEngine.Debug.Log("AdditionalFKNodes: Loaded additional_fknodes TextAsset from " + assetBundleName);

                    if (!fkCtrlDictionary.TryGetValue(chaControl.name, out FKCtrlInfo fKCtrlInfo))
                        return;

                    var boneInfoDictionary = Singleton<Info>.Instance.dicBoneInfo;
                    if (boneInfoDictionary == null)
                        return;

                    var newInfoBoneInfoList = new List<Info.BoneInfo>();

                    string[] lines = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        string[] cells = line.Split('\t');
                        if (cells.Length < 5)
                            continue;
                        UnityEngine.Debug.Log("AdditionalFKNodes: Found matching line for asset " + assetName + "\n" + line);

                        int boneInfoKey = int.Parse(cells[0]);
                        if (!boneInfoDictionary.TryGetValue(boneInfoKey, out var infoBoneInfo))
                        {
                            infoBoneInfo = new Info.BoneInfo
                            {
                                no = boneInfoKey,
                                bone = cells[1],
                                name = cells[2],
                                group = int.Parse(cells[3]),
                                level = int.Parse(cells[4])
                            };
                            boneInfoDictionary.Add(boneInfoKey, infoBoneInfo);
                        }

                        newInfoBoneInfoList.Add(infoBoneInfo);
                    }

                    foreach (var newInfoBoneInfo in newInfoBoneInfoList)
                    {
                        GameObject gameObject = null;
                        IEnumerable<Transform> transforms;
                        if (newInfoBoneInfo.group >= 7 && newInfoBoneInfo.group <= 9)
                            transforms = fKCtrlInfo.charReference.GetReferenceInfo(ChaReference.RefObjKey.HeadParent).GetComponentsInChildren<Transform>().Where(x => x.name.Equals(newInfoBoneInfo.bone));
                        else
                            transforms = fKCtrlInfo.fkCtrl.transform.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(newInfoBoneInfo.bone));

                        if (transforms.Count() == 0)
                            continue;

                        for (int transformIndex = transforms.Count()-1; transformIndex >= 0; transformIndex--)
                        {
                            if (chaControl != transforms.ElementAt(transformIndex).GetComponentInParent<ChaControl>())
                                continue;

                            gameObject = transforms.ElementAt(transformIndex).gameObject;
                        }

                        if (gameObject == null)
                            continue;

                        OIBoneInfo.BoneGroup targetBoneGroup;
                        switch (newInfoBoneInfo.group)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                targetBoneGroup = OIBoneInfo.BoneGroup.Body;
                                break;
                            case 5:
                            case 6:
                                targetBoneGroup = (OIBoneInfo.BoneGroup)(1 << newInfoBoneInfo.group);
                                break;
                            case 7:
                            case 8:
                            case 9:
                                targetBoneGroup = OIBoneInfo.BoneGroup.Hair;
                                break;
                            case 10:
                                targetBoneGroup = OIBoneInfo.BoneGroup.Neck;
                                break;
                            case 11:
                            case 12:
                                targetBoneGroup = OIBoneInfo.BoneGroup.Breast;
                                break;
                            case 13:
                                targetBoneGroup = OIBoneInfo.BoneGroup.Skirt;
                                break;
                            default:
                                targetBoneGroup = (OIBoneInfo.BoneGroup)(1 << newInfoBoneInfo.group);
                                break;
                        }

                        if (!fKCtrlInfo.ociChar.oiCharInfo.bones.TryGetValue(newInfoBoneInfo.no, out OIBoneInfo oiboneInfo))
                        {
                            oiboneInfo = new OIBoneInfo(Studio.Studio.GetNewIndex())
                            {
                                changeAmount = new ChangeAmount(),
                                treeState = TreeNodeObject.TreeState.Close,
                                visible = true
                            };
                            Studio.Studio.AddChangeAmount(oiboneInfo.dicKey, oiboneInfo.changeAmount);
                            fKCtrlInfo.ociChar.oiCharInfo.bones.Add(newInfoBoneInfo.no, oiboneInfo);
                        }

                        oiboneInfo.group = targetBoneGroup;
                        oiboneInfo.level = newInfoBoneInfo.level;


                        OCIChar.BoneInfo charBoneInfo = null;
                        foreach (var charListBone in fKCtrlInfo.ociChar.listBones)
                        {
                            if (charListBone == null)
                                continue;

                            if (charListBone.boneID == newInfoBoneInfo.no)
                            {
                                charBoneInfo = charListBone;
                                break;
                            }
                        }

                        if (charBoneInfo == null || charBoneInfo.guideObject == null)
                        {
                            GuideObject guideObject = AddObjectAssist.AddBoneGuide(gameObject.transform, oiboneInfo.dicKey, fKCtrlInfo.ociChar.guideObject, newInfoBoneInfo.name);
                            OIBoneInfo.BoneGroup group = oiboneInfo.group;
                            guideObject.scaleSelect = 0.025f;
                            charBoneInfo = new OCIChar.BoneInfo(guideObject, oiboneInfo, newInfoBoneInfo.no);
                            fKCtrlInfo.ociChar.listBones.Add(charBoneInfo);
                            guideObject.SetActive(false, true);
                            oiboneInfo.changeAmount = guideObject.changeAmount;
                        }
                        else
                        {
                            GuideObject guideObject = charBoneInfo.guideObject;
                            guideObject.transformTarget = gameObject.transform;
                            oiboneInfo.changeAmount = guideObject.changeAmount;
                        }

                        FKCtrl.TargetInfo targetInfo = null;
                        foreach (var listBone in fKCtrlInfo.fkCtrl.listBones)
                        {
                            if (listBone == null)
                                continue;

                            if (listBone.m_Transform.name == gameObject.transform.name)
                            {
                                targetInfo = listBone;
                                break;
                            }
                        }

                        if (targetInfo == null)
                        {
                            targetInfo = new FKCtrl.TargetInfo(gameObject, oiboneInfo.changeAmount, targetBoneGroup, newInfoBoneInfo.level);
                            fKCtrlInfo.fkCtrl.listBones.Add(targetInfo);
                        }
                        else
                        {
                            targetInfo.gameObject = gameObject;
                        }
                    }

                    fKCtrlInfo.fkCtrl.count = fKCtrlInfo.fkCtrl.listBones.Count;
                    for (int partIndex = 0; partIndex < FKCtrl.parts.Count(); partIndex++)
                        fKCtrlInfo.ociChar.ActiveFK(FKCtrl.parts[partIndex], fKCtrlInfo.ociChar.oiCharInfo.activeFK[partIndex], false);
                    fKCtrlInfo.ociChar.ActiveKinematicMode(OICharInfo.KinematicMode.FK, fKCtrlInfo.info.enableFK, true);
                }*/
    }
}
