using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace ObjectDumperTests
{
    public class ObjectDumper<T>
    {
        private readonly IList<KeyValuePair<string, Func<dynamic, string>>> propertiesAndFunctionsToApply;
        public ObjectDumper()
        {
            propertiesAndFunctionsToApply = new List<KeyValuePair<string, Func<dynamic, string>>>();
        }

        public void AddTemplateFor(Expression<Func<T, object>> propertyLambda, Func<dynamic, string> templateToApply)
        {
            var simpleMember = propertyLambda.Body as UnaryExpression;
            var complexMember = propertyLambda.Body as MemberExpression;

            var propertyName = simpleMember != null ? simpleMember.Operand.Type.Name : complexMember.Member.Name;

            propertiesAndFunctionsToApply.Add(new KeyValuePair<string, Func<dynamic, string>>(propertyName, templateToApply));
        }

        public IEnumerable<KeyValuePair<string, string>> Dump(T ufo)
        {
            var properties = ufo.GetType().GetProperties().Where(x => x.CanRead).OrderBy(x => x.Name);

            foreach (var property in properties)
            {
                if (propertiesAndFunctionsToApply.Count > 0 &&
                    propertiesAndFunctionsToApply.FirstOrDefault(x => x.Key == property.Name).Value != null)
                {
                    var funcToApply = propertiesAndFunctionsToApply.First(x => x.Key == property.Name).Value;

                    var valuePropertyInObject = property.GetValue(ufo);

                    var valueBeforeApplyFunction = funcToApply.Invoke(valuePropertyInObject);

                    yield return new KeyValuePair<string, string>(property.Name, valueBeforeApplyFunction);
                }
                else
                {
                    var valueOfProperty = ufo.GetType().GetProperty(property.Name).GetValue(ufo);

                    yield return new KeyValuePair<string, string>(property.Name, valueOfProperty.ToString());
                }

            }
        }
    }
}