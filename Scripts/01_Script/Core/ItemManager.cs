using System;
using System.Collections;
using UnityEngine;

namespace LJS.Item
{
    public class ItemManager : MonoSingleton<ItemManager>
    {
        private IEnumerator _effectLoopCoro;

        public void Delay(Action endEvent, float endTime){
            StartCoroutine(DelayCoro(endEvent, endTime));
        }

        public IEnumerator DelayCoro(Action endEvent, float endTime)
        {
            yield return new WaitForSeconds(endTime);
            endEvent?.Invoke();
        }

        public void EffectLoop(Action loopAction, float cycleTime)
        {
            _effectLoopCoro = EffectLoopCoro(loopAction, cycleTime);
            StartCoroutine(_effectLoopCoro);
        }

        private IEnumerator EffectLoopCoro(Action loopAction, float cycleTime)
        {
            while (true)
            {
                loopAction?.Invoke();
                yield return new WaitForSeconds(cycleTime);
            }
        }

        public void EffectLoopStop()
        {
            if(_effectLoopCoro != null)
                StopCoroutine(_effectLoopCoro);
        }
    }
}
