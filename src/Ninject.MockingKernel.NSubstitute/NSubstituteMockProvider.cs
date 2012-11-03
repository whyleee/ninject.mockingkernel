﻿//-------------------------------------------------------------------------------
// <copyright file="NSubstituteMockProvider.cs" company="Andre Loker IT Services">
//   Copyright (c) 2011 Andre Loker IT Services
//   Author: Andre Loker (mail@loker-it.de)
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Also licenced under Microsoft Public License (Ms-PL).
// </copyright>
//-------------------------------------------------------------------------------

using System.Linq;
using System.Reflection;

namespace Ninject.MockingKernel.NSubstitute
{
    using System;
    using Activation;
    using Components;
    using global::NSubstitute;

    /// <summary>
    /// Creates mocked instances via <c>NSubstitute</c>.
    /// </summary>
    public class NSubstituteMockProvider : NinjectComponent, IProvider, IMockProviderCallbackProvider
    {
        /// <summary>
        ///   Gets the type (or prototype) of instances the provider creates.
        /// </summary>
        public Type Type
        {
            get { return typeof(Substitute); }
        }

        /// <summary>
        /// Gets a callback that creates an instance of the <see cref="NSubstituteMockProvider"/>.
        /// </summary>
        /// <returns>
        /// The created callback.
        /// </returns>
        public Func<IContext, IProvider> GetCreationCallback()
        {
            return ctx => this;
        }

        /// <summary>
        /// Creates an instance within the specified context.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The created instance.
        /// </returns>
        public object Create(IContext context)
        {
            var service = context.Request.Service;

            if (service.IsInterface || service.IsAbstract)
            {
                return Substitute.For(new[] {service}, null);
            }

            var constructor = SelectConstructor(service);
            var arguments = constructor.GetParameters()
                .Select(parameter => context.Kernel.Get(parameter.ParameterType)).ToArray();

            return Substitute.For(new[] {service}, arguments);
        }

        /// <summary>
        /// Selects most valuable constructor in the service type.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>Most valuable constructor from the service type.</returns>
        private ConstructorInfo SelectConstructor(Type service)
        {
            var constructor = service.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();

            if (constructor == null)
            {
                throw new ArgumentException(string.Format("Error resolving service '{0}': no constructors", service.FullName));
            }

            return constructor;
        }
    }
}