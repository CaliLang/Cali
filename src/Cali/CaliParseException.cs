using System;
using Cali.Parser;

namespace Cali
{
    public class CaliParseException : Exception
    {
        public CaliParseException(string message, int line, int col) : base(message)
        {
            Line = line;
            Column = col;
        }

        internal CaliParseException(string message, IFileCoordinatesAware fileCoordinates) : this(message, fileCoordinates.Line,
            fileCoordinates.Column)
        {
        }

        public int Line { get; }

        public int Column { get; }
    }
}