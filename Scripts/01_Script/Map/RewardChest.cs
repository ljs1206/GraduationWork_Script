using System.Collections.Generic;
using BIS.Manager;
using DG.Tweening;
using KHJ;
using KHJ.SO;
using LJS.Item;
using LJS.UI;
using Main.Runtime.Core.Events;
using Main.Shared;
using UnityEngine;
using TMPro;
using BIS.Data;
using Main.Core;

//using T

namespace LJS.Map
{
    public enum RewardType
    {
        Posion = 0,
        Piece
    }

    public class RewardChest : MonoBehaviour, IPoolable, IInteractable
    {
        [SerializeField] private ItemDataTableSO _itemDataTableSO;
        [SerializeField] private SynergyBlockTableSO _blockTableSO;

        [SerializeField] private ParticleSystem _openParticle;
        [SerializeField] private ParticleSystem _smokeParticle;

        [SerializeField] private RewardPanel _rewardPanel;

        [SerializeField] private PoolManagerSO _poolManager;
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private CurrencySO _coinData;
        private List<ItemSOBase> _selectedList;
        private Animation _animationCompo;

        public bool OpenNow { get; set; }

        private GameEventChannelSO _gameEventChannel;

        private void Awake()
        {
            _gameEventChannel = AddressableManager.Load<GameEventChannelSO>("GameEventChannel");
            _gameEventChannel.AddListener<EndBattle>(HandleEndBattle);
            _coinText.text = "5,000";
        }

        private void OnDestroy()
        {
            _gameEventChannel.RemoveListener<EndBattle>(HandleEndBattle);
        }

        private void HandleEndBattle(EndBattle evt)
        {
            _pool.Push(this);
        }

        public void Open(int rewardCount)
        {
            if (OpenNow) return;
            OpenNow = true;
            _smokeParticle.Play();
            _animationCompo.PlayQueued("ChestOpen");
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
            _coinData.ChangeValue(_coinData.CurrentAmmount + 5000);

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
                        item.UseItem();
                    }
                        break;
                    case RewardType.Piece:
                    {
                        SynergyBoardManager.Instance.SetFistSlotBlock(synergy, true);
                        list.Add(synergy);
                    }
                        break;
                }
            }
        }

        private void CloseChest()
        {
            OpenNow = false;
            _animationCompo.PlayQueued("ChestClose");
        }

        #region IPoolable

        [SerializeField] private PoolTypeSO _typeSO;
        public PoolTypeSO PoolType => _typeSO;

        public GameObject GameObject => gameObject;

        private Pool _pool;

        public void SetUpPool(Pool pool)
        {
            _animationCompo = GetComponentInChildren<Animation>();
            _pool = pool;
        }

        public void ResetItem()
        {
            _rewardPanel.ResetPanel();
        }

        #endregion

        #region Interact

        public Transform UIDisplayTrm => transform;
        public Vector3 AdditionalUIDisplayPos => Vector3.up * 1;
        [field: SerializeField] public string Description { get; set; }

        public void Interact(Transform Interactor)
        {
            Open(5);
        }

        #endregion
    }
}