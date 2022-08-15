using System.Linq.Expressions;
namespace Tester;
public interface IDatabaseTester{
    public string getName();
    public void setDatabase(string database);
    public void addElement(string collection, models.productModel product);

    public void addElements(string collection, List<models.productModel> products);

    public void deleteElement(string collection, Expression<Func<models.productModel, bool>> searchFunction);

    public void deleteElements(string collection, Expression<Func<models.productModel, bool>> searchFunction);

    public List<models.productModel> getElements(string collection, Expression<Func<models.productModel, bool>> searchFunction);

    public long count(string collection);

    public void transferElements(string source, string target);
}