# Introduction
This repository contains a solution for a mail migrator.

The solution is not intended to be a real-life product. It does, however, show the idea of how a mail migrator service could be built.

# Architecture
The solution assumes an api that handles user calls and a background service that processes the migration.

The services are loosely coupled by the message queue service provided by RabbitMQ.

```
  * BeginMigration endpoint
  |
  |  * GetStatus endpoint
  |  |
  |  |           +-------------------------------------+
  |  |           |                                     |
+============+   |    RabbitMQ        +============+   |   +-----------+
|            | <-+   +--------+       | Migration  |   +-- | Mail      |
|  REST API  | ====> |  |  |  | ====> | Background | ====> | Providers |
|            |       +--------+       | Service    |       +-----------+
+------------+        |               +============+
| migration  | <------+
| state      | feedback
+============+


Key to symbols:
-* represents an endpoint
-> represents a lightweight information about a mail flow
=> represents a full mail flow
```
## Key features
- The architecture uses loose coupling between services provided by a third party AMQP RabbitMQ, allowing them to run independently.
- The mails are being scheduled for migration asynchronously in the background. This gives the following advantages:
  - An Api response about started migration is provided immediately.
  - Mails are scheduled to be enqueued one by one, which means that if two or more requests are made at almost the same time they will likely finish also more or less at the same time (provided they have similar mail count),
    without favoring the one that was scheduled like 1ms before the other.
# Contents
- **MigratorApi** - Contains the REST API
  - BeginMigrationController - serves as an endpoint for scheduling a migration - validates the user input, in the background acquires mails from the source provider and enqueues them to be processed (migrated).
  - GetStatusController - serves as an endpoint for getting status of a specified migration - gets the status of the migration of the specified mailbox.
- **Migrator.Receiver** - The background service whose sole purpose is to dequeue mails from the queue and migrate them from the source provider to the destination provider.
- **Migrator.Common** - A common library containing definition of mail provider interfaces and common code for the migration.
- **AlmostMailProvider**, **MerelyMailProvider** - mail providers.
- *OmmMigration* - Early scrapped idea of an Azure Function as the REST API (to be removed).
- *MigratorWorker* - Scrapped attempt for a custom implementation of a message queue worker; replaced by the RabbitMQ (to be removed).

# Getting started
## Prerequisites
- .NET 8.0 Runtime https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Visual Studio 2022 Community https://visualstudio.microsoft.com/free-developer-offers/
  - direct download link https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&passive=false&cid=2030
- RabbitMQ https://www.rabbitmq.com/download.html
- Erlang (required by RabbitMQ) https://www.erlang.org/downloads
## Step-by-step
- Download/Clone and build the source (if you are using the latest version of Microsoft Visual Studio, the .NET 8.0 Runtime should already be available on your system at this point).
- Download and install RabbitMQ (the Erlang would probably be downloaded and installed along with the RabbitMQl if not, please download and install it manually).
- Run the RabbitMQ Service from Start Menu.
- Once the source is built, the executable of the Migration Background Service can be run immediately or using the Windows Task Scheduler.
- Once the source is built, the Rest Api can be published on local IIS server. If possible, it is recommended to run in on the IIS Express using Visual Studio Debugger.
## Remarks
- Please note that the RabbitMQ service must be running in order for this solution to work properly.
  There's no protection or validation for this requirements and any attempt at calling the BeginMigration Api will result an application crash leading to an invalid state where the migration is started but there's no queue that can handle mails.
  The Background service will crash instantly if run without RabbitMQ Service available.
- A convenient way of running the solution is to start debugging from Visual Studio. The solution is set to start both MigratorApi and Migrator.Receiver applications.

# Usage
The MigratorApi is equipped with Swagger which drastically simplifies testing the Api process and also serves as a source of documentation.

Default host for the api when run on IIS Express is `https://localhost:7035`

Swagger page is available at `/swagger`

There are two endpoints that control the migration.
Both endpoints accept POST method only.
- **BeginMigration** - `/BeginMigration` - call body examples:
  ```JSON
  {
    "sourceMailProvider": "MerelyMail",
    "destinationMailProvider": "AlmostMail",
    "mailbox": {
      "name": "customer1@merely.mail",
      "password": "procaciously1",
      "quota": 0
    }
  }
  ```
  ```JSON
  {
    "sourceMailProvider": "AlmostMail",
    "destinationMailProvider": "MerelyMail",
    "mailbox": {
      "name": "gYJAzV.eRgHgPZOQlmeE@BAYvfk.pl",
      "password": "TUmuM",
      "quota": 0
    }
  }
  ```
- GetStatus - `/GetStatus` - call body examples:
   ```JSON
  {
    "sourceMailProvider": "MerelyMail",
    "destinationMailProvider": "",
    "mailbox": {
      "name": "customer1@merely.mail",
      "password": "procaciously1",
      "quota": -1
    }
  }
  ```
  ```JSON
  {
    "sourceMailProvider": "AlmostMail",
    "destinationMailProvider": "",
    "mailbox": {
      "name": "gYJAzV.eRgHgPZOQlmeE@BAYvfk.pl",
      "password": "TUmuM",
      "quota": -1
    }
  }
  ```
  Note that `destinationMailProvider` and `mailbox.quota` parameters are not used in this api but due to a bug in the data model they are still required, so please use some dummy values for them, like an empty string and `-1` respectively.
## Remarks
- Attempting at running a migration for a mailbox that already has been scheduled for migration or has already been migrated will result in an error. This is by design.
- Attempting at getting status for a mailbox that is not being migrated at the moment will result in an error (404 most likely).
- Migrating a mailbox from the same source and destination provider is not allowed.
- Migrating the same mailbox more than once is not allowed.
- Working directory has been hardcoded to `D:\omm`. Should this path be changed, please edit the `Migrator.Common.Migration.PathBase` constant field value in the `Migrator.Common\Migration.cs` file.
