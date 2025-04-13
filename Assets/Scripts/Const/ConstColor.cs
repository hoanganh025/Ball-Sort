using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Definition
{
    public class ConstColor
    {
        public static readonly Dictionary<ConstBall.BallType, Color> BallColors;

        static ConstColor()
        {
            BallColors = new Dictionary<ConstBall.BallType, Color>()
            {
                { ConstBall.BallType.RedBall, Color.red },
                { ConstBall.BallType.BlueBall, new Color(0.2429245f, 0.8235087f, 0.9716981f)},
                { ConstBall.BallType.GreenBall, new Color(0.7098039f, 0.9176471f, 0.8431373f)},
                { ConstBall.BallType.YellowBall, Color.yellow },
                { ConstBall.BallType.PurpleBall, new Color(0.8313726f, 0.6470588f, 0.8980392f) },
            };
        }
        
        public static ConstBall.BallType? GetBubbleTypeFromColor(Color color)
        {
            var pair = BallColors.FirstOrDefault(x => x.Value == color);
        
            if (BallColors.ContainsKey(pair.Key))
            {
                return pair.Key;
            }
        
            return null;
        }
    }
}