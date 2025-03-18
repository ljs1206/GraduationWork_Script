using Main.Runtime.Agents;
using Main.Runtime.Manager;
using UnityEngine;

namespace LJS.Item.Effect
{
    [CreateAssetMenu(fileName = "Damage", menuName = "SO/LJS/ItemEffect/Damage")]
    public class Damage : ItemEffectBase
    {       
        public override void UseEffect(float value)
        {
            (PlayerManager.Instance.Player as Agent).HealthCompo.CurrentHealth += value;
        }
    }
}
