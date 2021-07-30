using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using KKAPI.Studio;
using BepInEx.Bootstrap;
using Studio;
using UnityEngine;
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
        internal const string VERSION = "1.0.1.0";

        private static Type _uncensorSelectorType;

        private static readonly Dictionary<string, int> additionalNodes = new Dictionary<string, int>();

        internal void Main()
        {
            Debug.Log("AdditionalFKNodes: Trying to patch methods...");

            var harmony = new Harmony(GetType().Name);

            Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.uncensorselector", out PluginInfo info);
            if (info == null || info.Instance == null)
                return;

            _uncensorSelectorType = info.Instance.GetType();
            Type uncensorSelectorControllerType = _uncensorSelectorType.GetNestedType("UncensorSelectorController", AccessTools.all);
            if (uncensorSelectorControllerType == null)
                return;

            Debug.Log("AdditionalFKNodes: UncensorSelector found, trying to patch");
            MethodInfo uncensorSelectorReloadCharacterBody = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterBody");
            if (uncensorSelectorReloadCharacterBody != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterBody, postfix: new HarmonyMethod(GetType(), nameof(AdditionalFKNodes_AddBodyNodes)));
                Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterBody correctly");
            }

            MethodInfo uncensorSelectorReloadCharacterPenis = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterPenis");
            if (uncensorSelectorReloadCharacterPenis != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterPenis, postfix: new HarmonyMethod(GetType(), nameof(AdditionalFKNodes_AddPenisNodes)));
                Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterPenis correctly");
            }

            MethodInfo uncensorSelectorReloadCharacterBalls = AccessTools.Method(uncensorSelectorControllerType, "ReloadCharacterBalls");
            if (uncensorSelectorReloadCharacterBalls != null)
            {
                harmony.Patch(uncensorSelectorReloadCharacterBalls, postfix: new HarmonyMethod(GetType(), nameof(AdditionalFKNodes_AddBallNodes)));
                Debug.Log("AdditionalFKNodes: UncensorSelector patched ReloadCharacterBalls correctly");
            }
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

            AdditionalFKNodes_AddNodes(chaControl, assetBundleName, assetName);
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

            AdditionalFKNodes_AddNodes(chaControl, assetBundleName, assetName);
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

            AdditionalFKNodes_AddNodes(chaControl, assetBundleName, assetName);
        }

        private static void AdditionalFKNodes_AddNodes(ChaControl chaControl, string assetBundleName, string assetName)
        {
            TextAsset textAsset = CommonLib.LoadAsset<TextAsset>(assetBundleName, "additional_fknodes", true);
            if (textAsset == null)
                return;

            Debug.Log("AdditionalFKNodes: Loaded additional_fknodes TextAsset from " + assetBundleName);

            var fKCtrl = chaControl.gameObject.GetComponent<FKCtrl>();
            if (fKCtrl == null)
                return;

            var animeCtrl = chaControl.gameObject.GetComponentInChildren<CharAnimeCtrl>(true);
            if (animeCtrl == null)
                return;

            var ociCharInfo = animeCtrl.oiCharInfo;
            if (ociCharInfo == null)
                return;

            var ociChar = StudioObjectExtensions.GetOCIChar(chaControl);
            if (ociChar == null)
                return;

            var manager = Resources.FindObjectsOfTypeAll<GuideObjectManager>().FirstOrDefault();
            if (manager == null)
                return;

            Transform Dan109 = chaControl.GetComponentsInChildren<Transform>().Where(x => x.name.Equals("cm_J_dan109_00")).FirstOrDefault();
            if (Dan109 == null)
                return;

            manager.dicGuideObject.TryGetValue(Dan109, out GuideObject dan109GuideObject);
            if (dan109GuideObject == null)
                return;

            var boneInfoDictionary = Singleton<Info>.Instance.dicBoneInfo;

            string[] lines = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] cells = line.Split('\t');
                if (cells.Length < 4 || !cells[0].Equals(assetName))
                    continue;
                Debug.Log("AdditionalFKNodes: Found matching line for asset " + assetName + "\n" + line);

                string newCategory = cells[2];
                List<string> fkNodeList = new List<string>();

                for (int i = 3; i < cells.Length; i++)
                {
                    if (cells[i].IsNullOrEmpty() || cells[i].IsNullOrWhiteSpace())
                        continue;

                    fkNodeList.Add(cells[i]);
                }

                foreach (var fkNode in fkNodeList)
                {
                    if (!additionalNodes.TryGetValue(fkNode, out int boneKey))
                    {
                        boneKey = 0;
                        foreach (var info in boneInfoDictionary)
                        {
                            if (info.Value.bone == fkNode)
                                continue;

                            if (boneKey < info.Key)
                                boneKey = info.Key;
                        }
                        boneKey++;

                        additionalNodes.Add(fkNode, boneKey);
                    }

                    if (!boneInfoDictionary.TryGetValue(boneKey, out var boneInfo))
                    {
                        boneInfo = new Info.BoneInfo
                        {
                            no = boneKey,
                            bone = fkNode,
                            name = newCategory
                        };
                        if (Int32.TryParse(cells[1], out int newGroup))
                            boneInfo.group = newGroup;
                        boneInfoDictionary.Add(boneKey, boneInfo);
                    }

                    var transforms = chaControl.GetComponentsInChildren<Transform>().Where(x => x.name.Equals(boneInfo.bone));
                    if (transforms.Count() == 0)
                        continue;

                    var gameObject = transforms.Last().gameObject;
                    if (gameObject == null)
                        continue;

                    if (!ociCharInfo.bones.TryGetValue(boneInfo.no, out OIBoneInfo oiboneInfo))
                    {
                        oiboneInfo = new OIBoneInfo(boneInfo.no)
                        {
                            group = OIBoneInfo.BoneGroup.Body,
                            level = 0
                        };
                        ociCharInfo.bones.Add(boneKey, oiboneInfo);
                    }

                    OIBoneInfo.BoneGroup boneGroup;
                    switch (boneInfo.group)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            boneGroup = OIBoneInfo.BoneGroup.Body;
                            break;
                        case 5:
                        case 6:
                            boneGroup = (OIBoneInfo.BoneGroup)(1 << boneInfo.group);
                            break;
                        case 7:
                        case 8:
                        case 9:
                            boneGroup = OIBoneInfo.BoneGroup.Hair;
                            break;
                        case 10:
                            boneGroup = OIBoneInfo.BoneGroup.Neck;
                            break;
                        case 11:
                        case 12:
                            boneGroup = OIBoneInfo.BoneGroup.Breast;
                            break;
                        case 13:
                            boneGroup = OIBoneInfo.BoneGroup.Skirt;
                            break;
                        default:
                            boneGroup = (OIBoneInfo.BoneGroup)(1 << boneInfo.group);
                            break;
                    }

                    var targetInfo = new FKCtrl.TargetInfo(gameObject, oiboneInfo.changeAmount, boneGroup, 0);
                    fKCtrl.listBones.Add(targetInfo);
					fKCtrl.count++;

                    var charBoneInfo = ociChar.listBones.Find((OCIChar.BoneInfo info) => info.boneID == boneInfo.no);
                    if (charBoneInfo != null)
                        continue;

                    GuideObject guideObject = null;
                    foreach (var existingGuideObject in manager.dicGuideObject)
                    {
                        if (existingGuideObject.Value.dicKey == boneInfo.no && existingGuideObject.Value.parentGuide.transformTarget == chaControl.transform)
                        {
                            guideObject = existingGuideObject.Value;
                            guideObject.transformTarget = gameObject.transform;
                            break;
                        }
                    }

                    if (guideObject == null && !manager.dicGuideObject.TryGetValue(gameObject.transform, out guideObject))
                    {
                        guideObject = manager.Add(gameObject.transform, boneInfo.no);
                        guideObject.changeAmount = targetInfo.changeAmount;
                        guideObject.scaleRate = dan109GuideObject.scaleRate;
                        guideObject.scaleRot = dan109GuideObject.scaleRot;
                        guideObject.scaleSelect = dan109GuideObject.scaleSelect;
                        guideObject.parentGuide = dan109GuideObject.parentGuide;
                        guideObject.enablePos = dan109GuideObject.enablePos;
                        guideObject.enableScale = dan109GuideObject.enableScale;
                        guideObject.calcScale = dan109GuideObject.calcScale;
                        guideObject.enableMaluti = dan109GuideObject.enableMaluti;
                        guideObject.isActive = dan109GuideObject.isActive;
                        guideObject.gameObject.SetActive(dan109GuideObject.gameObject.activeSelf);
                        guideObject.SetLayer(guideObject.gameObject, 28);
                    }

                    if (!ociCharInfo.bones.TryGetValue(boneInfo.no, out OIBoneInfo oiBoneInfo))
                        continue;

                    charBoneInfo = new OCIChar.BoneInfo(guideObject, oiBoneInfo, boneInfo.no);
                    ociChar.listBones.Add(charBoneInfo);
                }
            }
        }
    }
}
