using System.Net;
using System.Web.ModelBinding;

namespace Nice
{
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
}