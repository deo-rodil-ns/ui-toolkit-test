using System.Collections.Generic;
using System.Linq;
using GothicVampire.Cycles;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.Unrest
{
    public sealed class UnrestSnapshot
    {
        public float Value { get; private set; }
        public float PreviousValue { get; private set; }
        public float Delta => Value - PreviousValue;
        public float Normalized => Mathf.Clamp01(Value / Settings.MaxValue);
        public float UnclampedValue { get; private set; }
        public float UnclampedDelta => UnclampedValue - PreviousValue;
        
        public IReadOnlyCollection<UnrestSource> Sources => _sources;
        public IReadOnlyCollection<UnrestSource> PreResolvedSources => _sources.Where(s => !s.ApplyOnResolve).ToList();
        public IReadOnlyCollection<UnrestSource> PostResolvedSources => _sources.Where(s => s.ApplyOnResolve).ToList();
        
        public Cycle Cycle { get; private set; }
        public UnrestSettings Settings { get; private set; }
        public bool IsDirty { get; private set; }

        public IReadOnlyCollection<UnrestCategorySnapshot> AllCategories { get; private set; } = new List<UnrestCategorySnapshot>();
        public IReadOnlyCollection<UnrestCategorySnapshot> PreResolvedCategories { get; private set; } = new List<UnrestCategorySnapshot>();
        public IReadOnlyCollection<UnrestCategorySnapshot> PostResolvedCategories { get; private set; } = new List<UnrestCategorySnapshot>();
        
        private readonly List<UnrestSource> _sources = new();

        /// <summary>
        /// Create UnrestSnapshot
        /// </summary>
        /// <param name="cycle"></param>
        /// <param name="settings"></param>
        /// <param name="startValue">Start value can be used to generate delta value.</param>
        public UnrestSnapshot(Cycle cycle, UnrestSettings settings, float startValue = 0f)
        {
            Settings = settings;
            PreviousValue = startValue;
            Cycle = cycle;
        }

        public void AddSource(UnrestSource source)
        {
            // Ignore invalid unrest
            if (!source?.IsValid ?? false) return;
            
            _sources.Add(source);
            IsDirty = true;
        }
        
        public void AddSources(IReadOnlyCollection<UnrestSource> sources) => sources.ForEach(AddSource);

        // Build cached values. New sources added after Build() will not be included.
        public void Build()
        {
            // Compute end value. Only add sources that are applied on resolve.
            UnclampedValue = PreviousValue + Sources.Where(s => s.ApplyOnResolve).Sum(s => s.Value);
            Value = Mathf.Clamp(UnclampedValue, 0, Settings.MaxValue);
            
            // All categories
            var categories = Sources.Select(s => s.Category).Distinct().ToList();
            AllCategories = categories.Select(c => new UnrestCategorySnapshot(c, Sources)).ToList();
            
            // Pre-resolved categories
            PreResolvedCategories = PreResolvedSources
                .Select(s => s.Category).Distinct()
                .Select(c => new UnrestCategorySnapshot(c, PreResolvedSources))
                .ToList();
            
            // Post-resolved categories
            PostResolvedCategories = PostResolvedSources
                .Select(s => s.Category).Distinct()
                .Select(c => new UnrestCategorySnapshot(c, PostResolvedSources))
                .ToList();

            IsDirty = false;
        }

        public void LogToConsole(string prefix = "")
        {
            prefix = string.IsNullOrEmpty(prefix) ? string.Empty : $"{prefix} ";
            var header = $"{prefix} Value: {Value} ({Delta.ToStringWithPrefix("N0")})";
            var categories = string.Join("\n\t", AllCategories.Select(c => c.ToString()));
            Debug.Log($"{header}\n\t{categories}");
        }
    }
    
    public sealed class UnrestCategorySnapshot
    {
        public UnrestCategory Category { get; private set; }
        public float ProjectedValue { get; private set; }
        public float AppliedValue { get; private set; }
        public float AppliedImmediateValue { get; private set; }
        public float AppliedResolvedValue { get; private set; }
        public IReadOnlyCollection<UnrestSource> Sources { get; private set; }

        internal UnrestCategorySnapshot(UnrestCategory category, IReadOnlyCollection<UnrestSource> sources)
        {
            Category = category;
            Sources = sources.ToList();

            // Calculate value
            foreach (var source in sources)
            {
                ProjectedValue += source.Value;
                AppliedValue += source.AppliedValue;
                AppliedImmediateValue += source.AppliedImmediateValue;
                AppliedResolvedValue += source.AppliedResolvedValue;
            }
        }

        public override string ToString()
        {
            return $"{Category.ShortDescription}: {AppliedValue.ToStringWithPrefix("N0")}";
        }
    }
}