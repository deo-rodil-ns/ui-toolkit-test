using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Currencies;
using GothicVampire.Cycles;
using GothicVampire.Game;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Productions
{
    /// <summary>
    /// Represents a batch of production orders. This will produce currency as defined in the added ProductionOrder.
    /// </summary>
    public sealed class ProductionBatch
    {
        private readonly List<ProductionOrder> _orders = new();
        
        public Cycle Cycle { get; private set; }
        public Wallet Wallet { get; private set; }
        public IReadOnlyCollection<ProductionOrder> Orders => _orders;
        public float SpeedModifier { get; set; } = 1f;

        public IReadOnlyCollection<Currency> InputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> OutputProjection { get; private set; } = new List<Currency>();
        public IReadOnlyCollection<Currency> NetProjection { get; private set; } = new List<Currency>();

        public float TimeElapsed => Cycle.TimeElapsed;
        public float TimeRemaining => Cycle.TimeRemaining;
        public float Duration => Cycle.Duration;
        public float Progress => Cycle.Progress;

        public bool Active { get; set; } = true;

        #region Events
        
        public UnityEvent<ProductionBatchReport> EvtCompleted { get; } = new();
        public UnityEvent<ProductionBatch> EvtProjectionUpdated { get; } = new();
        
        #endregion

        public ProductionBatch(CycleData cycle, Wallet wallet) 
        {
            if (!cycle) throw new ArgumentNullException(nameof(cycle));
            if (!wallet) throw new ArgumentNullException(nameof(wallet));
            
            Wallet = wallet;
            Cycle = World.Current?.GetService<WorldCycleManager>()?.GetCycle(cycle) ??
                    throw new Exception($"{cycle.Id} cycle not found");

            UpdateProjection();
        }

        public ProductionBatch(CycleData cycle, IReadOnlyCollection<ProductionOrder> orders, Wallet wallet)
        {
            if (cycle == null) throw new ArgumentNullException(nameof(cycle));
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            if (_orders.Any(o => o.Cycle != cycle)) throw new Exception($"{nameof(CycleData)} mismatch");
            
            Cycle = World.Current?.GetService<WorldCycleManager>()?.GetCycle(cycle) 
                    ?? throw new Exception($"{cycle.Id} cycle not found");
            Wallet = wallet;
            _orders = orders.ToList();
            
            UpdateProjection();
            _orders.ForEach(o => o.EvtUpdated.AddListener(OnOrderUpdated));
        }

        public void AddOrder(ProductionOrder order)
        {
            if (order.Cycle != Cycle.Data) throw new Exception($"{nameof(CycleData)} mismatch");
            _orders.Add(order);
            
            UpdateProjection();
            order.EvtUpdated.AddListener(OnOrderUpdated);
        }

        public void RemoveOrder(ProductionOrder order)
        {
            _orders.Remove(order);
            
            UpdateProjection();
            order.EvtUpdated.RemoveListener(OnOrderUpdated);
        }

        /// <summary>
        /// Process orders. Deduct input and gain output from orders.
        /// Called internally by cycle tasks.
        /// </summary>
        /// <returns></returns>
        public ProductionBatchReport ResolveOrders(CycleBehaviorSnapshot snapshot)
        {
            // Evaluate orders to process.
            var acceptedOrders = EvaluateOrdersToBeProcessed();
            var failedOrders = _orders.Where(o => !acceptedOrders.Contains(o)).ToList();
            var failedInput = failedOrders.SelectMany(o => o.Input).Collate();
            var failedOutput = failedOrders.SelectMany(o => o.Output).Collate();
            
            // Process accepted orders
            var consumed = new CurrencyCollection();
            var produced = new CurrencyCollection();
            var net = new CurrencyCollection();
            foreach (var order in acceptedOrders)
            {
                var input = order.Input;
                var output = order.Output;
                
                // Deduct input
                Wallet.Deduct(input);
                consumed.Add(input);
                net.Deduct(input);
                
                // Add output
                Wallet.Add(output);
                produced.Add(output);
                net.Add(output);
            }
            
            // Conclude orders
            acceptedOrders.ForEach(o => o.Conclude(true));
            failedOrders.ForEach(o => o.Conclude(false));
            
            Debug.Log($"[Production] [{Cycle.Data.Id}] batch completed ({acceptedOrders.Count}/{Orders.Count}). {net.FormatToString()}" +
                      $"\nConsumed: {consumed.FormatToString()}" +
                      $"\nProduced: {produced.FormatToString()}" +
                      $"\nFailed Input: {failedInput.FormatToString()}" +
                      $"\nFailed Output: {failedOutput.FormatToString()}");

            var report = new ProductionBatchReport()
            {
                Consumed = consumed,
                Produced = produced,
                NetProduced = net,
                FailedInput = failedInput,
                FailedOutput = failedOutput,
                CompletedOrders = acceptedOrders,
                FailedOrders = failedOrders,
                ProductionBatch = this,
            };
            EvtCompleted?.Invoke(report);

            // Snapshot
            snapshot.CurrencyChanged.Add(net);
            snapshot.ProductionBatchReports.Add(report);

            return report;
        }

        private IReadOnlyCollection<ProductionOrder> EvaluateOrdersToBeProcessed()
        {
            // Sort orders
            var sortedOrders = _orders.OrderByDescending(o => o.Priority).ToList();
            
            // Simulate wallet changes (not applied)
            var current = new CurrencyCollection(Wallet.Currencies);
            
            // Evaluate input. Collect orders that can be processed.
            var acceptedOrders = new List<ProductionOrder>();
            foreach (var order in sortedOrders)
            {
                var input = order.Input;

                // Skip order if wallet doesn't have enough.
                if (!current.HasEnough(input)) continue;
                
                // Track consumption (not yet deducted from wallet)
                current.Deduct(input);
                
                // Accept order
                acceptedOrders.Add(order);
            }
            
            return acceptedOrders;
        }

        private void UpdateProjection()
        {
            InputProjection = _orders.SelectMany(o => o.Input).Collate();
            OutputProjection = _orders.SelectMany(o => o.Output).Collate();
            NetProjection = OutputProjection.Separate(InputProjection) ;

            EvtProjectionUpdated?.Invoke(this);
        }

        private void OnOrderUpdated(ProductionOrder order)
        {
            UpdateProjection();
        }
    }

    public sealed class ProductionBatchReport
    {
        public IReadOnlyCollection<Currency> Consumed { get; internal set; }
        public IReadOnlyCollection<Currency> Produced { get; internal set; }
        public IReadOnlyCollection<Currency> NetProduced { get; internal set; }
        public IReadOnlyCollection<Currency> FailedInput { get; internal set; }
        public IReadOnlyCollection<Currency> FailedOutput { get; internal set; }
        public IReadOnlyCollection<ProductionOrder> CompletedOrders { get; internal set; }
        public IReadOnlyCollection<ProductionOrder> FailedOrders { get; internal set; }

        public ProductionBatch ProductionBatch { get; internal set; }
        public Cycle Cycle => ProductionBatch?.Cycle;
    }
}