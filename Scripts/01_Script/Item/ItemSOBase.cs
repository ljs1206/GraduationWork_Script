using LJS.Item.Effect;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LJS.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LJS.Item
{
    [Flags]
    public enum ItemType
    {
        None = 0,
        Healing = 1 << 0, 
        Buff = 1 << 1, 
        DeBuff = 1 << 2
    }

    public class ItemSOBase : ScriptableObject, IBinding
    {
        [Header("BaseInfo")]
        [BoxGroup(GroupName = "BaseInfo")]
        public string itemName;
        [BoxGroup(GroupName = "BaseInfo")]
        public ItemType typeOfItem;

        [TextArea]
        [BoxGroup(GroupName = "BaseInfo")]
        [SerializeField]
        public string description;
        [BoxGroup(GroupName = "BaseInfo")]
        [PreviewField(75)]
        [HideLabel]
        [SerializeField]
        public Sprite icon;
        
        [Header("Effect Settings")]
        [BoxGroup(GroupName = "EndEffect")]
        [SerializeField]
        public bool endImmediately;
        [BoxGroup(GroupName = "EndEffect")]
        [SerializeField]
        public int effectEndTime;

        [BoxGroup(GroupName = "EndEffect")]
        public List<ItemEffectBase> effectList;
        
        #region itemValue
        
        [Header("Effect Value")]
        [BoxGroup(GroupName = "EffectValue")]
        [SerializeField]
        public int healCount;
        [BoxGroup(GroupName = "EffectValue")]
        [SerializeField]
        public int damageCount;
        [BoxGroup(GroupName = "EffectValue")]
        [SerializeField]
        public int defenceDecrease;
        [BoxGroup(GroupName = "EffectValue")]
        [SerializeField]
        public int defenceIncrease;
        [BoxGroup(GroupName = "EffectValue")]
        [SerializeField]
        public int strength;
        [BoxGroup(GroupName = "EffectValue")]
        [SerializeField]
        public int weakness;
        #endregion
        
        CancellationTokenSource destroyCancellation;

        public ItemSOBase()
        {
            destroyCancellation = new CancellationTokenSource();
        }

        public void OnDestroy()
        {
            destroyCancellation.Cancel();
            destroyCancellation.Dispose();
        }
        
        public virtual void UseItem()
        {
            foreach(ItemEffectBase effect in effectList)
            {
                float value = 0;
                switch (effect.effectType)
                {
                    case EffectType.Heal:
                        value = healCount;;
                        break;
                    case EffectType.Damage:
                        value = damageCount;;
                        break;
                    case EffectType.Strength:
                        value = strength;
                        break;
                    case EffectType.Weakness:
                        value = weakness;
                        break;
                    case EffectType.DefenceDecrease:
                        value = defenceDecrease;
                        break;
                    case EffectType.DefenceIncrease:
                        value = defenceIncrease;
                        break;
                }
                
                effect.UseEffect(value);
            }
            
            if (endImmediately)
                EndItemEffect();
            else
                Delay(EndItemEffect, effectEndTime);
        }

        protected async UniTask Delay(Action action, int delayTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime),
                false,PlayerLoopTiming.Update, 
                destroyCancellation.Token);
            action?.Invoke();
        }

        public virtual void EndItemEffect()
        {
            foreach(ItemEffectBase effect in effectList){
                if(effect is IDeleteable)
                    (effect as IDeleteable).DeleteEffect();
            }
        }

        public void PreUpdate()
        {
            
        }

        public void Update()
        {
            
        }

        public void Release()
        {
            
        }
    }
}
