namespace Antigen.Models.Settings;

public enum IgnoreType
{
    /// <summary>
    ///     Ignores a specific instance identified by its full result identity
    /// </summary>
    Instance,

    /// <summary>
    ///     Ignores all topics with the same topic definition ID
    /// </summary>
    Topic,

    /// <summary>
    ///     Ignores all instances of a specific record
    /// </summary>
    Record
}