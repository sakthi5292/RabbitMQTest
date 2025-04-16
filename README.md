# Message Consumer Microservice (.NET + RabbitMQ + PostgreSQL)

This microservice reads messages from RabbitMQ, buffers them, and writes to PostgreSQL in batches of 30 messages per second using Entity Framework Core.

---

## Features

-  Consumes messages from RabbitMQ queue
-  Stores messages into PostgreSQL
-  Batches inserts to 30 records/second for performance
-  Built with .NET Worker Service

---

## Tech Stack

- .NET 8
- RabbitMQ (AMQP)
- PostgreSQL
- EF Core (with Npgsql)

---

Note: 

Read or Publish Microservices uses FileWatcherPublisher.cs
Consumer Microservices uses RabbitConsumerService.cs


1. Setup Docker for Windows & Run Docker with RabbitMQ

   -> https://docs.docker.com/desktop/setup/install/windows-install/

   run following command to pull RabbitMQ image

   -> docker pull rabbitmq:4.0.9-management
   -> docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4.0.9-management

2. Setup PostgreSQL
   -> https://www.postgresql.org/download/
   

3. Update appsettings.json

{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=RabbitMQDemo;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "demo-queue"
  },
}

4. Add Required NuGet Packages

Install-Package Microsoft.EntityFrameworkCore
Install-Package Microsoft.EntityFrameworkCore.Design
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL
Install-Package RabbitMQ.Client

5. Create Initial Migration & Update DB

dotnet tool install --global dotnet-ef
dotnet ef migrations add InitDataRecord
dotnet ef database update

6. Run the Service using Multiple Startup Projects

7. using Project RabbitMQSubscriber swagger Test pagination api /WeatherForecast/search