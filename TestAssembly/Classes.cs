using System;
using System.Net;
using System.Web.ModelBinding;
using Neat;
using Neat.Cool;
using Nice;

namespace Nice
{
    [SomeType(typeof(Awesome))]
    public class Foo
    {
        public class NestedBar {

        }
    }
}

namespace Neat {
    namespace Cool {
        public struct Awesome {
            public void Foo([Cookie] string cookie) {
            }
        }
    }

    public class SomeTypeAttribute : Attribute {
        public Type TypeParameter;

        public SomeTypeAttribute(Type typeParameter) {
            TypeParameter = typeParameter;
        }
    }
}