using System.Collections.Generic;
using System.Linq;

namespace Sylpheed.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<System.Type, object> Services = new();

        /// <summary>
        /// Gets a service. Service must be previously registered.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : class
        {
            object obj;
            if (Services.TryGetValue(typeof(T), out obj))
            {
                // Clear registered singleton if it was destroyed
                if (obj == null) Services.Remove(typeof(T));
            }

            //Assert.IsNotNull(obj, typeof(T).Name + " doesn't exist");
            return obj as T;
        }

        /// <summary>
        /// Registers the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The service</param>
        public static void Register<T>(T obj) where T : class
        {
            //Assert.IsFalse(services.ContainsKey(typeof(T)), typeof(T).Name + " already exists");
            Services[typeof(T)] = obj;
        }

        /// <summary>
        /// Removes the service from the reference list.
        /// This will not destroy the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Remove<T>() where T : class
        {
            Services.Remove(typeof(T));
        }

        /// <summary>
        /// Removes the service from the reference list.
        /// This will not destroy the object.
        /// </summary>
        /// <param name="obj"></param>
        public static void Remove(object obj)
        {
            var kv = Services.FirstOrDefault(s => ReferenceEquals(s.Value, obj));
            if ((kv.Key, kv.Value) != default)
            {
                Services.Remove(kv.Key);
            }
        }

        /// <summary>
        /// Removes all services from the reference list
        /// </summary>
        public static void RemoveAll()
        {
            Services.Clear();
        }
    }
}