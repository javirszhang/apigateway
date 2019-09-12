using System;
using System.Collections.Generic;
using System.Text;
using Winner.Framework.Utils;

namespace Winner.Gateway.DataResolver
{
    internal class MerchantCache
    {
        public static Merchant GetMerchant(string code)
        {
            Tgw_Merchant daMer = new Tgw_Merchant();
            if (!daMer.SelectByMerchantCode(code))
            {
                return null;
            }
            Merchant merchant = MapProvider.Map<Merchant>(daMer.DataRow);
            return merchant;
        }
    }
}
