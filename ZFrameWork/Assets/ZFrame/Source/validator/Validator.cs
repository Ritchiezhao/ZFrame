using System;
using zf.core;
namespace zf.util
{
    public partial class Validator : EnvObject
    {
        public Validator ()
        {
        }

        public override int GetHashCode()
        {
            return Tid.GetHashCode();
        }
    }
}
