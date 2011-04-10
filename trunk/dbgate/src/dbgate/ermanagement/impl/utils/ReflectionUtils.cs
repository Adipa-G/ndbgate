using System;
using System.Collections.Generic;

namespace dbgate.ermanagement.impl.utils
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

            foreach (Type aClass in type.GetInterfaces())
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

            Type superType = type.BaseType;
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
            Type iteratedType = type;
            do
            {
                interfacesMatched = true;
                foreach (Type iType in interfaceType)
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
    }
}