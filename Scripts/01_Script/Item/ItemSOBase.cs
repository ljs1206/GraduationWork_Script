using LJS.Item.Effect;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LJS.Utils;
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

    public class ItemSOBase : ScriptableObject, IGetValueable, IBinding
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
        
        private Dictionary<EffectType, int>  _effectDict = new();
		private CancellationTokenSource _destroyCancellation;
		
        public ItemSOBase()
        {
            _effectDict = new Dictionary<EffectType, int>
            {
                { EffectType.Heal, healCount },
                { EffectType.Damage, damageCount },
                { EffectType.Strength, strength },
                { EffectType.Weakness, weakness },
                { EffectType.DefenceDecrease, defenceDecrease },
                { EffectType.DefenceIncrease, defenceIncrease }
            };
			_destroyCancellation = new CancellationTokenSource();
        }
		
		public void OnDestroy()
        {
            _destroyCancellation.Cancel();
            _destroyCancellation.Dispose();
        }

        
        public virtual void UseItem()
        {
            foreach(ItemEffectBase effect in effectList)
            {
                effect.SetEffectValue(this);
            }
            
            if (endImmediately)
                EndItemEffect();
            else
                Delay(EndItemEffect, effectEndTime);
        }

        public virtual void EndItemEffect()
        {
            foreach(ItemEffectBase effect in effectList){
                if(effect is IDeleteable)
                    (effect as IDeleteable).DeleteEffect();
            }
        }

        public float GetValue(EffectType effectType)
        {
            return _effectDict.ContainsKey(effectType) ? _effectDict[effectType] : 0;
        }
		
		protected async UniTask Delay(Action action, int delayTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delayTime),
                false,PlayerLoopTiming.Update, 
                _destroyCancellation.Token);
            action?.Invoke();
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
