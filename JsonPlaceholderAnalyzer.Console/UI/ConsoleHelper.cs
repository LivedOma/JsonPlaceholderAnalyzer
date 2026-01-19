using JsonPlaceholderAnalyzer.Domain.Common;

namespace JsonPlaceholderAnalyzer.Console.UI;

/// <summary>
/// Utilidades para la consola.
/// </summary>
public static class ConsoleHelper
{
    public static void WriteHeader(string title)
    {
        var width = Math.Max(title.Length + 4, 50);
        var border = new string('═', width);
        var padding = (width - title.Length - 2) / 2;
        
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"╔{border}╗");
        System.Console.WriteLine($"║{new string(' ', padding)} {title} {new string(' ', width - padding - title.Length - 2)}║");
        System.Console.WriteLine($"╚{border}╝");
        System.Console.ResetColor();
    }

    public static void WriteSubHeader(string title)
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"═══ {title} ═══");
        System.Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"  ✓ {message}");
        System.Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"  ✗ {message}");
        System.Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"  ⚠ {message}");
        System.Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"  ℹ {message}");
        System.Console.ResetColor();
    }

    public static void WriteItem(string message, ConsoleColor color = ConsoleColor.White)
    {
        System.Console.ForegroundColor = color;
        System.Console.WriteLine($"    • {message}");
        System.Console.ResetColor();
    }

    public static void WriteTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> Selector, int Width)[] columns)
    {
        // Encabezados
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        foreach (var (header, _, width) in columns)
        {
            System.Console.Write($" {header.PadRight(width)} │");
        }
        System.Console.WriteLine();
        
        // Línea separadora
        foreach (var (_, _, width) in columns)
        {
            System.Console.Write($" {new string('─', width)} │");
        }
        System.Console.WriteLine();
        System.Console.ResetColor();
        
        // Datos
        foreach (var item in items)
        {
            foreach (var (_, selector, width) in columns)
            {
                var value = selector(item);
                var displayValue = value.Length > width ? value[..(width - 3)] + "..." : value;
                System.Console.Write($" {displayValue.PadRight(width)} │");
            }
            System.Console.WriteLine();
        }
    }

    public static string? ReadLine(string prompt)
    {
        System.Console.ForegroundColor = ConsoleColor.Gray;
        System.Console.Write($"  {prompt}: ");
        System.Console.ResetColor();
        return System.Console.ReadLine();
    }

    public static int? ReadInt(string prompt)
    {
        var input = ReadLine(prompt);
        return int.TryParse(input, out var result) ? result : null;
    }

    public static bool Confirm(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.Write($"  {message} (s/n): ");
        System.Console.ResetColor();
        
        var key = System.Console.ReadKey();
        System.Console.WriteLine();
        
        return key.KeyChar is 's' or 'S' or 'y' or 'Y';
    }

    public static void Pause(string message = "Presione cualquier tecla para continuar...")
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.Write($"  {message}");
        System.Console.ResetColor();
        System.Console.ReadKey(true);
        System.Console.WriteLine();
    }

    public static void Clear()
    {
        System.Console.Clear();
    }

    public static int ShowMenu(string title, params string[] options)
    {
        WriteSubHeader(title);
        System.Console.WriteLine();
        
        for (int i = 0; i < options.Length; i++)
        {
            var number = i == options.Length - 1 ? "0" : (i + 1).ToString();
            System.Console.WriteLine($"    [{number}] {options[i]}");
        }
        
        System.Console.WriteLine();
        
        while (true)
        {
            var input = ReadInt("Seleccione una opción");
            
            if (input == 0)
                return options.Length - 1; // Última opción (Salir/Volver)
            
            if (input > 0 && input < options.Length)
                return input.Value - 1;
            
            WriteError("Opción inválida. Intente de nuevo.");
        }
    }

    // Agregar al final de la clase ConsoleHelper:

    public static void WriteResult<T>(Result<T> result, Func<T, string>? successMessage = null)
    {
        if (result.IsSuccess)
        {
            var message = successMessage?.Invoke(result.Value!) ?? "Operation successful";
            WriteSuccess(message);
        }
        else
        {
            // Color según tipo de error usando Pattern Matching
            var (color, icon) = result.ErrorType switch
            {
                ErrorType.NotFound => (ConsoleColor.Yellow, "🔍"),
                ErrorType.Validation => (ConsoleColor.Magenta, "⚠"),
                ErrorType.Unauthorized => (ConsoleColor.Red, "🔒"),
                ErrorType.Network => (ConsoleColor.DarkYellow, "🌐"),
                ErrorType.Timeout => (ConsoleColor.DarkYellow, "⏱"),
                ErrorType.Exception => (ConsoleColor.DarkRed, "💥"),
                _ => (ConsoleColor.Red, "✗")
            };

            System.Console.ForegroundColor = color;
            System.Console.WriteLine($"  {icon} [{result.ErrorType}] {result.Error}");
            System.Console.ResetColor();
        }
    }

    public static void WriteResultDetails<T>(Result<T> result)
    {
        System.Console.WriteLine();
        System.Console.WriteLine($"    IsSuccess: {result.IsSuccess}");
        System.Console.WriteLine($"    ErrorType: {result.ErrorType}");
        
        if (result.IsSuccess)
        {
            System.Console.WriteLine($"    Value: {result.Value}");
        }
        else
        {
            System.Console.WriteLine($"    Error: {result.Error}");
            if (result.Exception != null)
            {
                System.Console.WriteLine($"    Exception: {result.Exception.GetType().Name}");
            }
        }
    }

}

