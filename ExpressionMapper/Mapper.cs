using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionMapper
{
    public class ParentClass
    {
        [MapperAttribute(Key = "TEST_KEY")]
        public int MyProperty { get; set; }
        public string MyStringProperty { get; set; }
    }

    internal class MapperAttribute : Attribute
    {
        public string Key { get; set; }
    }

    class Mapper<TRequest>
    {
        Dictionary<string, Func<TRequest, object>> _mappers = new Dictionary<string, Func<TRequest, object>>();
    public Mapper()
    {
        var attributedProperties = typeof(TRequest).GetProperties().Where(p => p.PropertyType.IsPrimitive).Where(p => p.GetCustomAttributes(typeof(MapperAttribute), true).Count() > 0);
    }

}

}
