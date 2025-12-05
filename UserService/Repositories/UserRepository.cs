using FitnessApp.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserRepository(IMongoDatabase database)
    {
        _usersCollection = database.GetCollection<User>("Users");
    }
   
    public User CreateUser(User user)
    {
        try
        {
            // Validate input
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required", nameof(user));
                
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required", nameof(user));
                
            if (string.IsNullOrWhiteSpace(user.HashedPassword))
                throw new ArgumentException("Password is required", nameof(user));

            User hashedUser = HashPassword(user);
            _usersCollection.InsertOne(hashedUser);
            
            return hashedUser;
        }
        catch (MongoException ex)
        {
            throw new InvalidOperationException($"Database error during user creation: {ex.Message}", ex);
        }
        catch (ArgumentException)
        {
            // Re-throw validation errors as-is
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unexpected error during user creation: {ex.Message}", ex);
        }
    }

    public List<User> GetAllUsers()
    {
        return _usersCollection.Find(new BsonDocument()).ToList();
    }

    public User? GetUserById(string id)
    {
        try
        {
            return _usersCollection.Find(u => u.Id == id).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by ID {id}: {ex.Message}");
            return null;
        }
    }

    public User? GetUserByUsername(string username)
    {
        return _usersCollection.Find(u => u.Username == username).FirstOrDefault();
    }

    public User HashPassword(User user)
    {
        string unhashedPassword = user.HashedPassword!;
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(unhashedPassword, salt);
        user.Salt = salt;
        user.HashedPassword = hashedPassword;
        return user;
    }
}