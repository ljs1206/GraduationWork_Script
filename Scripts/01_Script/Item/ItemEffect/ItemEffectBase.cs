using UnityEngine;

namespace LJS.Item.Effect
{
    public enum EffectType
    {
        Heal = 0,
        Damage,
        DefenceDecrease,
        DefenceIncrease,
        Strength,
        Weakness,
        Blind
    }
    
    public abstract class ItemEffectBase : ScriptableObject
    {
        public EffectType effectType;
        
        public abstract void UseEffect(float value);
    }
}
