using System.Xml;
using System.Linq.Expressions;
using Npgsql;

namespace Tester;
class MainTester{
    static void Main(string[] args){
        //AppDomain.CurrentDomain.ProcessExit += new EventHandler (OnProcessExit!); 
        //Console.WriteLine("Hello world!");

        //autoLoader();
        //getTester();
        //increasedLoader();
        autoThreadLoader();
        //tests();

        /*string tableSettings = "sku VARCHAR(10), name VARCHAR(500), url VARCHAR(500), imgUrl VARCHAR(500), description VARCHAR(5000), distributor BOOL, price FLOAT8, shipPrice FLOAT8, quantity INT, productBrand VARCHAR(200), productCategory VARCHAR(1000), barcode VARCHAR(20)";
        postgreTester.createTable("products", tableSettings);*/
        //postgreTester.dropTable("products");
        
        //List<models.productModel> founds = postgreTester.getElements("products", "true");
        //List<models.productModel> founds = mongoTester.getElements("products", _ => true);
        
        //Console.WriteLine(founds.Count);
        /*
        foreach(models.productModel found in founds){
            Console.WriteLine(found);
        }*/
    }

    static void tests(){
        IDatabaseTester dbTester = new MongoTester("Host", "Port", "userID", "Password", "DatabaseName");
        System.Console.WriteLine(dbTester.count("productfeed"));
        dbTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
        System.Console.WriteLine(dbTester.count("productfeed"));
    }

    static void autoThreadLoader(){/*
        System.Console.WriteLine("Clearing databases...");
        long mongo0 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        MongoTester mongoTester = new MongoTester("Host", "Port", "userID", "Password", "DatabaseName");
        long mongo1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        mongoTester.deleteElements("productfeed", _ => true);
        long mongo2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        System.Console.WriteLine($"MongoDB connection time -> {mongo1-mongo0} ms\nMongoDB delete time -> {mongo2-mongo1} ms");
        long postgre0 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        PostgreTester postgreTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
        long postgre1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        postgreTester.deleteElements("productfeed", "true");
        long postgre2 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        System.Console.WriteLine($"PostgreSQL connection time -> {postgre1-postgre0} ms\nPostgreSQL delete time -> {postgre2-postgre1} ms");*/
        System.Console.WriteLine("\nStarting tests...");

        IDatabaseTester dbTester = new MongoTester("Host", "Port", "userID", "Password", "DatabaseName");
        System.Console.WriteLine("\nDatabase = MongoDB");
        long databasePopulation = dbTester.count("productfeed");
        for(int i=0; i<1; i++){
            System.Console.WriteLine("Database population -> {0}", databasePopulation);
            Dictionary<string, long> results = multiThreadedLoader(1, 100000);
            System.Console.WriteLine("Read time -> {0} ms\nLoad time -> {1} ms\nTotal sent -> {2}", results["readTime"], results["loadTime"], results["totalSent"]);
            databasePopulation += results["totalSent"];
        }
        long transferStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        dbTester.transferElements("productfeed", "products");
        long transferEnd = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        System.Console.WriteLine("Transfer time -> {0}\n", transferEnd-transferStart);

        dbTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
        System.Console.WriteLine("\nDatabase = PostgreSQL");
        databasePopulation = dbTester.count("productfeed");
        ((PostgreTester)dbTester).Close();
        for(int i=0; i<1; i++){
            System.Console.WriteLine("Database population -> {0}", databasePopulation);
            Dictionary<string, long> results = multiThreadedLoader(2, 100000);
            System.Console.WriteLine("Read time -> {0} ms\nLoad time -> {1} ms\nTotal sent -> {2}", results["readTime"], results["loadTime"], results["totalSent"]);
            databasePopulation += results["totalSent"];
        }
        dbTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
        transferStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        dbTester.transferElements("productfeed", "products");
        transferEnd = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        System.Console.WriteLine("Transfer time -> {0}\n", transferEnd-transferStart);
    }

    static Dictionary<string, long> multiThreadedLoader(int dbType, int batchSize = 100000, int threadCount = 3){ //dpType 1 for mongoDB, 2 for postgreSQL
        Dictionary<string, long> returnData = new Dictionary<string, long>();
        List<long> times = new List<long>();
        long firstThreadStart = 0;
        ManualResetEvent readHandler = new ManualResetEvent(true);
        
        
        List<CustomThreads.LoadThread> threads = new List<CustomThreads.LoadThread>();
        for(int i=0; i<threadCount; i++){
            threads.Add(new CustomThreads.LoadThread(dbType, readHandler));
        }

        CustomThreads.LoadThread getThread(){
            foreach(CustomThreads.LoadThread thread in threads){
                if(!thread.isWorking){return thread;}
            }
            readHandler.Reset();
            readHandler.WaitOne();
            return getThread();
        }

        
        List<models.productModel> productBatch = new List<models.productModel>();
        int totalSent = 0;
        long readStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        XmlReader reader = XmlReader.Create(new StreamReader("./files/test2.xml"));
        foreach(models.productModel product in reader.products()){
            productBatch.Add(product);
            totalSent += 1;
            
            if(batchSize != 0 && productBatch.Count()>batchSize){
                CustomThreads.LoadThread activeThread = getThread();
                activeThread.batch = productBatch;
                if(firstThreadStart == 0){firstThreadStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;}
                activeThread.waitHandler.Set();
                productBatch = new List<models.productModel>();
            }
        }
        if(productBatch.Count != 0){
            CustomThreads.LoadThread activeThread = getThread();
            activeThread.batch = productBatch;
            activeThread.waitHandler.Set();
        }


        long readEnd = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        foreach(CustomThreads.LoadThread customT in threads){
            customT.finished = true;
            customT.waitHandler.Set();
            customT.thread.Join();
        }
        long lastThreadEnd = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        returnData["readTime"] = readEnd-readStart;
        returnData["loadTime"] = lastThreadEnd-firstThreadStart;
        returnData["totalSent"] = totalSent;
        return returnData;
    }

