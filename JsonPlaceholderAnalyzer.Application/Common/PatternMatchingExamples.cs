using JsonPlaceholderAnalyzer.Application.DTOs;
using JsonPlaceholderAnalyzer.Domain.Entities;
using JsonPlaceholderAnalyzer.Domain.Enums;

namespace JsonPlaceholderAnalyzer.Application.Common;

/// <summary>
/// Ejemplos educativos de Pattern Matching en C#.
/// Este archivo es para referencia y aprendizaje.
/// </summary>
public static class PatternMatchingExamples
{
    #region 1. Type Patterns

    /// <summary>
    /// TYPE PATTERNS: Verifican el tipo de un objeto.
    /// </summary>
    public static string TypePatternExample(object obj)
    {
        // Sintaxis antigua (C# 6 y anterior)
        if (obj is string)
        {
            string s = (string)obj;
            return $"Es string: {s}";
        }
        
        // Sintaxis moderna (C# 7+): Type pattern con declaración
        if (obj is int number)
        {
            return $"Es int: {number}";
        }
        
        // Con switch expression (C# 8+)
        return obj switch
        {
            string s => $"String: {s}",
            int i => $"Int: {i}",
            double d => $"Double: {d}",
            bool b => $"Bool: {b}",
            null => "Es null",
            _ => $"Tipo desconocido: {obj.GetType().Name}"
        };
    }

    #endregion

    #region 2. Property Patterns

    /// <summary>
    /// PROPERTY PATTERNS: Verifican propiedades de un objeto.
    /// </summary>
    public static string PropertyPatternExample(User? user)
    {
        return user switch
        {
            // Verifica si user es null
            null => "Usuario no existe",
            
            // Property pattern simple
            { Name: "Leanne Graham" } => "¡Es Leanne!",
            
            // Property pattern con múltiples propiedades
            { Company.Name: "Romaguera-Crona", Address.City: "Gwenborough" } 
                => "Usuario de Romaguera-Crona en Gwenborough",
            
            // Property pattern con condición adicional (when guard)
            { Email: var email } when email.EndsWith(".biz") 
                => $"Email de negocios: {email}",
            
            // Property pattern con extracción de valores
            { Name: var name, Username: var username } 
                => $"{name} (@{username})"
        };
    }

    #endregion

    #region 3. Relational Patterns

    /// <summary>
    /// RELATIONAL PATTERNS: Comparan con operadores relacionales.
    /// Introducidos en C# 9.
    /// </summary>
    public static string RelationalPatternExample(int value)
    {
        return value switch
        {
            < 0 => "Negativo",
            0 => "Cero",
            > 0 and < 10 => "Positivo pequeño (1-9)",
            >= 10 and < 100 => "Dos dígitos (10-99)",
            >= 100 and < 1000 => "Tres dígitos (100-999)",
            >= 1000 => "Mil o más"
        };
    }

    /// <summary>
    /// Ejemplo con double y rangos.
    /// </summary>
    public static string GradeExample(double score)
    {
        return score switch
        {
            < 0 or > 100 => "Puntuación inválida",
            >= 90 => "A - Excelente",
            >= 80 => "B - Muy bien",
            >= 70 => "C - Bien",
            >= 60 => "D - Suficiente",
            _ => "F - Insuficiente"
        };
    }

    #endregion

    #region 4. Logical Patterns

    /// <summary>
    /// LOGICAL PATTERNS: Combinan patterns con and, or, not.
    /// Introducidos en C# 9.
    /// </summary>
    public static string LogicalPatternExample(object obj)
    {
        return obj switch
        {
            // AND pattern - Más específico primero
            int i and > 0 and < 100 => $"Entero entre 1 y 99: {i}",
            
            // OR pattern - Valores literales específicos
            "yes" or "si" or "sí" => "Afirmativo",
            "no" or "nein" or "non" => "Negativo",
            
            // Combinación compleja con strings
            string s and { Length: > 0 and < 10 } => $"String corto: {s}",
            string s and { Length: >= 10 } => $"String largo: {s[..10]}...",
            
            // NOT pattern - Más general, va después de los específicos
            not null => "No es null (otro tipo)",
            
            // null - El más general
            null => "Es null"
        };
    }

    #endregion

    #region 5. Positional Patterns (Deconstruction)

    /// <summary>
    /// POSITIONAL PATTERNS: Usan deconstrucción de objetos.
    /// </summary>
    public static string PositionalPatternExample(Point point)
    {
        // Point tiene Deconstruct(out int x, out int y)
        return point switch
        {
            (0, 0) => "Origen",
            (var x, 0) => $"En eje X: ({x}, 0)",
            (0, var y) => $"En eje Y: (0, {y})",
            (var x, var y) when x == y => $"En diagonal: ({x}, {y})",
            (var x, var y) when x > 0 && y > 0 => $"Cuadrante I: ({x}, {y})",
            (var x, var y) when x < 0 && y > 0 => $"Cuadrante II: ({x}, {y})",
            (var x, var y) when x < 0 && y < 0 => $"Cuadrante III: ({x}, {y})",
            (var x, var y) => $"Cuadrante IV: ({x}, {y})"
        };
    }

