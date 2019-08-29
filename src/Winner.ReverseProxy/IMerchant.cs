using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Winner.ReverseProxy
{
    public interface IMerchant
    {
        string SignData(string original);
        string Encrypt(string text);
        string Decrypt(string cipher);
    }
}
