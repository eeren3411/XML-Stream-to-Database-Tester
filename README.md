# XML-Stream-to-Database-Tester
PostgreSQL and MongoDB performance tests. Testing both PostgreSQL and MongoDB with threads and using PostgreSQL copyhelper. 

# Why?
This was my second internship project. When you're a back-end intern, companies don't really want to share their main database systems with you and you're usually assigned with a proof-of-work project.

It contains all the tests I made so far. From adding one element a time to auto multi-threaded bulk inserting. From a bad wifi to 500 mbit ethernet connection.

# But why in github?
I don't know? There wasn't any c# project in my github so I wanted to add this.

# How does it work?
I extended standart XMLReader to work with StreamReader to read my custom objects. Every "reader.product()" returns a product from xml file.

There are countless amount of functions in the "program.cs" file. One does it with multi-threading, one does it with only bulk insert, one does it even without bulk-insert and so on. The function names usually is enough to read it. But insides can a little bit tricky to understand.