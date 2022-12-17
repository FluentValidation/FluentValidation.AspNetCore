#region License
// Copyright (c) .NET Foundation and contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
#endregion

namespace FluentValidation.AspNetCore;

using System;

/// <summary>
/// Auto-validation configuration.
/// </summary>
public class FluentValidationAutoValidationConfiguration {

	/// <summary>
	/// Whether or not child properties should be implicitly validated if a matching validator can be found. By default this is false, and you should wire up child validators using SetValidator.
	/// </summary>
	[Obsolete("Implicit validation of child properties deprecated and will be removed in a future release. Please use SetValidator instead. For details see https://github.com/FluentValidation/FluentValidation/issues/1960")]
	public bool ImplicitlyValidateChildProperties { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the elements of a root model should be implicitly validated when
	/// the root model is a collection type and a matching validator can be found for the element type.
	/// By default this is <see langword="false"/>, and you will need to create a validator for the collection type
	/// (unless <see cref="ImplicitlyValidateChildProperties"/> is <see langword="true"/>.
	/// </summary>
	[Obsolete("Implicit validation of root collection elements is deprecated and will be removed in a future release. Please use an explicit collection validator instead. For details see https://github.com/FluentValidation/FluentValidation/issues/1960")]
	public bool ImplicitlyValidateRootCollectionElements { get; set; }

	/// <summary>
	/// The type of validator factory to use. Uses the ServiceProviderValidatorFactory by default.
	/// </summary>
	[Obsolete("IValidatorFactory and its implementors are deprecated and will be removed in FluentValidation 13. Please use the Service Provider directly. For details see https://github.com/FluentValidation/FluentValidation/issues/1961")]
	public Type ValidatorFactoryType { get; set; }

	/// <summary>
	/// The validator factory to use. Uses the ServiceProviderValidatorFactory by default.
	/// </summary>
	[Obsolete("IValidatorFactory and its implementors are deprecated and will be removed in FluentValidation 13. Please use the Service Provider directly. For details see https://github.com/FluentValidation/FluentValidation/issues/1961")]
	public IValidatorFactory ValidatorFactory { get; set; }

	/// <summary>
	/// By default Data Annotations validation will also run as well as FluentValidation.
	/// Setting this to true will disable DataAnnotations and only run FluentValidation.
	/// </summary>
	public bool DisableDataAnnotationsValidation { get; set; }
}
