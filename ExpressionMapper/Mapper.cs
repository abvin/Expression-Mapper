using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        class PropertyAccessor
        {
            public Func<object, object> Getter { get; set; }
            public Action<object, object> Setter { get; set; }
        }
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyAccessor>> propertyAccessorCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyAccessor>>();
        public Mapper()
        {
            var attributedProperties = typeof(TRequest).GetProperties().Where(p => p.PropertyType.IsPrimitive).Where(p => p.GetCustomAttributes(typeof(MapperAttribute), true).Count() > 0);
        }

        private void PopulateResponseObject(Hashtable hash, object response)
        {
            Type currentType = response.GetType();
            if (!propertyAccessorCache.ContainsKey(currentType))
            {
                throw new Exception("Cannot find mapping for the type");
            }

            var propertySetters = propertyAccessorCache[currentType];
            foreach (string hashKey in hash.Keys)
            {
                if (!propertySetters.ContainsKey(hashKey))
                    continue;

                var propertyAccessor = propertySetters[hashKey];

                if (!(hash[hashKey].GetType() == typeof(Hashtable)))
                {
                    propertyAccessor.Setter(response, hash[hashKey]);
                }
                else //child hashtable
                {
                    //instantiate object
                    propertyAccessor.Setter(response, null);
                    var childObject = propertyAccessor.Getter(response);
                    PopulateResponseObject(hash[hashKey] as Hashtable, childObject);
                }
            }
        }

        public Action<object, object> GetPropertySetter(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(propertyInfo.DeclaringType, "obj");
            var valueOfProperty = Expression.Parameter(typeof(object), "valueOfProperty");
            var typedInstance = Expression.TypeAs(instance, propertyInfo.DeclaringType);
            if (!propertyInfo.PropertyType.IsClass)
            {
                var setterCall = Expression.Call(
                    typedInstance,
                    propertyInfo.GetSetMethod(),
                    Expression.Convert(valueOfProperty, propertyInfo.PropertyType));
                return (Action<object, object>)Expression.Lambda(setterCall, instance, valueOfProperty)
                                                .Compile();
            }
            else
            {
                var constructorExpression = Expression.New(propertyInfo.PropertyType);
                var setterCall = Expression.Call(
                    typedInstance,
                    propertyInfo.GetSetMethod(),
                    constructorExpression);
                return (Action<object, object>)Expression.Lambda(setterCall, instance, valueOfProperty)
                                                .Compile();
            }

        }


    }

}
