using System;
using System.Collections.Generic;
using System.Text;

namespace Winner.ReverseProxy.Gateway.Interfaces
{
    public interface IMerchant
    {
        string SignData(string original);
        string Encrypt(string text);
        string Decrypt(string cipher);
    }
}
