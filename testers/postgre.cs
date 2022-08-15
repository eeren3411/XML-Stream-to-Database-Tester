using Npgsql;
using System.Linq.Expressions;
using System.Globalization;
using PostgreSQLCopyHelper;

namespace Tester;

class PostgreTester : IDatabaseTester{
    public string getName(){return "PostgreSQL";}
    string conString;
    NpgsqlConnection client;
    public PostgreTester(
        string host,
        string port,
        string usrName,
        string password,
        string database,
        bool userTls = false
    ){
        conString = $"Host={host};Port={port};Username={usrName};Password={password}; Timeout=300; CommandTimeout=300;";
        client = new NpgsqlConnection($"{conString};Database={database}");
        client.Open();
    }
    public void setDatabase(string database){
        client.Close();
        client = new NpgsqlConnection($"{conString};Database={database}");
    }

    public void Close(){
        client.Close();
    }

    public void runCommand(string command){
        NpgsqlCommand cmd = new NpgsqlCommand(command, client);
        cmd.ExecuteNonQuery();
    }

    public void createTable(string tableName, string tableConfig){
        NpgsqlCommand cmd = new NpgsqlCommand($"CREATE TABLE {tableName}({tableConfig})", client);
        cmd.ExecuteNonQuery();
    }

    public void dropTable(string tableName){
        NpgsqlCommand cmd = new NpgsqlCommand($"DROP TABLE IF EXISTS {tableName}", client);
        cmd.ExecuteNonQuery();
    }

    public void addElement(string collection, models.productModel product){
        string values = $"'({product.sku}', '{product.name.Replace("'", String.Empty)}', '{product.url.Replace("'", String.Empty)}', '{product.imgUrl.Replace("'", String.Empty)}', '{product.description.Replace("'", String.Empty)}', '{product.distributor.ToString()}', '{product.price.ToString().Replace(",", ".")}', '{product.shipPrice.ToString().Replace(",", ".")}', '{product.quantity.ToString()}', '{product.productBrand.Replace("'", String.Empty)}', '{product.productCategory.Replace("'", String.Empty)}', '{product.barcode}')";
        string cmdString = $"INSERT INTO {collection}(sku, name, url, imgUrl, description, distributor, price, shipPrice, quantity, productBrand, productCategory, barcode) VALUES {values};";
        NpgsqlCommand cmd = new NpgsqlCommand(cmdString, client);
        cmd.ExecuteNonQuery();
    }

    public void addElementsOld(string collection, List<models.productModel> products){
        string cmdString = $"INSERT INTO {collection}(sku, name, url, imgUrl, description, distributor, price, shipPrice, quantity, productBrand, productCategory, barcode) VALUES ";
        
        foreach(models.productModel product in products){
            string values = $"('{product.sku}', '{product.name.Replace("'", String.Empty)}', '{product.url.Replace("'", String.Empty)}', '{product.imgUrl.Replace("'", String.Empty)}', '{product.description.Replace("'", String.Empty)}', '{product.distributor.ToString()}', '{product.price.ToString().Replace(",", ".")}', '{product.shipPrice.ToString().Replace(",", ".")}', '{product.quantity.ToString()}', '{product.productBrand.Replace("'", String.Empty)}', '{product.productCategory.Replace("'", String.Empty)}', '{product.barcode}'), ";
            cmdString += values;
        }
        cmdString = cmdString.Remove(cmdString.Length-2); cmdString += ";";
        //Console.WriteLine(cmdString + "\n\n");
        NpgsqlCommand cmd = new NpgsqlCommand(cmdString, client);
        cmd.ExecuteNonQuery();
    }

