namespace Funca.Abstractions.Metadata;

/// <summary>
/// Specifies the field type in a state object, providing information about the data format or behavior of the field.
/// </summary>
public enum FieldType
{
    Id,
    Text,
    Integer,
    Decimal,
    Boolean,
    Date,
    DateTime,
    SingleSelect,
    MultiSelect
}