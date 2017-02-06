using System.Collections.Generic;

namespace System.Reflection
{
    /*
    // For .Net 4
    public static class ReflectionExtensions
    {
        public static IEnumerable<T> GetCustomAttributes<T>(this PropertyInfo element, bool inherit) where T : Attribute
        {
            Type t = typeof(T);
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, t, inherit);
            if (customAttributes != null && customAttributes.Length != 0)
            {
                foreach (Attribute a in customAttributes)
                    if (a.GetType().IsAssignableFrom(t))
                        yield return (T)a;
            }
        }
        public static T GetCustomAttribute<T>(this PropertyInfo element) where T : Attribute
        {
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, typeof(T), true);
            if (customAttributes == null || customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 1)
            {
                return (T)customAttributes[0];
            }

            return default(T);
        }
        public static T GetCustomAttribute<T>(this ParameterInfo element) where T : Attribute
        {
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, typeof(T), true);
            if (customAttributes == null || customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 1)
            {
                return (T)customAttributes[0];
            }

            return default(T);
        }
        public static T GetCustomAttribute<T>(this Type element) where T : Attribute
        {
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, typeof(T), true);
            if (customAttributes == null || customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 1)
            {
                return (T)customAttributes[0];
            }

            return default(T);
        }
        public static T GetCustomAttribute<T>(this MethodInfo element) where T : Attribute
        {
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, typeof(T), true);
            if (customAttributes == null || customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 0)
            {
                return default(T);
            }
            if (customAttributes.Length == 1)
            {
                return (T)customAttributes[0];
            }

            return default(T);
        }
    }
    */
}