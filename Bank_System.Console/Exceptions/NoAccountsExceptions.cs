using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class NoAccountsException : Exception
    {
        public NoAccountsException() : base("No accounts exist in the system.") { }
        public NoAccountsException(string message) : base(message) { }
    }
}
