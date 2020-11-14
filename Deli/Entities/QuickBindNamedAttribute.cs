using System;

namespace Deli
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class QuickNamedBindAttribute : Attribute
    {
        public Type[] AsServices { get; }

        public string Name { get; }

        public QuickNamedBindAttribute(string name, params Type[] asServices)
        {
            AsServices = asServices;
            Name = name;
        }
    }
}