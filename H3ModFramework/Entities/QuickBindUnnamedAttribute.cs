using System;

namespace H3ModFramework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class QuickUnnamedBindAttribute : Attribute
    {
        public Type[] AsServices { get; }

        public QuickUnnamedBindAttribute(params Type[] asServices)
        {
            AsServices = asServices;
        }
    }
}