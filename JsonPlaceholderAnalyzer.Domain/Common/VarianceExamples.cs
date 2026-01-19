namespace JsonPlaceholderAnalyzer.Domain.Common;

/// <summary>
/// Este archivo contiene ejemplos documentados de covarianza y contravarianza.
/// Es solo para fines educativos y demostración.
/// </summary>
public static class VarianceExamples
{
    /// <summary>
    /// COVARIANZA (out): Permite usar un tipo más derivado donde se espera un tipo base.
    /// Se usa cuando el tipo genérico solo SALE (se retorna) del método.
    /// 
    /// Ejemplo con IEnumerable<out T>:
    /// </summary>
    public static void CovarianceExample()
    {
        // IEnumerable es covariante (out T)
        IEnumerable<string> strings = new List<string> { "a", "b", "c" };
        
        // Podemos asignar IEnumerable<string> a IEnumerable<object>
        // porque string deriva de object y IEnumerable es covariante
        IEnumerable<object> objects = strings; // ✓ Esto compila

        // Esto funciona porque solo LEEMOS de la colección, nunca escribimos
        foreach (object obj in objects)
        {
            Console.WriteLine(obj);
        }
    }

    /// <summary>
    /// CONTRAVARIANZA (in): Permite usar un tipo base donde se espera un tipo derivado.
    /// Se usa cuando el tipo genérico solo ENTRA (es parámetro) del método.
    /// 
    /// Ejemplo con Action<in T>:
    /// </summary>
    public static void ContravarianceExample()
    {
        // Action<T> es contravariante (in T)
        Action<object> printObject = obj => Console.WriteLine(obj?.ToString());
        
        // Podemos asignar Action<object> a Action<string>
        // porque podemos pasar un string a algo que acepta object
        Action<string> printString = printObject; // ✓ Esto compila

        // Funciona porque el método que acepta object puede recibir string
        printString("Hello"); // Internamente llama a printObject
    }

    /// <summary>
    /// INVARIANZA: El tipo debe coincidir exactamente.
    /// Se usa cuando el tipo genérico tanto ENTRA como SALE.
    /// 
    /// Ejemplo con IList<T>:
    /// </summary>
    public static void InvarianceExample()
    {
        // IList<T> es invariante (sin modificador in/out)
        IList<string> strings = new List<string>();
        
        // NO podemos asignar IList<string> a IList<object>
        // IList<object> objects = strings; // ✗ Error de compilación
        
        // Esto es porque IList permite tanto leer como escribir
        // Si permitiera la asignación, podríamos hacer:
        // objects.Add(123); // Intentar agregar un int a una lista de strings!
    }

    /// <summary>
    /// LIMITACIÓN IMPORTANTE: Clases genéricas no pueden ser covariantes/contravariantes.
    /// Solo las INTERFACES y DELEGADOS pueden usar 'in' y 'out'.
    /// 
    /// Por esto, nuestra clase Result<T> es INVARIANTE y no podemos hacer:
    ///   interface IReadOnlyRepository<out T> { Task<Result<T>> GetAsync(); }
    /// 
    /// El error sería: "Varianza no válida: Result<T> no es covariante"
    /// </summary>
    public static void ClassVarianceLimitation()
    {
        // Result<T> es una CLASE, no una interfaz
        // Las clases son siempre INVARIANTES en C#
        
        // Esto NO compila:
        // Result<string> stringResult = Result<string>.Success("hello");
        // Result<object> objectResult = stringResult; // ✗ Error!
        
        // Para que funcionara, Result<T> tendría que ser:
        // 1. Una interfaz: interface IResult<out T> { T Value { get; } }
        // 2. O usar conversiones explícitas
        
        // En la práctica, esto rara vez es un problema porque:
        // - Usamos Result<T> para manejar errores, no para polimorfismo
        // - Los repositorios trabajan con tipos concretos
    }

    /// <summary>
    /// Ejemplo de cómo SÍ funciona la covarianza con interfaces puras.
    /// </summary>
    public static void WorkingCovarianceWithInterfaces()
    {
        // IEnumerable<out T> es covariante
        IEnumerable<string> strings = new List<string> { "a", "b" };
        IEnumerable<object> objects = strings; // ✓ Funciona!
        
        // Func<out TResult> es covariante en TResult
        Func<string> getString = () => "hello";
        Func<object> getObject = getString; // ✓ Funciona!
        
        // IReadOnlyList<out T> es covariante
        IReadOnlyList<string> readOnlyStrings = new List<string> { "x", "y" };
        IReadOnlyList<object> readOnlyObjects = readOnlyStrings; // ✓ Funciona!
    }
}