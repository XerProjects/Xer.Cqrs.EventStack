﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xunit.Abstractions;

namespace Xer.Cqrs.EventStack.Tests.Entities
{
    #region Event Handler

    public class TestEventHandler : IEventAsyncHandler<TestEvent1>,
                                    IEventAsyncHandler<TestEvent2>,
                                    IEventAsyncHandler<TestEvent3>,
                                    IEventAsyncHandler<LongRunningEvent>,
                                    IEventAsyncHandler<ExceptionTriggeringEvent>,
                                    IEventHandler<TestEvent1>,
                                    IEventHandler<TestEvent2>,
                                    IEventHandler<TestEvent3>,
                                    IEventHandler<LongRunningEvent>,
                                    IEventHandler<ExceptionTriggeringEvent>
    {
        private readonly List<object> _handledEvents = new List<object>();
        
        protected ITestOutputHelper OutputHelper { get; }

        public IReadOnlyCollection<object> HandledEvents => _handledEvents.AsReadOnly();

        public TestEventHandler(ITestOutputHelper testOutputHelper)
        {
            OutputHelper = testOutputHelper;
        }

        public bool HasHandledEvent<TEvent>()
        {
            return _handledEvents.Any(e => e is TEvent);
        }

        public IEventAsyncHandler<TEvent> AsEventAsyncHandler<TEvent>() where TEvent : class
        {
            return this as IEventAsyncHandler<TEvent>;
        }

        public IEventHandler<TEvent> AsEventSyncHandler<TEvent>() where TEvent : class
        {
            return this as IEventHandler<TEvent>;
        }

        public virtual Task HandleAsync(TestEvent1 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(@event);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(TestEvent2 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(@event);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(TestEvent3 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(@event);
            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(LongRunningEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(@event);
            return Task.Delay(@event.DurationInMilliseconds, cancellationToken);
        }

        public virtual Task HandleAsync(ExceptionTriggeringEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            BaseHandle(@event);
            return Task.FromException(new TestEventHandlerException($"This is a triggered post-processing exception at { GetType().Name }."));
        }

        public virtual void Handle(TestEvent1 @event)
        {
            BaseHandle(@event);
        }

        public virtual void Handle(TestEvent2 @event)
        {
            BaseHandle(@event);
        }

        public virtual void Handle(TestEvent3 @event)
        {
            BaseHandle(@event);
        }

        public virtual void Handle(LongRunningEvent @event)
        {
            BaseHandle(@event);
            Task.Delay(@event.DurationInMilliseconds).GetAwaiter().GetResult();
        }

        public virtual void Handle(ExceptionTriggeringEvent @event)
        {
            BaseHandle(@event);
        }

        protected void BaseHandle<TEvent>(TEvent @event) where TEvent : class
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            OutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event.");
            _handledEvents.Add(@event);
        }
    }

    #endregion Event Handler

    public class TestEventHandlerException : Exception
    {
        public TestEventHandlerException() { }
        public TestEventHandlerException(string message) : base(message) { }
    }
}
