using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;

namespace SkyBook.Business.Service;

public class UserService : IUserService
{
    private ApplicationDbContext context = new ApplicationDbContext();

    public void CancelAccount()
    {
        
    }
    

    public void RegisterUser()
    {
        throw new NotImplementedException();
    }

    public void LoinUser()
    {
        throw new NotImplementedException();
    }
}