namespace SkyBook.Business.Interfaces;

public interface IBookingService
{
    //Passenger use: [My Booking]
    
    // public List<BookingVM> GetAll();
    // public BookingVM GetById (int id);
    public void Cancel(int id);
    
    //Admin 

    public void Search(int id);
    // public void List<PassengerVM> PassengerINFO();
    

}