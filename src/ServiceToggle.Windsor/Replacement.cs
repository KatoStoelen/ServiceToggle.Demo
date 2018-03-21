using System;

namespace ServiceToggle.Windsor
{
    internal class Replacement
    {
        public Type[] Services { get; set; }
        public Type[] Implementations { get; set; }
        public string Name { get; set; }

        public bool IsByName => !string.IsNullOrEmpty(Name);
    }
}
