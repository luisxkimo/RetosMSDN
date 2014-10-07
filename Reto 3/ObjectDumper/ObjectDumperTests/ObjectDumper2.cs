using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDumperTests
{
    public class ObjectDumper2<T>
    {
        // A little optimization to avoid extra reflection calls
        private static readonly Lazy<IEnumerable<PropertyInfo>> PROPERTIES =
            new Lazy<IEnumerable<PropertyInfo>>(() =>
                typeof(T).GetProperties()
                    .Where(x => x.CanRead)
                    .OrderBy(x => x.Name)
                    .ToList());

        private readonly IDictionary<PropertyInfo, Func<object, string>> formatters =
            new Dictionary<PropertyInfo, Func<object, string>>();

        public IEnumerable<KeyValuePair<string, string>> Dump(T obj)
        {
            return PROPERTIES.Value.Select(x => new KeyValuePair<string, string>(x.Name, Format(obj, x)));
        }

        private string Format(T obj, PropertyInfo property)
        {
            var value = property.GetValue(obj, null);
            var formatter = GetFormatter(property);
            return formatter(value);
        }

        private Func<object, string> GetFormatter(PropertyInfo property)
        {
            Func<object, string> formatter;

            if (formatters.TryGetValue(property, out formatter))
                return formatter;

            return x => x.ToString();
        }

        public void AddTemplateFor<R>(Expression<Func<T, R>> member, Func<R, string> format)
        {
            var body = member.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("member must be a property expression");

            var property = body.Member as PropertyInfo;
            if (body == null)
                throw new ArgumentException("member must be a property (not field) expression");

            formatters.Add(property, value => format((R)value));
        }
    }

}
