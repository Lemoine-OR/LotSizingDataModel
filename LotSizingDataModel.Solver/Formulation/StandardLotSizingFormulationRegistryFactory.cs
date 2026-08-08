using System;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates formulation registries preconfigured with the
/// standard lot-sizing formulation.
/// </summary>
public static class StandardLotSizingFormulationRegistryFactory
{
    /// <summary>
    /// Creates a formulation registry containing the default
    /// standard formulation.
    /// </summary>
    /// <returns>
    /// New formulation registry.
    /// </returns>
    public static MathematicalModelFormulationRegistry CreateDefault()
    {
        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        return registry;
    }

    /// <summary>
    /// Creates a formulation registry containing a standard
    /// formulation configured with the supplied options.
    /// </summary>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <returns>
    /// New formulation registry.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static MathematicalModelFormulationRegistry Create(
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.Create(
                options));

        return registry;
    }

    /// <summary>
    /// Registers or replaces the standard formulation in an
    /// existing formulation registry.
    /// </summary>
    /// <param name="registry">
    /// Target formulation registry.
    /// </param>
    /// <param name="options">
    /// Standard formulation options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when an argument is <see langword="null"/>.
    /// </exception>
    public static void RegisterOrReplace(
        MathematicalModelFormulationRegistry registry,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        ArgumentNullException.ThrowIfNull(
            options);

        registry.RegisterOrReplace(
            StandardLotSizingFormulationFactory.Create(
                options));
    }
}
