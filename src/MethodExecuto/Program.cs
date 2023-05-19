// Copyright (c) 2022 George Volsky. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// Full license text: see LICENSE file in the root folder of this repository

namespace rextextau.MethodExecuto
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Utility.CommandLine;
    using rextextau.MethodExecuto.Core;

    /// <summary>
    /// Console app to quickly execute a particular method of a particular type in a particular assembly.
    /// </summary>
    internal class Program
    {
        #region Const

        private const string DLL_SUFFIX = ".dll";

        #endregion

        #region Fields

        [Argument('h', "help", "Show help")]
        private static bool ShowHelp { get; set; }

        [Argument('d', "directory", "Set working directory")]
        private static string WorkingDir { get; set; }

        [Argument('a', "assembly", "Specify the assembly name")]
        private static string AssemblyName { get; set; }

        [Argument('t', "type", "Specity the type name")]
        private static string TypeName { get; set; }

        [Argument('m', "method", "Specify the method name")]
        private static string MethodName { get; set; }

        [Argument('p', "parameter", "Specify the parameters' values")]
        private static string[] ParameterValues { get; set; }

        [Operands]
        private static List<string> Operands { get; set; }

        #endregion

        #region Methods

        private static void PrintHelp()
        {
            Console.WriteLine("}-=[ Welcome to MethodExecuto! ]=-{");
            Console.WriteLine(string.Empty);

            Console.WriteLine("List of available arguments:");
            Console.WriteLine("Short Long        Description");
            Console.WriteLine("----- ----        -----------");

            foreach (var item in Arguments.GetArgumentInfo(typeof(Program)))
            {
                Console.WriteLine(
                    item.ShortName.ToString().PadRight(6) +
                    item.LongName.PadRight(12) +
                    item.HelpText);
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine("If you don't specify the working directory, current directory will be used");
            Console.WriteLine("If you don't specify the assembly name, all available assemblies within current directory will be listed");
            Console.WriteLine("If you don't specify the type name, all types within the selected assembly will be listed");
            Console.WriteLine("If you don't specify the method name, all methods within the selected type will be listed");
            Console.WriteLine("If method you've selected to call is not staic, parameterless type ctor will be executed (if any)");
            Console.WriteLine("If method's visibility is private, internal, protected or protected internal - it'll just be executed no matter what.");
            Console.WriteLine("Multiple parameters' values allowed, i.e. -p \"x\" -p \"2.22\" -p \"3\" will lead to call ('x', 2.22, 3)");
            Console.WriteLine("If there are several methods with one name and different signatures, the method with the signature that fits most list of given parameters will be called.");
            Console.WriteLine(string.Empty);
            Console.WriteLine("}-=[ Have a nice day! Regards, rextextau ]=-{");
        }

        private static string GetAssemblyPath()
        {
            return
                Path.IsPathRooted(AssemblyName) ?
                AssemblyName :
                Path.Combine(WorkingDir, AssemblyName);
        }

        private static void PrintAssemblies()
        {
            foreach (string assemblyFilePath in Directory.GetFiles(WorkingDir, "*" + DLL_SUFFIX))
            {
                Assembly a = Loader.LoadAssembly(assemblyFilePath);
                if (a != null)
                {
                    Console.WriteLine(Path.GetFileName(assemblyFilePath));
                }
            }
        }

        private static void PrintTypes()
        {
            foreach (Type t in Loader.LoadTypes(GetAssemblyPath()))
            {
                Console.WriteLine(
                    (t.IsPublic ? "public " : string.Empty) +
                    (t.IsAbstract ? "abstract " : string.Empty) +
                    (t.IsSealed ? "sealed " : string.Empty) +
                    (t.IsEnum ? "enum " : string.Empty) +
                    (t.IsClass ? "class " : string.Empty) +
                    t.Name);
            }
        }

        private static void PrintMethods()
        {
            foreach (MethodInfo m in Loader.LoadMethods(GetAssemblyPath(), TypeName))
            {
                Console.WriteLine(
                    (m.IsPublic ? "public " : string.Empty) +
                    (m.IsPrivate ? "private " : string.Empty) +
                    (m.IsStatic ? "static " : string.Empty) +
                    (m.IsVirtual ? "virtual " : string.Empty) +
                    (m.IsAbstract ? "abstract " : string.Empty) +
                    m.Name);
            }
        }

        private static void PrintMethodInvocationResult()
        {
            var result = Invoker.Invoke(GetAssemblyPath(), TypeName, MethodName, ParameterValues, WorkingDir);
            string resultString;
            try
            {
                if (result is IConvertible)
                {
                    resultString = (string)Convert.ChangeType(result, typeof(string));
                }
                else if (result == null)
                {
                    resultString = "NULL";
                }
                else
                {
                    resultString = result.ToString();
                }
            }
            catch (Exception e)
            {
                resultString = "CANNOT CONVERT";
                Debug.WriteLine($"Error while converting method invocation result to string: {e.Message}");
            }
            Console.WriteLine(resultString);
        }

        private static void Main()
        {
            Environment.ExitCode = 0;
            Arguments.Populate();
            Debug.WriteLine("Operands: " + string.Join(',', Operands.Skip(1)));

            if (ShowHelp)
            {
                PrintHelp();
                return;
            }

            if (string.IsNullOrWhiteSpace(WorkingDir))
            {
                WorkingDir = Loader.GetAssemblyDir(Assembly.GetExecutingAssembly());
            }

            try
            {
                if (!AssemblyName.EndsWith(DLL_SUFFIX))
                {
                    AssemblyName += DLL_SUFFIX;
                }

                if (string.IsNullOrWhiteSpace(AssemblyName))
                {
                    PrintAssemblies();
                    return;
                }

                if (string.IsNullOrWhiteSpace(TypeName))
                {
                    PrintTypes();
                    return;
                }

                if (string.IsNullOrWhiteSpace(MethodName))
                {
                    PrintMethods();
                    return;
                }

                PrintMethodInvocationResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                Environment.ExitCode = -1;
            }
        }

        #endregion
    }
}
