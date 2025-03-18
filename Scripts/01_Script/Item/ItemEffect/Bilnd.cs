    using DG.Tweening;
    using LJS.Item;
    using LJS.Item.Effect;
    using LJS.Utils;
    using UnityEngine;
    using UnityEngine.Rendering;

    [CreateAssetMenu(fileName = "Bilnd", menuName = "SO/LJS/ItemEffect/Bilnd")]
    public class Bilnd : ItemEffectBase, IDeleteable
    {
        [SerializeField] private float _cycleTime;
        [SerializeField] private float _blindDuration;
        [SerializeField] private float _finalblur;
        
        private Beautify.Universal.Beautify _beautify;
        private bool isPlaying;
        
        private float _originFinalblur;
        private Tween _finalblurTween;
        
        public override void UseEffect(float value)
        {
            Volume volume = Object.FindAnyObjectByType<Volume>();
            volume.profile.TryGet(out _beautify);
            
            isPlaying = false;
            _originFinalblur = _beautify.blurIntensity.value;
            
            ItemManager.Instance.EffectLoopStop();
            ItemManager.Instance.EffectLoop(SetFinalBlur, _cycleTime);
        }
        
        public void SetFinalBlur(float value)
        {
            _beautify.blurIntensity.Override(value);
        }
        
        public void SetFinalBlur()
        {
            if (_finalblurTween != null && _finalblurTween.IsActive()) _finalblurTween.Kill();
            
            if (isPlaying)
            {
                isPlaying = false;
                _finalblurTween = DOTween.To(() => _beautify.blurIntensity.value, x => SetFinalBlur(x),
                    _originFinalblur, _blindDuration);
            }
            else
            {
                isPlaying = true;
                _finalblurTween = DOTween.To(() => _beautify.blurIntensity.value, x => SetFinalBlur(x),
                    _finalblur, _blindDuration);
            }
        }

        public void DeleteEffect()
        {
            SetFinalBlur(_originFinalblur);
            _finalblurTween.Kill();
        }
    }
