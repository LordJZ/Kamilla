using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Kamilla
{
    /// <summary>
    /// Provides an interface to manage types in the default application domain.
    /// </summary>
    public static class TypeManager
    {
        static bool s_initialized;
        static Type[] s_types;

        /// <summary>
        /// Gets all the custom types loaded into the default application domain.
        /// </summary>
        public static IEnumerable<Type> Types
        {
            get
            {
                if (!s_initialized)
                    Initialize();

                return s_types;
            }
        }

        /// <summary>
        /// Initializes the <see cref="Kamilla.TypeManager"/>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The <see cref="Kamilla.TypeManager"/> must be initialized
        /// in the default application domain.
        /// </exception>
        public static void Initialize()
        {
            if (s_initialized)
                return;

            if (!AppDomain.CurrentDomain.IsDefaultAppDomain())
                throw new InvalidOperationException("The " + typeof(TypeManager).ToString()
                    + " must be initialized in the default application domain.");

            s_initialized = true;

            // Resolve the current directory.
            string currentDirectory;
            {
                // we can use System.Windows.Forms.Application.StartupPath, it uses Win32API.GetModuleFileName
                var entry = Assembly.GetEntryAssembly();
                if (entry.IsDynamic)
                {
                    Console.WriteLine("Error: Warning: Entry assembly is dynamic, using current directory to load modules.");
                    currentDirectory = ".";
                }
                else
                {
                    currentDirectory = Path.GetDirectoryName(entry.Location);
                    if (string.IsNullOrEmpty(currentDirectory))
                        currentDirectory = ".";
                }
            }

            var files = Directory.GetFiles(currentDirectory, "Kamilla.*.dll");
            foreach (var file in files)
            {
                try
                {
                    Assembly.LoadFile(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Failed to load '{0}': {1}", Path.GetFileName(file), e);
                }
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = new List<Type>(assemblies.Length * 200);
            int nKamillaAssemblies = 0;

            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.IndexOf("Kamilla") == -1)
                    continue;

                try
                {
                    var _types = assembly.GetTypes();
                    types.AddRange(_types);

                    ++nKamillaAssemblies;
                    Console.WriteLine("Loaded {0} types from {1}", _types.Length, assembly);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Failed to load types from assembly {0}: {1}", assembly, e);
                    if (e is ReflectionTypeLoadException)
                    {
                        var ex = e as ReflectionTypeLoadException;
                        foreach (var exc in ex.LoaderExceptions)
                            Console.WriteLine("Error: Loader Exception: {0}", exc);
                    }
                    continue;
                }
            }

            s_types = types.ToArray();

            Console.WriteLine("TypeManager loaded {1} types from {0} assemblies.",
                nKamillaAssemblies, s_types.Length);
        }
    }
}
