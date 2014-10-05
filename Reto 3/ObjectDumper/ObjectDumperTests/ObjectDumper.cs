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
        private readonly IList<KeyValuePair<string, Func<dynamic,string>>> listPropertiesAndTemplates;
        public ObjectDumper()
        {
            this.listPropertiesAndTemplates = new List<KeyValuePair<string, Func<dynamic, string>>>();
        }

        public void AddTemplateFor(Expression<Func<T, object>> propertyLambda, Func<dynamic, string> templateToApply)
        {
            var simpleMember = propertyLambda.Body as UnaryExpression;
            var complexMember = propertyLambda.Body as MemberExpression;

            var property = simpleMember != null ? simpleMember.Operand.Type : complexMember.Type;
            var propertyName = simpleMember != null ? simpleMember.Operand.Type.Name: complexMember.Member.Name;
            
            bool isClass = false;
            if (property.IsPrimitive)
            {
                isClass = false;
            }
            if (property.IsClass)
            {
                isClass = true;
                var x = property.GetProperties();
                listPropertiesAndTemplates.Add(new KeyValuePair<string, Func<dynamic, string>>(propertyName, templateToApply));

            }

        }

        public IEnumerable<KeyValuePair<string, string>> Dump(T ufo)
        {
            var getProperties = ufo.GetType().GetProperties().Where(x => x.CanRead).OrderBy(x => x.Name);

            foreach (var property in getProperties)
            {
                if (listPropertiesAndTemplates.Count > 0 &&
                    listPropertiesAndTemplates.FirstOrDefault(x => x.Key == property.Name).Value != null)
                {
                    var funcToApply = listPropertiesAndTemplates.First(x => x.Key == property.Name).Value;
                    var rightProperty = property.GetValue(ufo);

                    var finalstring = funcToApply.Invoke(rightProperty);

                    yield return new KeyValuePair<string, string>(property.Name, finalstring);
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