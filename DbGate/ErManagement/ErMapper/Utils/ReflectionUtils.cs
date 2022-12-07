using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DbGate.Exceptions.Common;

namespace DbGate.ErManagement.ErMapper.Utils
{
    public class ReflectionUtils
    {
        public static bool IsImplementInterface(Type type, Type interfaceType)
        {
            if (type == interfaceType
                || (type.IsGenericType && interfaceType.IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceType.GetGenericTypeDefinition()))
            {
                return true;
            }

            foreach (var aClass in type.GetInterfaces())
            {
                if (aClass == interfaceType)
                {
                    return true;
                }
                if (IsImplementInterface(aClass, interfaceType))
                {
                    return true;
                }
            }

            var superType = type.BaseType;
            while (superType != null)
            {
                if (IsImplementInterface(superType, interfaceType))
                {
                    return true;
                }
                superType = superType.BaseType;
            }
            return false;
        }

        public static Type[] GetSuperTypesWithInterfacesImplemented(Type type, Type[] interfaceType)
        {
            var superTypes = new List<Type>();

            bool interfacesMatched;
            var iteratedType = type;
            do
            {
                interfacesMatched = true;
                foreach (var iType in interfaceType)
                {
                    interfacesMatched &= IsImplementInterface(iteratedType, iType);
                }
                if (interfacesMatched)
                {
                    superTypes.Add(iteratedType);
                }
                iteratedType = iteratedType.BaseType;
            } while (interfacesMatched);

            return superTypes.ToArray();
        }

        public static bool IsSubClassOf(Type type, Type superType)
        {
            if (type == superType)
            {
                return true;
            }

            if (type.BaseType != null)
            {
                if (IsSubClassOf(type.BaseType, superType))
                {
                    return true;
                }
            }
            return false;
        }

        public static Object GetValue(PropertyInfo property, Object target)
        {
            try
            {
                return property.GetValue(target);
            }
            catch (Exception ex)
            {
                var message = String.Format("Exception while trying get property {0} value of entity {1}"
                                               , property.Name, target.GetType().FullName);
                throw new MethodInvocationException(message, ex);
            }
        }

        public static void SetValue(PropertyInfo property, Object target, Object value)
        {
            try
            {
                property.SetValue(target, value);
            }
            catch (Exception ex)
            {
                var message = String.Format("Exception while trying to set property {0} of entity {1}"
                                               , property.Name, target.GetType().FullName);
                throw new MethodInvocationException(message, ex);
            }
        }

        public static Object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                var message = String.Format("Exception while trying to create an instance of type {0}"
                                               , type.FullName);
                throw new EntityInstantiationException(message, ex);
            }
        }

        public static object GetFieldValue(FieldInfo field,object target)
        {
            try
            {
                var value = field.GetValue(target);
                return value;
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception while trying to extract field value of {0} of entity {1}",field,target.GetType().FullName);
                throw new FieldValueExtractionException(message,ex);
            }
        }

        public static string GetPropertyNameFromExpression<T>(Expression<Func<T, object>> prop)
        {
            var lambda = prop as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            var fieldName = memberExpression.Member.Name;
            return fieldName;
        }
    }
}