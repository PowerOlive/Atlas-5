#region License
// =================================================================================================
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =================================================================================================
#endregion

using System;
using System.Linq;
using System.Reflection;
using DataArt.Atlas.Core.Shell;

namespace DataArt.Atlas.Core.Application
{
    public static class TypeLocator
    {
        public static Assembly GetEntryPointAssembly()
        {
#if NET452
            return GetTypeEntryPointAssembly(typeof(ServiceShell));
#endif

#if NETSTANDARD2_0
            return GetTypeEntryPointAssembly(typeof(Startup));
#endif
        }

        public static Assembly GetTypeEntryPointAssembly(Type baseType)
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                return assembly;
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(t => t.IsSubclassOf(baseType)))
                .ToList();
            var type = types.FirstOrDefault(t => !types.Any(x => x.IsSubclassOf(t)));
            if (type == null)
            {
                throw new InvalidOperationException($"Unable to locate EntryPointAssembly with type {baseType}");
            }

            return type.Assembly;
        }
    }
}