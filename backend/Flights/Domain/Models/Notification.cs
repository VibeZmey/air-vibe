using Flights.Domain.Events;

namespace Flights.Domain.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPayload Payload { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static Notification CreateCheckInOpened(CheckInOpenedEvent notification, Guid userId)
    {
        return new Notification()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = NotificationType.CheckInOpened,
            Payload = new NotificationPayload()
            {
                FlightNumber = notification.FlightNumber,
                DepartureTime = notification.DepartureTime,
                StartTime = notification.StartTime,
                EndTime = notification.EndTime,
                Status = notification.NewStatus
            }
        };
    }
    public static Notification CreateOrderConfirmed(OrderConfirmedEvent notification)
    {
        return new Notification()
        {
            Id = Guid.NewGuid(),
            UserId = notification.UserId,
            Type = NotificationType.OrderConfirmed,
            Payload = new NotificationPayload()
            {
                FlightNumber = notification.FlightNumber,
                TotalPrice = notification.TotalPrice,
            }
        };
    }
}

public record NotificationPayload
{
    // Для рейсов
    public string? FlightNumber { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? Gate { get; set; }
    public string? CityFrom { get; set; }
    public string? CityTo { get; set; }
    public FlightStatus Status { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    // Для бронирований
    public Guid? BookingId { get; set; }
    public string? BookingReference { get; set; }
    
    public string? PassengerName { get; set; }
    public string? PassengerEmail { get; set; }
    
    // Для оплаты
    public decimal? TotalPrice { get; set; }
    public string? Currency { get; set; }
    
    // Для напоминаний
    public TimeSpan? HoursBefore { get; set; }
    
    // Общие
    public string? Title { get; set; }
    public string? Message { get; set; }
}

public enum NotificationType
{
    OrderConfirmed,
    OrderCancelled,
    OrderCreated,
    
    BookingConfirmed,       
    BookingCancelled,        
    BookingExpired,           
    
    CheckInOpened,         
    BoardingStarted,      
    
    FlightDelayed,        
    FlightCancelled,        
    FlightDeparted,            
    FlightArrived,           
    
    FlightReminder24h,    
    FlightReminder3h,       
    FlightReminder1h,       
    
    GeneralAnnouncement,     
    SystemMaintenance,       
    FeedbackRequest       
}