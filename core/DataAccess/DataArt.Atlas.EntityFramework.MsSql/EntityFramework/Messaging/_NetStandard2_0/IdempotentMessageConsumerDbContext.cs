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
#if NETSTANDARD2_0
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.CallContext.Idempotency;
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;

namespace DataArt.Atlas.EntityFramework.MsSql.EntityFramework.Messaging
{
    public abstract class IdempotentMessageConsumerDbContext<TContext> : BaseDbContext<TContext>
        where TContext : DbContext
    {
        private readonly ILogger logger = AtlasLogging.CreateLogger<IdempotentMessageConsumerDbContext<TContext>>();

        private DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();

        public override int SaveChanges()
        {
            if (EnsureConsumerMessageAsync().GetAwaiter().GetResult())
            {
                var result = base.SaveChanges();
                MessagingContext.IsConsumed = true;
                return result;
            }

            return 0;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await EnsureConsumerMessageAsync())
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                MessagingContext.IsConsumed = true;
                return result;
            }

            return 0;
        }

        public Task EraseAsync(int daysAgo)
        {
            var moment = DateTimeOffset.UtcNow.AddDays(-daysAgo);
            return ConsumedMessages.Where(m => m.ConsumedAt < moment).DeleteAsync();
        }

        protected IdempotentMessageConsumerDbContext(DbContextOptions<TContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ConsumedMessage>()
                .HasIndex(e => e.ConsumedAt)
                .IsUnique(false)
                .ForSqlServerIsClustered(false);
        }

        private async Task<bool> EnsureConsumerMessageAsync()
        {
            if (!MessagingContext.MessageId.HasValue)
            {
                return true;
            }

            if (!MessagingContext.ProvideIdempotency.HasValue)
            {
                throw new InvalidOperationException($"{nameof(MessagingContext)}.{nameof(MessagingContext.ProvideIdempotency)} should be specified explicitly");
            }

            if (!MessagingContext.ProvideIdempotency.Value)
            {
                return true;
            }

            if (MessagingContext.IsConsumed)
            {
                throw new InvalidOperationException("Idempotency could be provided in a single transaction only");
            }

            var messageId = MessagingContext.MessageId.Value;

            var message = await ConsumedMessages.FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                ConsumedMessages.Add(new ConsumedMessage { Id = messageId });
                return true;
            }

            logger.LogWarning("Attempt to consume already consumed message: {@Message}", message);

            return false;
        }
    }
}
#endif