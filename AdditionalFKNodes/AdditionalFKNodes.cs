using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if AI || HS2
using AIChara;
#endif
using Studio;
using UnityEngine;
using ToolBox;
using ToolBox.Extensions;

namespace AdditionalFKNodes
{
    class AdditionalFKNodes
    {
        private static readonly Dictionary<string, FKCtrlInfo> fkCtrlDictionary = new Dictionary<string, FKCtrlInfo>();
        private static readonly Dictionary<int, Info.BoneInfo> additionalBoneInfoDictionary = new Dictionary<int, Info.BoneInfo>();

        public static void AddFKCtrlInfo(FKCtrl fkCtrl, OCIChar ociChar, OICharInfo charInfo, ChaControl chaControl, ChaReference charReference)
        {
            if (chaControl == null)
                return;

            if (fkCtrlDictionary.TryGetValue(chaControl.name, out _))
                fkCtrlDictionary.Remove(chaControl.name);

            fkCtrlDictionary.Add(chaControl.name, new FKCtrlInfo(fkCtrl, ociChar, charInfo, chaControl, charReference));
        }

        public static void AddFKCtrlInfo(FKCtrl fkCtrl, OICharInfo charInfo, ChaControl chaControl, ChaReference charReference)
        {
            if (chaControl == null)
                return;

            if (!fkCtrlDictionary.TryGetValue(chaControl.name, out _))
            {
                fkCtrlDictionary.Add(chaControl.name, new FKCtrlInfo(fkCtrl, charInfo, chaControl, charReference));
                return;
            }

            fkCtrlDictionary[chaControl.name].fkCtrl = fkCtrl;
            fkCtrlDictionary[chaControl.name].info = charInfo;
            fkCtrlDictionary[chaControl.name].chaControl = chaControl;
            fkCtrlDictionary[chaControl.name].charReference = charReference;
        }

        public static void AddFKCtrlInfo(OCIChar ociChar)
        {
            ChaControl chaControl = ociChar.charInfo;

            if (chaControl == null)
                return;

            if (!fkCtrlDictionary.TryGetValue(chaControl.name, out _))
            {
                fkCtrlDictionary.Add(chaControl.name, new FKCtrlInfo(ociChar, chaControl));
                return;
            }

            fkCtrlDictionary[chaControl.name].ociChar = ociChar;
            fkCtrlDictionary[chaControl.name].chaControl = chaControl;
        }

        internal static void ResetFKNodes(ChaControl chaControl, OIBoneInfo.BoneGroup group)
        {
            if (chaControl == null)
                return;

            foreach (var boneInfo in additionalBoneInfoDictionary)
            {
                if (group != Tools.GetBoneGroup(boneInfo.Value.group))
                    continue;

                var transform = Tools.GetTransformOfChaControl(chaControl, boneInfo.Value.bone);
                if (transform == null)
                    continue;

                transform.localRotation = Quaternion.identity;
            }
        }

