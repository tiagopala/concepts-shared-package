using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Concepts.Shared.Package.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Concepts.Shared.Package.Repositories
{
    /// <summary>
    ///     <para>A classe DynamoBaseRepository tem como objetivo auxiliar novas implementações que utilizam o DynamoDb.</para>
    ///     <para>Fornece métodos de persistência no dynamoDb para uma entidade genérica.</para>
    /// </summary>
    /// <remarks>
    ///     <para>Contém métodos para Adicionar, Obter, Atualizar e Excluir em uma tabela do DynamoDb.</para>
    /// </remarks>
    /// <typeparam name="T">Entidade a ser persistida. Deve ser a mesma utilizada para a criação e manipulação da tabela.</typeparam>
    /// <list type="number">
    ///     <item>
    ///         <term>SaveAsync</term>
    ///         <description>Criar ou atualizar uma entidade.</description>
    ///     </item>
    ///         <term>SaveAsyncWithSortKey</term>
    ///         <description>Criar ou atualizar uma entidade que possui uma sort/range key.</description>
    ///     <item>
    ///         <term>GetAsync</term>
    ///         <description>Obter uma entidade.</description>
    ///     </item>
    ///         <term>GetListAsyncByIndex</term>
    ///         <description>Obter um conjunto de entidades por meio de um index (índice).</description>
    ///     <item>
    ///         <term>GetListAsyncByIndexWithReservedKeyword</term>
    ///         <description>Obter um conjunto de entidades por meio de um index (índice) que utilizam uma palavra reservada.</description>
    ///     </item>
    /// </list>
    public class DynamoBaseRepository<T> : IDynamoBaseRepository<T>
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly string _tableName;

        /// <summary>
        /// Construtor padrão, deve receber um IAmazonDynamoDB (client), IDynamoDBContext (context) e o nome da tabela que será persistida.
        /// </summary>
        /// <param name="dynamoDbClient">IAmazonDynamoDb (client).</param>
        /// <param name="dynamoDbContext">IDynamoDBContext (context).</param>
        /// <param name="tableName">Nome da tabela a ser persistida.</param>
        public DynamoBaseRepository(
            IAmazonDynamoDB dynamoDbClient,
            IDynamoDBContext dynamoDbContext,
            string tableName)
        {
            _dynamoDbClient = dynamoDbClient;
            _dynamoDbContext = dynamoDbContext;
            _tableName = tableName;
        }

        /// <summary>
        /// Realiza a criação ou atualização de uma entidade caso ela não exista em sua tabela.
        /// </summary>
        /// <param name="entity">Entidade a ser persistida.</param>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <exception cref="Exception">Caso ocorra uma exceção ela será lançada.</exception>
        public async Task SaveAsync(T entity, string attributeName, string attributeValue)
        {
            try
            {
                var document = _dynamoDbContext.ToDocument(entity);

                var request = new UpdateItemRequest()
                {
                    TableName = _tableName,
                    Key = GetKey(attributeName, attributeValue),
                    AttributeUpdates = GetAttributeUpdates(document, attributeName)
                };

                UpdateItemResponse response = null;

                response = await _dynamoDbClient.UpdateItemAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Erros aconteceram durante a tentativa de salvar/atualizar a entidade.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Realiza a criação ou atualização, caso ela não exista, de uma entidade que possua uma chave primária composta por Hash e Range/Sorte key.
        /// </summary>
        /// <param name="entity">Entidade a ser persistida.</param>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <param name="sortKeyName">Nome do atributo utilizado como Range/Sort key da tabela.</param>
        /// <param name="sortKeyValue">Valor do atributo utilizado como Range/Sort key da tabela.</param>
        /// <exception cref="Exception">Caso ocorra uma exceção ela será lançada.</exception>
        public async Task SaveAsyncWithSortKey(T entity, string attributeName, string attributeValue, string sortKeyName, string sortKeyValue)
        {
            try
            {
                var document = _dynamoDbContext.ToDocument(entity);

                var request = new UpdateItemRequest()
                {
                    TableName = _tableName,
                    Key = GetKey(attributeName, attributeValue, sortKeyName, sortKeyValue),
                    AttributeUpdates = GetAttributeUpdates(document, attributeName, sortKeyName)
                };

                UpdateItemResponse response = null;

                response = await _dynamoDbClient.UpdateItemAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Erros aconteceram durante a tentativa de salvar/atualizar a entidade.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Obter uma entidade do tipo T.
        /// </summary>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <returns>Retorna uma entidade do tipo T.</returns>
        public async Task<T> GetAsync(string attributeName, string attributeValue)
        {
            try
            {
                var key = GetKey(attributeName, attributeValue);

                GetItemResponse response = null;

                response = await _dynamoDbClient.GetItemAsync(_tableName, key);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Erros aconteceram durante a tentativa de salvar/atualizar a entidade.");

                if (response.Item.Count.Equals(0))
                    return default;

                var document = Document.FromAttributeMap(response.Item);
                var data = _dynamoDbContext.FromDocument<T>(document);

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Obter uma lista de entidades do tipo T.
        /// </summary>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <param name="index">Index (índice) que será utilizado na busca.</param>
        /// <param name="expression">Expressão que se refere a busca a ser realizada.</param>
        /// <returns>Retorna uma lista de entidades do tipo T.</returns>
        public async Task<List<T>> GetListAsyncByIndex(string attributeName, string attributeValue, string index, string expression)
        {
            List<T> entitiesResponse = new List<T>();
            QueryResponse response = null;

            do
            {
                var request = new QueryRequest
                {
                    TableName = _tableName,
                    IndexName = index,
                    KeyConditionExpression = expression,
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {$":{attributeName}", new AttributeValue { S = attributeValue.ToString() }}
                    }
                };

                if (response != null)
                    request.ExclusiveStartKey = response.LastEvaluatedKey;

                response = await _dynamoDbClient.QueryAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Erros aconteceram durante a tentativa de obter as entidades.");

                if (response.Items.Count.Equals(0))
                    return default;

                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    var document = Document.FromAttributeMap(item);
                    var entity = _dynamoDbContext.FromDocument<T>(document);
                    entitiesResponse.Add(entity);
                }

            } while (response.LastEvaluatedKey.Count > 0);

            return entitiesResponse;
        }

        /// <summary>
        /// Obter uma lista de entidades do tipo T.
        /// </summary>
        /// <remarks>
        /// Deve ser utilizado quando o atributo para uma busca for uma palavra reservada do DynamoDb.
        /// </remarks>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <param name="index">Index (índice) que será utilizado na busca.</param>
        /// <param name="expression">Expressão que se refere a busca a ser realizada.</param>
        /// <returns>Retorna uma lista de entidades do tipo T.</returns>
        public async Task<List<T>> GetListAsyncByIndexWithReservedKeyword(string attributeName, string attributeValue, string index, string expression)
        {
            List<T> entitiesResponse = new List<T>();
            QueryResponse response = null;

            do
            {
                var request = new QueryRequest
                {
                    TableName = _tableName,
                    IndexName = index,
                    KeyConditionExpression = expression,
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        {$"#{attributeName}",$"{attributeName}"}
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {$":{attributeName}", new AttributeValue { S = attributeValue.ToString() }}
                    }
                };

                if (response != null)
                    request.ExclusiveStartKey = response.LastEvaluatedKey;

                response = await _dynamoDbClient.QueryAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception("Erros aconteceram durante a tentativa de obter as entidades.");

                if (response.Items.Count.Equals(0))
                    return default;

                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    var document = Document.FromAttributeMap(item);
                    var entity = _dynamoDbContext.FromDocument<T>(document);
                    entitiesResponse.Add(entity);
                }

            } while (response.LastEvaluatedKey.Count > 0);

            return entitiesResponse;
        }

        protected Dictionary<string, AttributeValue> GetKey(string attributeName = "Id", string attributeValue = null)
        {
            return new Dictionary<string, AttributeValue> { { attributeName, new AttributeValue { S = attributeValue } } };
        }

        protected Dictionary<string, AttributeValue> GetKey(string attributeName = "Id", string attributeValue = null, string sortKeyName = null, string sortKeyValue = null)
        {
            return new Dictionary<string, AttributeValue>
            {
                { attributeName, new AttributeValue { S = attributeValue } },
                { sortKeyName, new AttributeValue { S = sortKeyValue } }
            };
        }

        protected Dictionary<string, AttributeValueUpdate> GetAttributeUpdates(Document document, string keyName = "Id")
        {
            var attributeUpdates = document.ToAttributeUpdateMap(false);
            attributeUpdates.Remove(keyName);
            return attributeUpdates;
        }

        protected Dictionary<string, AttributeValueUpdate> GetAttributeUpdates(Document document, string keyName = "Id", string sortName = "")
        {
            var attributeUpdates = document.ToAttributeUpdateMap(false);
            attributeUpdates.Remove(keyName);
            attributeUpdates.Remove(sortName);
            return attributeUpdates;
        }
    }
}
