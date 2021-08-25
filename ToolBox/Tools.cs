using Studio;
using UnityEngine;
using System.Linq;
#if AI || HS2
using AIChara;
#endif

namespace ToolBox
{
    class Tools
    {
        public static OIBoneInfo.BoneGroup GetBoneGroup(int group)
        {
            if (group <= 4)
                return OIBoneInfo.BoneGroup.Body;
            if (group <= 6)
                return (OIBoneInfo.BoneGroup)(1 << group);
            if (group <= 9)
                return OIBoneInfo.BoneGroup.Hair;
            if (group <= 10)
                return OIBoneInfo.BoneGroup.Neck;
            if (group <= 12)
                return OIBoneInfo.BoneGroup.Breast;
            if (group <= 13)
                return OIBoneInfo.BoneGroup.Skirt;

            return (OIBoneInfo.BoneGroup)(1 << group);
        }

        public static Transform GetTransformOfChaControl(ChaControl chaControl, string transformName)
        {
            Transform transform = null;
            if (chaControl == null)
                return transform;

            var transforms = chaControl.GetComponentsInChildren<Transform>().Where(x => x.name != null && x.name.Equals(transformName));
            if (transforms.Count() == 0)
                return transform;

            for (int transformIndex = transforms.Count() - 1; transformIndex >= 0; transformIndex--)
            {
                transform = transforms.ElementAt(transformIndex);
                if (transform != null && chaControl == transform.GetComponentInParent<ChaControl>())
                    return transform;
            }

            return transform;
        }
    }
}
