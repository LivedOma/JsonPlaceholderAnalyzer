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
    /// Ejemplo práctico con nuestras interfaces del dominio.
    /// </summary>
    public static void DomainVarianceExample()
    {
        // Ejemplo de cómo se usaría en nuestra aplicación:
        
        // IReadOnlyRepository es covariante
        // IReadOnlyRepository<Post> postRepo = GetPostRepository();
        // IReadOnlyRepository<EntityBase<int>> baseRepo = postRepo; // ✓ Válido
        
        // IDataProcessor es contravariante
        // IDataProcessor<EntityBase<int>> baseProcessor = GetBaseProcessor();
        // IDataProcessor<Post> postProcessor = baseProcessor; // ✓ Válido
        
        // IRepository es invariante (tiene métodos con T como entrada y salida)
        // IRepository<Post> postRepo = GetPostRepository();
        // IRepository<EntityBase<int>> baseRepo = postRepo; // ✗ NO válido
    }
}