using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Concepts.Shared.Package.Repositories.Interfaces
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
    public interface IDynamoBaseRepository<T>
    {
        /// <summary>
        /// Realiza a criação ou atualização de uma entidade caso ela não exista em sua tabela.
        /// </summary>
        /// <param name="entity">Entidade a ser persistida.</param>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <exception cref="Exception">Caso ocorra uma exceção ela será lançada.</exception>
        Task SaveAsync(T entity, string attributeName, string attributeValue);

        /// <summary>
        /// Realiza a criação ou atualização, caso ela não exista, de uma entidade que possua uma chave primária composta por Hash e Range/Sorte key.
        /// </summary>
        /// <param name="entity">Entidade a ser persistida.</param>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <param name="sortKeyName">Nome do atributo utilizado como Range/Sort key da tabela.</param>
        /// <param name="sortKeyValue">Valor do atributo utilizado como Range/Sort key da tabela.</param>
        /// <exception cref="Exception">Caso ocorra uma exceção ela será lançada.</exception>
        Task SaveAsyncWithSortKey(T entity, string attributeName, string attributeValue, string sortKeyName, string sortKeyValue);

        /// <summary>
        /// Obter uma entidade do tipo T.
        /// </summary>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <returns>Retorna uma entidade do tipo T.</returns>
        Task<T> GetAsync(string attributeName, string attributeValue);

        /// <summary>
        /// Obter uma lista de entidades do tipo T.
        /// </summary>
        /// <param name="attributeName">Nome do atributo utilizado como Hash key da tabela.</param>
        /// <param name="attributeValue">Valor do atributo utilizado como Hash key da tabela.</param>
        /// <param name="index">Index (índice) que será utilizado na busca.</param>
        /// <param name="expression">Expressão que se refere a busca a ser realizada.</param>
        /// <returns>Retorna uma lista de entidades do tipo T.</returns>
        Task<List<T>> GetListAsyncByIndex(string attributeName, string attributeValue, string index, string expression);

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
        Task<List<T>> GetListAsyncByIndexWithReservedKeyword(string attributeName, string attributeValue, string index, string expression);
    }
}
