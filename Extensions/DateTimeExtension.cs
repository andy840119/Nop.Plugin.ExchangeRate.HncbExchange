using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.ExchangeRate.HncbExchange.Extensions
{
    /// <summary>
    /// 把時間轉換成其他格式
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// Convert datetime to timestamp
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToTimeStamp(this DateTime dateTime)
        {
            var timestamp =  Convert.ToInt32(dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
            return timestamp;
        }

        /// <summary>
        /// Convert timestamp to <see cref="DateTime"/>
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this int timeStamp)
        {
            DateTime gtm = (new DateTime(1970, 1, 1, 0, 0, 0)).AddSeconds(Convert.ToInt32(timeStamp));
            return gtm;
        }
    }
}
