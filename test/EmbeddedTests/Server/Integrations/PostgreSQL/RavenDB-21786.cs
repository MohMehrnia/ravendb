﻿#if NET8_0
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Raven.Client;
using Raven.Client.Documents;
using Xunit;

namespace EmbeddedTests.Server.Integrations.PostgreSQL;

public class RavenDB_21786 : PostgreSqlIntegrationTestBase
{
    [Fact]
    public async Task SkipsGettingUncheckedPropertyValue_ForDocument_WhichDoesNotHaveTheProperty()
    {
        const string collectionName = "Members";
        const string query = $"from {collectionName}";

        using (var store = GetDocumentStore())
        {
            using (var session = store.OpenSession())
            {
                var memberWithNullSurname = new JObject
                {
                    ["Name"] = "Krzysztof",
                    ["Surname"] = null,
                    [Constants.Documents.Metadata.Key] = new JObject
                    {
                        [Constants.Documents.Metadata.Collection] = collectionName
                    }
                };
                
                var memberWithoutSurname = new JObject
                {
                    ["Name"] = "Adrian",
                    [Constants.Documents.Metadata.Key] = new JObject
                    {
                        [Constants.Documents.Metadata.Collection] = collectionName
                    }
                };
                
                session.Store(memberWithNullSurname, "Members/1-A");
                session.Store(memberWithoutSurname, "Members/2-A");

                session.SaveChanges();
            }

            using (var session = store.OpenAsyncSession())
            {
                var members = await session
                    .Query<Member>()
                    .ToListAsync();

                var result = await Act(store, query);

                Assert.NotNull(result);
                Assert.NotEmpty(result.Rows);
                Assert.Equal(members.Count, result.Rows.Count);
            }
        }
    }

    private class Member
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
#endif
