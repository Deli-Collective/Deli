using System;

namespace Deli
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class QuickUnnamedBindAttribute : Attribute
    {
        public QuickUnnamedBindAttribute(params Type[] asServices)
        {
            AsServices = asServices;
        }

        public Type[] AsServices { get; }
    }
}