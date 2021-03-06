﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MassActivation
{
    internal class TypeNameHelper
    {
        private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>
            {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" }
            };

        public static string GetTypeDisplayName(Type type, bool fullName = true)
        {
            var sb = new StringBuilder();
            ProcessTypeName(type, sb, fullName);
            return sb.ToString();
        }

        public static string GetMethodDisplayName(MethodInfo method, bool fullName = true)
        {
            var sb = new StringBuilder();
            sb.Append(method.Name);
            if (method.IsGenericMethod)
            {
                var args = method.GetGenericArguments();
                AppendGenericArguments(args, 0, args.Length, sb, fullName);
            }
            sb.Append("(");
            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                if (parameters[i].IsOut)
                {
                    sb.Append("out ");
                }
                else if (parameters[i].ParameterType.IsByRef)
                {
                    sb.Append("ref ");
                }
                if (parameters[i].ParameterType.IsByRef)
                {
                    ProcessTypeName(parameters[i].ParameterType.GetElementType(), sb, fullName);
                }
                else
                {
                    ProcessTypeName(parameters[i].ParameterType, sb, fullName);
                }
                sb.Append(" ").Append(parameters[i].Name);
            }
            sb.Append(")");
            return sb.ToString();
        }

        private static void AppendGenericArguments(Type[] args, int startIndex, int numberOfArgsToAppend, StringBuilder sb, bool fullName)
        {
            var totalArgs = args.Length;
            if (totalArgs >= startIndex + numberOfArgsToAppend)
            {
                sb.Append("<");
                for (int i = startIndex; i < startIndex + numberOfArgsToAppend; i++)
                {
                    ProcessTypeName(args[i], sb, fullName);
                    if (i + 1 < startIndex + numberOfArgsToAppend)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(">");
            }
        }

        private static void ProcessTypeName(Type t, StringBuilder sb, bool fullName)
        {
#if NetCore
            if (t.GetTypeInfo().IsGenericType)
#else
            if (t.IsGenericType)
#endif
            {
                ProcessNestedGenericTypes(t, sb, fullName);
                return;
            }
            if (_builtInTypeNames.ContainsKey(t))
            {
                sb.Append(_builtInTypeNames[t]);
            }
            else
            {
                sb.Append(fullName ? t.FullName : t.Name);
            }
        }

        private static void ProcessNestedGenericTypes(Type t, StringBuilder sb, bool fullName)
        {
            var genericFullName = t.GetGenericTypeDefinition().FullName;
            var genericSimpleName = t.GetGenericTypeDefinition().Name;
            var parts = genericFullName.Split('+');
#if NetCore
            var genericArguments = t.GetTypeInfo().IsGenericType ? t.GenericTypeArguments : new Type[0];
#else
            var genericArguments = t.IsGenericType ? t.GetGenericArguments() : Type.EmptyTypes;
#endif
            var index = 0;
            var totalParts = parts.Length;
            if (totalParts == 1)
            {
                var part = parts[0];
                var num = part.IndexOf('`');
                if (num == -1) return;

                var name = part.Substring(0, num);
                var numberOfGenericTypeArgs = int.Parse(part.Substring(num + 1));
                sb.Append(fullName ? name : genericSimpleName.Substring(0, genericSimpleName.IndexOf('`')));
                AppendGenericArguments(genericArguments, index, numberOfGenericTypeArgs, sb, fullName);
                return;
            }
            for (var i = 0; i < totalParts; i++)
            {
                var part = parts[i];
                var num = part.IndexOf('`');
                if (num != -1)
                {
                    var name = part.Substring(0, num);
                    var numberOfGenericTypeArgs = int.Parse(part.Substring(num + 1));
                    if (fullName || i == totalParts - 1)
                    {
                        sb.Append(name);
                        AppendGenericArguments(genericArguments, index, numberOfGenericTypeArgs, sb, fullName);
                    }
                    if (fullName && i != totalParts - 1)
                    {
                        sb.Append("+");
                    }
                    index += numberOfGenericTypeArgs;
                }
                else
                {
                    if (fullName || i == totalParts - 1)
                    {
                        sb.Append(part);
                    }
                    if (fullName && i != totalParts - 1)
                    {
                        sb.Append("+");
                    }
                }
            }
        }
    }
}