        internal static void AddAdditionalNodes(ChaControl chaControl, string assetBundleName, string assetName)
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
#if AI || HS2
            var syncBoneDictionary = new Dictionary<int, FKCtrl.TargetInfo>();
            var dictionary = new Dictionary<int, OCIChar.BoneInfo>();
#endif
            string[] lines = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] cells = line.Split('\t');
                if (cells.Length < 5)
                    continue;
                UnityEngine.Debug.Log("AdditionalFKNodes: Found matching line for asset " + assetName + "\n" + line);
                try
                {
                    int boneInfoKey = int.Parse(cells[0]);

                    if (!boneInfoDictionary.TryGetValue(boneInfoKey, out var infoBoneInfo))
                    {
#if AI || HS2
                        infoBoneInfo = new Info.BoneInfo(boneInfoKey, cells[1], new List<string> { cells[2] })
                        {
                            group = int.Parse(cells[3]),
                            level = int.Parse(cells[4])
                        };
#else
                        infoBoneInfo = new Info.BoneInfo
                        {
                            no = boneInfoKey,
                            bone = cells[1],
                            name = cells[2],
                            group = int.Parse(cells[3]),
                            level = int.Parse(cells[4])
                        };
#endif
                        boneInfoDictionary.Add(boneInfoKey, infoBoneInfo);

                        if (!additionalBoneInfoDictionary.TryGetValue(boneInfoKey, out _))
                            additionalBoneInfoDictionary.Add(boneInfoKey, infoBoneInfo);
                    }

                newInfoBoneInfoList.Add(infoBoneInfo);
                }
                catch (FormatException)
                {
                    UnityEngine.Debug.Log("AdditionalFKNodes: Asset file is the wrong format");
                    return;
                }
            }

            foreach (var newInfoBoneInfo in newInfoBoneInfoList)
            {
#if AI || HS2
                bool boneWeight = true;
#endif
                GameObject gameObject = Tools.GetTransformOfChaControl(chaControl, newInfoBoneInfo.bone)?.gameObject;
                if (gameObject == null)
                    continue;

#if AI || HS2
                if (dictionary.TryGetValue(newInfoBoneInfo.sync, out OCIChar.BoneInfo ociCharBoneInfo))
                {
                    ociCharBoneInfo.AddSyncBone(gameObject);

                    if (syncBoneDictionary.TryGetValue(newInfoBoneInfo.sync, out var syncTargetInfo))
                        syncTargetInfo.AddSyncBone(gameObject);

                    continue;
                }
#endif
                OIBoneInfo.BoneGroup targetBoneGroup = Tools.GetBoneGroup(newInfoBoneInfo.group);
#if AI || HS2
                if (targetBoneGroup == OIBoneInfo.BoneGroup.Skirt)
                {
                    boneWeight = false;
                    boneWeight |= fKCtrlInfo.fkCtrl.UsedBone(fKCtrlInfo.chaControl.GetCustomClothesComponent(0), gameObject.transform);
                    boneWeight |= fKCtrlInfo.fkCtrl.UsedBone(fKCtrlInfo.chaControl.GetCustomClothesComponent(1), gameObject.transform);
                    fKCtrlInfo.ociChar.listBones.Find((OCIChar.BoneInfo _boneInfo) => _boneInfo.boneID == newInfoBoneInfo.no).SafeProc(delegate (OCIChar.BoneInfo _boneInfo)
                    {
                        _boneInfo.boneWeight = boneWeight;
                    });
                }
#endif
                if (fKCtrlInfo.ociChar == null || fKCtrlInfo.ociChar.listBones == null || fKCtrlInfo.ociChar.guideObject == null || 
                    fKCtrlInfo.ociChar.oiCharInfo == null || fKCtrlInfo.ociChar.oiCharInfo.bones == null)
                    continue;

                if (!fKCtrlInfo.ociChar.oiCharInfo.bones.TryGetValue(newInfoBoneInfo.no, out OIBoneInfo oiboneInfo))
                {
                    oiboneInfo = new OIBoneInfo(Studio.Studio.GetNewIndex());
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
                    if (group == OIBoneInfo.BoneGroup.RightHand || group == OIBoneInfo.BoneGroup.LeftHand)
#if AI || HS2
                        guideObject.scaleSelect = 0.025f;
#else
                        guideObject.scaleSelect = 0.01f;
#endif
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

                if (!Singleton<Studio.Studio>.Instance.dicChangeAmount.TryGetValue(oiboneInfo.dicKey, out var _))
                    Studio.Studio.AddChangeAmount(oiboneInfo.dicKey, oiboneInfo.changeAmount);

                if (fKCtrlInfo.fkCtrl == null || fKCtrlInfo.fkCtrl.listBones == null)
                    continue;

                FKCtrl.TargetInfo targetInfo = null;
                foreach (var listBone in fKCtrlInfo.fkCtrl.listBones)
                {
                    if (listBone == null)
                        continue;
#if AI || HS2
                    if (listBone.boneID == newInfoBoneInfo.no)
#else
                    if (listBone.m_Transform != null && listBone.m_Transform.name == gameObject.transform.name)
#endif
                    {
                        targetInfo = listBone;
                        break;
                    }
                }

                if (targetInfo == null)
                {
#if AI || HS2
                    targetInfo = new FKCtrl.TargetInfo(gameObject, oiboneInfo.changeAmount, targetBoneGroup, newInfoBoneInfo.level, boneWeight, newInfoBoneInfo.no);
#else
                    targetInfo = new FKCtrl.TargetInfo(gameObject, oiboneInfo.changeAmount, targetBoneGroup, newInfoBoneInfo.level);
#endif
                    fKCtrlInfo.fkCtrl.listBones.Add(targetInfo);
                }
                else
                {
                    targetInfo.gameObject = gameObject;
                }
#if AI || HS2
                if (newInfoBoneInfo.sync != -1)
                {
                    dictionary.Add(newInfoBoneInfo.no, charBoneInfo);
                    syncBoneDictionary.Add(newInfoBoneInfo.no, targetInfo);
                }
#endif
            }

            fKCtrlInfo.fkCtrl.count = fKCtrlInfo.fkCtrl.listBones.Count;
            for (int partIndex = 0; partIndex < FKCtrl.parts.Count(); partIndex++)
                fKCtrlInfo.ociChar.ActiveFK(FKCtrl.parts[partIndex], fKCtrlInfo.ociChar.oiCharInfo.activeFK[partIndex], false);
        }
    }
}
