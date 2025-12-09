using FitnessApp.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;

public class SoloTrainingRepository : ISoloTrainingRepository
{
        private readonly IMongoCollection<SoloTrainingSession> _SolotrainingCollection;

    public SoloTrainingRepository(IMongoDatabase database)
    {
        _SolotrainingCollection = database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
    }
    //TBA
    public SoloTrainingSession CreateSoloTraining(string userId, SoloTrainingSession soloTraining)
    {
        soloTraining.UserId = userId.ToString();
        _SolotrainingCollection.InsertOne(soloTraining);
        return soloTraining;
    }

    public List<SoloTrainingSession> GetAllSoloTrainingsForUser(string userId)
    {
        var filter = Builders<SoloTrainingSession>.Filter.Eq(s => s.UserId, userId);
        return _SolotrainingCollection.Find(filter).ToList();
    }
}