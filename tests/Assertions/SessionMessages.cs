using System.Collections.Immutable;
using QaaS.Framework.SDK.Session.SessionDataObjects;
using Soldier = OmegaSolider.Messages.OmegaSolider;
using Expert = OmegaSolider.Messages.FireflyExpert;

namespace OmegaFireflyTests.Assertions;

public static class SessionMessages
{
    public static (List<Soldier> Inputs, List<Expert> Outputs) Read(IImmutableList<SessionData> sessions)
    {
        if (sessions.Count != 1) throw new InvalidDataException("Expected exactly one isolated session.");
        var inputs = (sessions[0].Inputs ?? []).SelectMany(x => x.Data).Select(x => x.Body switch
        {
            Soldier s => s,
            byte[] b => Soldier.Parser.ParseFrom(b),
            _ => throw new InvalidDataException("Unsupported input body.")
        }).ToList();
        var outputs = (sessions[0].Outputs ?? []).SelectMany(x => x.Data).Select(x => x.Body switch
        {
            Expert e => e,
            byte[] b => Expert.Parser.ParseFrom(b),
            _ => throw new InvalidDataException("Unsupported output body.")
        }).ToList();
        if (inputs.Count == 0) throw new InvalidDataException("No input evidence.");
        return (inputs, outputs);
    }
}
