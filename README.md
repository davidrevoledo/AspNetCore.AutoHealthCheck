<p align="center">
  <img src="inyector.jpg" alt="Inyector" width="100"/>
</p>

# Inyector

What inyector is ? 

Injector is a very simple tool that allow application that use Microsoft DI inyection to auto configure things.

***This Library Is NOT*** a dependency injection engine.

It is simply an abstraction layer to configure our objects no matter what technology we use as an injection engine.

You can use Injector with your favorites libraries like Asp.Net Core DI, Autofac, Ninject and others ...

[![CodeFactor](https://www.codefactor.io/repository/github/davidrevoledo/inyector/badge)](https://www.codefactor.io/repository/github/davidrevoledo/inyector)
[![Build status](https://ci.appveyor.com/api/projects/status/j7f6vfv3s4nwwak6?svg=true)](https://ci.appveyor.com/project/davidrevoledo/inyector)
![NuGet](https://img.shields.io/nuget/dt/Inyector.svg)
![NuGet](https://img.shields.io/nuget/v/Inyector.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)


### Installation
Grab the latest Inyector NuGet package and install in your solution. https://www.nuget.org/packages/Inyector/
```sh
PM > Install-Package Inyector 
NET CLI - dotnet add package Inyector 
paket add Inyector --version 1.2
```

### How to use
           -  Scan (Allow you to declare what assemblies Inyector need to listen to apply the rules, this is made to avoid processing all the referenced assmeblies)
```c#
           c.Scan(typeof(Startup).Assembly)
```    
           
           - Modes 
           (The way to declare inyection engine without repeat code,
           you define a Mode with name an with an action that get both types)
           
```c#
           c.AddMode("MyCustomMode", (type, interf) => services.AddScoped(interf, type));
```    
           
           - Rules
            You can apply any rule in an assembly or in all the shared scaned assemblies
            You can create your custom rules as well.
           
           - AddRuleForNamingConvention
           (You can auto-inyect all the objects that have the convention of Class and IClass (Interface) )
            
          -  AddRuleForEndsWithNamingConvention
           (You can auto-inyect all the objects that finish with a list of key values like "Helper" and "Factory" then
           if you have FooFactory and IFoo2Factory they can be auto-inyected)
           
           - AvoidInyectorAttribute you can avoid to apply any Inyector Rule
           with this attribute
           
           - InyectAttribute 
           with this attribute you can declare what object Inyector should auto-inyect
           You set the Interface and the Mode, if not Default will the mode (if exist) that apply to this Attribute
           
```c#
               [Inyect(typeof(IFooHelper))]
               public class CarHelper : IFooHelper
               {
               }
```    
   
```c#
               [Inyect]
               public class CarHelper 
               {
               }
```    
   
```c#
               [Inyect(typeof(IFooHelper), mode : "MyCustomMode")]
               public class CarHelper : IFooHelper
               {
               }
```    

#### AspNetCore
```sh
PM > Install-Package Inyector-AspNetCore 	
NET CLI - dotnet add package Inyector-AspNetCore 
```

With AspNet core you can avoid to configure modes, using the ServiceLifetime Enum to apply pre-builded Modes,
Also you can define your owns.

```c#
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // use injector
            services.UseInjector(configurations =>
            {
                configurations.Scan(typeof(Startup).Assembly)
                    .DefaultMode(services, ServiceLifetime.Singleton)
                    .AddRuleForNamingConvention(ServiceLifetime.Singleton);
            });
        }
```

#### Raw
To use injector directly you have call ```C# InyectorStartup ``` class like this :
```c#
InyectorStartup.Init(c =>
            {
                c.Scan(typeof(AnyClass).Assembly)
                    .AddRuleForNamingConvention((type, interf) => services.AddSingleton(interf, type))
                    .AddRule((type, inter)=> services.RegisterType(inter, type));
            });
```



### Licensing
Inyector is licensed under the MIT License

### Development
Want to contribute? Great!



