using System;
using System.Linq;
using Ardalis.Specification;
using PluralsightDdd.SharedKernel;

namespace FrontDesk.Core.ScheduleAggregate.Specifications
{
  public class ScheduleByIdAndDateWithAppointmentsSpec : Specification<Schedule>, ISingleResultSpecification
  {
    public ScheduleByIdAndDateWithAppointmentsSpec(Guid scheduleId, DateTimeOffset date)
    {
      var dateRange = new DateTimeOffsetRange(date.Date, date.Date.AddDays(1));
      Query
        .Where(schedule => schedule.Id == scheduleId)
        .Include(schedule => schedule.Appointments.Where(a => a.TimeRange.Overlaps(dateRange)));
    }
  }
}
