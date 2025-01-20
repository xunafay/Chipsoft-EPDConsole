namespace Chipsoft.Assignments.EPDConsole.Tests;

[TestClass]
public class TimeToolsTests
{
    [TestMethod]
    public void TestFindAvailableTimeSlots()
    {
        var appointments = new List<Appointment>
        {
            new(new DateTime(2025, 1, 20, 10, 0, 0), null, 0, 0),
            new(new DateTime(2025, 1, 20, 10, 45, 0), null, 0, 0),
            new(new DateTime(2025, 1, 20, 11, 15, 0), null, 0, 0),
        };
        // 4 slots from 9:00 to 10:00
        // 2 slot from 10:15 to 10:45
        // 1 slot from 11:00 to 11:15
        // 2 slots from 11:30 to 12:00

        DateTime day = new(2025, 1, 20);

        var availableSlots = TimeTools.FindAvailableTimeSlots(appointments, day, new(9, 0), new(12, 0));
        var expectedSlots = new List<DateTime>
        {
            // 9:00 to 10:00
            new(2025, 1, 20, 9, 0, 0),
            new(2025, 1, 20, 9, 15, 0),
            new(2025, 1, 20, 9, 30, 0),
            new(2025, 1, 20, 9, 45, 0),

            // 10:00 to 11:00
            new(2025, 1, 20, 10, 15, 0),
            new(2025, 1, 20, 10, 30, 0),

            // 11:00 to 12:00
            new(2025, 1, 20, 11, 00, 0),
            new(2025, 1, 20, 11, 30, 0),
            new(2025, 1, 20, 11, 45, 0),
        };

        Assert.AreEqual(expectedSlots.Count, availableSlots.Count, "Number of available slots mismatch.");
        for (int i = 0; i < expectedSlots.Count; i++)
        {
            Assert.AreEqual(expectedSlots[i], availableSlots[i], $"Mismatch at slot {i}: Expected {expectedSlots[i]}, but got {availableSlots[i]}.");
        }
    }
}