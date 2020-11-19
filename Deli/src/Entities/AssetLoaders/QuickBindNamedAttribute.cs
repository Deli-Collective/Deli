using System;

namespace Deli
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class QuickNamedBindAttribute : Attribute
    {
        public QuickNamedBindAttribute(string name, params Type[] asServices)
        {
            AsServices = asServices;
            Name = name;
        }

        public Type[] AsServices { get; }

        public string Name { get; }
    }
}