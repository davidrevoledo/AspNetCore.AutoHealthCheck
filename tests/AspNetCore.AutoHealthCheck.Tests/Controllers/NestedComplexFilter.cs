using System;

namespace AspNetCore.AutoHealthCheck.Tests.Controllers
{
    public class NestedComplexFilter
    {
        public class SubItem
        {
            public string Foo { get; set; }
        }

        public string Name { get; set; }

        public int Id { get; set; }

        public DateTime Date { get; set; }
    }
}