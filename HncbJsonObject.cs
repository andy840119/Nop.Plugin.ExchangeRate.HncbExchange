using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.ExchangeRate.HncbExchange
{
    public class HncbJsonObject
    {
        //ID
        public string CUR_ID { get; set; }

        //不知道這三洨
        public string BUY_AMT_BOARD { get; set; }

        //時間
        public string TIME { get; set; }

        //日期
        public string DATE { get; set; }

        //英文說明，上面會有匯率代碼
        public string DESC_ENG { get; set; }

        //匯率
        public string SELL_AMT_BOARD { get; set; }

        //中文說明
        public string DESC_CHI { get; set; }

        //類型，forward是指30 60 180天那種
        public string TYPE { get; set; }
    }
}