    /// <summary>
    /// Record para demostrar deconstrucción.
    /// </summary>
    public record Point(int X, int Y);

    #endregion

    #region 6. List Patterns (C# 11)

    /// <summary>
    /// LIST PATTERNS: Hacen pattern matching en colecciones.
    /// Introducidos en C# 11.
    /// </summary>
    public static string ListPatternExample(int[] numbers)
    {
        return numbers switch
        {
            [] => "Array vacío",
            [var single] => $"Un elemento: {single}",
            [var first, var second] => $"Dos elementos: {first}, {second}",
            [1, 2, 3] => "Es [1, 2, 3]",
            [0, ..] => "Empieza con cero",
            [.., 0] => "Termina con cero",
            [var first, .., var last] => $"Primero: {first}, Último: {last}",
            _ => $"Array con {numbers.Length} elementos"
        };
    }

    /// <summary>
    /// List pattern con slice pattern (..)
    /// </summary>
    public static string SlicePatternExample(string[] words)
    {
        return words switch
        {
            ["hello", .. var rest] => $"Saludo seguido de {rest.Length} palabras",
            [var first, "world"] => $"{first} world!",
            [var first, .. var middle, var last] 
                => $"De '{first}' a '{last}' con {middle.Length} en medio",
            _ => "Patrón no reconocido"
        };
    }

    #endregion

    #region 7. Pattern Matching en Expresiones

    /// <summary>
    /// USO EN EXPRESIONES: is, switch, when.
    /// </summary>
    public static void ExpressionExamples()
    {
        object value = 42;
        
        // 'is' pattern en if
        if (value is int num and > 0)
        {
            Console.WriteLine($"Número positivo: {num}");
        }
        
        // 'is not' pattern
        if (value is not null)
        {
            Console.WriteLine("No es null");
        }
        
        // 'is' con property pattern
        User? user = new User 
        { 
            Id = 1, 
            Name = "Test", 
            Username = "test", 
            Email = "test@test.com",
            Address = new Address("St", "Suite", "City", "12345", new GeoLocation("0", "0")),
            Company = new Company("Co", "Phrase", "bs")
        };
        
        if (user is { Email: var email } && email.Contains("@"))
        {
            Console.WriteLine($"Email válido: {email}");
        }
        
        // Conditional expression con pattern
        string result = value is int i ? $"Es entero: {i}" : "No es entero";
    }

    #endregion

    #region 8. Practical Examples

    /// <summary>
    /// Ejemplo práctico: Clasificar entidad para UI.
    /// </summary>
    public static (string Icon, ConsoleColor Color, string Label) ClassifyForUI(object entity)
    {
        return entity switch
        {
            User { Company.Name: var company } => ("👤", ConsoleColor.Cyan, $"Usuario de {company}"),
            Post { WordCount: > 100 } => ("📚", ConsoleColor.Green, "Post extenso"),
            Post { WordCount: < 20 } => ("📄", ConsoleColor.Gray, "Post breve"),
            Post => ("📝", ConsoleColor.White, "Post"),
            Todo { Completed: true } => ("✅", ConsoleColor.Green, "Completado"),
            Todo { Completed: false, Priority: TodoPriority.High } => ("🔥", ConsoleColor.Red, "Urgente"),
            Todo { Completed: false } => ("⏳", ConsoleColor.Yellow, "Pendiente"),
            Comment => ("💬", ConsoleColor.Blue, "Comentario"),
            Album => ("📸", ConsoleColor.Magenta, "Álbum"),
            Photo => ("🖼️", ConsoleColor.DarkMagenta, "Foto"),
            null => ("❌", ConsoleColor.DarkGray, "Nulo"),
            _ => ("❓", ConsoleColor.Gray, entity.GetType().Name)
        };
    }

    /// <summary>
    /// Ejemplo práctico: Validar y procesar request.
    /// </summary>
    public static Result<string> ProcessRequest(QueryRequest? request)
    {
        return request switch
        {
            null => new Result<string>("Request no puede ser null", false),
            { Pagination.Page: < 1 } => new Result<string>("Página debe ser >= 1", false),
            { Pagination.PageSize: < 1 or > 100 } => new Result<string>("PageSize debe estar entre 1 y 100", false),
            { SearchTerm: { Length: > 100 } } => new Result<string>("Búsqueda muy larga", false),
            { HasSearch: true, SearchTerm: var term } => new Result<string>($"Buscando: {term}", true),
            _ => new Result<string>("Request válido", true)
        };
    }

    /// <summary>
    /// Resultado simple para el ejemplo.
    /// </summary>
    public record Result<T>(T Value, bool IsSuccess);

    #endregion
}