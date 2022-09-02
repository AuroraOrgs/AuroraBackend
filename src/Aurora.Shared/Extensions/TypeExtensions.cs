﻿namespace Aurora.Shared.Extensions;

public static class TypeExtensions
{
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        bool result;
        if (givenType == genericType
            || givenType.InheritsGenericType(genericType)
            || givenType.IsGeneticType(genericType)
            )
        {
            result = true;
        }
        else
        {
            Type baseType = givenType.BaseType;
            if (baseType is not null)
            {
                result = IsAssignableToGenericType(baseType, genericType);
            }
            else
            {
                result = false;
            }
        }
        return result;
    }

    private static bool IsGeneticType(this Type givenType, Type genericType)
    {
        return givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType;
    }

    private static bool InheritsGenericType(this Type givenType, Type genericType)
    {
        return givenType.GetInterfaces().Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType);
    }
}
