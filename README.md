We created an app, based on client - server architecture, that can send/recive text and image through the network. 

We began with two protocols: text and files, these were sent with a native synchronous socket method. Then, we handled the paralelism with threads.

After that we migrated all the proyect.
-> threads to tasks
-> sockets to tcpClient, tcpListener, network stream
-> synchronous to asynchronous methods.

Finally, we implemented message queues with RabbitMq.
-> Publish/Suscribe
-> RPC
-> Routing
-> Queues
-> Topics
