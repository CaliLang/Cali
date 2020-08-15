using System.IO;
using System.Text;

namespace Cali.Tests
{
    public static class TestUtil
    {
        public static StreamReader ToStreamReader(this string code)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(code));
            return new StreamReader(ms, true);
        }
    }
}