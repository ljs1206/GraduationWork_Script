using System.Collections.Generic;
using BIS.Manager;
using DG.Tweening;
using KHJ;
using KHJ.SO;
using LJS.Item;
using LJS.UI;
using Main.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LJS.Map
{
    public enum RewardType
    {
        Posion = 0, Piece
    }
    public class RewardChest : MonoBehaviour, IPoolable, IInteractable
    {
        [SerializeField] private ItemDataTableSO _itemDataTableSO;
        [SerializeField] private SynergyBlockTableSO _blockTableSO;
        
        [SerializeField] private ParticleSystem _openParticle;
        [SerializeField] private ParticleSystem _smokeParticle;
        
        [SerializeField] private RewardPanel _rewardPanel;

        [SerializeField] private PoolManagerSO _poolManager;
        
        private List<ItemSOBase> _selectedList;
        private Animation _animation;

        public bool OpenNow { get; set; }

        private void Update()
        {
            if(OpenNow & Managers.UI.GetPopupCount() == 0 & 
                Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseChest();
        }
        
        public void Open(int rewardCount)
        {
            OpenNow = true;
            _smokeParticle.Play();
            _animation.PlayQueued("ChestOpen");
            OpenAction(rewardCount);
        }

        private void ParticleStart()
        {
            _openParticle.Play();
        }

        private void UIOpen()
        {
            _rewardPanel.OpenPanel(list);
        }

        private void ChestDelete()
        {
            _rewardPanel.ClosePanel();
            _openParticle.Stop();
            transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.OutExpo).OnComplete(() =>
                {
                    _rewardPanel.ResetPanel();
                    _poolManager.Push(this);
                });
        }
        
        List<ScriptableObject> list = new();
        private void OpenAction(int rewardCount)
        {
            list = new();
            for (int i = 0; i < rewardCount; ++i)
            {
                int rand = UnityEngine.Random.Range(0, 2);
                ItemSOBase item = _itemDataTableSO.RandomItemOutput();
                SynergySO synergy = _blockTableSO.RandomSynergyOutput();

                switch ((RewardType)rand)
                {
                    case RewardType.Posion:
                    {
                        list.Add(item);
                        item.UseItem(); // 나중에 추가 인벤토리로 옮겨야함
                    }
                        break;
                    case RewardType.Piece:
                    {
                        SynergyBoardManager.Instance.SetFistSlotBlock(synergy);
                        list.Add(synergy);
                    }
                        break;
                }
            }
        }

        private void CloseChest()
        {
            OpenNow = false;
            _animation.PlayQueued("ChestClose");
        }

        #region IPoolable
        [SerializeField] private PoolTypeSO _typeSO;
        public PoolTypeSO PoolType => _typeSO;
        
        public GameObject GameObject => gameObject;
        
        public void SetUpPool(Pool pool)
        {
            _animation = GetComponent<Animation>();
        }

        public void ResetItem()
        {
            _rewardPanel.ResetPanel();
        }
        #endregion

        #region Interact

        public Transform UIDisplayTrm => transform;
        public Vector3 AdditionalUIDisplayPos => Vector3.up * 1;
        [field:SerializeField] public string Description { get; set; }
        public void Interact(Transform Interactor)
        {
            Open(5);
        }

        #endregion
    }
}
