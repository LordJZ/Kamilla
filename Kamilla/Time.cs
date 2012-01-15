using System;
using System.Collections.Generic;

namespace Kamilla
{
    /// <summary>
    /// Contains methods to work with time-representing variables.
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// Represents a year in seconds. This field is constant.
        /// </summary>
        public const long Year = 12 * Month;

        /// <summary>
        /// Represents a month (28 days) in seconds. This field is constant.
        /// </summary>
        public const long Month = 28 * Day;

        /// <summary>
        /// Represents a week in seconds. This field is constant.
        /// </summary>
        public const long Week = 7 * Day;

        /// <summary>
        /// Represents a day in seconds. This field is constant.
        /// </summary>
        public const long Day = 24 * Hour;

        /// <summary>
        /// Represents an hour in seconds. This field is constant.
        /// </summary>
        public const long Hour = 60 * Minute;

        /// <summary>
        /// Represents a minute in seconds. This field is constant.
        /// </summary>
        public const long Minute = 60 * Second;

        /// <summary>
        /// Represents a single second. This field is constant.
        /// </summary>
        public const long Second = 1;

        /// <summary>
        /// Represents a second in milliseconds. This field is constant.
        /// </summary>
        public const long InMilliseconds = 1000;

        static readonly DateTime s_unixEpochReference = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// Represents the beginning of the Unix Epoch in local time.
        /// </summary>
        public static readonly DateTime UnixEpoch = (0L).AsUnixTime();

        /// <summary>
        /// Converts the number of seconds passed since UTC Unix Epoch to local time.
        /// </summary>
        /// <param name="unixTimeUtc">
        /// Number of seconds passed since UTC Unix Epoch.
        /// </param>
        /// <returns>
        /// <see cref="System.DateTime"/> converted from UTC Unix Epoch.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting <see cref="System.DateTime"/> is less than <see href="System.DateTime.MinValue"/> or greater than <see href="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime AsUnixTime(this uint unixTimeUtc)
        {
            return AsUnixTime((long)unixTimeUtc);
        }

        /// <summary>
        /// Converts the number of seconds passed since UTC Unix Epoch to local time.
        /// </summary>
        /// <param name="unixTimeUtc">
        /// Number of seconds passed since UTC Unix Epoch.
        /// </param>
        /// <returns>
        /// <see cref="System.DateTime"/> converted from UTC Unix Epoch.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// unixTimeUtc is negative.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The resulting <see cref="System.DateTime"/> is less than <see href="System.DateTime.MinValue"/> or greater than <see href="System.DateTime.MaxValue"/>.
        /// </exception>
        public static DateTime AsUnixTime(this long unixTimeUtc)
        {
            if (unixTimeUtc < 0)
                throw new ArgumentOutOfRangeException();

            return TimeZone.CurrentTimeZone.ToLocalTime(s_unixEpochReference.AddSeconds(unixTimeUtc));
        }

        /// <summary>
        /// Converts the <see cref="System.DateTime"/> to UTC Unix Timestamp.
        /// </summary>
        /// <param name="dateTime">
        /// <see cref="System.DateTime"/> to convert to UTC Unix Timestamp.
        /// </param>
        /// <returns>
        /// Number of seconds passed since UTC Unix Epoch.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// The provided <see cref="System.DateTime"/> cannot be converted to UTC Unix Timestamp.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// dateTime is null.
        /// </exception>
        public static uint ToUnixTime(this DateTime dateTime)
        {
            var ret = ToUnixTimeLong(dateTime);
            if (ret >= int.MaxValue)            // year >= 2038
                throw new ArgumentOutOfRangeException();

            return (uint)ret;
        }

        /// <summary>
        /// Converts the <see cref="System.DateTime"/> to UTC Unix Timestamp.
        /// </summary>
        /// <param name="dateTime">
        /// <see cref="System.DateTime"/> to convert to UTC Unix Timestamp.
        /// </param>
        /// <returns>
        /// Number of seconds passed since UTC Unix Epoch.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// dateTime is null.
        /// </exception>
        public static uint ToUnixTimeOrZero(this DateTime dateTime)
        {
            var ret = ToUnixTimeOrZeroLong(dateTime);
            if (ret >= int.MaxValue)            // year >= 2038
                return 0U;

            return (uint)ret;
        }

        /// <summary>
        /// Converts the <see cref="System.DateTime"/> to UTC Unix Timestamp.
        /// </summary>
        /// <param name="dateTime">
        /// <see cref="System.DateTime"/> to convert to UTC Unix Timestamp.
        /// </param>
        /// <returns>
        /// Number of seconds passed since UTC Unix Epoch.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// The provided <see cref="System.DateTime"/> cannot be converted to UTC Unix Timestamp.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// dateTime is null.
        /// </exception>
        public static long ToUnixTimeLong(this DateTime dateTime)
        {
            if (dateTime == null)
                throw new ArgumentNullException("dateTime");

            dateTime = dateTime.ToUniversalTime();
            if (dateTime < s_unixEpochReference)
                throw new ArgumentOutOfRangeException("dateTime");

            return (long)(dateTime - s_unixEpochReference).TotalSeconds;
        }

        /// <summary>
        /// Converts the <see cref="System.DateTime"/> to UTC Unix Timestamp.
        /// </summary>
        /// <param name="dateTime">
        /// <see cref="System.DateTime"/> to convert to UTC Unix Timestamp.
        /// </param>
        /// <returns>
        /// Number of seconds passed since UTC Unix Epoch.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// dateTime is null.
        /// </exception>
        public static long ToUnixTimeOrZeroLong(this DateTime dateTime)
        {
            if (dateTime == null)
                throw new ArgumentNullException();

            dateTime = dateTime.ToUniversalTime();
            if (dateTime < s_unixEpochReference)
                return 0;

            return (long)(dateTime - s_unixEpochReference).TotalSeconds;
        }
    }
}
