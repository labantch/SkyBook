using SkyBook.Business.Interfaces;
using SkyBook.Data.Data;

namespace SkyBook.Business.Service;

public class UserService : IUserService
{
    private readonly ApplicationDbContext context;

    public UserService(ApplicationDbContext _context)
    {
        context = _context;
    }

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