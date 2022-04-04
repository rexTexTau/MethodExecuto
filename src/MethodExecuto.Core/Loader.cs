// Copyright (c) 2022 George Churochkin. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// Full license text: see LICENSE file in the root folder of this repository

namespace rextextau.MethodExecuto.Core
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Utility class to perform loading reflection data from assembly.
    /// </summary>
    public static class Loader
    {
        public static string GetAssemblyDir(Assembly assembly)
        {
            string codeBase = assembly.Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static Assembly LoadAssembly(string assemblyFilePath)
        {
            Assembly a = null;
            try
            {
                a = Assembly.LoadFile(assemblyFilePath);
            }
            catch (Exception)
            {
                Debug.WriteLine($"Error loading {Path.GetFileName(assemblyFilePath)}");
            }

            return a;
        }

        private static Type[] LoadTypes(Assembly a)
        {
            Type[] types;
            try
            {
                types = a.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            catch (Exception)
            {
                return null;
            }

            return types.Where(t => t != null &&
                    Attribute.GetCustomAttribute(t, typeof(CompilerGeneratedAttribute)) == null &&
                    !t.Attributes.HasFlag(TypeAttributes.NestedPrivate))
                .ToArray();
        }

        public static Type[] LoadTypes(string assemblyFilePath)
        {
            Assembly a = Assembly.LoadFile(assemblyFilePath);
            if (a == null)
            {
                throw new Exception($"Error loading {Path.GetFileName(assemblyFilePath)}");
            }

            Type[] types = LoadTypes(a);
            if (types == null)
            {
                throw new Exception($"Error getting types from {Path.GetFileName(assemblyFilePath)}");
            }

            return types;
        }

        public static Type LoadType(string assemblyFilePath, string className)
        {
            var types = LoadTypes(assemblyFilePath);

            var t = types.FirstOrDefault(t => t.FullName == className);
            if (t == null)
            {
                t = types.FirstOrDefault(t => t.Name == className);
            }

            if (t == null)
            {
                throw new Exception($"No {className} found in {Path.GetFileName(assemblyFilePath)}");
            }

            return t;
        }

        public static MethodInfo[] LoadMethods(Type t)
        {
            MethodInfo[] methods = null;
            try
            {
                methods = t.GetMethods(
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.Instance);
            }
            catch (Exception)
            {
                return null;
            }

            return methods.Where(m => !m.IsConstructor && !m.IsSpecialName).ToArray();
        }

        public static MethodInfo[] LoadMethods(string assemblyFilePath, string className)
        {
            var t = LoadType(assemblyFilePath, className);
            var methods = LoadMethods(t);
            if (methods == null)
            {
                throw new Exception($"Error getting methods from {className}");
            }

            return methods;
        }

        public static object[] ConvertParameters(ParameterInfo[] parameterInfos, string[] parameterStrings)
        {
            var result = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var paramIsMissing = parameterStrings == null || parameterStrings.Length <= i;
                if (paramIsMissing)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                        result[i] = Type.Missing;
                    }
                    else
                    {
                        throw new ArgumentException($"Method's parameter #{i + 1} has neither default value, nor value specified. Please use additional -p argument or correct method's signature");
                    }
                }
                else
                {
                    result[i] = Convert.ChangeType(parameterStrings[i], parameterInfos[i].ParameterType);
                }
            }

            return result;
        }
    }
}
