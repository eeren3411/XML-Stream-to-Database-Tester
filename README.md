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

#Results? I guess.

## Note
The different tests made in different days so the results may affected by connection and my computers speed (Which is bad, very bad). So I suggest you that only look for differences between databases. 
## Note2
Same tests with different databases made back-to-back.


## Simple insert operation
![speedtests](https://user-images.githubusercontent.com/77689346/184593180-2bad980c-7e8c-4dfa-aee1-3f2a9cbcee1b.png)

![WriteTests](https://user-images.githubusercontent.com/77689346/184593220-a2b49b1e-8a31-433d-bd83-1c0e51ff9729.png)

## Get tests
![getTests](https://user-images.githubusercontent.com/77689346/184593248-7b6b38a1-97d4-45eb-9da8-1c28781c20e7.png)

 ## Bulk insert
![WriteTestsBulk](https://user-images.githubusercontent.com/77689346/184593335-a2292f29-b7ee-474f-94e3-a3a49a54d375.png)

## Increased bulk tests (Fills database continuously)
![increasedLoader2](https://user-images.githubusercontent.com/77689346/184593454-47f04357-b692-4a87-8698-832da556bd1d.png)

## Multi-threaded write
![multithreadedWrite](https://user-images.githubusercontent.com/77689346/184593520-bb65f53b-ce11-4619-a3eb-abc2ca996678.png)

## Multi-threaded Write with signals
![signal-multithreadedWrite](https://user-images.githubusercontent.com/77689346/184593573-c34ba942-2e3d-4d7f-8d80-1d7f879de01b.png)

## Multi-threaded write with signals with ethernet connection
![Ethernet-Signal-MultithreadedWrite](https://user-images.githubusercontent.com/77689346/184593621-68bd2ccc-6787-493e-a1df-af8ab1639303.png)
