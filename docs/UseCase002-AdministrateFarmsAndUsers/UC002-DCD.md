```mermaid
---
title: "UC002: Domain Class Diagram for Person and Farm Management"
crossreferencer: UC002-DomainModel.mmd, UC002-SSD.mmd
---
classDiagram
    direction TB

    namespace ArlaNatureConnect.Domain.Entities {
        class Person {
            +Guid Id
            +Guid RoleId
            +Guid AddressId
            +string FirstName
            +string LastName
            +string Email
            +bool IsActive
        }

        class Role {
            Guid Id
            +string Name
        }

        class Farm {
            +Guid Id
            +Guid AddressId
            +Guid PersonId
            +string Name
            +string CVR
        }

        class Address {
            +Guid Id
            +string Street
            +string City
            +string PostalCode
            +string Country
        }
    }

    namespace ArlaNatureConnect.Core.Abstract {
        %% Repositories

        class IRepository~TEntity~ {
            <<interface>>
            +GetByIdAsync(Guid id) Task~TEntity~
            +GetAllAsync() Task~List__TEntity~
            +AddAsync(TEntity entity) Task
            +UpdateAsync(TEntity entity) Task
            +DeleteAsync(Guid id) Task
        }

        class IRoleRepository {
            <<interface>>
        }

        class IPersonRepository {
            <<interface>>
        }

        class IAddressRepository {
            <<interface>>
        }

        class IFarmRepository {
            <<interface>>
        }
    }

    namespace ArlaNatureConnect.Infrastructure.Repositories {
        class Repository~TEntity~ {
            #AppDbContect _context
            +Repository() void
            +SaveChangesAsync(AppDbContext context) Task
        }

        class RoleRepository {
            +RoleRepository() void
        }

        class PersonRepository {
            +PersonRepository() void
        }

        class AddressRepository {
            +AddressRepository() void
        }

        class FarmRepository {
            +FarmRepository() void
        }
    }

    %% Associations
    Role --* RoleRepository : manages
    Person --* PersonRepository : manages
    Farm --* FarmRepository : manages
    Address --* AddressRepository : manages
    Role --* IRoleRepository : manages

    %% Composition
    Person --o Role : has
    Person --o Address : has
    Person --o Farm : may have
    Farm --o Address : has

    %% Inheritance and Implementation
    Repository --|> IRepository : implements
    RoleRepository --|> IRoleRepository : implements
    PersonRepository --|> IPersonRepository : implements
    AddressRepository --|> IAddressRepository : implements
    FarmRepository --|> IFarmRepository : implements

    IRoleRepository --|> IRepository : implements
    IPersonRepository --|> IRepository : implements
    IAddressRepository --|> IRepository : implements
    IFarmRepository --|> IRepository : implements

    RoleRepository ..|> Repository : inheritance
    PersonRepository ..|> Repository : inheritance
    FarmRepository ..|> Repository : inheritance
    AddressRepository ..|> Repository  : inheritance

    %% note right of ArlaNatureConnect.Core.IPersonService: Service enforces authorization, validation and mapping
    ```