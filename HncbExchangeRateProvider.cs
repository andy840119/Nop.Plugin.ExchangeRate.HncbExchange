using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Plugin.ExchangeRate.HncbExchange.Extensions;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Newtonsoft.Json;

namespace Nop.Plugin.ExchangeRate.HncbExchange
{
    public class HncbExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public HncbExchangeRateProvider(ILocalizationService localizationService,
            ILogger logger)
        {
            this._localizationService = localizationService;
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets currency live rates
        /// Rate Url is : http://event.hncb.com.tw/wps/portal/HNCB/exchange-rate/!ut/p/z1/jZBNC4JAEIZ_Swevzpga0W0Lv8L0FNpewsJWQ11ZN_fvtxAdoqzmNvC8zwwvUMiBdsVYs0LWvCsavR_o4oipG4WY4i5NLEQSu44Tzj07SBCyBzAxBIH-lZ8G6Hd9BvT1BG7Q0gZvbXm-bwURPoFpx_bXk7oF0p3sJQMqykspSmHehC6nkrIfVgYaqJQyGeesKc0zb02pjI-pig8S8jcY-naf49VtxpjM7lzGdIY!/?1dmy&urile=wcm%3apath%3a/wps/wcm/connect/hncb/site_map/hncb/various_forex/exchange_rate_current_inquiry
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public IList<Core.Domain.Directory.ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            if (exchangeRateCurrencyCode == null)
                throw new ArgumentNullException(nameof(exchangeRateCurrencyCode));

            //add twd with rate 1
            var ratesToTwd = new List<Core.Domain.Directory.ExchangeRate>
            {
                new Core.Domain.Directory.ExchangeRate
                {
                    CurrencyCode = "TWD",
                    Rate = 1,
                    UpdatedOn = DateTime.UtcNow
                }
            };

            try
            { 
                using (WebClient wc = new WebClient())
                {
                    int timestramp = DateTime.UtcNow.ToTimeStamp();
                    var jsonString = wc.DownloadString("http://event.hncb.com.tw/hncb/rest/exRate/all?_=" + timestramp);
                    var rates = JsonConvert.DeserializeObject<List<HncbJsonObject>>(jsonString);
            
                    foreach(var rate in rates)
                    { 
                        var currencyCode = rate.DESC_ENG;//直接用英文說明當作Code

                        if(string.IsNullOrEmpty(rate.TYPE)) 
                        { 
                            ratesToTwd.Add(new Core.Domain.Directory.ExchangeRate()
                            {
                                CurrencyCode = currencyCode,
                                Rate = 1 / Convert.ToDecimal(rate.SELL_AMT_BOARD),
                                UpdatedOn = DateTime.UtcNow,
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            { 
                _logger.Error("HNCB exchange rate provider", ex);
            }

            //return result for the twd
            if (exchangeRateCurrencyCode.Equals("twd", StringComparison.InvariantCultureIgnoreCase))
                return ratesToTwd;

            //use only currencies that are supported by Hncb
            var exchangeRateCurrency = ratesToTwd.FirstOrDefault(rate => rate.CurrencyCode.Equals(exchangeRateCurrencyCode, StringComparison.InvariantCultureIgnoreCase));
            if (exchangeRateCurrency == null)
                throw new NopException(_localizationService.GetResource("Plugins.ExchangeRate.HncbExchange.Error"));

            //return result for the selected (not twd) currency
            return ratesToTwd.Select(rate => new Core.Domain.Directory.ExchangeRate
            {
                CurrencyCode = rate.CurrencyCode,
                Rate = Math.Round(rate.Rate / exchangeRateCurrency.Rate, 4),
                UpdatedOn = rate.UpdatedOn
            }).ToList();
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.ExchangeRate.HncbExchange.Error", "You can use HNCB (Hua Nan bank) exchange rate provider only when the primary exchange rate currency is supported by Hncb");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.ExchangeRate.HnchExchange.Error");

            base.Uninstall();
        }

        #endregion

    }
}