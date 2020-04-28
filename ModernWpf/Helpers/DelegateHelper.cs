using System;
using System.Reflection;

namespace ModernWpf
{
    internal static class DelegateHelper
    {
        public static T CreateDelegate<T>(MethodInfo method) where T : Delegate
        {
            return (T)Delegate.CreateDelegate(typeof(T), method);
        }

        public static T CreateDelegate<T>(object firstArgument, MethodInfo method) where T : Delegate
        {
            return (T)Delegate.CreateDelegate(typeof(T), firstArgument, method);
        }

        public static T CreateDelegate<T>(Type target, string method, bool nonPublic = false) where T : Delegate
        {
            if (nonPublic)
            {
                var methodInfo = target.GetMethod(method, NonPublicLookup);
                if (methodInfo != null)
                {
                    return CreateDelegate<T>(methodInfo);
                }
                return null;
            }
            else
            {
                return (T)Delegate.CreateDelegate(typeof(T), target, method);
            }
        }

        public static T CreateDelegate<T>(object target, string method, bool nonPublic = false) where T : Delegate
        {
            if (nonPublic)
            {
                var methodInfo = target.GetType().GetMethod(method, NonPublicLookup);
                if (methodInfo != null)
                {
                    return CreateDelegate<T>(target, methodInfo);
                }
                return null;
            }
            else
            {
                return (T)Delegate.CreateDelegate(typeof(T), target, method);
            }
        }

        public static Func<TType, TProperty> CreatePropertyGetter<TType, TProperty>(string name, bool nonPublic = false)
        {
            var property = typeof(TType).GetProperty(name, nonPublic ? NonPublicLookup : DefaultLookup);
            if (property != null)
            {
                var getMethod = property.GetGetMethod(nonPublic);
                if (getMethod != null)
                {
                    return CreateDelegate<Func<TType, TProperty>>(getMethod);
                }
            }
            return null;
        }

        public static Action<TType, TProperty> CreatePropertySetter<TType, TProperty>(string name, bool nonPublic = false)
        {
            var property = typeof(TType).GetProperty(name, nonPublic ? NonPublicLookup : DefaultLookup);
            if (property != null)
            {
                var setMethod = property.GetSetMethod(nonPublic);
                if (setMethod != null)
                {
                    return CreateDelegate<Action<TType, TProperty>>(setMethod);
                }
            }
            return null;
        }

        private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
        private const BindingFlags NonPublicLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
    }
}
