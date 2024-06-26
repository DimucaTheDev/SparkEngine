﻿using System.Diagnostics;
using System.Reflection;

namespace SparkEngine.Configuration;

internal class CommandLineInterpreter
{
    [Description("Prints data to console")]
    [Command("echo")]
    public static CommandResult Echo(string args)
    {
        Overlay.Instance!.Log += args + "\n";
        return CommandResult.Success;
    }

    [Description("Prints data with debug level")]
    [Command("dbg")]
    [Command("debug")]
    [Command("debg")]
    public static CommandResult Debug(string args)
    {
        Logger.PrintLine(args);
        return CommandResult.Success;
    }

    [Description("Prints data with error level")]
    [Command("err")]
    [Command("error")]
    public static CommandResult Error(string args)
    {
        Logger.PrintLine(args, LogLevel.Error);
        return CommandResult.Success;
    }

    [Description("Prints data with warning level")]
    [Command("warn")]
    public static CommandResult Warn(string args)
    {
        Logger.PrintLine(args, LogLevel.Warning);
        return CommandResult.Success;
    }

    [Description("Stops engine")]
    [Command("exit")]
    [Command("close")]
    public static CommandResult Exit(string args)
    {
        Logger.PrintLine("Exiting!");
        Program.Instance.Close();
        return CommandResult.Success;
    }

    [Description("Clears console")]
    [Command("cls")]
    [Command("clear")]
    public static CommandResult Clear(string args)
    {
        Overlay.Instance.Log = "";
        return CommandResult.Success;
    }

    [Description("Sets fov. Usage: fov {10-360}")]
    [Command("fov")]
    public static CommandResult Fov(string args)
    {
        if (int.Parse(args.Split(" ")[0]) < 10 || int.Parse(args.Split(" ")[0]) > 360) return CommandResult.Fail;
        Camera.Fov = int.Parse(args.Split(" ")[0]);
        if (Camera.Fov > 179) Warn("Shitty result may occur!");
        return CommandResult.Success;
    }

    [Description("Shows callstack in debugger")]
    [Command("assert")]
    public static CommandResult Assert(string args)
    {
        System.Diagnostics.Debug.Assert(!Debugger.Launch());
        return CommandResult.Success;
    }

    [Description("Shows help")]
    [Command("help")]
    [Command("h")]
    [Command("?")]
    public static CommandResult Help(string args)
    {
        typeof(CommandLineInterpreter)
            .GetMethods()
            .Where(m => m.GetCustomAttributes<CommandAttribute>().Any())
            .ToList()
            .ForEach(m => {
                m.GetCustomAttributes<CommandAttribute>()
                    .ToList()
                    .ForEach(_ => { Overlay.Instance.Log += $"{_.Name}, "; });
                Overlay.Instance.Log += $"- {m.GetCustomAttribute<DescriptionAttribute>()?.Description}\n";
            });
        return CommandResult.Success;
    }

    [Description("Sets sensitivity. Usage: sensitivity {1+}")]
    [Command("sensitivity")]
    public static CommandResult Sens(string args)
    {
        if (int.Parse(args.Split(" ")[0]) < 1) return CommandResult.Fail;
        Camera.Sensitivity = double.Parse(args.Split(" ")[0]) / 100;
        return CommandResult.Success;
    }

    [Command("screenshot")]
    [Command("ss")]
    [Description("Makes a screenshot of a windw to screenshots/ folder")]
    public static CommandResult Screenshot(string args)
    {
        Program.ScreenshotRequired = true;
        return CommandResult.Success;
    }

    public static CommandResult Execute(string command)
    {
        Overlay.Instance.Log += $"> {command}\n";
        var result = CommandResult.Success;
        try
        {
            typeof(CommandLineInterpreter)
                .GetMethods()
                .Where(m => m.GetCustomAttributes<CommandAttribute>().Any())
                .ToList()
                .ForEach(m => m.GetCustomAttributes<CommandAttribute>()
                    .Where(a => a.Name == command.Split(" ").FirstOrDefault(""))
                    .ToList()
                    .ForEach(
                        _ => result = (CommandResult)m.Invoke(null, [string.Join(" ", command.Split(" ")[1..])])));
        }
        catch (Exception e)
        {
            Error("An error occured:");
            Error(e.ToString());
            result = CommandResult.Fail;
        }

        return result;
    }
}
public struct CommandResult
{
    public string Message;
    public static CommandResult Success = new() { Message = "Success" };
    public static CommandResult Fail = new() { Message = "Failed" };

    public static bool operator ==(CommandResult a, CommandResult b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(CommandResult a, CommandResult b)
    {
        return !a.Equals(b);
    }
}
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class DescriptionAttribute : Attribute
{
    public string Description = "";

    public DescriptionAttribute(string desc)
    {
        Description = desc;
    }
}
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
internal sealed class CommandAttribute : Attribute
{
    public string Name = "";

    public CommandAttribute(string name)
    {
        Name = name;
    }
}