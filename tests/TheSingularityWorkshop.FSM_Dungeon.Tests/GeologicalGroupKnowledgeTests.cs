using Xunit;
using TheSingularityWorkshop.FSM_Dungeon.Geology;

namespace TheSingularityWorkshop.FSM_Dungeon.Tests;

public sealed class GeologicalGroupKnowledgeTests
{
    [Fact]
    public void Scout_discoveries_remain_private_until_report_is_delivered()
    {
        var scoutKnowledge = new[]
        {
            new GeologicalKnowledgeRecord("quartz-vein-exposed", 1, 0.9, "A quartz vein is exposed.")
        };
        var group = new GeologicalGroupKnowledge("cave-scavengers");

        Assert.False(group.KnowsEvent("quartz-vein-exposed"));

        var report = new ScoutGeologicalReport("scout-01", "cave-scavengers", scoutKnowledge, 12);
        var informedGroup = group.ReceiveReport(report);

        Assert.False(group.KnowsEvent("quartz-vein-exposed"));
        Assert.True(informedGroup.KnowsEvent("quartz-vein-exposed"));
        Assert.Empty(group.Records);
    }

    [Fact]
    public void Scout_killed_before_return_does_not_automatically_inform_group()
    {
        var privateScoutKnowledge = new[]
        {
            new GeologicalKnowledgeRecord("enemy-outpost", 1)
        };
        var group = new GeologicalGroupKnowledge("gate-wardens");
        var reporting = new ScoutReportingStatus("scout-02", expectedReportTick: 10);

        // The scout is lost before a report is delivered. Private knowledge is not
        // passed to the group; the group can only assess the missing report.
        Assert.False(group.KnowsEvent(privateScoutKnowledge[0].EventId));
        Assert.Equal("Report not yet due", reporting.Assess(10));
        Assert.Equal("Overdue — whereabouts unknown", reporting.Assess(11));
        Assert.False(group.KnowsEvent(privateScoutKnowledge[0].EventId));
    }

    [Fact]
    public void Report_for_another_group_is_rejected()
    {
        var group = new GeologicalGroupKnowledge("gate-wardens");
        var report = new ScoutGeologicalReport(
            "scout-03", "cave-scavengers",
            [new GeologicalKnowledgeRecord("sealed-route", 1)], 8);

        Assert.Throws<ArgumentException>(() => group.ReceiveReport(report));
    }

    [Fact]
    public void Duplicate_report_does_not_duplicate_group_knowledge()
    {
        var record = new GeologicalKnowledgeRecord("old-collapse", 1);
        var group = new GeologicalGroupKnowledge("miners");
        var report = new ScoutGeologicalReport("scout-04", "miners", [record], 5);

        var informed = group.ReceiveReport(report);
        var informedAgain = informed.ReceiveReport(report);

        Assert.Single(informedAgain.Records);
        Assert.Same(informed, informedAgain);
    }
}
