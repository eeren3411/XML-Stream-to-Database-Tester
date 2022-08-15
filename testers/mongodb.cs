using MongoDB.Driver;
using System.Linq.Expressions;

namespace Tester;
public class MongoTester : IDatabaseTester{
    public string getName(){return "MongoDB";}
    private MongoClient client;
    private IMongoDatabase db;
    public MongoTester(
        string host,
        string port,
        string usrName,
        string password,
        string database,
        bool userTls = false
    ){
        string conString = $"mongodb://{usrName}:{password}@{host}:{port}";
        client = new MongoClient(conString);
        db = client.GetDatabase(database);
        //Console.WriteLine(conString);
    }

    public void setDatabase(string database){
        db = client.GetDatabase(database);
    }

    public void addElement(string collection, models.productModel product){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        productCollection.InsertOne(product);
    }

    public void addElements(string collection, List<models.productModel> products){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        productCollection.InsertMany(products);
    }

    public void deleteElement(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        productCollection.DeleteOne(searchFunction);
    }

    public void deleteElements(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        productCollection.DeleteMany(searchFunction);
    }

    public List<models.productModel> getElements(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        return productCollection.Find(searchFunction).ToList();
    }

    public long count(string collection){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        return productCollection.CountDocuments(_ => true);
    }
    public long count(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        IMongoCollection<models.productModel> productCollection = db.GetCollection<models.productModel>(collection);

        return productCollection.CountDocuments(searchFunction);
    }

    public void transferElements(string source, string target){
        IMongoCollection<models.productModel> sourceCollection = db.GetCollection<models.productModel>(source);
        IMongoCollection<models.productModel> targetCollection = db.GetCollection<models.productModel>(target);
        
        List<string> distinctSkus = sourceCollection.Distinct<string>("sku", FilterDefinition<models.productModel>.Empty).ToList();
        targetCollection.DeleteMany(_ => distinctSkus.Contains(_.sku));
        targetCollection.InsertMany(sourceCollection.AsQueryable());
        sourceCollection.DeleteMany(_ => true);
    }
}