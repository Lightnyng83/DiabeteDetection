using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Security.Service
{
    public interface ITokenService
    {
        void AuthenticateClient(HttpClient client, int expiryMinutes);
    }
}
