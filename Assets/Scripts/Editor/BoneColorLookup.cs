using System;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(fileName = "BoneColorLookup", menuName = "BoneColorLookup")]
    public class BoneColorLookup : ScriptableObject
    {
        [Serializable]
        private class BoneColor
        {
            [Serializable]
            private class ColorEntry
            {
                [ColorUsage(showAlpha: true, hdr: true)]
                [SerializeField] public Color Color;
                [SerializeField] public float Time;
            }

            [SerializeField] public string BoneName;
            [SerializeField] private ColorEntry[] Entries;

            public Color Sample(float t)
            {
                if (Entries == null || Entries.Length == 0)
                    return Color.clear;
                for (int i = 0; i < Entries.Length; i++)
                {
                    if (t <= Entries[i].Time)
                    {
                        return i == 0 ? Entries[i].Color : Color.Lerp
                        (
                            a: Entries[i - 1].Color,
                            b: Entries[i].Color,
                            t: Mathf.InverseLerp(Entries[i - 1].Time, Entries[i].Time, t)
                        );
                    }
                }
                return Entries[Entries.Length - 1].Color;
            }
        }

        [SerializeField] private BoneColor[] bones;

        public Color GetColor(string boneName, float t)
        {
            if (bones == null)
                return Color.clear;
            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i].BoneName.Equals(boneName))
                    return bones[i].Sample(t);
            }
            return Color.clear;
        }
    }
}
