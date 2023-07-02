using Maxst;
using Unicommon.Swatches;
using UnityEngine;

namespace Maxst.Avatar
{
    [CreateAssetMenu(fileName = "ColorsSO", menuName = "ScriptableObjects/ColorsSO", order = 1000)]
    public class ColorsSO : AppColorsSO
    {
        public static ColorsSO Object => Instance as ColorsSO;

        public Color32 Primary = new(95, 81, 251, 255);
        public Color32 Black = new(33, 37, 41, 255);
        public Color32 Gray900 = new(55, 60, 70, 255);
        public Color32 Gray800 = new(80, 90, 100, 255);
        public Color32 Gray700 = new(132, 140, 148, 255);
        public Color32 Gray200 = new(200, 205, 210, 255);
        public Color32 Gray100 = new(220, 225, 230, 255);
        public Color32 Gray50 = new(233, 236, 239, 255);
        public Color32 White = new(255, 255, 255, 255);
        public Color32 GradientGray900 = new(217, 217, 217, 255);
        public Color32 GradientGray100 = new(248, 248, 248, 255);
        public Color32 GradientBlue900 = new(223, 236, 248, 255);
        public Color32 GradientBlue100 = new(240, 245, 250, 255);
    }
}