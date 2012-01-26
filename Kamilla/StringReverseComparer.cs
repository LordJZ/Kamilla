using System;
using System.Collections.Generic;

namespace Kamilla
{
    public struct StringReverseComparer : IComparer<string>
    {
        public static int Compare(string x, string y)
        {
            var xObj = (object)x;
            var yObj = (object)y;

            if (xObj == yObj)
                return 0;
            else if (xObj == null && yObj != null)
                return -1;
            else if (yObj == null && xObj != null)
                return 1;

            int xLength = x.Length;
            int yLength = y.Length;

            int min = Math.Min(xLength, yLength);

            for (int i = 0; i < min; i++)
            {
                var xChar = x[xLength - i - 1];
                var yChar = y[yLength - i - 1];

                if (xChar > yChar)
                    return -1;
                else if (yChar > xChar)
                    return 1;
            }

            if (xLength > yLength)
                return -1;
            else if (yLength > xLength)
                return 1;

            // o_O
            return 0;
        }

        int IComparer<string>.Compare(string x, string y)
        {
            return Compare(x, y);
        }
    }
}
