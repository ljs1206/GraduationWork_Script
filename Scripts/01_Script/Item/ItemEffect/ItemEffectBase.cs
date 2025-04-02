using BIS;
using LJS.Utils;
using UnityEngine;

namespace LJS.Item.Effect
{

    public abstract class ItemEffectBase : ScriptableObject
    {
        public EffectType effectType;

        private float _value = 0;

        public abstract void UseEffect(float value);

        public void SetEffectValue(IGetValueable getValueable)
        {
            float value = getValueable.GetValue(effectType);
            UseEffect(value);
        }
    }
}