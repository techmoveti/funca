using Funca.Abstractions.Data;

namespace Funca.Abstractions.Metadata;

public static class MetadataModule
{
    public static FieldMetadata<T> Field<T>(
        string codigo,
        string nome,
        FieldType tipo,
        Expression<Func<T, object?>> selector) where T : IState
        => new(codigo, nome, tipo, selector);
}