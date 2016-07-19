using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace MeetUpMeUai.Persistence
{
    public static class DocumentDBRepository<T> where T : class
    {
        private static readonly string DatabaseId = "";//enter the database name here
        private static readonly string CollectionId = "";//enter the collection
        private static readonly string EndPoint = "https://yourfooAzureId.documents.azure.com:443/";//enter the URI from the Keys blade of the Azure Portal
        private static readonly string AuthKey = "";//enter the PRIMARY KEY, or the SECONDARY KEY, from the Keys blade of the Azure  Portal
        private static DocumentClient client;

        public static void Initialize()
        {            
            client = new DocumentClient(new Uri(EndPoint), AuthKey);
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            bool succeed = true;

            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    succeed = false;
                }
                else
                {
                    throw;
                }
            }

            if (!succeed)
            {
                await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            bool succeed = true;

            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {

                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    succeed = false;
                }
                else
                {
                    throw;
                }
            }

            //hack for await "in" catch. c# 6.0 supports await in catch statement
            if (!succeed)
            {
                await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 400 });
            }
        }

        public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId))
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public static async Task<Document> CreateItemAsync(T item)
        {
            Document documentCreated;

            try
            {
                documentCreated = await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
            }
            catch (Exception ex)
            {

                throw;
            }

            return documentCreated;
        }

    }
}
