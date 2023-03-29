namespace FluentValidation.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Controllers;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using FormData = System.Collections.Generic.Dictionary<string, string>;

public class FluentValidationModelValidatorFilterIntegrationTests : IClassFixture<WebAppFixture> {
	private readonly WebAppFixture _webApp;

	public FluentValidationModelValidatorFilterIntegrationTests(ITestOutputHelper output, WebAppFixture webApp) {
		CultureScope.SetDefaultCulture();
		_webApp = webApp;
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Not_Run_Validation_If_Filter_Does_Not_Match_ModelType() { 
		var form = new FormData { 
			{"Email", "foo"}, 
			{"Surname", "foo"}, 
			{"Forename", "foo"}, 
			{"DateOfBirth", null}, 
			{"Address1", null} 
		}; 
 
		// model type opted out - do not run auto validation
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(cfg => { 
				cfg.Filter = modelType => modelType != typeof(AutoFilterModel); 
			}); 
			services.AddMvc().AddNewtonsoftJson(); 
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilter", form); 
 
		result.Count.ShouldEqual(0); 
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_If_No_Filter_Specified() { 
		var form = new FormData { 
			{"Email", "foo"}, 
			{"Surname", "foo"}, 
			{"Forename", "foo"}, 
			{"DateOfBirth", null}, 
			{"Address1", null} 
		}; 

		// No filter specified - we run validation rules as normal
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(); 
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilter", form);

		result.Count.ShouldEqual(4); 
		result.IsValidField("Email").ShouldBeFalse(); //Email validation failed 
		result.IsValidField("DateOfBirth").ShouldBeFalse(); //Date of Birth not specified
		result.IsValidField("Surname").ShouldBeFalse(); //Surname not specified
		result.IsValidField("Address1").ShouldBeFalse(); //Address1 not specified
	} 
 
	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_If_Filter_Matches_ModelType() { 
		var form = new FormData { 
			{"Email", "foo"}, 
			{"Surname", "foo"}, 
			{"Forename", "foo"}, 
			{"DateOfBirth", null}, 
			{"Address1", null} 
		}; 

		//AutoFilterChildModel opted into auto validation - will run
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(cfg => { 
				cfg.Filter = modelType => modelType == typeof(AutoFilterModel); 
			}); 
			services.AddMvc().AddNewtonsoftJson(); 
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilter", form); 
 
 		result.Count.ShouldEqual(4); 
		result.IsValidField("Email").ShouldBeFalse(); //Email validation failed 
		result.IsValidField("DateOfBirth").ShouldBeFalse(); //Date of Birth not specified
		result.IsValidField("Surname").ShouldBeFalse(); //Surname not specified
		result.IsValidField("Address1").ShouldBeFalse(); //Address1 not specified
	}

	[Fact]
	public async Task AddFluentValidationAutoValidation_Should_Not_Run_Validation_If_Filter_Only_Includes_Child_Type() {
		var form = new FormData {
			{"Id", null},
			{"ChildModel", null}
		};

		// Child model is "opted in", but parent is not - we skip validation entirely because auto validation looks at top-level type
		var client = _webApp.CreateClientWithServices(services => {
			services.AddFluentValidationAutoValidation(cfg => {
				cfg.Filter = modelType => modelType == typeof(AutoFilterModel);
			});
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		});

		var result = await client.GetErrors("AutoFilterParent", form);

		result.Count.ShouldEqual(0);
	}

	[Fact]
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_If_Filter_Returns_True_For_Parent() {
		var form = new FormData {
			{"Id", null},
			{"ChildModel.Email", "foo"}, 
			{"ChildModel.Surname", "foo"}, 
			{"ChildModel.Forename", "foo"}, 
			{"ChildModel.DateOfBirth", null}, 
			{"ChildModel.Address1", null} 
		};

		// Parent model is "opted in" - we should run all validation rules
		var client = _webApp.CreateClientWithServices(services => {
			services.AddFluentValidationAutoValidation(cfg => {
				cfg.Filter = modelType => modelType == typeof(AutoFilterParentModel);
			});
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		});

		var result = await client.GetErrors("AutoFilterParent", form);

		result.Count.ShouldEqual(5); 
		result.IsValidField("Id").ShouldBeFalse(); //Id not specified for parent
		result.IsValidField("ChildModel.Email").ShouldBeFalse(); //Email validation failed for child
		result.IsValidField("ChildModel.DateOfBirth").ShouldBeFalse(); //Date of Birth not specified for child
		result.IsValidField("ChildModel.Surname").ShouldBeFalse(); //surname not specified for child
		result.IsValidField("ChildModel.Address1").ShouldBeFalse(); //Address1 not specified for child
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Not_Run_Validation_For_Root_Collection_If_Filter_Does_Not_Match_CollectionType() { 
		var form = new FormData { 
			{"test[0].Email", "foo"}, 
			{"test[0].Surname", "foo"}, 
			{"test[0].Forename", "foo"}, 
			{"test[0].DateOfBirth", null}, 
			{"test[0].Address1", null} 
		}; 
 
		// collection type is not opted in - should not run validation
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(cfg => {
				cfg.Filter = modelType => modelType == typeof(AutoFilterModel);
			});
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilterRootCollection", form); 
 
		result.Count.ShouldEqual(0);
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_For_Root_Collection_If_Filter_Matches_CollectionType() { 
		var form = new FormData { 
			{"test[0].Email", "foo"}, 
			{"test[0].Surname", "foo"}, 
			{"test[0].Forename", "foo"}, 
			{"test[0].DateOfBirth", null}, 
			{"test[0].Address1", null},
			{"test[1].Email", "foo"}, 
			{"test[1].Surname", "foobar"}, 
			{"test[1].Forename", "foo"}, 
			{"test[1].DateOfBirth", DateTime.UtcNow.Date.ToString()}, 
			{"test[1].Address1", "foo"} 
		}; 
 
		// collection type is opted in - should run validation
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(cfg => {
				cfg.Filter = modelType => modelType == typeof(List<AutoFilterModel>);
			});
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilterRootCollection", form); 
 
		result.Count.ShouldEqual(5);
		result.IsValidField("test.x[0].Email").ShouldBeFalse(); //Email validation failed for collection item #1
		result.IsValidField("test.x[0].DateOfBirth").ShouldBeFalse(); //Date of Birth not specified for collection item #1
		result.IsValidField("test.x[0].Surname").ShouldBeFalse(); //surname not specified for collection item #1
		result.IsValidField("test.x[0].Address1").ShouldBeFalse(); //Address1 not specified for collection item #1
		result.IsValidField("test.x[1].Email").ShouldBeFalse(); //Email validation failed for collection item #2
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_For_Root_Collection_If_No_Filter_Specified() { 
		var form = new FormData { 
			{"test[0].Email", "foo"}, 
			{"test[0].Surname", "foo"}, 
			{"test[0].Forename", "foo"}, 
			{"test[0].DateOfBirth", null}, 
			{"test[0].Address1", null},
			{"test[1].Email", "foo"}, 
			{"test[1].Surname", "foobar"}, 
			{"test[1].Forename", "foo"}, 
			{"test[1].DateOfBirth", DateTime.UtcNow.Date.ToString()}, 
			{"test[1].Address1", "foo"} 
		};  
 
		// no filter applied - run collection validation as normal
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation();
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilterRootCollection", form); 
 
		result.Count.ShouldEqual(5);
		result.IsValidField("test.x[0].Email").ShouldBeFalse(); //Email validation failed for collection item #1
		result.IsValidField("test.x[0].DateOfBirth").ShouldBeFalse(); //Date of Birth not specified for collection item #1
		result.IsValidField("test.x[0].Surname").ShouldBeFalse(); //surname not specified for collection item #1
		result.IsValidField("test.x[0].Address1").ShouldBeFalse(); //Address1 not specified for collection item #1
		result.IsValidField("test.x[1].Email").ShouldBeFalse(); //Email validation failed for collection item #2
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Not_Run_Validation_For_Parent_With_Collection_If_Filter_Does_Not_Match_Type() { 
		var form = new FormData { 
			{"test.Id", null}, 
			{"test.ChildModels[0].Surname", "foo"}, 
			{"test.ChildModels[0].Forename", "foo"}, 
			{"test.ChildModels[0].DateOfBirth", null}, 
			{"test.ChildModels[0].Address1", null},
			{"test.ChildModels[1].Email", "foo"}, 
			{"test.ChildModels[1].Surname", "foobar"}, 
			{"test.ChildModels[1].Forename", "foo"}, 
			{"test.ChildModels[1].DateOfBirth", DateTime.UtcNow.Date.ToString()}, 
			{"test.ChildModels[1].Address1", "foo"} 
		};  
 
		// opt out of automatic validation - don't run validation
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(cfg => {
				cfg.Filter = modelType => modelType != typeof(AutoFilterParentWithCollectionModel);
			});
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilterParentWithCollection", form); 
 
		result.Count.ShouldEqual(0);
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_For_Parent_With_Collection_If_Filter_Matches_Type() { 
		var form = new FormData { 
			{"test.Id", null}, 
			{"test.ChildModels[0].Surname", "foo"}, 
			{"test.ChildModels[0].Forename", "foo"}, 
			{"test.ChildModels[0].DateOfBirth", null}, 
			{"test.ChildModels[0].Address1", null},
			{"test.ChildModels[1].Email", "foo"}, 
			{"test.ChildModels[1].Surname", "foobar"}, 
			{"test.ChildModels[1].Forename", "foo"}, 
			{"test.ChildModels[1].DateOfBirth", DateTime.UtcNow.Date.ToString()}, 
			{"test.ChildModels[1].Address1", "foo"} 
		};  
 
		// opt into auto validation -> run validation
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation(cfg => {
				cfg.Filter = modelType => modelType == typeof(AutoFilterParentWithCollectionModel);
			});
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>();  
		}); 
 
		var result = await client.GetErrors("AutoFilterParentWithCollection", form); 
 
		result.Count.ShouldEqual(6);
		result.IsValidField("test.Id").ShouldBeFalse(); //Is not specified at root
		result.IsValidField("test.ChildModels[0].Email").ShouldBeFalse(); //Email validation failed for collection item #1
		result.IsValidField("test.ChildModels[0].DateOfBirth").ShouldBeFalse(); //Date of Birth not specified for collection item #1
		result.IsValidField("test.ChildModels[0].Surname").ShouldBeFalse(); //surname not specified for collection item #1
		result.IsValidField("test.ChildModels[0].Address1").ShouldBeFalse(); //Address1 not specified for collection item #1
		result.IsValidField("test.ChildModels[1].Email").ShouldBeFalse(); //Email validation failed for collection item #2
	}

	[Fact] 
	public async Task AddFluentValidationAutoValidation_Should_Run_Validation_For_Parent_With_Collection_If_No_Filter_Specified() { 
		var form = new FormData { 
			{"test.Id", null}, 
			{"test.ChildModels[0].Surname", "foo"}, 
			{"test.ChildModels[0].Forename", "foo"}, 
			{"test.ChildModels[0].DateOfBirth", null}, 
			{"test.ChildModels[0].Address1", null},
			{"test.ChildModels[1].Email", "foo"}, 
			{"test.ChildModels[1].Surname", "foobar"}, 
			{"test.ChildModels[1].Forename", "foo"}, 
			{"test.ChildModels[1].DateOfBirth", DateTime.UtcNow.Date.ToString()}, 
			{"test.ChildModels[1].Address1", "foo"} 
		};  
 
		// no filter applied - run validation as normal
		var client = _webApp.CreateClientWithServices(services => { 
			services.AddFluentValidationAutoValidation();
			services.AddMvc().AddNewtonsoftJson();
			services.AddScoped<IValidator<AutoFilterParentWithCollectionModel>, AutoFilterParentWithCollectionModelValidator>(); 
			services.AddScoped<IValidator<List<AutoFilterModel>>, AutoFilterRootCollectionValidator>(); 
			services.AddScoped<IValidator<AutoFilterParentModel>, AutoFilterParentModelValidator>(); 
			services.AddScoped<IValidator<AutoFilterModel>, AutoFilterChildModelValidator>(); 
		}); 
 
		var result = await client.GetErrors("AutoFilterParentWithCollection", form); 
 
		result.Count.ShouldEqual(6);
		result.IsValidField("test.Id").ShouldBeFalse(); //Is not specified at root
		result.IsValidField("test.ChildModels[0].Email").ShouldBeFalse(); //Email validation failed for collection item #1
		result.IsValidField("test.ChildModels[0].DateOfBirth").ShouldBeFalse(); //Date of Birth not specified for collection item #1
		result.IsValidField("test.ChildModels[0].Surname").ShouldBeFalse(); //surname not specified for collection item #1
		result.IsValidField("test.ChildModels[0].Address1").ShouldBeFalse(); //Address1 not specified for collection item #1
		result.IsValidField("test.ChildModels[1].Email").ShouldBeFalse(); //Email validation failed for collection item #2
	}
}
