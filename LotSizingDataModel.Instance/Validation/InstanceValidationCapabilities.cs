namespace LotSizingDataModel.Instance.Validation;

/// <summary>
/// Describes which downstream operations are permitted after
/// instance-model validation.
/// </summary>
/// <remarks>
/// This type is UI-independent. A future MVVM application may bind
/// directly to these capabilities without introducing presentation
/// dependencies into the domain libraries.
/// </remarks>
public sealed class InstanceValidationCapabilities
{
    internal InstanceValidationCapabilities(
        bool canSaveDraft,
        bool canValidate,
        bool canClassify,
        bool canGenerateNotation,
        bool canSolve,
        bool canExportAsValidatedInstance)
    {
        CanSaveDraft = canSaveDraft;
        CanValidate = canValidate;
        CanClassify = canClassify;
        CanGenerateNotation = canGenerateNotation;
        CanSolve = canSolve;
        CanExportAsValidatedInstance =
            canExportAsValidatedInstance;
    }

    /// <summary>
    /// Gets a value indicating whether the current instance may
    /// be saved as an incomplete or invalid draft.
    /// </summary>
    public bool CanSaveDraft { get; }

    /// <summary>
    /// Gets a value indicating whether validation can be run.
    /// </summary>
    public bool CanValidate { get; }

    /// <summary>
    /// Gets a value indicating whether automatic classification
    /// may safely use the instance.
    /// </summary>
    public bool CanClassify { get; }

    /// <summary>
    /// Gets a value indicating whether the universal notation
    /// may safely be generated.
    /// </summary>
    public bool CanGenerateNotation { get; }

    /// <summary>
    /// Gets a value indicating whether a solver may safely be
    /// invoked for this instance.
    /// </summary>
    public bool CanSolve { get; }

    /// <summary>
    /// Gets a value indicating whether the instance may be
    /// exported while claiming validated-instance status.
    /// </summary>
    public bool CanExportAsValidatedInstance { get; }
}
