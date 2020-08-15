using System;

namespace Cali
{
    public class CaliException : Exception
    {
        public CaliException(string message) : base(message)
        {
        }
    }
}