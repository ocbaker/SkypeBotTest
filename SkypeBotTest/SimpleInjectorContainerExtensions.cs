using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleInjector;
using Container = SimpleInjector.Container;

namespace SkypeBotTest
{
    public static class SimpleInjectorContainerExtensions
    {
        static readonly Type[] reserved = {
            typeof (INotifyPropertyChanged), typeof (INotifyPropertyChanging),
            typeof (IDisposable)
        };
        static readonly Type[] reservedRoot = {

        };

        public static void RegisterPlugins<T>(this Container container, IEnumerable<Assembly> assemblies,
            Lifestyle lifestyle = null) where T : class
        {
            var pluginTypes = GetTypes<T>(assemblies).ToArray();
            HandleLifestyle<T>(container, lifestyle, pluginTypes);
            container.RegisterAll<T>(pluginTypes);
        }

        static void HandleLifestyle<T>(Container container, Lifestyle lifestyle, IEnumerable<Type> pluginTypes)
            where T : class
        {
            if (lifestyle == null || lifestyle == Lifestyle.Transient)
                return;
            var serviceType = typeof(T);
            foreach (var t in pluginTypes)
                container.Register(serviceType, t, lifestyle);
        }

        public static void RegisterPlugins<TImplements, TExport>(this Container container,
            IEnumerable<Assembly> assemblies, Lifestyle lifestyle = null) where TExport : class
        {
            var pluginTypes = GetTypes<TImplements>(assemblies).ToArray();
            HandleLifestyle<TExport>(container, lifestyle, pluginTypes);
            container.RegisterAll<TExport>(pluginTypes);
        }

        public static IEnumerable<Type> GetTypes<T>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies.GetTypes(typeof(T));
        }

        public static IEnumerable<Type> GetTypes(this IEnumerable<Assembly> assemblies, Type t)
        {
            return from assembly in assemblies
                from type in assembly.GetTypes()
                where t.IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where !type.IsInterface
                select type;
        }

        public static void RegisterAllInterfaces<T>(this Container container, IEnumerable<Assembly> assemblies)
        {
            var ifaceType = typeof(T);
            foreach (var s in assemblies.GetTypes<T>())
            {
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.Register(i, s);
            }
        }

        static bool Predicate(Type x, Type ifaceType)
        {
            return x != ifaceType && !reserved.Contains(x) && !reservedRoot.Any(t => t.IsAssignableFrom(x));
        }

        public static void RegisterAllInterfacesAndType<T>(this Container container,
            IEnumerable<Assembly> assemblies, Func<Type, bool> predicate = null)
        {
            var enumerable = assemblies.GetTypes<T>();
            if (predicate != null)
                enumerable = enumerable.Where(predicate);
            container.RegisterInterfacesAndType<T>(enumerable);
        }

        public static void RegisterInterfacesAndType<T>(this Container container, IEnumerable<Type> types)
        {
            var ifaceType = typeof(T);
            foreach (var s in types)
            {
                container.Register(s, s);
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.Register(i, s);
            }
        }

        public static void RegisterInterfaces<T>(this Container container, IEnumerable<Type> types)
        {
            var ifaceType = typeof(T);
            foreach (var s in types)
            {
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.Register(i, s);
            }
        }

        public static void RegisterSingleInterfacesAndType<T>(this Container container, IEnumerable<Type> types)
        {
            var ifaceType = typeof(T);
            foreach (var s in types)
            {
                container.RegisterSingle(s, s);
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.RegisterSingle(i, () => container.GetInstance(s));
            }
        }

        public static void RegisterSingleInterfaces<T>(this Container container, IEnumerable<Type> types)
        {
            var ifaceType = typeof(T);
            foreach (var s in types)
            {
                foreach (var i in s.GetInterfaces().Where(x => Predicate(x, ifaceType)))
                    container.RegisterSingle(i, s);
            }
        }

        public static void RegisterSingleAllInterfaces<T>(this Container container, IEnumerable<Assembly> assemblies,
            Func<Type, bool> predicate = null)
        {
            var enumerable = assemblies.GetTypes<T>();
            if (predicate != null)
                enumerable = enumerable.Where(predicate);
            container.RegisterSingleInterfaces<T>(enumerable);
        }

        public static void RegisterSingleAllInterfacesAndType<T>(this Container container,
            IEnumerable<Assembly> assemblies, Func<Type, bool> predicate = null)
        {
            var enumerable = assemblies.GetTypes<T>();
            if (predicate != null)
                enumerable = enumerable.Where(predicate);
            container.RegisterSingleInterfacesAndType<T>(enumerable);
        }

        public static void RegisterLazy(this Container container, Type serviceType)
        {
            var method = typeof(SimpleInjectorContainerExtensions).GetMethod("CreateLazy")
                .MakeGenericMethod(serviceType);

            var lazyInstanceCreator =
                Expression.Lambda<Func<object>>(
                    Expression.Call(method, Expression.Constant(container)))
                    .Compile();

            var lazyServiceType =
                typeof(Lazy<>).MakeGenericType(serviceType);

            container.Register(lazyServiceType, lazyInstanceCreator);
        }

        public static void RegisterLazy<T>(this Container container) where T : class
        {
            Func<T> factory = container.GetInstance<T>;
            container.Register(() => new Lazy<T>(factory));
        }
    }
}