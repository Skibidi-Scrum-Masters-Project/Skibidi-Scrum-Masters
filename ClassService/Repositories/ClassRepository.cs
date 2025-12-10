using ClassService.Model;
using MongoDB.Driver;

public class ClassRepository : IClassRepository
{
      private readonly IMongoCollection<FitnessClass> _classesCollection;

    public ClassRepository(IMongoDatabase database)
    {
        _classesCollection = database.GetCollection<FitnessClass>("Classes");
    }
    //TBA
    public async Task<FitnessClass> CreateClassAsync(FitnessClass fitnessClass)
    {
        fitnessClass.IsActive = true;
        await  _classesCollection.InsertOneAsync(fitnessClass);
        return fitnessClass;
    }

    public Task<IEnumerable<FitnessClass>> GetAllActiveClassesAsync()
    {
        List<FitnessClass> classes =  _classesCollection.Find(c => c.IsActive).ToList();
        if (classes == null)
        {
            return Task.FromResult(Enumerable.Empty<FitnessClass>());
        }
        return Task.FromResult(classes.AsEnumerable());
    }
}