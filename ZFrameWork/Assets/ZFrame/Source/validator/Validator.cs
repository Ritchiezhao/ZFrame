using System;
using zf.core;
namespace sgaf.util
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
