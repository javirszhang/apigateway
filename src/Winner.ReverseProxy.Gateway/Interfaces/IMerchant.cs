using System;
using System.Collections.Generic;
using System.Text;

namespace Winner.ReverseProxy.Gateway.Interfaces
{
    public interface IMerchant
    {
        string SignData(string original);
        bool VerifySignature(string original, string sign);
        string Encrypt(string text);
        string Decrypt(string cipher);
    }
}
