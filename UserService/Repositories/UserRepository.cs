using FitnessApp.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IMongoDatabase database, ILogger<UserRepository> logger)
    {
        _usersCollection = database.GetCollection<User>("Users");
        _logger = logger;
    }
    public UserRepository(IMongoDatabase database)
        : this(database, NullLogger<UserRepository>.Instance)
    {
    }
   
    public User CreateUser(User user)
    {
        _logger.LogInformation("Creating user with username: {Username} in the repo", user.Username);
        try
        {
            // Validate input
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required", nameof(user));
            if (GetUserByUsername(user.Username) != null)
                throw new ArgumentException("Username already exists", nameof(user));
                
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

    public List<UserDTO> GetAllUsers()
    {
        try
        {
            return _usersCollection.Find(_ => true).Project<UserDTO>(Builders<User>.Projection
                .Include(u => u.Id)
                .Include(u => u.Username)
                .Include(u => u.FirstName)
                .Include(u => u.LastName)
                .Include(u => u.Email)
                .Include(u => u.Role)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving all users: {ex.Message}");
            return new List<UserDTO>();
        }

    }

    public UserDTO? GetUserById(string id)
    {
        try
        {
            return _usersCollection.Find(u => u.Id == id).Project<UserDTO>(Builders<User>.Projection
                .Include(u => u.Id)
                .Include(u => u.Username)
                .Include(u => u.FirstName)
                .Include(u => u.LastName)
                .Include(u => u.Email)
                .Include(u => u.Role)).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by ID {id}: {ex.Message}");
            return null;
        }
    }

    public UserDTO? GetUserByUsername(string username)
    {
        try
        {
        return _usersCollection.Find(u => u.Username == username).Project<UserDTO>(Builders<User>.Projection
                .Include(u => u.Id)
                .Include(u => u.Username)
                .Include(u => u.FirstName)
                .Include(u => u.LastName)
                .Include(u => u.Email)
                .Include(u => u.Role)).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by username {username}: {ex.Message}");
            return null;
        }
    }
        public User? GetUserByUsernameSecure(string username)
    {
        try
        {
        return _usersCollection.Find(u => u.Username == username).FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user by username {username}: {ex.Message}");
            return null;
        }
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
    public UserDTO UpdateUser(User updatedUser)
    {
        var userInDb = GetUserById(updatedUser.Id!);
        if (userInDb == null)
        {
            throw new ArgumentException("User not found", nameof(updatedUser));
        }
        _usersCollection.ReplaceOne(u => u.Id == updatedUser.Id, updatedUser);
        return GetUserById(updatedUser.Id!)!;
    }
    public bool DeleteUser(string id)
    {
        var userInDb = GetUserById(id);
        if (userInDb == null)
        {
            return false;
        }

        _usersCollection.DeleteOne(u => u.Id == id);
        return true;
    }
    public List<UserDTO> GetUsersByRole(Role role)
    {
        var users = _usersCollection.Find(u => u.Role == role).Project<UserDTO>(Builders<User>.Projection
                .Include(u => u.Id)
                .Include(u => u.Username)
                .Include(u => u.FirstName)
                .Include(u => u.LastName)
                .Include(u => u.Email)
                .Include(u => u.Role)).ToList();
        return users;
    }


}