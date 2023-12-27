using Code.Scripts.Attributes;
using UnityEngine;
using SlimeModel = Code.Scripts.Slime.Slime;

namespace com.DartsGames.SlimeShopManage._Scripts._Code.Scripts.NodeBasedEditor
{
    [System.Serializable]
    public class ConnectionCondition
    {
        public Element element;
        public int min;
        public int max;
        private AnimationCurve animationCurve;

        private int selectedElementIndex = 0;
        
        public ConnectionCondition()
        {
        }

        public bool Evaluate(SlimeModel slime)
        {
            animationCurve = new AnimationCurve(
                new Keyframe(min, 0.3f),
                new Keyframe(max, 1f));
            
            if (!slime.Elements.TryGetValue(element, out var count))
                count = 0;
            return count >= min && count <= max /*&&
                   animationCurve.Evaluate(count) >= UnityEngine.Random.Range(0.01f, 1f)*/;
        }
    }
}