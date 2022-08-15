namespace CustomThreads;

public class LoadThread{
    public Thread thread;
    private int dbType;
    private ManualResetEvent readHandler;
    public List<models.productModel> ?batch;
    public AutoResetEvent waitHandler;
    public bool isWorking;
    public bool finished;
    
    public LoadThread(int dbType, ManualResetEvent readHandler){
        this.dbType = dbType;
        this.waitHandler = new AutoResetEvent(false);
        this.readHandler = readHandler;
        this.isWorking = false;
        this.finished = false;

        thread = new Thread(new ThreadStart(threadFunction));
        thread.Start();
    }

    private Tester.IDatabaseTester getDatabase(){
        if(dbType == 1){
            return new Tester.MongoTester("Host", "Port", "userID", "Password", "FeedDataAF");
        }else{
            return new Tester.PostgreTester("Host", "Port", "userID", "Password", "feedDataAF");
        }
    }
    private void closeDatabase(Tester.IDatabaseTester dbTester){
        if(dbType == 1){return;}
        ((Tester.PostgreTester)dbTester).Close();
    }

    private void threadFunction(){
        for(;;){
            Tester.IDatabaseTester dbTester = getDatabase();

            waitHandler.WaitOne();
            if(finished){closeDatabase(dbTester); return;}

            isWorking = true;
            dbTester.addElements("productfeed", batch!);

            isWorking = false;
            batch!.Clear();
            readHandler.Set();
            closeDatabase(dbTester);
        }
    }
}