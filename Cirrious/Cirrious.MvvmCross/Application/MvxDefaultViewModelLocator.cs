#region Copyright
// <copyright file="MvxDefaultViewModelLocator.cs" company="Cirrious">
// (c) Copyright Cirrious. http://www.cirrious.com
// This source is subject to the Microsoft Public License (Ms-PL)
// Please see license.txt on http://opensource.org/licenses/ms-pl.html
// All other rights reserved.
// </copyright>
// 
// Project Lead - Stuart Lodge, Cirrious. http://www.cirrious.com
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Platform.Diagnostics;

namespace Cirrious.MvvmCross.Application
{
    public class MvxDefaultViewModelLocator
        : IMvxViewModelLocator
    {
        #region IMvxViewModelLocator Members

        public bool TryLoad(Type viewModelType, IDictionary<string, string> parameters, out IMvxViewModel model)
        {
            model = null;
            var specifiedParamCount = parameters == null ? 0 : parameters.Count;
            var constructors = viewModelType
#if NETFX_CORE
                .GetTypeInfo().DeclaredConstructors
#else
                .GetConstructors()
#endif
                // all the parameters must be strings or there must be no 
                // parameters at all
                .Where(c => c.GetParameters().All(p => p.ParameterType == typeof(string)) || !c.GetParameters().Any())
                .Select(c => new { Constructor = c, Params = c.GetParameters(), ParamCount = c.GetParameters().Length })
                // no use looking at a constructor that has fewer arguments 
                // than was specified
                .Where(c => c.ParamCount >= specifiedParamCount)
                // sort to make sure that we select, if it exists, the 
                // constructor that has the same parameter count as specified
                .OrderBy(c => c.ParamCount)
                .ToArray();

            if (!constructors.Any())
                return false;

            var invokeWith = new List<object>();

            // we have specified no parameters and there is a parameterless 
            // constructor
            if (specifiedParamCount == 0 && constructors.Any(c => c.ParamCount == 0))
            {
                // use the parameterless constructor
            }
            else
            {
                // just in case there are no exact matches, just pick the first
                var constructor = constructors.FirstOrDefault();

                // try and find an exact parameter-name match
                if (specifiedParamCount > 0)
                {
                    var specifParamNames = parameters.Keys.ToArray();
                    var exactMatches = from constr in constructors
                                       let constrParamNames = constr.Params.Select(p => p.Name)
                                       let intersect = constrParamNames.Intersect(specifParamNames)
                                       where intersect.Count() == specifiedParamCount
                                       select constr;
                    // just pick the first one (or nothing)
                    var exactMatch = exactMatches.FirstOrDefault();
                    if (exactMatch != null)
                    {
                        // there is an exact names match
                        constructor = exactMatch;
                    }
                }

                if (constructor != null)
                {
                    // try and load a value out of the specifed parameters that
                    // match the constructor parameter name
                    foreach (var parameter in constructor.Params)
                    {
                        string parameterValue = null;
                        if (parameters == null || !parameters.TryGetValue(parameter.Name, out parameterValue))
                        {
                            MvxTrace.Trace(
                                "Missing parameter in call to {0} - missing parameter {1} - asssuming null",
                                viewModelType,
                                parameter.Name);
                        }
                        invokeWith.Add(parameterValue);
                    }
                }
            }

            model = Activator.CreateInstance(viewModelType, invokeWith.ToArray()) as IMvxViewModel;
            return (model != null);
        }

        #endregion
    }
}