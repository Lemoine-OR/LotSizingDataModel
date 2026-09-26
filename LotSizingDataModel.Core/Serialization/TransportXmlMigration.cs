using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Serialization;

/// <summary>Lossless, in-memory migration of legacy nested transport lanes. Input files are never written.</summary>
public static class TransportXmlMigration
{
    /// <summary>Loads XML through the caller's secure reader and returns a migrated copy.</summary>
    public static XDocument ReadAndMigrate(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var document = XDocument.Load(reader);
        foreach (var chain in document.Descendants().Where(e => e.Name.LocalName == "supplyChain"))
            MigrateSupplyChain(chain);
        return document;
    }

    private static void MigrateSupplyChain(XElement chain)
    {
        XNamespace ns = chain.Name.Namespace;
        var version = (string?)chain.Attribute("transportFormatVersion");
        if (version is not null && version != "1" && version != "2")
            throw new InvalidOperationException($"Unsupported transport format version '{version}'.");
        var resources = chain.Element(ns + "transportResources")?.Elements(ns + "transportResource").ToArray() ?? [];
        var containers = resources.SelectMany(r => r.Elements(ns + "lanes")).ToArray();
        bool central = chain.Element(ns + "transportLanes") is not null || chain.Element(ns + "transportAssignments") is not null;
        if (chain.Element(ns + "transportLanes")?.Elements(ns + "transportLane").Any(l => l.Attribute("leadTime") is not null) == true)
            throw new InvalidOperationException("Central lanes cannot carry delays; put the delay on each assignment.");
        if (containers.Length == 0)
        {
            if (version == "1" && central)
                throw new InvalidOperationException("Version 1 cannot contain central transport data.");
            if (version == "1") chain.SetAttributeValue("transportFormatVersion", "2");
            return;
        }
        if (central || version == "2")
            throw new InvalidOperationException("Mixed legacy and central transport representations cannot be merged implicitly.");

        var entries = resources.SelectMany(resource => resource.Elements(ns + "lanes")
            .SelectMany(c => c.Elements()).Select(lane =>
            {
                if (lane.Name != ns + "transportLane" || lane.Attributes().Any(a => a.Name != "leadTime") ||
                    lane.Elements().Any(e => e.Name != ns + "origin" && e.Name != ns + "destination"))
                    throw new InvalidOperationException("Unrecognized legacy lane data; migration would lose information.");
                var origin = ReadWarehouse(lane, ns + "origin");
                var destination = ReadWarehouse(lane, ns + "destination");
                if (origin.Key == destination.Key) throw new InvalidOperationException("A legacy lane connects a warehouse to itself.");
                int delay = ReadInteger(lane, "leadTime");
                int resourceId = ReadInteger(resource, "id");
                if (delay < 0 || resourceId < 0) throw new InvalidOperationException("Negative legacy delay or resource identifier.");
                return new { ResourceId = resourceId, Delay = delay, Origin = origin.Element, Destination = destination.Element,
                    Key = (origin.Key.Kind, origin.Key.Id, destination.Key.Kind, destination.Key.Id) };
            })).ToArray();

        var lanes = new XElement(ns + "transportLanes");
        var assignments = new XElement(ns + "transportAssignments");
        int id = 0;
        // Numeric tuple ordering is culture-independent and independent of resource/input ordering.
        foreach (var group in entries.GroupBy(e => e.Key).OrderBy(g => g.Key))
        {
            id = checked(id + 1);
            var first = group.First();
            lanes.Add(new XElement(ns + "transportLane", new XAttribute("id", id),
                new XElement(first.Origin), new XElement(first.Destination)));
            foreach (var pair in group.GroupBy(e => e.ResourceId).OrderBy(g => g.Key))
            {
                int[] delays = pair.Select(e => e.Delay).Distinct().ToArray();
                if (delays.Length != 1)
                    throw new InvalidOperationException($"Conflicting legacy delays for lane {group.Key}, resource {pair.Key}.");
                assignments.Add(new XElement(ns + "transportAssignment", new XAttribute("laneId", id),
                    new XAttribute("transportResourceId", pair.Key), new XAttribute("leadTime", delays[0])));
            }
        }
        // Commit only after every pair has passed conflict detection.
        foreach (var container in containers) container.Remove();
        chain.Add(lanes, assignments);
        chain.SetAttributeValue("transportFormatVersion", "2");
    }

    private static ((int Kind, int Id) Key, XElement Element) ReadWarehouse(XElement lane, XName name)
    {
        var element = lane.Elements(name).SingleOrDefault()
            ?? throw new InvalidOperationException($"Missing legacy {name.LocalName}.");
        string? kindText = (string?)element.Attribute("kind");
        if (!Enum.TryParse<WarehouseReferenceKind>(kindText, ignoreCase: true, out var kind) || !Enum.IsDefined(kind))
            throw new InvalidOperationException($"Invalid warehouse kind '{kindText}'.");
        int id = ReadInteger(element, "id");
        if (id < 0) throw new InvalidOperationException("Negative warehouse identifier.");
        return (((int)kind, id), element);
    }

    private static int ReadInteger(XElement element, string name) =>
        int.TryParse((string?)element.Attribute(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value : throw new InvalidOperationException($"Missing or invalid integer '{name}' in legacy transport data.");
}
