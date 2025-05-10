using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

class Program
{
    static async Task Main(string[] args)
    {
        MSBuildLocator.RegisterDefaults();

        if (args.Length != 2)
        {
            Console.WriteLine("Usage: RoslynDepExplorer <path-to-csproj> <fully-qualified-class-name>");
            return;
        }

        var projectPath = args[0];
        var className = args[1];

        using var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectPath);
        var compilation = await project.GetCompilationAsync();
        if (compilation == null)
        {
            Console.WriteLine("Compilation came back null");
            return;
        }

        foreach (var tree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync();
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var cls in classes)
            {
                var symbol = semanticModel.GetDeclaredSymbol(cls);
                if (symbol == null || symbol.ToDisplayString() != className)
                    continue;

                Console.WriteLine($"Class: {className}");

                foreach (var ctor in cls.Members.OfType<ConstructorDeclarationSyntax>())
                {
                    foreach (var param in ctor.ParameterList.Parameters)
                    {
                        var type = semanticModel.GetTypeInfo(param.Type!).Type;
                        Console.WriteLine("-------------");
                        Console.WriteLine($"Dependency: {type?.ToDisplayString()}");
                        Console.WriteLine($"From Assembly: {type?.ContainingAssembly?.Name}");

                        if (type?.TypeKind == TypeKind.Interface)
                        {
                            foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
                            {
                                var paramList = string.Join(", ",
                                    member.Parameters.Select(p =>
                                        $"{p.Type.ToDisplayString()} {p.Name}"
                                    )
                                );

                                Console.WriteLine($" - Method: {member.Name}({paramList})");
                            }
                        }
                    }
                }

                return;
            }
        }

        Console.WriteLine("Class not found.");
    }
}
