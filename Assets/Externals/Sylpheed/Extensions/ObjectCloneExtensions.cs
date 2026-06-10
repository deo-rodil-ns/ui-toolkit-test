using System;
using UnityEngine;

namespace Sylpheed.Extensions
{
    public static class ObjectCloneExtensions
    {
        public static T Clone<T>(this T obj)
        {
            var json = JsonUtility.ToJson(obj);
            var copy = JsonUtility.FromJson(json, obj.GetType());
            return (T)copy;
        }
    }
}