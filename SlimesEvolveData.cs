using System.Collections.Generic;
using System.Linq;
using Code.Scripts.Attributes;
using com.DartsGames.SlimeShopManage._Scripts._Code.Scripts.NodeBasedEditor;
using DartsGames.Extensions;
using UnityEngine;

namespace SlimesShopManager.Core.SlimeEvolveTree
{
    public class SlimesEvolveData : ScriptableObject
    {
        [SerializeField] private List<SlimeEvolveStage> evolveStages = new();
        [SerializeField] private List<EvolveTransition> evolveTransitions = new();


        public IReadOnlyList<SlimeEvolveStage> EvolveStages => evolveStages;
        public IReadOnlyList<EvolveTransition> EvolveTransitions => evolveTransitions;
        
        public SlimeEvolveStage GetEvolveStageById(int argId)
        {
            return evolveStages.First(x => x.Id == argId);
        }
        public SlimeEvolveStage GetEvolveStage(SlimeType argSlimeType)
        {
            return evolveStages.First(x => x.slimeType == argSlimeType);
        }
        public EvolveTransition[] GetEvolveTransition(SlimeType argSlimeType)
        {
            var stage = GetEvolveStage(argSlimeType);
            return GetEvolveTransition(stage);
        }
        public EvolveTransition[] GetEvolveTransition(SlimeEvolveStage argEvolveStage)
        {
            return evolveTransitions.Where(x => x.fromId == argEvolveStage.Id).ToArray();
        }

        public void AddStage(SlimeEvolveStage argStage)
        {
            argStage.Id = evolveStages.GetUniqueId();
            evolveStages.Add(argStage);
        }
        public void AddTransition(EvolveTransition transition)
        {
            transition.Id = evolveTransitions.GetUniqueId();
            evolveTransitions.Add(transition);
        }
        
        
        [ContextMenu("Set Ids")]
        public void SetIds()
        {
            for (var i = 0; i < evolveStages.Count; i++)
            {
                evolveStages[i].Id = i;
            }
        }
    }

    [System.Serializable]
    public class SlimeEvolveStage: IGetId
    {
        [SerializeField] private int id;       
        public SlimeType slimeType;
        public RarityLevel rarityLevel;
        public List<Element> baseElements;
        public SlimeVisual prefab;
        public PriceBlock prices;
        public Rect rect;
        
        public int Id
        {
            get => id;
            set => id = value;
        }
    }
    
    [System.Serializable]
    public class EvolveTransition : IGetId
    {
        private int id;
        
        public int fromId;
        public int toId;
        public List<ConnectionCondition> conditions = new();

        public int Id
        {
            get => id;
            set => id = value;
        }
    }

    [System.Serializable]
    public class PriceBlock
    {
        public int sickPrice;
        public int boredPrice;
        public int normalPrice;
        public int contentPrice;
        public int excitedPrice;
    }
}
