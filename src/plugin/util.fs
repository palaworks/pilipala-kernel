module pilipala.plugin.util

open System
open System.Runtime.Loader

let pluginCtx pluginDllPath =

    let resolver = AssemblyDependencyResolver(pluginDllPath)

    { new AssemblyLoadContext() with

        override b.Load assemblyName =

            match resolver.ResolveAssemblyToPath assemblyName with
            | null -> AssemblyLoadContext.Default.Load(assemblyName)
            | path -> b.LoadFromAssemblyPath path

        override b.LoadUnmanagedDll unmanagedDllName =

            match resolver.ResolveUnmanagedDllToPath unmanagedDllName with
            | null -> IntPtr.Zero
            | path -> b.LoadUnmanagedDllFromPath path }
