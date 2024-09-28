using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace NamespaceCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: namespacecleaner <assembly-path>");
                return;
            }

            string assemblyPath = args[0];
            ModuleDefMD module = ModuleDefMD.Load(assemblyPath);

            var namespaces = module.GetTypes().Select(t => t.Namespace).Distinct().ToList();

            foreach (var ns in namespaces)
            {
                var typesInNamespace = module.GetTypes().Where(t => t.Namespace == ns).ToList();

                // determine if all classes in the namespace are empty
                bool allEmpty = typesInNamespace.All(t => t.IsClass && !t.HasMethods && !t.HasFields && !t.HasProperties && !t.HasEvents);

                if (allEmpty)
                {
                    // all classes are empty remove them
                    Console.WriteLine($"Removing namespace with empty classes: {ns}");
                    foreach (var type in typesInNamespace)
                    {
                        module.Types.Remove(type);
                    }
                }
            }

            string outputPath = $"{assemblyPath}.cleaned.exe";

            var ass = AssemblyDef.Load(assemblyPath);
            var options = new ModuleWriterOptions(ass.Modules[0]);
            options.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

            module.Write(outputPath, options);
            Console.WriteLine($"Cleaned assembly saved as {outputPath}");
        }
    }
}
