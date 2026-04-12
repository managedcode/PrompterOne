using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace PrompterOne.Core.AI.Tools;

internal static class ScriptAgentMcpToolFunctionFactory
{
    public static AITool Create(Delegate toolMethod)
    {
        var method = toolMethod.Method;
        var metadata = ReadMetadata(method);
        var function = AIFunctionFactory.Create(
            toolMethod,
            new AIFunctionFactoryOptions
            {
                Name = metadata.Name
            });

        return metadata.RequiresApproval
            ? new ApprovalRequiredAIFunction(function)
            : function;
    }

    private static ToolMetadata ReadMetadata(MethodInfo method)
    {
        var tool = method.GetCustomAttribute<McpServerToolAttribute>()
            ?? throw new InvalidOperationException($"{method.DeclaringType?.FullName}.{method.Name} must declare {nameof(McpServerToolAttribute)}.");

        if (string.IsNullOrWhiteSpace(tool.Name))
        {
            throw new InvalidOperationException($"{method.DeclaringType?.FullName}.{method.Name} must declare an MCP tool name.");
        }

        if (method.GetCustomAttribute<DescriptionAttribute>() is null)
        {
            throw new InvalidOperationException($"{method.DeclaringType?.FullName}.{method.Name} must declare a tool description.");
        }

        return new ToolMetadata(tool.Name, RequiresApproval(tool));
    }

    private static bool RequiresApproval(McpServerToolAttribute tool) =>
        !tool.ReadOnly && tool.Destructive;

    private readonly record struct ToolMetadata(string Name, bool RequiresApproval);
}
