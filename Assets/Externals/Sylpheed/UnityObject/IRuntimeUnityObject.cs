using System;
using System.Collections.Generic;

namespace Sylpheed.UnityObject
{
    public interface IRuntimeUnityObject<T> where T : UnityEngine.Object
    {
        T SourceAsset { get; set; } // Need to figure out how to make this readonly
    }

    public static class RuntimeUnityObject
    {
        public static T CreateRuntimeObject<T>(this T asset)
            where T : UnityEngine.Object, IRuntimeUnityObject<T>
        {
            if (!asset.IsAssetObject()) throw new Exception("Can only create runtime object from asset objects");
            
            var instance = UnityEngine.Object.Instantiate(asset);
            instance.SourceAsset = asset;
            
            return instance;
        }

        public static bool IsRuntimeObject(this UnityEngine.Object obj)
        {
            return obj.GetInstanceID() <= 0;
        }
        
        public static bool IsAssetObject(this UnityEngine.Object obj)
        {
            return obj.GetInstanceID() > 0;
        }

        public static bool IsSameSource<T>(this T obj, T other)
            where T : UnityEngine.Object, IRuntimeUnityObject<T>
        {
            var source1 = obj.IsAssetObject() ? obj : obj?.SourceAsset ?? throw new Exception("SourceAsset is null");
            var source2 = other.IsAssetObject() ? other : other?.SourceAsset ?? throw new Exception("SourceAsset is null");
            
            return source1 == source2;
        }
    }
}