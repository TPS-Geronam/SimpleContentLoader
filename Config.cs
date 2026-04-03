using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SimpleContentLoader
{
    public abstract class Config : ScriptableObject
    {
        [field: SerializeField, Tooltip("Identifier of this content config.")]
        public string ConfigId { get; private set; }

        [field: SerializeField, Tooltip("Content labels this config manages.")]
        public List<string> ContentLabels { get; private set; } = new();

        [field: SerializeField, Tooltip("Merge mode for loading content locations.")]
        public Addressables.MergeMode MergeMode { get; private set; } = Addressables.MergeMode.Intersection;

        [field: SerializeField, Tooltip("Whether a content loader should contain only the exact labels as in this config (true)" +
            ", or whether the content loader can contain more than the given labels (false).")]
        public bool ExactLoaderMatch { get; private set; } = true;
    }
}
