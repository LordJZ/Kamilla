using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamilla
{
    public struct ValueTuple<T1>
    {
        public readonly T1 Item1;

        public ValueTuple(T1 item1)
        {
            this.Item1 = item1;
        }
    }

    public struct ValueTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }

    public struct ValueTuple<T1, T2, T3>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;
        public readonly T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }
    }
}
