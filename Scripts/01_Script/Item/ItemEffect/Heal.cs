using LJS.Item.Effect;
using Main.Runtime.Agents;
using Main.Runtime.Manager;
using UnityEngine;

namespace LJS.Item.Effect
{
    [CreateAssetMenu(fileName = "Heal", menuName = "SO/LJS/ItemEffect/Heal")]
    public class Heal : ItemEffectBase
    {
        public override void UseEffect(float value)
        {
            // (PlayerManager.Instance.Player as Agent).HealthCompo.CurrentHealth += healCount;
        }
    }
}