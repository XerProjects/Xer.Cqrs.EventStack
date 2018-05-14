## Build

| Branch | Status |
|--------|--------|
| Master | [![Build status](https://ci.appveyor.com/api/projects/status/sq6a7sdi7rnjpnl7/branch/master?svg=true)](https://ci.appveyor.com/project/XerProjects25246/xer-cqrs-eventstack/branch/master) |
| Dev | [![Build status](https://ci.appveyor.com/api/projects/status/sq6a7sdi7rnjpnl7/branch/dev?svg=true)](https://ci.appveyor.com/project/XerProjects25246/xer-cqrs-eventstack/branch/dev) |

# Table of contents
* [Overview](#overview)
* [Features](#features)
* [Installation](#installation)
* [Getting Started](#getting-started)
   * [Event Handling](#event-handling)
      * [Event Handler Registration](#event-handler-registration)
      * [Delegating Events to Event Handlers](#delegating-events-to-event-handlers)

# Overview
Simple CQRS library

This project composes of components for implementing the CQRS pattern (Event Handling). This library was built with simplicity, modularity and pluggability in mind.

## Features
* Send event to registered event handlers.
* Provides simple abstraction for hosted event handlers which can be registered just like an regular event handler.
* Multiple ways of registering event handlers:
    * Simple handler registration (no IoC container).
    * IoC container registration 
      * achieved by creating implementations of IContainerAdapter or using pre-made extensions packages for supported containers:
        * Microsoft.DependencyInjection
        
          [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.Extensions.Microsoft.DependencyInjection.svg)](https://www.nuget.org/packages/Xer.Cqrs.Extensions.Microsoft.DependencyInjection/)
        
        * SimpleInjector
        
          [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.Extensions.SimpleInjector.svg)](https://www.nuget.org/packages/Xer.Cqrs.Extensions.SimpleInjector/)
        
        * Autofac
        
          [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.Extensions.Autofac.svg)](https://www.nuget.org/packages/Xer.Cqrs.Extensions.Autofac/)
        
    * Attribute registration 
      * achieved by marking methods with [EventHandler] attributes from the Xer.Cqrs.EventStack.Extensions.Attributes package.
      
        [![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.EventStack.Extensions.Attributes.svg)](https://www.nuget.org/packages/Xer.Cqrs.EventStack.Extensions.Attributes/)
        
      * See https://github.com/XerProjects/Xer.Cqrs.EventStack.Extensions.Attributes for documentation.

## Installation
You can simply clone this repository, build the source, reference the dll from the project, and code away!

Xer.Cqrs.EventStack library is available as a Nuget package: 

[![NuGet](https://img.shields.io/nuget/v/Xer.Cqrs.EventStack.svg)](https://www.nuget.org/packages/Xer.Cqrs.EventStack/)

To install Nuget packages:
1. Open command prompt
2. Go to project directory
3. Add the packages to the project:
    ```csharp
    dotnet add package Xer.Cqrs.EventStack
    ```
4. Restore the packages:
    ```csharp
    dotnet restore
    ```

## Getting Started
(Samples are in ASP.NET Core)

### Event Handling

```csharp
public class ProductRegisteredEvent
{
    public int ProductId { get; }
    public string ProductName { get; }

    public ProductRegisteredEvent(int productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }
}
```
#### Event Handler Registration

Before we can delegate any events, first, we need to register our event handlers. There are several ways to do this:

##### 1. Simple Registration (No IoC container)
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();

    // Register event delegator.
    services.AddSingleton<EventDelegator>((serviceProvider) =>
    {
        // Allows registration of a multiple message handlers per message type.
        var registration = new MultiMessageHandlerRegistration();
        registration.RegisterEventHandler<ProductRegisteredEvent>(() => new ProductRegisteredEventHandler());
        registration.RegisterEventHandler<ProductRegisteredEvent>(() => new ProductRegisteredEmailNotifier());
        
        return new EventDelegator(registration.BuildMessageHandlerResolver());
    });
    ...
}

// Sync event handler
public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

// Async event handler
public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
        return Task.CompletedTask;
    }
}
```

##### 2. Container Registration
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{            
    ...
    // Repository.
    services.AddSingleton<IProductRepository, InMemoryProductRepository>();
    
    // Register event handlers to the container. 
    // The AddCqrs extension method is in Xer.Cqrs.Extensions.Microsoft.DependencyInjection package.
    services.AddCqrs(typeof(ProductRegisteredEventHandler).Assembly);
    ...
}

// Sync event handler 1.
public class ProductRegisteredEventHandler : IEventHandler<ProductRegisteredEvent>
{
    public void Handle(ProductRegisteredEvent @event)
    {
        System.Console.WriteLine($"ProductRegisteredEventHandler handled {@event.GetType()}.");
    }
}

// Async event handler 2.
public class ProductRegisteredEmailNotifier : IEventAsyncHandler<ProductRegisteredEvent>
{
    public Task HandleAsync(ProductRegisteredEvent @event, CancellationToken ct = default(CancellationToken))
    {
        System.Console.WriteLine($"Sending email notification...");
        return Task.CompletedTask;
    }
}
```

#### Delegating Events to Event Handlers
After setting up the event delegator in the Ioc container, events can now be delegated by simply doing:
```csharp
...
private readonly EventDelegator _eventDelegator;

public ProductsController(EventDelegator eventDelegator)
{
    _eventDelegator = eventDelegator;
}

[HttpGet("{productId}")]
public async Task<IActionResult> Notify(ProductRegisteredEventDto model)
{
    await _eventDelegator.SendAsync(new ProductRegisteredEvent(model.ProductId, model.ProductName))
    return Accepted();
}
...
```