    static void getTester(){
        MongoTester mongoTester = new MongoTester("Host", "Port", "userID", "Password", "DatabaseName");
        

        Expression<Func<models.productModel, bool>>[] mongoExpressions = {
            _ => true,
            _ => _.price>5000,
            _ => _.quantity>10,
            _ => _.productBrand=="Magic Form"
        };

        string[] postgreExpressions = {
            "true",
            "price>5000",
            "quantity>10",
            "productBrand = 'Magic Form'"
        };

        for(int i=0; i<mongoExpressions.Length; i++){
            Console.WriteLine($"database: MongoDB\nExpression: {i}");
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            List<models.productModel> founds = mongoTester.getElements("products", mongoExpressions[i]);
            long finishTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"Found items: {founds.Count}\nTime consumed: {finishTime-startTime}");
            Console.WriteLine();
        }

        for(int i=0; i<postgreExpressions.Length; i++){
            PostgreTester postgreTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
            Console.WriteLine($"database: PostgreSQL\nExpression: {i}");
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            List<models.productModel> founds = postgreTester.getElements("products", postgreExpressions[i]);
            long finishTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"Found items: {founds.Count}\nTime consumed: {finishTime-startTime}");
            Console.WriteLine();
            postgreTester.Close();
        }

        
    }

    static void autoLoader(){
        int[] batchSizes = {50, 200, 1000, 2000, 5000, 10000};
        
        foreach(int batchSize in batchSizes){
            Console.WriteLine($"database = MongoDB\nbatchSize = {batchSize}");
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            MongoTester mongoTester = new MongoTester("Host", "Port", "userID", "Password", "DatabaseName");
            long connectionTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"start -> connection = {connectionTime-startTime} ms");
            mongoTester.deleteElements("products", _ => true);
            long deleteElementsTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"connection -> delete elements = {deleteElementsTime-connectionTime} ms");
            int totalSent = loadTests(mongoTester, batchSize);
            long loadElementsTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"delete elements -> load tests = {loadElementsTime-deleteElementsTime} ms");
            Console.WriteLine($"total sent = {totalSent}");
            Console.WriteLine();
        }

        foreach(int batchSize in batchSizes){
            Console.WriteLine($"database = PostgreSQL\nbatchSize = {batchSize}");
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            PostgreTester postgreTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
            long connectionTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"start -> connection = {connectionTime-startTime} ms");
            postgreTester.deleteElements("products", "true");
            long deleteElementsTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"connection -> delete elements = {deleteElementsTime-connectionTime} ms");
            int totalSent = loadTests(postgreTester, batchSize);
            long loadElementsTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"delete elements -> load tests = {loadElementsTime-deleteElementsTime} ms");
            Console.WriteLine($"total sent = {totalSent}");
            postgreTester.Close();
            Console.WriteLine();
        }
    }

    static void increasedLoader(){
        int batchSize = 10000;
        int totalLoaded = 0;
        Console.WriteLine($"database = MongoDB\nbatchSize = {batchSize}");
        long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        MongoTester mongoTester = new MongoTester("Host", "Port", "userID", "Password", "DatabaseName");
        long connectionTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        Console.WriteLine($"start -> connection = {connectionTime-startTime} ms");
        mongoTester.deleteElements("products", _ => true);
        long deleteElementsTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        Console.WriteLine($"connection -> delete elements = {deleteElementsTime-connectionTime} ms\n");

        for(int i=0; i<10; i++){
            Console.WriteLine($"total elements in database: {totalLoaded}");
            long beforeSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int totalSent = loadTests(mongoTester, batchSize);
            long afterSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"total sent: {totalSent}\ntime consumed: {afterSend-beforeSend}\n");
            totalLoaded += totalSent;
        }

        totalLoaded = 0;
        Console.WriteLine($"database = PostgreSQL\nbatchSize = {batchSize}");
        startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        PostgreTester postgreTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
        connectionTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        Console.WriteLine($"start -> connection = {connectionTime-startTime} ms");
        postgreTester.deleteElements("products", "true");
        deleteElementsTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        Console.WriteLine($"connection -> delete elements = {deleteElementsTime-connectionTime} ms\n");
        postgreTester.Close();

        for(int i=0; i<10; i++){
            postgreTester = new PostgreTester("Host", "Port", "userID", "Password", "DatabaseName");
            Console.WriteLine($"total elements in database: {totalLoaded}");
            long beforeSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int totalSent = loadTests(postgreTester, batchSize);
            long afterSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine($"total sent: {totalSent}\ntime consumed: {afterSend-beforeSend}\n");
            totalLoaded += totalSent;
            postgreTester.Close();
        }
    }
    
    static int loadTests(IDatabaseTester dbTester, int batchSize = 0){
        string xmlUrl = "./files/test2.xml";
        StreamReader streamReader = new StreamReader(xmlUrl);
        XmlReader reader = XmlReader.Create(streamReader);
        var products = from u in reader.products() select u;
        
        int totalSent = 0;
        int counter = 1;
        List<models.productModel> productBatch = new List<models.productModel>();
        foreach(var product in products){
            productBatch.Add(product);
            counter++;
            totalSent++;
            if(batchSize != 0 && counter > batchSize){
                counter = 1;
                dbTester.addElements("products", productBatch);
                productBatch.Clear();
            }
        }
        if(productBatch.Count != 0){dbTester.addElements("products", productBatch);} //send last ones if there are any last ones
        
        return totalSent;
    }
}