﻿using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace P3D.Legacy.Server.Application.Utils
{
    public class NotificationRegistrar : IEnumerable
    {
        private class NotificationServiceFactory<TNotification> : BaseServiceFactory where TNotification : INotification
        {
            private readonly List<Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>>> _notifications = new();

            public void Register(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> func) => _notifications.Add(func);

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
            private void RegisterInternal(Func<IServiceProvider, IEnumerable> func)
            {
                IEnumerable<INotificationHandler<TNotification>> Convert(IServiceProvider sp, Func<IServiceProvider, IEnumerable> func_)
                {
                    foreach (var obj in func(sp))
                    {
                        yield return (obj as INotificationHandler<TNotification>)!;
                    }
                }

                _notifications.Add(sp => Convert(sp, func));
            }

            public override IEnumerable<INotificationHandler<TNotification>> ServiceFactory(IServiceProvider sp) => _notifications.SelectMany(func => func(sp));
        }

        private static readonly MethodInfo GenericAddMethod = typeof(NotificationRegistrar).GetMethod("Add", new Type[] { typeof(ServiceLifetime) })!;


        private readonly Dictionary<Type, BaseServiceFactory> _containers = new();
        private readonly Dictionary<Type, (Type, ServiceLifetime)> _direct = new();

        public void Add(Type @base, Type impl, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (!@base.IsInterface || @base.GetGenericTypeDefinition() != typeof(INotificationHandler<>))
                throw new Exception();

            if (!@base.IsAssignableFrom(impl))
                throw new Exception();

            var notificationType = @base.GenericTypeArguments[0];
            if (notificationType is null)
                throw new Exception();

            var method = GenericAddMethod.MakeGenericMethod(impl, notificationType);
            method.Invoke(this, new object?[] { lifetime });
        }

        public void Add(IEnumerable<(Type, (Type, ServiceLifetime))> tuples)
        {
            foreach (var (@base, (impl, lifetime)) in tuples)
            {
                Add(@base, impl, lifetime);
            }
        }

        public void Add<TImpl, TNotification>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TNotification : INotification where TImpl : INotificationHandler<TNotification>
        {
            var key = typeof(IEnumerable<INotificationHandler<TNotification>>);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new NotificationServiceFactory<TNotification>();
                _containers.Add(key, cont);
            }

            if (cont is NotificationServiceFactory<TNotification> container)
            {
                container.Register(sp => new[] { sp.GetRequiredService<TImpl>() as INotificationHandler<TNotification> });
            }

            _direct[typeof(TImpl)] = (typeof(TImpl), lifetime);
        }

        public void Add<TNotification>(Func<IServiceProvider, INotificationHandler<TNotification>> factory) where TNotification : INotification
        {
            var key = typeof(IEnumerable<INotificationHandler<TNotification>>);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new NotificationServiceFactory<TNotification>();
                _containers.Add(key, cont);
            }

            if (cont is NotificationServiceFactory<TNotification> container)
            {
                container.Register(sp => new[] { factory(sp) });
            }
        }

        public void Add<TNotification>(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> factory) where TNotification : INotification
        {
            var key = typeof(IEnumerable<INotificationHandler<TNotification>>);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new NotificationServiceFactory<TNotification>();
                _containers.Add(key, cont);
            }

            if (cont is NotificationServiceFactory<TNotification> container)
            {
                container.Register(factory);
            }
        }

        public IServiceCollection Register(IServiceCollection services)
        {
            foreach (var (type, container) in _containers)
                services.Add(ServiceDescriptor.Describe(type, sp => container.ServiceFactory(sp), ServiceLifetime.Transient));

            foreach (var (@base, (impl, lifetime)) in _direct)
                services.Add(ServiceDescriptor.Describe(@base, impl, lifetime));

            return services;
        }

        public IEnumerator GetEnumerator() => _containers.GetEnumerator();
    }
}