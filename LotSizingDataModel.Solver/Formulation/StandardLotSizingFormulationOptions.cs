using System;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Configures the standard solver-independent mixed-integer
/// lot-sizing formulation.
/// </summary>
public sealed class StandardLotSizingFormulationOptions
{
    /// <summary>Gets or sets whether production setups are included.</summary>
    public bool IncludeProductionSetups { get; set; } = true;

    /// <summary>Gets or sets whether backlog is included.</summary>
    public bool IncludeBacklog { get; set; } = true;

    /// <summary>Gets or sets whether shortages are included.</summary>
    public bool IncludeShortage { get; set; } = true;

    /// <summary>Gets or sets whether procurement is included.</summary>
    public bool IncludeProcurement { get; set; } = true;

    /// <summary>Gets or sets whether transport is included.</summary>
    public bool IncludeTransport { get; set; } = true;

    /// <summary>Gets or sets whether transport setups are included.</summary>
    public bool IncludeTransportSetups { get; set; } = true;

    /// <summary>Gets or sets whether additional capacities are included.</summary>
    public bool IncludeAdditionalCapacity { get; set; } = true;

    /// <summary>Gets or sets whether resource activations are included.</summary>
    public bool IncludeResourceActivation { get; set; } = true;

    /// <summary>Gets or sets whether safety-stock constraints are included.</summary>
    public bool IncludeSafetyStock { get; set; } = true;

    /// <summary>Gets or sets whether safety-stock violations are allowed.</summary>
    public bool IncludeSafetyStockViolation { get; set; } = true;

    /// <summary>Gets or sets whether BOM relations are included.</summary>
    public bool IncludeBillOfMaterials { get; set; } = true;

    /// <summary>Gets or sets whether structurally zero variables are omitted.</summary>
    public bool RemoveStructurallyZeroVariables { get; set; } = true;

    /// <summary>Gets or sets the structural zero tolerance.</summary>
    public double StructuralZeroTolerance { get; set; } = 1e-12;

    /// <summary>
    /// Gets or sets whether production/setup Big-M values are
    /// estimated automatically from the instance structure.
    /// </summary>
    public bool UseAutomaticProductionSetupBigM { get; set; } = true;

    /// <summary>
    /// Gets or sets the fallback Big-M used when the automatic
    /// estimator cannot establish a finite structural bound, or
    /// the fixed Big-M when automatic estimation is disabled.
    /// </summary>
    /// <remarks>
    /// This property is preserved for backward compatibility.
    /// It is no longer the normal Big-M used by every production
    /// setup-link constraint.
    /// </remarks>
    public double ProductionSetupBigM { get; set; } = 1e6;

    /// <summary>Creates an independent copy.</summary>
    public StandardLotSizingFormulationOptions Clone()
    {
        return (StandardLotSizingFormulationOptions)MemberwiseClone();
    }

    /// <summary>Validates the option values.</summary>
    public void EnsureValid()
    {
        if (!double.IsFinite(StructuralZeroTolerance) ||
            StructuralZeroTolerance < 0.0)
        {
            throw new InvalidOperationException(
                "The structural zero tolerance must be finite and non-negative.");
        }

        if (!double.IsFinite(ProductionSetupBigM) ||
            ProductionSetupBigM <= 0.0)
        {
            throw new InvalidOperationException(
                "ProductionSetupBigM must be finite and strictly positive.");
        }

        if (IncludeTransportSetups && !IncludeTransport)
        {
            throw new InvalidOperationException(
                "Transport setups cannot be enabled when transport is disabled.");
        }

        if (IncludeSafetyStockViolation && !IncludeSafetyStock)
        {
            throw new InvalidOperationException(
                "Safety-stock violations cannot be enabled when safety stock is disabled.");
        }
    }
}
