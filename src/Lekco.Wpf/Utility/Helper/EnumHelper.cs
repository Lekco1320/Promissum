using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Lekco.Wpf.Utility.Helper
{
    public static class EnumHelper
    {
        public static string GetDiscription(this Enum @enum)
        {
            Type temType = @enum.GetType();
            MemberInfo[] memberInfos = temType.GetMember(@enum.ToString());
            if (memberInfos != null && memberInfos.Length > 0)
            {
                object[] objs = memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (objs != null && objs.Length > 0)
                {
                    return ((DescriptionAttribute)objs[0]).Description;
                }
            }
            return @enum.ToString();
        }

        // 获取单个标志枚举
        public static IEnumerable<T> GetIndividualFlags<T>(this T flags) where T : struct, Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (value.Equals(default(T))) continue;

                if (flags.HasFlag(value) && IsSingleFlag(value))
                {
                    yield return value;
                }
            }
        }

        // 获取组合标志枚举，优先返回更大的组合
        public static T GetMatchingFlag<T>(this T flags) where T : struct, Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)).Cast<T>().OrderByDescending(f => Convert.ToInt32(f)))
            {
                if (flags.HasFlag(value))
                {
                    return value;
                }
            }
            return default;
        }

        // 判断是否为单个标志位
        private static bool IsSingleFlag<T>(T flag) where T : struct, Enum
        {
            int intValue = Convert.ToInt32(flag);
            return (intValue & intValue - 1) == 0;
        }

        // 拆分标志并优先输出更大的组合
        public static IEnumerable<T> SplitFlags<T>(this T flags) where T : struct, Enum
        {
            T remainingFlags = flags;
            while (!remainingFlags.Equals(default(T)))
            {
                T matchingFlag = remainingFlags.GetMatchingFlag();
                yield return matchingFlag;
                remainingFlags = RemoveFlag(remainingFlags, matchingFlag);
            }
        }

        // 移除指定的标志位
        private static T RemoveFlag<T>(T flags, T flagToRemove) where T : struct, Enum
        {
            int intValue = Convert.ToInt32(flags);
            int removeValue = Convert.ToInt32(flagToRemove);
            return (T)Enum.ToObject(typeof(T), intValue & ~removeValue);
        }

        /// <summary>
        /// Retrieves the index value associated with an enum field or property that is decorated with the <see cref="IndexAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="enumValue">The enum value whose index is to be retrieved.</param>
        /// <returns>The index associated with the enum field if found; otherwise, -1.</returns>
        public static int GetIndex<T>(this T enumValue) where T : struct, Enum
        {
            var type = enumValue.GetType();
            var memberInfo = type.GetMember(enumValue.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(IndexAttribute), false);
                if (attributes.Length > 0)
                {
                    return ((IndexAttribute)attributes[0]).Index;
                }
            }
            return -1;
        }

        /// <summary>
        /// 根据索引值获取对应的枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型。</typeparam>
        /// <param name="index">索引值。</param>
        /// <returns>对应的枚举值，如果没有找到则返回默认值。</returns>
        public static T GetEnumByIndex<T>(int index) where T : struct, Enum
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(typeof(IndexAttribute), false);
                if (attributes.Length > 0 && ((IndexAttribute)attributes[0]).Index == index)
                {
                    return (T)field.GetValue(null)!;
                }
            }

            throw new ArgumentException($"没有找到与索引 {index} 对应的枚举值。");
        }
    }
}
