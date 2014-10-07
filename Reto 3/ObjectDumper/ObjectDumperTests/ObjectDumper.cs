using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectDumperTests
{
    public class ObjectDumper<T>
    {
        private readonly Dictionary<string, Func<object, string>> propertiesAndFunctionsToApply;

        public ObjectDumper()
        {
            propertiesAndFunctionsToApply = new Dictionary<string, Func<object, string>>();
        }

        public void AddTemplateFor<TR>(Expression<Func<T, TR>> propertyLambda, Func<TR, string> templateToApply)
        {
            // Al hacer la funcion parametrizada, evitamos el problema del "Unboxing"

            /*
             * Con Unary y Member lo que estabamos haciendo antes era:
             * 
             * 1º Si venia un 'struct' (int,double,etc) al hacerle el cast a object lo estabamos encapsulando en un objeto
             * (mirar el valor de la variable y ver como tenemos un Convert(o.value)). Este conver es
             * la encapsulacion que se ha hecho del struct a tipo object.
             * 
             * 2º Cuando le pasamos ya un object (o.Origin) no hace falta hacer ningun boxing, y ya es un member, ya podemos
             * acceder a el como member.
             * 
             * SOLUCION: Parametrizar el tipo de salida
             * Expresamos directamente que el tipo de salida será de un tipo especifico, lo que nos ayuda a
             * en la segunda funcion, poder expresar que el tipo de entrada es del mismo tipo del devuelto en la anterior
             * funcion.
             * 
             */

            var complexMember = propertyLambda.Body as MemberExpression;
            var unaryMember = propertyLambda.Body as UnaryExpression;
            var propertyName = complexMember.Member.Name;

            propertiesAndFunctionsToApply.Add(propertyName, 
                obj => templateToApply((TR)obj));

            /*Esta ultima funcion de "obj" es para poder tipar la funcion añadida al diccionario, ya que antes,
            * en la inicializacion del diccionario al principio de la clase, no podemos ponerle de tipo 'R'.
             * Perdemos el control arriba en la declaracion, pero por otro lado estamos seguros de que como solo
             * modificamos aqui el diccionario, lo que va a ir dentro será en realidad siempre de tipo R.
             */
        }

        public IEnumerable<KeyValuePair<string, string>> Dump(T ufo)
        {
            var properties = ufo.GetType().GetProperties().Where(x => x.CanRead).OrderBy(x => x.Name);

            foreach (var property in properties)
            {
                Func<object, string> value = propertiesAndFunctionsToApply.FirstOrDefault(x => x.Key == property.Name).Value;

                if (value != null)
                {
                    var funcToApply = value;

                    var valuePropertyInObject = property.GetValue(ufo);

                    var valueBeforeApplyFunction = funcToApply.Invoke(valuePropertyInObject);

                    yield return new KeyValuePair<string, string>(property.Name, valueBeforeApplyFunction);
                }
                else
                {
                    var valueOfProperty = ufo.GetType().GetProperty(property.Name).GetValue(ufo);

                    yield return new KeyValuePair<string, string>(property.Name, (valueOfProperty ?? "").ToString());
                }
            }
        }
    }
}