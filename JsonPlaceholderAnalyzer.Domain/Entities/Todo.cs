using JsonPlaceholderAnalyzer.Domain.Enums;

namespace JsonPlaceholderAnalyzer.Domain.Entities;

/// <summary>
/// Representa una tarea de JSONPlaceholder.
/// Demuestra: propiedades booleanas, status basado en estado.
/// </summary>
public class Todo : EntityBase<int>
{
    public required int UserId { get; init; }
    public required string Title { get; init; }
    public required bool Completed { get; init; }
    
    // Propiedades calculadas
    public string Status => Completed ? "✓ Completed" : "○ Pending";
    public TodoPriority Priority => DeterminePriority();
    
    private TodoPriority DeterminePriority()
    {
        // Lógica ficticia basada en palabras clave del título
        var lowerTitle = Title.ToLowerInvariant();
        
        if (lowerTitle.Contains("urgent") || lowerTitle.Contains("important"))
            return TodoPriority.High;
        if (lowerTitle.Contains("optional") || lowerTitle.Contains("maybe"))
            return TodoPriority.Low;
            
        return TodoPriority.Medium;
    }
}