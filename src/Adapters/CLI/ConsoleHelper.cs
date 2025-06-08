using System.Diagnostics;
using PdfTemplateCLI.Domain.Exceptions;

namespace PdfTemplateCLI.Presentation.ConsoleUI;

public static class ConsoleHelper
{
    private static readonly object ConsoleLock = new();
    
    public static void WriteHeader(string text)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {text} ===");
        Console.ResetColor();
        Console.WriteLine();
    }
    
    public static void WriteSuccess(string message)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("✓ ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
    
    public static void WriteError(string message)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("✗ ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
    
    public static void WriteWarning(string message)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("⚠ ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
    
    public static void WriteInfo(string message)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("ℹ ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }
    
    public static void WriteStep(string step)
    {
        lock (ConsoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("→ ");
            Console.ResetColor();
            Console.WriteLine(step);
        }
    }
    
    public static async Task ShowProgressAsync(string message, Func<Task> action)
    {
        var spinner = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
        var spinnerIndex = 0;
        var completed = false;
        var stopwatch = Stopwatch.StartNew();
        
        var spinnerTask = Task.Run(async () =>
        {
            while (!completed)
            {
                lock (ConsoleLock)
                {
                    Console.Write($"\r{spinner[spinnerIndex]} {message}... ");
                }
                spinnerIndex = (spinnerIndex + 1) % spinner.Length;
                await Task.Delay(100);
            }
        });
        
        try
        {
            await action();
            completed = true;
            await spinnerTask;
            
            lock (ConsoleLock)
            {
                Console.Write($"\r");
                WriteSuccess($"{message} ({stopwatch.ElapsedMilliseconds}ms)");
            }
        }
        catch
        {
            completed = true;
            await spinnerTask;
            
            lock (ConsoleLock)
            {
                Console.Write($"\r");
                WriteError($"{message} (failed after {stopwatch.ElapsedMilliseconds}ms)");
            }
            throw;
        }
    }
    
    public static void ShowException(Exception ex, bool showDetails = false)
    {
        WriteError($"Error: {ex.Message}");
        
        if (ex is DomainException domainEx)
        {
            WriteInfo($"Error Code: {domainEx.Code}");
        }
        
        if (showDetails && ex.InnerException != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Details: {ex.InnerException.Message}");
            Console.ResetColor();
        }
        
        if (showDetails && !string.IsNullOrEmpty(ex.StackTrace))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }
    
    public static string ReadInput(string prompt, string? defaultValue = null)
    {
        if (!string.IsNullOrEmpty(defaultValue))
        {
            Console.Write($"{prompt} [{defaultValue}]: ");
        }
        else
        {
            Console.Write($"{prompt}: ");
        }
        
        var input = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(defaultValue))
        {
            return defaultValue;
        }
        
        return input ?? string.Empty;
    }
    
    public static bool Confirm(string message, bool defaultValue = true)
    {
        var defaultText = defaultValue ? "Y/n" : "y/N";
        Console.Write($"{message} [{defaultText}]: ");
        
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();
        
        if (string.IsNullOrEmpty(input))
        {
            return defaultValue;
        }
        
        return input == "y" || input == "yes";
    }
    
    public static void ShowFileInfo(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"  File: {fileInfo.Name}");
            Console.WriteLine($"  Size: {FormatFileSize(fileInfo.Length)}");
            Console.WriteLine($"  Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        }
        catch
        {
            Console.WriteLine($"  File: {Path.GetFileName(filePath)}");
        }
    }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}