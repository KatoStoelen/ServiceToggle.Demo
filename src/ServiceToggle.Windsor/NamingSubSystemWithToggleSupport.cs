using System;
using Castle.MicroKernel;
using Castle.MicroKernel.SubSystems.Naming;
using ServiceToggle.Windsor.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceToggle.Windsor
{
    internal class NamingSubSystemWithToggleSupport : AbstractSubSystem, INamingSubSystem
    {
        private readonly Dictionary<string, TogglableHandler> _handlersByName = new Dictionary<string, TogglableHandler>();
        private readonly Dictionary<Type, TogglableHandler> _handlersByService = new Dictionary<Type, TogglableHandler>();
        private readonly Dictionary<ReplacementKey, TogglableHandler> _replacementsByService = new Dictionary<ReplacementKey, TogglableHandler>();
        private readonly Dictionary<string, TogglableHandler> _replacementsByName = new Dictionary<string, TogglableHandler>();
        private readonly Dictionary<Type, List<TogglableHandler>> _togglableHandlersByService = new Dictionary<Type, List<TogglableHandler>>();

        private readonly List<IHandlerSelector> _handlerSelectors = new List<IHandlerSelector>();
        private readonly List<IHandlersFilter> _handlerFilters = new List<IHandlersFilter>();

        private readonly object _registerLock = new object();
        private readonly object _selectorLock = new object();
        private readonly object _filterLock = new object();

        public NamingSubSystemWithToggleSupport()
            : this(null)
        {
        }

        public NamingSubSystemWithToggleSupport(INamingSubSystem previousNamingSubSystem)
        {
            previousNamingSubSystem?.GetAllHandlers().ForEach(Register);
        }

        public int ComponentCount => _handlersByName.Count;

        public void AddHandlerSelector(IHandlerSelector selector)
        {
            lock (_selectorLock)
            {
                _handlerSelectors.Add(selector);
            }
        }

        public void AddHandlersFilter(IHandlersFilter filter)
        {
            lock (_filterLock)
            {
                _handlerFilters.Add(filter);
            }
        }

        public bool Contains(string name)
        {
            return _handlersByName.ContainsKey(name) && _handlersByName[name].IsActive;
        }

        public bool Contains(Type service)
        {
            return
                _handlersByService.ContainsKey(service) ||
                _togglableHandlersByService.ContainsKey(service) &&
                _togglableHandlersByService[service].Any(togglableHandler => togglableHandler.IsActive);
        }

        public IHandler[] GetAllHandlers()
        {
            return _handlersByName.Values
                .Select(togglableHandler => togglableHandler.Handler)
                .ToArray();
        }

        public IHandler[] GetAssignableHandlers(Type service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service == typeof(object))
                return GetAllHandlers();

            // If none of these handlers CanToggle, this array can be cached
            return _handlersByName.Values
                .Where(togglableHandler => togglableHandler.IsActive)
                .Select(togglableHandler => togglableHandler.Handler)
                .Where(handler => handler.SupportsAssignable(service))
                .ToArray();
        }

        public IHandler GetHandler(string name)
        {
            if (!_handlersByName.TryGetValue(name, out var togglableHandler))
                return null;

            _replacementsByName.TryGetValue(name, out var replacement);

            if (replacement?.IsActive ?? false)
            {
                return replacement.Handler;
            }

            // If replacement == null, this handler can be cached

            return togglableHandler.Handler;
        }

        public IHandler GetHandler(Type service)
        {
            _handlersByService.TryGetValue(service, out var serviceHandler);

            if (serviceHandler == null && service.GetTypeInfo().IsGenericType && service.GetTypeInfo().IsGenericTypeDefinition == false)
            {
                var openService = service.GetGenericTypeDefinition();
                var handlerCandidates = GetHandlers(openService);
                foreach (var handlerCandidate in handlerCandidates)
                {
                    if (handlerCandidate.Supports(service))
                    {
                        return handlerCandidate;
                    }
                }
            }

            var key = new ReplacementKey
            {
                Service = service,
                Implementation = serviceHandler?.Handler.ComponentModel.Implementation
            };

            _replacementsByService.TryGetValue(key, out var replacement);

            if (replacement?.IsActive ?? false)
            {
                return replacement.Handler;
            }

            _togglableHandlersByService.TryGetValue(service, out var togglableHandlers);

            // If replacement == null && togglableHandlers == null, this handler can be cached

            return 
                togglableHandlers?
                    .FirstOrDefault(togglableHandler => togglableHandler.IsActive)?.Handler
                ?? serviceHandler?.Handler;
        }

        public IHandler[] GetHandlers(Type service)
        {
            return _handlersByName
                .Where(pair => pair.Value.Handler.Supports(service))
                .Select(pair => pair.Value.Handler)
                .ToArray();
        }

        public void Register(IHandler handler)
        {
            lock (_registerLock)
            {
                if (_handlersByName.ContainsKey(handler.ComponentModel.Name))
                    throw new ArgumentException($"Component with name '{handler.ComponentModel.Name}' already registered");

                var togglableHandler = new TogglableHandler(handler);

                _handlersByName[handler.ComponentModel.Name] = togglableHandler;

                RegisterHandlerOfServices(togglableHandler);
                RegisterReplacements(togglableHandler);
            }
        }

        private void RegisterHandlerOfServices(TogglableHandler togglableHandler)
        {
            if (togglableHandler.Replacement != null)
                return;

            if (togglableHandler.CanToggle)
            {
                RegisterTogglableHandlerOfServices(togglableHandler);
            }
            else
            {
                RegisterNonTogglableHandlerOfServices(togglableHandler);
            }
        }

        private void RegisterTogglableHandlerOfServices(TogglableHandler togglableHandler)
        {
            foreach (var serviceType in togglableHandler.Handler.ComponentModel.Services)
            {
                if (!_togglableHandlersByService.ContainsKey(serviceType))
                {
                    _togglableHandlersByService[serviceType] = new List<TogglableHandler>();
                }

                _togglableHandlersByService[serviceType].Add(togglableHandler);
            }
        }

        private void RegisterNonTogglableHandlerOfServices(TogglableHandler togglableHandler)
        {
            foreach (var serviceType in togglableHandler.Handler.ComponentModel.Services)
            {
                if (_handlersByService.ContainsKey(serviceType))
                {
                    var existingHandler = _handlersByService[serviceType];

                    if (existingHandler.GetPriorityOfService(serviceType) >= togglableHandler.GetPriorityOfService(serviceType))
                        continue;
                }

                _handlersByService[serviceType] = togglableHandler;
            }
        }

        private void RegisterReplacements(TogglableHandler togglableHandler)
        {
            if (togglableHandler.Replacement == null)
                return;

            if (togglableHandler.Replacement.IsByName)
            {
                RegisterReplacementByName(togglableHandler);
            }
            else
            {
                RegisterReplacementByService(togglableHandler);
            }
        }

        private void RegisterReplacementByName(TogglableHandler togglableHandler)
        {
            var componentName = togglableHandler.Replacement.Name;

            if (_replacementsByName.ContainsKey(componentName))
                throw new InvalidOperationException(
                    $"Replacement of component with name '{componentName}' already registered");

            _replacementsByName[componentName] = togglableHandler;
        }

        private void RegisterReplacementByService(TogglableHandler togglableHandler)
        {
            foreach (var implementationType in togglableHandler.Replacement.Implementations)
            {
                foreach (var serviceType in togglableHandler.Replacement.Services)
                {
                    var key = new ReplacementKey
                    {
                        Service = serviceType,
                        Implementation = implementationType
                    };

                    if (_replacementsByService.ContainsKey(key))
                        throw new InvalidOperationException(
                            $"Replacement of implementation '{implementationType.FullName}' " +
                            $"and service '{serviceType.FullName}' already registered");

                    _replacementsByService[key] = togglableHandler;
                }
            }
        }

        private class TogglableHandler
        {
            private readonly Func<bool> _isActivePredicate;
            private readonly Func<Type, int> _servicePriorityFunction;

            public TogglableHandler(IHandler handler)
            {
                Handler = handler;
                Replacement = handler.GetReplacement();

                _isActivePredicate = handler.GetIsActivePredicate();
                _servicePriorityFunction = handler.GetServicePriorityFunction();
            }

            public IHandler Handler { get; }
            public bool CanToggle => _isActivePredicate != null;
            public bool IsActive => _isActivePredicate == null || _isActivePredicate.Invoke();
            public Replacement Replacement { get; }

            public int GetPriorityOfService(Type serviceType)
            {
                return _servicePriorityFunction.Invoke(serviceType);
            }
        }

        private class ReplacementKey
        {
            public Type Service { get; set; }
            public Type Implementation { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is ReplacementKey otherKey)) return false;

                return Service == otherKey.Service && Implementation == otherKey.Implementation;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Service.GetHashCode() * 17 + Implementation?.GetHashCode() ?? 0;
                }
            }
        }
    }
}
