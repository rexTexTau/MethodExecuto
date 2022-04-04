// Copyright (c) 2022 George Churochkin. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// Full license text: see LICENSE file in the root folder of this repository

namespace rextextau.MethodExecuto.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Utility class to perform invoking particular method of assembly.
    /// </summary>
    public static class Invoker
    {
        #region Const

        private const string DLL_SUFFIX = ".dll";

        #endregion
        public static object Invoke(string assemblyPath, string typeName, string methodName, string[] parameters, string additionalAssemblyResolveDir = null)
        {
            ResolveEventHandler resolveEventHandler = null;

            // adding assemblies resolution from additionalAssemblyResolveDir too
            if (!string.IsNullOrEmpty(additionalAssemblyResolveDir))
            {
                resolveEventHandler = (object sender, ResolveEventArgs args) =>
                {
                    string assemblyPath = Path.Combine(additionalAssemblyResolveDir, new AssemblyName(args.Name).Name + DLL_SUFFIX);
                    try
                    {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                };
                AppDomain.CurrentDomain.AssemblyResolve += resolveEventHandler;
            }

            try
            {
                var type = Loader.LoadType(assemblyPath, typeName);
                var methods = Loader.LoadMethods(type);
                var candidateMethods = methods.Where(m => m.Name == methodName).ToList();
                object[] typedParameters = null;
                MethodInfo method = null;
                for (int i = 0; i < candidateMethods.Count; i++)
                {
                    try
                    {
                        method = candidateMethods[i];
                        typedParameters = Loader.ConvertParameters(method.GetParameters(), parameters);
                        break;
                    }
                    catch (Exception)
                    {
                        // if it's not the last method - ignore exception and continue trying
                        // if last method does not fit - then rethrow the exception, no way:
                        if (i == candidateMethods.Count - 1)
                        {
                            throw;
                        }
                    }
                }

                object instance = null;
                if (!method.IsStatic)
                {
                    // trying to execute ctor with no parameters
                    ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
                    instance = ctor.Invoke(null);
                }

                return method.Invoke(instance, parameters);
            }
            finally
            {
                if (resolveEventHandler != null)
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= resolveEventHandler;
                }
            }
        }
    }
}
