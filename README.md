# CacheInterceptorsNetCore
Caching attributes via interceptors on .netcore3.1 and Castle Windsor. 

# Usage
Declare the attribute either on an interface method:
````
public interface IPowerPlantRepository : IPpmRepository<PowerPlant, long>
{
    Task<string> GetPowerPlantName(long? id);

    [CachePerRequest]
    Task<List<PowerPlant>> GetAllActive();
}
````
Or a virtual method of a class:
````
[AbpAllowAnonymous]
[Cache]
[DisableAuditing]
public virtual async Task<GetProfilePictureOutput> GetProfilePictureById(Guid profilePictureId)
{
    return await GetProfilePictureByIdInternal(profilePictureId);
}

````
Since it is a requirement for intercepting via Dynamic Proxying of Castle.

# Cache Varies by Method Parameters:
Parameters are serialized to json objects via Newtonsoft.Json and if the same parameters are used for the call, the result will return from cache.

# [CachePerRequest]
Using traceIdentifier of a webrequest, it caches the method result and prevents execution of the method with the same parameters.
 

# [Cache]
Caches the method result globally, independent from the web request.

# Installation
CacheInterceptorsNetCore on nuget.org, then;

In startup.cs, register LazyCache service which is a prerequisite of this libary:
````
services.AddLazyCache();
````

Then register CacheInterceptors using Castle kernel:
````
CacheInterceptorsRegistrar.RegisterCacheInterceptors(IocManager.IocContainer, "PPM");
````
where "PPM" is root namespace of our project. You should adjust it to your project accordingly.
Our solution contains such projects
````
PPM.EntityFrameworkCore
PPM.Application
PPM.WebHost
etc...
````
