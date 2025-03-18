using LJS.Item.Effect;
using LJS.Utils;
using Main.Runtime.Agents;
using Main.Runtime.Core.StatSystem;
using Main.Runtime.Manager;
using UnityEngine;

namespace LJS.Item.Effect
{
    [CreateAssetMenu(fileName = "Weakening", menuName = "SO/LJS/ItemEffect/Weakening")]
    public class Weakening : ItemEffectBase, IDeleteable
    {
        [SerializeField] private StatSO _powerStat;

        public override void UseEffect(float value)
        {
            // (PlayerManager.Instance.Player as Agent).GetCompo<AgentStat>()
            //     .AddReductionValuePercent(_powerStat, this, _powerReductionPercentage);
        }

        public void DeleteEffect()
        {
            // (PlayerManager.Instance.Player as Agent).GetCompo<AgentStat>()
            //     .RemoveReductionValuePercent(_powerStat, this);
        }
    }
}