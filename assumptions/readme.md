# Writing OMM
## Introduction
OMM (OVH Mail Migrator https://omm.ovh.net/) is a tool developped internally at OVH for our customers. This tools allows to migrate mails, calendars, contacts and so on from and to a variety of providers (Exchange, plain IMAP, gmail, Zimbra)

There are a lot of possible ways to develop such a tool, the objective of the exercise is to understand how you would imagine it. Of course, the scope here is greatly reduced. Here we will replace mail protocols by files and a sqlite database. Throughout this document, they will be referred to as mail providers

## Information
* There will be two providers (json files, single sqlite database)
* Mails may or must have a subject, a body, a sender and a receiver. The specifics depend on the provider.
* The format of the json file is defined (see below)
* The schema of the sqlite database is defined (see below)
* In this exercise, we assume that only one level of hierarchy is possible in mailbox folders
* Both providers are international companies with clients in Europe, America and Asia
* A dataset for both providers is included
* The customer has a mailbox only on the source provider when starting the migration
* AlmostMail has mailboxes with a 50G quota
* MerelyMail has mailboxes with either 20G or 100G quota
* You should trust the email size property, not compute it yourself

## Goals
* Create a RESTful API using C# that allows a customer to :
    * create a migration for his mailbox from a provider to an other using his credentials. The resulting mailbox password is determined by the migration payload.
    * get the current status of a migration
* Write the code that will move mails from provider A to provider B and B to A
* The project _architecture_ should account for the fact that possibly hundreds of migrations will run side by side.
* The project should be in a working state (deployable/runnable) with instructions on how to compile/run/use
* Entire project should be stored on a git server (GitHub, Gitlab, ...)

## Non goals
* If your solution requires to store data, it is perfectly fine to use something simple like sqlite. We can discuss during the interview of what you would really use for a production grade project
* Any advanced mechanisms you might think of are very welcome in the discussion and the README, but it's not necessary to implement everything
* Bulk migration of many mailboxes should not done (although ideas for how to manage it can be discussed)

## Providers
### First provider: AlmostMail - uses json files

AlmostMail works with json files in a directory. Each mailbox is represented by a file, named `<email_address>.json`. That file contain all its emails. The format is as follow:
```json
{
    "mailbox_quota": 50, // in GB
    "mailbox_size": 12, // in GB
    "password": 123secure,
    "mails": [
        {
            "subject": "A subject", // NOT MANDATORY, CAN BE NULL
            "body": "A body",
            "from": "sender@ovhcloud.com",
            "to": "receiver@ovhcloud.com",
            "filedInto": "INBOX",
            "size" : "Size of the email" // in B
        },
    ]
}
```

### Second provider: MerelyMail - uses one sqlite database
Merelymail works with a sqlite database. The SQL file to recreate their schema is as follows:
```sql
CREATE TABLE "folders" (
    "Id"    INTEGER,
    "mailboxId"    INTEGER NOT NULL,
    "name"    TEXT NOT NULL,
    PRIMARY KEY("Id" AUTOINCREMENT)
);
CREATE TABLE "mailbox" (
    "id"    INTEGER,
    "email"    TEXT NOT NULL UNIQUE,
    "password"    TEXT NOT NULL,
    "quota"    INTEGER NOT NULL, --in MB
    PRIMARY KEY("id")
);
CREATE TABLE "mails" (
    "id"    INTEGER,
    "mailboxId"    INTEGER NOT NULL,
    "folderId"    INTEGER NOT NULL,
    "subject"    TEXT NOT NULL,
    "body"    TEXT,
    "from"    TEXT NOT NULL,
    "to"    TEXT NOT NULL,
    "size"    INTEGER NOT NULL, --Fake size of the mail in B
    PRIMARY KEY("id" AUTOINCREMENT)
);
```