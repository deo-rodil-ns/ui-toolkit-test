using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Cycles;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Productions
{
    /// <summary>
    /// Represents the instruction on how a currency will be produced once added to a ProductionBatch.
    /// </summary>
    [System.Serializable]
    public sealed class ProductionOrder
    {
        [SerializeField] private CycleData _cycle;
        [Header("Input/Output")] 
        [SerializeField] private List<Currency> _input = new();
        [SerializeField] private List<Currency> _output = new();
        [Tooltip("Higher priority will be evaluated first.")]
        [SerializeField] private int _priority;

        private float _inputMultModifier = 1f;
        private float _outputMultModifier = 1f;
        
        public CycleData Cycle => _cycle;
        public IReadOnlyCollection<Currency> InputBase
        {
            get => _input;
            set => _input = value.ToList();
        }

        public IReadOnlyCollection<Currency> OutputBase
        {
            get => _output;
            set => _output = value.ToList();
        }

        public float InputModifier
        {
            get => _inputMultModifier;
            set
            {
                if (Mathf.Approximately(_inputMultModifier, value)) return;
                _inputMultModifier = value;
                EvtUpdated?.Invoke(this);
            }
        }
        public float OutputModifier
        {
            get => _outputMultModifier;
            set
            {
                if (Mathf.Approximately(_outputMultModifier, value)) return;
                _outputMultModifier = value;
                EvtUpdated?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Higher priority will be evaluated first by the production.
        /// </summary>
        public int Priority { get =>  _priority; set => _priority = value; }
        
        /// <summary>
        /// Final input after modifiers
        /// </summary>
        public IReadOnlyCollection<Currency> Input => _input.Select(c => c * InputModifier).ToList();
        /// <summary>
        /// Final output after modifiers
        /// </summary>
        public IReadOnlyCollection<Currency> Output => _output.Select(c => c * OutputModifier).ToList();

        #region Events
        
        public UnityEvent<ProductionOrder> EvtUpdated { get; } = new();
        public ConcludedEvent EvtConcluded { get; } = new();

        public class ConcludedEvent : UnityEvent<ConcludedEvent.Args>
        {
            public class Args
            {
                public ProductionOrder Order { get; set; }
                public bool Completed { get; set; }
            }
        }
        
        #endregion

        public void Conclude(bool completed)
        {
            EvtConcluded?.Invoke(new ConcludedEvent.Args()
            {
                Order = this,
                Completed = completed,
            });
        }
    }
}