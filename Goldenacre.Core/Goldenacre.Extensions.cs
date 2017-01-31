﻿namespace Goldenacre.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    using Goldenacre.Core;

    #region Assembly Extensions

    public static class AssemblyExtensions
    {
        /// <summary>
        ///     Get the full resource name path for a given resourceFileName.
        /// </summary>
        public static string GetFullResourcePathForFileName(this Assembly @this, string resourceFileName)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this), Goldenacre_Resources.ArgumentCannotBeNull);
            }

            resourceFileName = resourceFileName.Trim().ToUpperInvariant();

            var resourceNames = @this.GetManifestResourceNames();
            
            var resourceName = resourceNames.FirstOrDefault(n => n.ToUpperInvariant().EndsWith(string.Concat(".", resourceFileName.ToUpperInvariant()), StringComparison.OrdinalIgnoreCase));

            return resourceName;
        }

        /// <summary>
        /// Gets the text from the specified assembly resource resourceFileName.
        /// </summary>
        /// <param name="this">The assembly to retrieve the resource from.</param>
        /// <param name="fileName">The resourceFileName of the resource.</param>
        /// <returns>
        /// The text contents of the resource file.
        /// </returns>
        public static string GetEmbeddedResourceText(this Assembly @this, string fileName)
        {
            return GetEmbeddedResourceText(@this, fileName, false);
        }

        /// <summary>
        /// Gets the text from the specified assembly resource resourceFileName.
        /// </summary>
        /// <param name="this">The assembly to retrieve the resource from.</param>
        /// <param name="fileName">The resourceFileName of the resource.</param>
        /// <param name="throwErrorIfNotFound">if set to <c>true</c> [throw error if not found].</param>
        /// <returns>
        /// The text contents of the resource file.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentException">fileName</exception>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string GetEmbeddedResourceText(this Assembly @this, string fileName, bool throwErrorIfNotFound)
        {
            string resourceText = null;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var resourceName = GetFullResourcePathForFileName(@this, fileName);

                if (resourceName == null)
                {
                    if (throwErrorIfNotFound)
                    {
                        throw new InvalidOperationException(Goldenacre_Resources.SpecifiedResourceNotFound);
                    }
                    return null;
                }

                using (var stream = @this.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            resourceText = reader.ReadToEnd();
                        }
                    }
                }
            }
            else if (throwErrorIfNotFound)
            {
                throw new ArgumentException(Goldenacre_Resources.ArgumentIsNotValid, "fileName");
            }

            return resourceText;
        }

        /// <summary>
        /// Gets the embedded resource bytes.
        /// </summary>
        /// <param name="this">The assembly.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentException">fileName</exception>
        public static byte[] GetEmbeddedResourceBytes(this Assembly @this, string fileName)
        {
            return GetEmbeddedResourceBytes(@this, fileName, false);
        }

        /// <summary>
        /// Gets the embedded resource bytes.
        /// </summary>
        /// <param name="this">The assembly.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="throwErrorIfNotFound">if set to <c>true</c> [throw error if not found].</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentException">fileName</exception>
        public static byte[] GetEmbeddedResourceBytes(this Assembly @this, string fileName, bool throwErrorIfNotFound)
        {
            byte[] resourceBytes = null;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var resourceName = GetFullResourcePathForFileName(@this, fileName);

                if (resourceName == null)
                {
                    if (throwErrorIfNotFound)
                    {
                        throw new InvalidOperationException(Goldenacre_Resources.SpecifiedResourceNotFound);
                    }
                    return null;
                }

                using (var stream = @this.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var mstream = stream as MemoryStream;
                        if (mstream != null)
                        {
                            resourceBytes = mstream.ToArray();
                        }
                        else
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                resourceBytes = memoryStream.ToArray();
                            }
                        }
                    }
                }
            }
            else if (throwErrorIfNotFound)
            {
                throw new ArgumentException(Goldenacre_Resources.ArgumentIsNotValid, "fileName");
            }

            return resourceBytes;
        }

        /// <summary>
        ///     Gets the date and time that the specified assembly was compiled.
        /// </summary>
        /// <param name="this">The assemebly to inspect.</param>
        /// <returns>The datetime the assembly was compiled.</returns>
        public static DateTime GetCompilationDateTimeUtc(this Assembly @this)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            const int bufferSize = 2048;
            var buffer = new byte[bufferSize];

            using (var fs = new FileStream(@this.Location, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, bufferSize);
            }

            var i = BitConverter.ToInt32(buffer, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, i + linkerTimestampOffset);
            var dt = secondsSince1970.FromUnixTimestamp().EnsureUtc();

            return dt;
        }
    }

    #endregion Assembly Extensions

    #region Collection Extensions

    public static class CollectionExtensions
    {
        public static ICollection<T> AddIfNotContains<T>(this ICollection<T> @this, T item)
        {
            if (!@this.Contains(item))
            {
                @this.Add(item);
            }
            return @this;
        }

        public static ICollection<T> AddIf<T>(this ICollection<T> @this, bool condition, T item)
        {
            if (condition)
            {
                @this.Add(item);
            }

            return @this;
        }

        public static ICollection<T> AddIf<T>(this ICollection<T> @this, Func<bool> condition, T item)
        {
            return AddIf(@this, condition(), item);
        }
    }

    #endregion Collection Extensions

    #region Array<T> Extensions

    public static class ArrayExtensions
    {
        public static MemoryStream ToMemoryStream(this byte[] @this)
        {
            return new MemoryStream(@this) { Position = 0 };
        }

        public static T[] Map<T>(this T[] @this, Func<int, T, T> func)
        {
		    for (var i = 0; i < @this.Length; i++)
            {
                @this[i] = func(i, @this[i]);
            }

            return @this;
        }

        public static T[] Map<T>(this T[] @this, Func<T, T> func)
        {
            for (var i = 0; i < @this.Length; i++)
            {
                @this[i] = func(@this[i]);
            }

            return @this;
        }
    }

    #endregion Array<T> Extensions

    #region DateTime Extensions

    public static class DateTimeExtensions
    {
        public static readonly DateTime EpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     The elapsed timespan since the given datetime.
        /// </summary>
        public static TimeSpan Elapsed(this DateTime @this)
        {
            return DateTime.UtcNow - @this.EnsureUtc();
        }

        /// <summary>
        ///     Converts a DateTime into a unix timestamp.
        ///     i.e. the number of seconds since 1970-01-01.
        /// </summary>
        public static long ToUnixTimestamp(this DateTime @this)
        {
            return (long)(@this.EnsureUtc() - EpochUtc).TotalSeconds;
        }

        /// <summary>
        ///     Converts a unix timestamp into a datetime.
        ///     i.e. the number of seconds since 1970-01-01.
        /// </summary>
        public static DateTime FromUnixTimestamp(this long @this)
        {
            return EpochUtc.AddSeconds(@this);
        }

        /// <summary>
        ///     Converts a unix timestamp into a datetime.
        ///     i.e. the number of seconds since 1970-01-01.
        /// </summary>
        public static DateTime FromUnixTimestamp(this int @this)
        {
            return EpochUtc.AddSeconds(@this);
        }

        /// <summary>
        ///     Converts the specified DateTime to Local if it isn't already.
        /// </summary>
        /// <param name="this">The DateTime to convert.</param>
        /// <param name="targetTimeZone">The target time zone to convert to. If null then the machine time zone is used.</param>
        /// <returns>A local DateTime.</returns>
        public static DateTime EnsureLocal(this DateTime @this, TimeZoneInfo targetTimeZone = null)
        {
            if (@this.Kind == DateTimeKind.Unspecified)
            {
                @this = DateTime.SpecifyKind(@this, DateTimeKind.Utc);
            }
            if (targetTimeZone == null)
            {
                targetTimeZone = TimeZoneInfo.Local;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(@this.ToUniversalTime(), targetTimeZone);
        }

        /// <summary>
        ///     Converts the specified DateTime to Local if it isn't already.
        /// </summary>
        /// <param name="this">The DateTime to convert. If null then returns null.</param>
        /// <param name="targetTimeZone">The target time zone to convert to. If null then the machine time zone is used.</param>
        /// <returns>A local DateTime or null if specified DateTime is null.</returns>
        public static DateTime? EnsureLocal(this DateTime? @this, TimeZoneInfo targetTimeZone = null)
        {
            return @this != null ? @this.Value.EnsureLocal(targetTimeZone) : (DateTime?)null;
        }

        /// <summary>
        ///     Converts the specified DateTime to UTC if it isn't already. If it is Unspecified then it is assumed to be UTC.
        /// </summary>
        /// <param name="this">The DateTime to convert.</param>
        /// <returns>A UTC DateTime.</returns>
        public static DateTime EnsureUtc(this DateTime @this)
        {
            if (@this.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(@this, DateTimeKind.Utc);
            }

            return @this.ToUniversalTime();
        }

        /// <summary>
        ///     Converts the specified DateTime to UTC if it isn't already. If it is Unspecified then it is assumed to be UTC.
        /// </summary>
        /// <param name="this">The DateTime to convert.</param>
        /// <returns>A UTC DateTime or null if specified DateTime is null.</returns>
        public static DateTime? EnsureUtc(this DateTime? @this)
        {
            return @this != null ? @this.Value.EnsureUtc() : (DateTime?)null;
        }

        /// <summary>
        ///     Indicates whether or not the specified DateTime is a weekend or not.
        /// </summary>
        /// <param name="this">The DateTime to check.</param>
        /// <param name="targetTimeZone"></param>
        /// <returns>True if DateTime is weekend.</returns>
        public static bool IsWeekend(this DateTime @this, TimeZoneInfo targetTimeZone = null)
        {
            return (@this.EnsureLocal(targetTimeZone).DayOfWeek == DayOfWeek.Saturday) || (@this.EnsureLocal(targetTimeZone).DayOfWeek == DayOfWeek.Sunday);
        }

        /// <summary>
        ///     Indicates whether or not the specified DateTime is a weekend or not.
        /// </summary>
        /// <param name="this">The DateTime to check.</param>
        /// <param name="targetTimeZone"></param>
        /// <returns>True if DateTime is weekend.</returns>
        public static bool IsWeekday(this DateTime @this, TimeZoneInfo targetTimeZone = null)
        {
            return (@this.EnsureLocal(targetTimeZone).DayOfWeek != DayOfWeek.Saturday) && (@this.EnsureLocal(targetTimeZone).DayOfWeek != DayOfWeek.Sunday);
        }

        /// <summary>
        ///     Convert a DateTime to a culture invariant date string: Thu 1st Jan 2015
        /// </summary>
        /// <param name="this">The DateTime to convert.</param>
        /// <returns>A DateTime formatted as a culture invariant date string.</returns>
        public static string ToNiceDateString(this DateTime @this)
        {
            var suff = @this.Day % 10 == 1 && @this.Day != 11 ? "st" : @this.Day % 10 == 2 && @this.Day != 12 ? "nd" : @this.Day % 10 == 3 && @this.Day != 13 ? "rd" : "th";

            return string.Format("{0:ddd d}{1} {0:MMM yyyy}", @this, suff);
        }

        /// <summary>
        ///     Convert a DateTime to a culture invariant date string: Thu 1st Jan 2015 13:34
        /// </summary>
        /// <param name="this">The DateTime to convert.</param>
        /// <returns>A DateTime formatted as a culture invariant date string.</returns>
        public static string ToNiceDateTimeString(this DateTime @this)
        {
            var suff = @this.Day % 10 == 1 && @this.Day != 11 ? "st" : @this.Day % 10 == 2 && @this.Day != 12 ? "nd" : @this.Day % 10 == 3 && @this.Day != 13 ? "rd" : "th";

            return string.Format("{0:ddd d}{1} {0:MMM yyyy} {0:HH:mm}", @this, suff);
        }
    }

    #endregion DateTime Extensions

    #region Enumerable Extensions

    public static class EnumerableExtensions
    {
        public static bool IsFirstElement<T>(this IEnumerable<T> @this, T element) where T : class
        {
            var first = @this.FirstOrDefault();

            return first != null && first.Equals(element);
        }

        public static bool IsLastElement<T>(this IEnumerable<T> @this, T element) where T : class
        {
            var last = @this.LastOrDefault();

            return last != null && last.Equals(element);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> @this, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return @this.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }
            if (action == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "action");
            }

            foreach (var element in @this)
            {
                action(element);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "source");
            }
            if (action == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "action");
            }

            var index = 0;
            foreach (var element in source)
            {
                action(index, element);

                index++;
            }
        }
    }

    #endregion Enumerable Extensions

    #region Exception Extensions

    public static class ExceptionExtensions
    {
        /// <summary>
        ///     <para>Creates a log-string from the Exception.</para>
        ///     <para>
        ///         The result includes the stacktrace, innerexception et cetera, separated by
        ///         <seealso cref="Environment.NewLine" />.
        ///     </para>
        /// </summary>
        /// <param name="this">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        /// <param name="includeDateTime"></param>
        /// <returns></returns>
        public static string ToLogString(this Exception @this, string additionalMessage = null, bool includeDateTime = false)
        {
            var msg = new StringBuilder();

            msg.AppendLine(@this.GetType().FullName);
            if (includeDateTime)
            {
                msg.AppendLine(DateTime.Now.ToString("F"));
            }
            msg.AppendLine("");

            msg.AppendLineIfNotNullOrWhiteSpace((additionalMessage ?? string.Empty).Trim() + Environment.NewLine);

            var orgEx = @this;

            while (orgEx != null)
            {
                msg.AppendLine(orgEx.Message);
                msg.AppendLineIfNotNullOrWhiteSpace(orgEx.HelpLink);
                orgEx = orgEx.InnerException;
            }

            foreach (var i in @this.Data)
            {
                msg.AppendLine("Data :");
                msg.AppendLine(i.ToString());
            }

            if (!string.IsNullOrWhiteSpace(@this.StackTrace))
            {
                msg.AppendLine(string.Empty);
                msg.AppendLine(@this.StackTrace.Trim());
            }

            if (@this.TargetSite != null)
            {
                msg.AppendLine(string.Empty);
                if (@this.TargetSite.DeclaringType != null)
                {
                    msg.Append(@this.TargetSite.DeclaringType.FullName);
                }
                msg.Append(@this.TargetSite.Name);
                msg.AppendLine(string.Empty);
            }
            return msg.ToString().Trim();
        }
    }

    #endregion Exception Extensions

    #region Object Extensions

    public static class ObjectExtensions
    {
        public static T EnsureBetween<T>(this T @this, T min, T max) where T : IComparable<T>
        {
            if (@this.CompareTo(min) < 0)
            {
                return min;
            }

            if (@this.CompareTo(max) > 0)
            {
                return max;
            }

            return @this;
        }

        public static bool IsTruthy<T>(this T @this)
        {
            if (@this == null)
            {
                return false;
            }

            if (typeof(T) == typeof(bool))
            {
                return Convert.ToBoolean(Convert.ChangeType(@this, typeof(bool)));
            }

            var s = Convert.ToString(@this);

            decimal num;
            if (s.IsNumeric() && decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out num))
            {
                return num > 0;
            }

            if (!string.IsNullOrWhiteSpace(s))
            {
                return s.EqualsAnyCI(
                    Goldenacre_Resources.TruthyStrings.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));
            }

            return false;
        }

        public static bool IsSet(this Enum @this, Enum matchTo)
        {
            return (Convert.ToUInt32(@this) & Convert.ToUInt32(matchTo)) != 0;
        }

        public static T SerializedClone<T>(this T @this) where T : ISerializable
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, @this);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        public static bool IsBetween<T>(this T @this, T from, T to) where T : IComparable<T>
        {
            return @this.CompareTo(from) >= 0 && @this.CompareTo(to) <= 0;
        }

        public static bool IsNullable<T>(this T @this)
        {
            if (@this == null)
            {
                return true; // obvious
            }
            var type = typeof(T);
            if (!type.IsValueType)
            {
                return true; // ref-type
            }
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return true; // Nullable<T>
            }
            return false; // value-type
        }

        /// <summary>
        ///     Calls DateTime.SpecifyKind on all DateTime or DateTime? properties and changes to UTC.
        ///     This is useful after an object has been materialized from the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="criteria"></param>
        public static void ConvertDateTimePropertiesToUtc<T>(this T @this, Expression<Func<PropertyInfo, bool>> criteria = null) where T : class
        {
            if (@this != null)
            {
                var properties = @this.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).Where(x => x.PropertyType == typeof(DateTime) || x.PropertyType == typeof(DateTime?));

                if (criteria != null)
                {
                    properties = properties.Where(criteria.Compile());
                }

                foreach (var property in properties)
                {
                    var dt = property.PropertyType == typeof(DateTime?) ? (DateTime?)property.GetValue(@this, null) : (DateTime)property.GetValue(@this, null);

                    if (dt != null && dt.Value.Kind == DateTimeKind.Unspecified)
                    {
                        var v = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
                        property.SetValue(@this, v, null);
                    }
                    else if (dt != null && dt.Value.Kind == DateTimeKind.Local)
                    {
                        var v = dt.Value.ToUniversalTime();
                        property.SetValue(@this, v, null);
                    }
                }
            }
        }
    }

    #endregion Object Extensions

    #region StringBuilder Extensions

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendIfNotNullOrWhiteSpace(this StringBuilder @this, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                @this.Append(text);
            }

            return @this;
        }

        public static StringBuilder AppendLineIfNotNullOrWhiteSpace(this StringBuilder @this, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                @this.AppendLine(text);
            }

            return @this;
        }

        public static StringBuilder AppendIf(this StringBuilder @this, bool condition, string text)
        {
            if (condition)
            {
                @this.Append(text);
            }

            return @this;
        }

        public static StringBuilder AppendLineIf(this StringBuilder @this, bool condition, string text)
        {
            if (condition)
            {
                @this.AppendLine(text);
            }

            return @this;
        }

        public static StringBuilder AppendIf(this StringBuilder @this, Func<bool> condition, string text)
        {
            if (condition())
            {
                @this.AppendLine(text);
            }

            return @this;
        }

        public static StringBuilder AppendLineIf(this StringBuilder @this, Func<bool> condition, string text)
        {
            if (condition())
            {
                @this.AppendLine(text);
            }

            return @this;
        }
    }

    #endregion StringBuilder Extensions

    #region String Extensions

    public static class StringExtensions
    {
        private const string SingleSpace = " ";

        private const string DoubleSpace = "  ";

        public static bool IsNullOrWhiteSpace(this string @this)
        {
            return string.IsNullOrWhiteSpace(@this);
        }

        // ReSharper disable once InconsistentNaming
        public static bool EqualsAnyCS(this string @this, params string[] input)
        {
            return input.Any(s => s.Equals(@this, StringComparison.Ordinal));
        }

        // ReSharper disable once InconsistentNaming
        public static bool EqualsAnyCS(this string @this, IEnumerable<string> input)
        {
            return input.Any(s => s.Equals(@this, StringComparison.Ordinal));
        }

        // ReSharper disable once InconsistentNaming
        public static bool EqualsAnyCI(this string @this, params string[] input)
        {
            return input.Any(s => s.Equals(@this, StringComparison.OrdinalIgnoreCase));
        }

        // ReSharper disable once InconsistentNaming
        public static bool EqualsCI(this string @this, IEnumerable<string> input)
        {
            return input.Any(s => s.Equals(@this, StringComparison.OrdinalIgnoreCase));
        }

        // ReSharper disable once InconsistentNaming
        public static bool EqualsCI(this string @this, string value2)
        {
            return @this.Equals(value2, StringComparison.OrdinalIgnoreCase);
        }

        // ReSharper disable once InconsistentNaming
        public static bool EqualsCS(this string @this, string value2)
        {
            return @this.Equals(value2, StringComparison.Ordinal);
        }

        public static bool ContainsAll(this string @this, params string[] values)
        {
            return values.All(one => @this.ToLowerInvariant().Contains(one.ToLowerInvariant()));
        }

        public static bool IsNumeric(this string @this)
        {
            if (!string.IsNullOrEmpty(@this))
            {
                decimal num;
                return decimal.TryParse(@this, NumberStyles.Any, CultureInfo.CurrentCulture, out num);
            }
            return false;
        }

        public static bool IsDate(this string @this)
        {
            if (!string.IsNullOrEmpty(@this))
            {
                DateTime dt;
                return DateTime.TryParse(@this, out dt);
            }
            return false;
        }

        public static bool IsGuid(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException();
            }

            var format =
                new Regex(
                    "^[A-Fa-f0-9]{32}$|" + "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" + "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            var match = format.Match(@this);

            return match.Success;
        }

        public static string SubstringToIndexOf(this string @this, string value, StringComparison comparison = StringComparison.CurrentCulture)
        {
            var idx = @this.IndexOf(value, comparison);

            if (idx > 0)
            {
                return @this.Substring(0, idx);
            }

            if (idx == 0)
            {
                return string.Empty;
            }

            return @this;
        }

        /// <summary>
        ///     Parses a string into an Enum
        /// </summary>
        /// <typeparam name="T">The type of the Enum</typeparam>
        /// <param name="this">String value to parse</param>
        /// <returns>The Enum corresponding to the stringExtensions</returns>
        public static T ToEnum<T>(this string @this)
        {
            return ToEnum<T>(@this, false);
        }

        /// <summary>
        ///     Parses a string into an Enum
        /// </summary>
        /// <typeparam name="T">The type of the Enum</typeparam>
        /// <param name="this">String value to parse</param>
        /// <param name="ignorecase">Ignore the case of the string being parsed</param>
        /// <returns>The Enum corresponding to the stringExtensions</returns>
        public static T ToEnum<T>(this string @this, bool ignorecase)
        {
            if (string.IsNullOrWhiteSpace(@this))
            {
                throw new ArgumentException(Goldenacre_Resources.StringArgumentCannotBeNullOrEmpty, "@this");
            }

            @this = @this.Trim();

            var t = typeof(T);

            if (!t.IsEnum)
            {
                throw new ArgumentException(Goldenacre_Resources.SpecifiedArgumentIsNotAnEnum);
            }

            return (T)Enum.Parse(t, @this, ignorecase);
        }

        public static int NthIndexOf(this string @this, string match, int occurrence)
        {
            var i = 1;
            var index = 0;

            while (i <= occurrence && (index = @this.IndexOf(match, index + 1, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                if (i == occurrence)
                {
                    return index;
                }

                i++;
            }

            return -1;
        }

        public static string RemoveAllWhitespace(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }

            @this = @this.Replace("\t", string.Empty);

            while (@this.Contains(SingleSpace))
            {
                @this = @this.Replace(SingleSpace, string.Empty);
            }

            return @this;
        }

        public static string TrimAndRemoveConsecutiveWhitespace(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }

            @this = @this.Replace("\t", SingleSpace).Trim();

            while (@this.Contains(DoubleSpace))
            {
                @this = @this.Replace(DoubleSpace, SingleSpace);
            }

            return @this;
        }

        public static string SplitOnCapitals(string @this)
        {
            var result = new StringBuilder(@this.Length);
            var countSinceLastSpace = 0;
            for (var i = 0; i < @this.Length - 1; i++)
            {
                result.Append(@this[i]);
                if (countSinceLastSpace > 2 && @this[i] != ' ' && @this[i] != '-' && (char.IsUpper(@this[i + 1]) || !char.IsDigit(@this[i]) && char.IsDigit(@this[i + 1])))
                {
                    result.Append(' ');
                    countSinceLastSpace = 0;
                }
                countSinceLastSpace++;
            }
            result.Append(@this[@this.Length - 1]);

            return result.ToString();
        }

        public static string ToTitleCase(this string @this)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(@this);
        }

        /// <summary>
        ///     Converts the first letter of each word in a
        ///     space delimited sentence into uppercase
        ///     and converts all others into lowercase.
        /// </summary>
        /// <param name="this">The sentence to convert.</param>
        /// <param name="ignore">A list of words to ignore. Useful for abbreviations etc.</param>
        /// <returns>Pascal cased sentence.</returns>
        public static string ToPascalCase(this string @this, params string[] ignore)
        {
            if (@this == null)
            {
                throw new ArgumentNullException();
            }

            var strReturn = string.Empty;

            if (@this.Trim().Length > 0)
            {
                var objSb = new StringBuilder();
                var leftSpace = new string(@this.TakeWhile(c => c == ' ').ToArray());
                var rightSpace = new string(@this.Reverse().TakeWhile(c => c == ' ').ToArray());
                var arrWords = @this.Trim().Split(new[] { ' ' }, StringSplitOptions.None);

                foreach (var word in arrWords)
                {
                    if (word != string.Empty && !ignore.Contains(word.ToAlphaNumeric()))
                    {
                        if (word.Length > 1 && word[0] == '\'' || word[0] == '"' || word[0] == '(' || word[0] == '[')
                        {
                            objSb.Append(char.ToUpperInvariant(word[0]));
                            objSb.Append(char.ToUpperInvariant(word[1]));
                            objSb.Append(word.Substring(2).ToLowerInvariant()); // Convert rest of word to LOWERCASE
                        }
                        else
                        {
                            objSb.Append(char.ToUpperInvariant(word[0])); // Convert 1st char to UPPERCASE
                            objSb.Append(word.Substring(1).ToLowerInvariant()); // Convert rest of word to LOWERCASE
                        }
                        objSb.Append(" ");
                    }
                    else
                    {
                        objSb.Append(word).Append(" ");
                    }
                }

                strReturn = leftSpace + objSb.ToString().Replace(" The ", " the ").Replace(" And ", " and ").Replace(" A ", " a ").Replace(" To ", " to ").Trim() + rightSpace;
            }

            return strReturn;
        }

        public static string ToAlphaNumeric(this string @this)
        {
            return new string(@this.Where(char.IsLetterOrDigit).ToArray());
        }

        /// <summary>
        ///     Encodes the specified string to a HEX encoded MD5Hash.
        /// </summary>
        /// <param name="this">The string to encode.</param>
        /// <returns>The encoded string.</returns>
        public static string ToHexMd5Hash(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException();
            }

            var md5Hasher = new MD5CryptoServiceProvider();
            var hashedDataBytes = md5Hasher.ComputeHash(Encoding.Default.GetBytes(@this));

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            foreach (var t in hashedDataBytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Encodes the specified string to a Base64 encoded MD5Hash.
        /// </summary>
        /// <param name="this">The string to encode.</param>
        /// <returns>The encoded string.</returns>
        public static string ToBase64Md5Hash(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }
            var md5Hasher = new MD5CryptoServiceProvider();
            var hashedDataBytes = md5Hasher.ComputeHash(Encoding.Default.GetBytes(@this));

            return Convert.ToBase64String(hashedDataBytes);
        }

        public static string ToAspNetPasswordHash(string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }

            byte[] salt;
            byte[] bytes;

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(@this, 16, 1000))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }

            var inArray = new byte[49];
            Buffer.BlockCopy(salt, 0, inArray, 1, 16);
            Buffer.BlockCopy(bytes, 0, inArray, 17, 32);

            return Convert.ToBase64String(inArray);
        }

        public static string Truncate(this string @this, int maxLength, bool appendEllipsis)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }

            if (appendEllipsis)
            {
                maxLength = Math.Max(0, maxLength - 3);
                return string.Concat(@this.Substring(0, maxLength), "...");
            }

            return @this.Substring(0, maxLength);
        }

        public static byte[] ToByteArray(this string @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(Goldenacre_Resources.ArgumentCannotBeNull, "@this");
            }

            return @this.Select(Convert.ToByte).ToArray();
        }
    }

    #endregion StringExtensions
}