//--------------------------------------------------------------------------------------------------
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
//--------------------------------------------------------------------------------------------------
#if NET452
using Autofac;
using DataArt.Atlas.Core.Shell;
using DataArt.Atlas.Services.Scheduler.Sdk;

namespace DataArt.Atlas.Services.Scheduler.Application
{
    public class SchedulerService : ServiceShell
    {
        protected override string ServiceKey => SchedulerClientConfig.ServiceKey;

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacConfig.Configure(builder);
        }
    }
}
#endif