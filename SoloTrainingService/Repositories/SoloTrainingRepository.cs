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
}