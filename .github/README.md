# No longer supported

## The FluentValidation.AspNetCore package is no longer being maintained and is now unsupported. We encourage users move away from this package and use the core FluentValidation package with a manual validation approach as detailed at  https://docs.fluentvalidation.net/en/latest/aspnet.html 

## Further details about this decision can be found at https://github.com/FluentValidation/FluentValidation/issues/1959

## The existing code and documentation remains here for reference but will not receive any new features or fixes.

# ASP.NET Core integration for FluentValidation

[![Build Status](https://github.com/FluentValidation/FluentValidation.AspNetCore/workflows/CI/badge.svg)](https://github.com/FluentValidation/FluentValidation.AspNetCore/actions?query=workflow%3ACI) [![NuGet](https://img.shields.io/nuget/v/FluentValidation.AspNetCore.svg)](https://nuget.org/packages/FluentValidation.AspNetCore)
 [![Nuget](https://img.shields.io/nuget/dt/FluentValidation.AspNetCore.svg)](https://nuget.org/packages/FluentValidation.AspNetCore)

## Supporting the project
If you use FluentValidation in a commercial project, please sponsor the project financially. FluentValidation is developed and supported by [@JeremySkinner](https://github.com/JeremySkinner) for free in his spare time and financial sponsorship helps keep the project going. You can sponsor the project via either [GitHub sponsors](https://github.com/sponsors/JeremySkinner) or [OpenCollective](https://opencollective.com/FluentValidation).

## Table of contents

- [Introduction](#introduction)
- [Supported Platforms](#supported-platforms)
- [Get Started](#get-started)
- [Automatic Validation](#automatic-validation)
- [Clientside Validation](#clientside-validation)

## Introduction

This package integrates [FluentValidation](https://github.com/FluentValidation/FluentValidation) with ASP.NET Core and provides the following features:

- Plugs into the ASP.NET Core MVC validation pipeline to provide automatic validation
- Clientside validation integration with jQuery Validate by providing adaptors for ASP.NET Core MVC's clientside validators. 

When you enable automatic validation, FluentValidation plugs into the validation pipeline that's part of ASP.NET Core MVC and allows models to be validated before a controller action is invoked (during model-binding). This approach to validation is more seamless than [manually invoking the validator](https://docs.fluentvalidation.net/en/latest/aspnet.html) but has several downsides:

- **Auto validation is not asynchronous**: If your validator contains asynchronous rules then your validator will not be able to run. You will receive an exception at runtime if you attempt to use an asynchronous validator with auto-validation.
- **Auto validation is MVC-only**: Auto-validation only works with MVC Controllers and Razor Pages. It does not work with the more modern parts of ASP.NET such as Minimal APIs or Blazor.

> **Warning**
> We no longer recommend using auto-validation for new projects for the reasons mentioned above, but this package is still available for legacy implementations. For new projects we recommend using [Manual Validation instead](https://docs.fluentvalidation.net/en/latest/aspnet.html).

## Supported Platforms

This package works with FluentValidation 11 when running inside an ASP.NET Core project targetting .NET Core 3.1 or .NET 6 (or newer). 

## Get Started
FluentValidation.AspNetCore can be installed using the Nuget package manager or the `dotnet` CLI.

```
dotnet add package FluentValidation.AspNetCore
```

The following examples will make use of a `Person` object which is validated using a `PersonValidator`. These classes are defined as follows:

```csharp
public class Person 
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string Email { get; set; }
  public int Age { get; set; }
}

public class PersonValidator : AbstractValidator<Person> 
{
  public PersonValidator() 
  {
    RuleFor(x => x.Id).NotNull();
    RuleFor(x => x.Name).Length(0, 10);
    RuleFor(x => x.Email).EmailAddress();
    RuleFor(x => x.Age).InclusiveBetween(18, 60);
  }
}
```

If you're using MVC, Web Api or Razor Pages you'll need to register your validator with the Service Provider in the `ConfigureServices` method of your application's `Startup` class. 

```csharp
public void ConfigureServices(IServiceCollection services) 
{
    // If you're using MVC or WebApi you'll probably have
    // a call to AddMvc() or AddControllers() already.
    services.AddMvc();
    
    // ... other configuration ...
    
    services.AddScoped<IValidator<Person>, PersonValidator>();
}
```

Here we register our `PersonValidator` with the service provider by calling `AddScoped`.

> **Note**
> Make sure you add using statements for the `FluentValidation` and `FluentValidation.AspNetCore` namespaces to ensure that appropriate extension methods show up. 

> **Note**
> You must register each validator as `IValidator<T>` where `T` is the type being validated. So if you have a `PersonValidator` that inherits from `AbstractValidator<Person>` then you should register it as `IValidator<Person>`


Alternatively you can register all validators in a specific assembly by using our Service Collection extensions. To do this you can call the appropriate `AddValidators...` extension method on the services collection. [See this page for more details](https://docs.fluentvalidation.net/en/latest/di.html#automatic-registration)

```csharp
public void ConfigureServices(IServiceCollection services) 
{
    services.AddMvc();

    // ... other configuration ...

    services.AddValidatorsFromAssemblyContaining<PersonValidator>();
}
```

Here we use the `AddValidatorsFromAssemblyContaining` method to automatically register all validators in the same assembly as `PersonValidator` with the service provider.

Now that the validators are registered with the service provider you can start working with manual validation or automatic validation.


## Automatic Validation

Once installed, you'll need to modify the `ConfigureServices` in your `Startup` to include a call to `AddFluentValidationAutoValidation()`:

```csharp
public void ConfigureServices(IServiceCollection services) 
{
    services.AddMvc();

    // ... other configuration ...

    services.AddFluentValidationAutoValidation();

    services.AddScoped<IValidator<Person>, PersonValidator>();
}
```

This method must be called after `AddMvc` (or `AddControllers`/`AddControllersWithViews`). Make sure you add `using FluentValidation.AspNetCore` to your startup file so the appropriate extension methods are available. 

> **Note**
> Auto validation only works with Controllers or Razor Pages. If you're using Minimal API's you should use the regular FluentValidation package. [See this section of the documentation for more details](https://docs.fluentvalidation.net/en/latest/aspnet.html#minimal-apis). If you're using Blazor then [please checkout one of the third-party integration packages](https://docs.fluentvalidation.net/en/latest/blazor.html).

We can use the `Person` class within our controller and associated view:

```csharp
public class PeopleController : Controller 
{
  public ActionResult Create() 
  {
    return View();
  }

  [HttpPost]
  public IActionResult Create(Person person) 
  {
    if(! ModelState.IsValid) 
    { 
      // re-render the view when validation failed.
      return View("Create", person);
    }

    Save(person); //Save the person to the database, or some other logic

    TempData["notice"] = "Person successfully created";
    return RedirectToAction("Index");
  }
}
```

The view is defined as follows:

```html
@model Person

<div asp-validation-summary="ModelOnly"></div>

<form asp-action="Create">
  Id: <input asp-for="Id" /> <span asp-validation-for="Id"></span>
  <br />
  Name: <input asp-for="Name" /> <span asp-validation-for="Name"></span>
  <br />
  Email: <input asp-for="Email" /> <span asp-validation-for="Email"></span>
  <br />
  Age: <input asp-for="Age" /> <span asp-validation-for="Age"></span>

  <br /><br />
  <input type="submit" value="submit" />
</form>
```

Now when you post the form, MVC's model-binding infrastructure will automatically instantiate the `PersonValidator`, invoke it and add the validation results to `ModelState`.

Unlike the manual validation example, we don't have a reference to the validator directly. Instead, ASP.NET will handle invoking the validator and adding the error messages to `ModelState` before the controller action is invoked. Inside the action, you only need to check `ModelState.IsValid`

> **Warning**
> Remember: you can't use asynchronous rules when using auto-validation as ASP.NET's validation pipeline is not asynchronous.

### Compatibility with ASP.NET's built-in Validation

After FluentValidation is executed, any other validator providers will also have a chance to execute. This means you can mix FluentValidation auto-validation with [DataAnnotations attributes](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) (or any other ASP.NET `ModelValidatorProvider` implementation).

If you want to disable this behaviour so that FluentValidation is the only validation library that executes, you can set `DisableDataAnnotationsValidation` to `true` in your application startup routine:

```csharp
services.AddFluentValidationAutoValidation(config => 
{
 config.DisableDataAnnotationsValidation = true;
});
```

*Note* If you do set `DisableDataAnnotationsValidation` then support for `IValidatableObject` will also be disabled.

### Implicit vs Explicit Child Property Validation

> **Warning**
> Implicit validation of child properties is deprecated and will be removed from a future release. The documentation remains here for reference but we no longer recommend taking this approach. [See this issue for details](https://github.com/FluentValidation/FluentValidation/issues/1960).

When validating complex object graphs you must explicitly specify any child validators for complex properties by using `SetValidator` ([see the section on validating complex properties](https://docs.fluentvalidation.net/en/latest/start.html#complex-properties))

When running an ASP.NET MVC application, you can also optionally enable implicit validation for child properties. When this is enabled, instead of having to specify child validators using `SetValidator`, MVC's validation infrastructure will recursively attempt to automatically find validators for each property. This can be done by setting `ImplicitlyValidateChildProperties` to true:

```csharp
services.AddFluentValidationAutoValidation(config => 
{
 config.ImplicitlyValidateChildProperties = true;
});
```

Note that if you enable this behaviour you should not use `SetValidator` for child properties, or the validator will be executed twice.

> **Note**
> The `AddFluentValidationAutoValidation` method is only available in version 11.1 and newer. In older versions, call `services.AddFluentValidation()` instead, which is the equivalent of calling `services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters()`

### Implicit Validation of Collection-Type Models

> **Warning**
> Implicit validation of collection-type models is deprecated and will be removed from a future release. The documentation remains here for reference but we no longer recommend taking this approach. [See this issue for details](https://github.com/FluentValidation/FluentValidation/issues/1960).

By default, you must create a specific collection validator or enable implicit child property validation to validate a model that is of a collection type. For example, no validation of the following model will occur with the default settings unless you define a validator that inherits from `AbstractValidator<List<Person>>`.

```csharp
public ActionResult DoSomething(List<Person> people) => Ok();
```

With implicit child property validation enabled (see above), you don't have to explicitly create a collection validator class as each person element in the collection will be validated automatically. However, any child properties on the `Person` object will be automatically validated too meaning you can no longer use `SetValidator`. If you don't want this behaviour, you can also optionally enable implicit validation for root collection elements only. For example, if you want each `Person` element in the collection to be validated automatically, but not its child properties you can set `ImplicitlyValidateRootCollectionElements` to true:

```csharp
services.AddFluentValidationAutoValidation(config => 
{
 config.ImplicitlyValidateRootCollectionElements = true;
});
```

Note that this setting is ignored when `ImplicitlyValidateChildProperties` is `true`.

> **Note**
> The `AddFluentValidationAutoValidation` method is only available in version 11.1 and newer. In older versions, call `services.AddFluentValidation()` instead, which is the equivalent of calling `services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters()`

### Validator customization

One downside to using auto-validation is that you don't have access to the validator instance meaning you don't have as much control over the validation processes compared to running the validator manually.

You can use the `CustomizeValidatorAttribute` to configure how the validator will be run. For example, if you want the validator to only run for a particular ruleset then you can specify that ruleset name by attributing the parameter that is going to be validated:

```csharp
public ActionResult Save([CustomizeValidator(RuleSet="MyRuleset")] Person person) 
{
  // ...
}
```

This is the equivalent of specifying the ruleset if you were to pass a ruleset name to a validator:

```csharp
var validator = new PersonValidator();
var person = new Person();
var result = validator.Validate(person, options => options.IncludeRuleSet("MyRuleset"));
```

The attribute can also be used to invoke validation for individual properties:

```csharp
public ActionResult Save([CustomizeValidator(Properties="Surname,Forename")] Person person) 
{
  // ...
}
```
…which would be the equivalent of specifying properties in the call to validator.Validate:

```csharp
var validator = new PersonValidator();
var person = new Person();
var result = validator.Validate(person, options => options.IncludeProperties("Surname", "Forename"));
```

You can also use the `CustomizeValidatorAttribute` to skip validation for a particular type. This is useful for if you need to validate a type manually (for example, if you want to perform async validation then you'll need to instantiate the validator manually and call `ValidateAsync` as MVC's validation pipeline is not asynchronous).

```csharp
public ActionResult Save([CustomizeValidator(Skip=true)] Person person) 
{
  // ...
}
```

### Validator Interceptors

You can further customize this process by using an interceptor. An interceptor has to implement the `IValidatorInterceptor` interface from the `FluentValidation.AspNetCore` namespace:

```csharp
public interface IValidatorInterceptor	
{
  IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext validationContext);
  ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result);
}

```

This interface has two methods – `BeforeAspNetValidation` and `AfterAspNetValidation`. If you implement this interface in your validator classes then these methods will be called as appropriate during the MVC validation pipeline.

`BeforeMvcValidation` is invoked after the appropriate validator has been selected but before it is invoked. One of the arguments passed to this method is a `ValidationContext` that will eventually be passed to the validator. The context has several properties including a reference to the object being validated. If we want to change which rules are going to be invoked (for example, by using a custom `ValidatorSelector`) then we can create a new `ValidationContext`, set its `Selector` property, and return that from the `BeforeAspNetValidation` method.

Likewise, `AfterAspNetValidation` occurs after validation has occurs. This time, we also have a reference to the result of the validation. Here we can do some additional processing on the error messages before they're added to `ModelState`.

As well as implementing this interface directly in a validator class, we can also implement it externally, and specify the interceptor by using a `CustomizeValidatorAttribute` on an action method parameter:

```csharp
public ActionResult Save([CustomizeValidator(Interceptor=typeof(MyCustomerInterceptor))] Customer cust) 
{
 //...
}
```

In this case, the interceptor has to be a class that implements `IValidatorInterceptor` and has a public, parameterless constructor.

Alternatively, you can register a default `IValidatorInterceptor` with the ASP.NET Service Provider. If you do this, then the interceptor will be used for all validators:

```csharp
public void ConfigureServices(IServiceCollection services) 
{
    services.AddMvc();

    services.AddFluentValidationAutoValidation();
    services.AddValidatorsFromAssemblyContaining<PersonValidator>());

    // Register a default interceptor, where MyDefaultInterceptor is a class that
    // implements IValidatorInterceptor.
    services.AddTransient<IValidatorInterceptor, MyDefaultInterceptor>();
}
```

Note that this is considered to be an advanced scenario. Most of the time you probably won't need to use an interceptor, but the option is there if you want it.

> **Note**
> The `AddFluentValidationAutoValidation` method is only available in version 11.1 and newer. In older versions, call `services.AddFluentValidation()` instead, which is the equivalent of calling `services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters()`

## Clientside Validation

FluentValidation is a server-library and does not provide any client-side validation directly. However, it can provide metadata which can be applied to the generated HTML elements for use with a client-side framework such as jQuery Validate in the same way that ASP.NET's default validation attributes work.

Note that not all rules defined in FluentValidation will work with ASP.NET's client-side validation. For example, any rules defined using a condition (with When/Unless), custom validators, or calls to `Must` will not run on the client side. Nor will any rules in a `RuleSet` (although this can be changed - see below). The following validators are supported on the client:

* NotNull/NotEmpty
* Matches (regex)
* InclusiveBetween (range)
* CreditCard
* Email
* EqualTo (cross-property equality comparison)
* MaxLength
* MinLength
* Length

To enable clientside integration you need to install the `FluentValidation.AspNetCore` package and call the `AddFluentValidationClientsideAdapters` in your application startup:

```csharp
public void ConfigureServices(IServiceCollection services) 
{
    services.AddMvc();

    services.AddFluentValidationClientsideAdapters();

    services.AddScoped<IValidator<Person>, PersonValidator>();
    // etc
}
```
> **Note**
> Note that the `AddFluentValidationClientsideAdapters` method is only available in FluentValidation 11.1 and newer. In older versions, you should use the `AddFluentValidation` method which enables *both* auto-validation and clientside adapters. If you only want clientside adapters and don't want auto validation in 11.0 and older, you can configure this by calling `services.AddFluentValidation(config => config.AutomaticValidationEnabled = false)`

Alternatively, instead of using client-side validation you could instead execute your full server-side rules via AJAX using a library such as [FormHelper](https://github.com/sinanbozkus/FormHelper). This allows you to use the full power of FluentValidation, while still having a responsive user experience.

### Specifying a RuleSet for client-side messages

If you're using rulesets alongside ASP.NET MVC, then you'll notice that by default FluentValidation will only generate client-side error messages for rules not part of any ruleset. You can instead specify that FluentValidation should generate clientside rules from a particular ruleset by attributing your controller action with a `RuleSetForClientSideMessagesAttribute`:

```csharp
[RuleSetForClientSideMessages("MyRuleset")]
public ActionResult Index() 
{
   return View(new Person());
}
```

You can also use the `SetRulesetForClientsideMessages` extension method within your controller action, which has the same affect:

```csharp
public ActionResult Index() 
{
   ControllerContext.SetRulesetForClientsideMessages("MyRuleset");
   return View(new Person());
}
```

You can force all rules to be used to generate client-side error message by specifying a ruleset of "*".

## Razor Pages

Both the manual validation and auto validation approaches can be used with Razor pages. 

Auto validation has an additional limitation with Razor pages in that you can't define a validator for the whole Page Model itself, only for models exposed as properties on the page model.

Additionally when using client side integration, you can't use the `[RuleSetForClientsideMessages]` attribute and should use the `SetRulesetForClientsideMessages` extension method within your page handler:

```csharp
public IActionResult OnGet() 
{
   PageContext.SetRulesetForClientsideMessages("MyRuleset");
   return Page();
}
```


## License, Copyright etc

FluentValidation has adopted the [Code of Conduct](https://github.com/FluentValidation/FluentValidation/blob/main/.github/CODE_OF_CONDUCT.md) defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

FluentValidation is copyright &copy; 2008-2022 .NET Foundation, [Jeremy Skinner](https://jeremyskinner.co.uk) and other contributors and is licensed under the [Apache2 license](https://github.com/JeremySkinner/FluentValidation/blob/master/License.txt).

This project is part of the [.NET Foundation](https://dotnetfoundation.org).