    public void addElements(string collection, List<models.productModel> products){
        PostgreSQLCopyHelper<models.productModel> copyHelper = new PostgreSQLCopyHelper<models.productModel>("public", collection)
            .MapVarchar("sku", x => x.sku)
            .MapVarchar("name", x => x.name)
            .MapVarchar("url", x => x.url)
            .MapVarchar("imgurl", x => x.imgUrl)
            .MapVarchar("description", x => x.description)
            .MapBoolean("distributor", x => x.distributor)
            .MapDouble("price", x => x.price)
            .MapDouble("shipprice", x => x.shipPrice)
            .MapInteger("quantity", x => x.quantity)
            .MapVarchar("productbrand", x => x.productBrand)
            .MapVarchar("productcategory", x => x.productCategory)
            .MapVarchar("barcode", x => x.barcode);

        copyHelper.SaveAll(client, products);
    }

    public void deleteElement(string collection, string condition){
        NpgsqlCommand cmd = new NpgsqlCommand($"DELETE FROM {collection} WHERE ctid IN (SELECT ctid FROM {collection} WHERE {condition} LIMIT 1)", client);
        cmd.ExecuteNonQuery();
    }

    public void deleteElements(string collection, string condition){
        NpgsqlCommand cmd = new NpgsqlCommand($"DELETE FROM {collection} WHERE {condition}", client);
        cmd.ExecuteNonQuery();
    }

    public List<models.productModel> getElements(string collection, string condition){
        NpgsqlCommand cmd = new NpgsqlCommand($"SELECT * FROM {collection} WHERE {condition}", client);
        NpgsqlDataReader dataReader = cmd.ExecuteReader();

        List<models.productModel> elements = new List<models.productModel>();
        while(dataReader.Read()){
            models.productModel tempObject = new models.productModel(
                dataReader[0].ToString()!,
                dataReader[1].ToString()!,
                dataReader[2].ToString()!,
                dataReader[3].ToString()!,
                dataReader[4].ToString()!,
                bool.Parse(dataReader[5].ToString()!),
                float.Parse(dataReader[6].ToString()!, CultureInfo.InvariantCulture),
                float.Parse(dataReader[7].ToString()!, CultureInfo.InvariantCulture),
                int.Parse(dataReader[8].ToString()!, CultureInfo.InvariantCulture),
                dataReader[9].ToString()!,
                dataReader[10].ToString()!,
                dataReader[11].ToString()!
            );

            elements.Add(tempObject);
        }

        return elements;
    }

    public void deleteElement(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        Console.WriteLine("PostgreSQL doesn't support c# expressions. Use SQL string overlaod of this function");
        throw new FormatException();
    }

    public void deleteElements(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        Console.WriteLine("PostgreSQL doesn't support c# expressions. Use SQL string overlaod of this function");
        throw new FormatException();
    }

    public List<models.productModel> getElements(string collection, Expression<Func<models.productModel, bool>> searchFunction){
        Console.WriteLine("PostgreSQL doesn't support c# expressions. Use SQL string overlaod of this function");
        throw new FormatException();
    }

    public long count(string collection){
        NpgsqlCommand cmd = new NpgsqlCommand($"SELECT COUNT(*) FROM {collection}", client);
        NpgsqlDataReader dataReader = cmd.ExecuteReader();
        dataReader.Read();
        return (long) dataReader[0];
    }

    public long count(string collection, string expression){
        NpgsqlCommand cmd = new NpgsqlCommand($"SELECT COUNT(*) FROM {collection} WHERE {expression}", client);
        NpgsqlDataReader dataReader = cmd.ExecuteReader();
        dataReader.Read();
        return (long) dataReader[0];
    }

    public void transferElements(string source, string target){
        NpgsqlCommand cmd = new NpgsqlCommand($"delete from {target} where sku in (select distinct sku from {source})", client);
        cmd.ExecuteNonQuery();

        cmd = new NpgsqlCommand($"insert into {target} select * from {source}", client);
        cmd.ExecuteNonQuery();

        cmd = new NpgsqlCommand($"delete from {source}", client);
        cmd.ExecuteNonQuery();
    }
}