using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Resources;

namespace Kamilla
{
    /// <summary>
    /// Contains extensions to <see cref="System.Enum"/> types.
    /// </summary>
    public static class EnumExtensions
    {
        #region Internals
        class EnumInfo : SortedDictionary<long, object>
        {
        }

        class CacheEntry : Tuple<ResourceManager, EnumInfo, bool>
        {
            public CacheEntry(ResourceManager mgr, EnumInfo info, bool flags)
                : base(mgr, info, flags)
            {
            }
        }

        class Cache : Dictionary<Type, CacheEntry>
        {
        }

        // enumType => [resourceManager, enumValue => string key | FieldInfo field]
        static readonly Cache s_cache = new Cache();
        static readonly Type s_attrType = typeof(LocalizedNameAttribute);
        static readonly Type s_attrContnrType = typeof(LocalizedNameContainerAttribute);
        static readonly Type s_attrFlagsType = typeof(FlagsAttribute);
        static readonly Type s_resMgrType = typeof(ResourceManager);
        static readonly MethodInfo s_EnumGetValue;

        static EnumExtensions()
        {
            s_EnumGetValue = typeof(Enum).GetMethod("GetValue",
                BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, new ParameterModifier[0]);

            if (s_EnumGetValue == null)
                throw new InvalidOperationException("Cannot find Enum.GetValue");
        }

        static CacheEntry CreateCacheEntry(Type enumType)
        {
            CacheEntry entry;

            // First get the Resource Manager.

            var attrs = (LocalizedNameContainerAttribute[])
                enumType.GetCustomAttributes(s_attrContnrType, false);

            if (attrs.Length == 0)
            {
                s_cache[enumType] = entry = new CacheEntry(null, null, false);
                return entry;
            }

            var container = attrs[0].ResourceManager;
            var method = container.GetProperty("ResourceManager",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                s_resMgrType, Type.EmptyTypes, new ParameterModifier[0]);

            if (method == null)
                throw new InvalidOperationException("Cannot find ResourceManager in type " + container);

            var resMgr = (ResourceManager)method.GetValue(null, null);
            if (resMgr == null)
                throw new InvalidOperationException("Failed to access ResourceManager in " + container);

            // Resource Manager retrieved, now get the fields.

            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var ei = new EnumInfo();

            foreach (var field in fields)
            {
                var obj = (IConvertible)field.GetRawConstantValue();

                long value;
                if (obj.GetTypeCode() == TypeCode.UInt64)
                    value = (long)(ulong)obj;
                else
                    value = obj.ToInt64(null);

                // Sometimes there are two fields with the same value
                if (ei.ContainsKey(value))
                    continue;

                ei.Add(value, field);
            }

            // Cache the found stuff

            s_cache[enumType] = entry = new CacheEntry(resMgr, ei, enumType.IsDefined(s_attrFlagsType, false));
            return entry;
        }

        static string GetName(long value, ResourceManager resources, EnumInfo info, bool checkForFlags)
        {
            object obj;
            if (!info.TryGetValue(value, out obj))
            {
                if (checkForFlags)
                {
                    ulong uvalue = (ulong)value;
                    var builder = new StringBuilder(1024);

                    bool builderEmpty = true;
                    ulong bit = 1;
                    for (int i = 0; i < sizeof(ulong) * 8; ++i, bit <<= 1)
                    {
                        if ((uvalue & bit) != 0)
                        {
                            if (!builderEmpty)
                                builder.Append(", ");

                            builder.Append(GetName((long)bit, resources, info, false));
                            builderEmpty = false;
                        }
                    }

                    return builder.ToString();
                }

                return value.ToString();
            }

            var keyField = obj as Tuple<string, string>;
            if (keyField == null)
            {
                var plainName = obj as Tuple<string>;
                if (plainName != null)
                    return plainName.Item1;

                var field = (FieldInfo)obj;
                var attrs = (LocalizedNameAttribute[])field.GetCustomAttributes(s_attrType, false);
                if (attrs.Length == 0)
                {
                    info[value] = new Tuple<string>(field.Name);
                    return field.Name;
                }

                info[value] = keyField = new Tuple<string, string>(attrs[0].Key, field.Name);
            }

            var result = resources.GetString(keyField.Item1);
            if (string.IsNullOrEmpty(result))
                result = keyField.Item2;
            return result;
        }
        #endregion

        /// <summary>
        /// Converts the provided enum value to <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="enm">
        /// Enum value to convert to <see cref="System.Int64"/>.
        /// </param>
        /// <returns>
        /// Enum value converted to <see cref="System.Int64"/>.
        /// </returns>
        public static long ToInt64(this Enum enm)
        {
            var obj = s_EnumGetValue.Invoke(enm, null);
            if (obj is ulong)
                return (long)(ulong)obj;

            return ((IConvertible)obj).ToInt64(null);
        }

        /// <summary>
        /// Gets the display name defined by <see cref="Kamilla.LocalizedNameAttribute"/>
        /// for the specified value of an enumeration.
        /// </summary>
        /// <param name="enm">
        /// Value of an enumeration.
        /// </param>
        /// <returns>
        /// The display name provided by <see cref="Kamilla.LocalizedNameAttribute"/>
        /// if exists; otherwise, name of the field.
        /// </returns>
        public static string GetLocalizedName(this Enum enm)
        {
            var enumType = enm.GetType();

            CacheEntry entry;
            if (!s_cache.TryGetValue(enumType, out entry))
                entry = CreateCacheEntry(enumType);

            if (entry.Item1 == null)
                return enm.ToString();

            return GetName(enm.ToInt64(), entry.Item1, entry.Item2, entry.Item3);
        }
    }
}
