using System;

namespace BlazorShared.Models.Appointment
{
  public class DeleteAppointmentRequest : BaseRequest
  {
    public const string Route = "api/schedule/{ScheduleId}/appointments/{AppointmentId}?date={Date}";

    public Guid ScheduleId { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTimeOffset Date { get; set; }
  }
}
