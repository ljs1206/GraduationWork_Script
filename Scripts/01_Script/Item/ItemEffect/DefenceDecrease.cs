using LJS.Utils;
using Main.Runtime.Agents;
using Main.Runtime.Core.StatSystem;
using Main.Runtime.Manager;
using UnityEngine;

namespace LJS.Item.Effect
{
    [CreateAssetMenu(fileName = "DefenceDecrease", menuName = "SO/LJS/ItemEffect/DefenceDecrease")]
    public class DefenceDecrease : ItemEffectBase, IDeleteable
    {
        [SerializeField] private StatSO _defenceStat;

        public override void UseEffect(float value)
        {
            (PlayerManager.Instance.Player as Agent).GetCompo<AgentStat>()
                .AddReductionValuePercent(_defenceStat, this, value);
        }

        public void DeleteEffect()
        {
            (PlayerManager.Instance.Player as Agent).GetCompo<AgentStat>()
                .RemoveReductionValuePercent(_defenceStat, this);
        }
    }
}
