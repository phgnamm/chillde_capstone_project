using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chillde.Repositories.Entities;
using Chillde.Services.Interfaces;
using Microsoft.Extensions.Options;
using Chillde.Services.Utils;
using Nest;
using Chillde.Services.Models.ResponseModels;

namespace Chillde.Services.Services
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly IElasticClient _client;
        private readonly KeywordGenerator _keywordGenerator;

        public ElasticsearchService(IElasticClient client)
        {
            _client = client;
            _keywordGenerator = new KeywordGenerator(); 
        }

        public async Task<ResponseModel> SuggestKeywordsAsync(string indexName, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ResponseModel { Data = null };
            }

            var searchResponse = await _client.SearchAsync<object>(s => s
                .Index(indexName)
                .Suggest(su => su
                    .Completion("keyword_suggestion", c => c
                        .Field("suggest")
                        .Prefix(query.ToLower())
                        .Size(10)
                    )
                )
            );

            if (searchResponse.Suggest == null || !searchResponse.Suggest.ContainsKey("keyword_suggestion"))
            {
                return new ResponseModel { Data = null };
            }

            var result = searchResponse.Suggest["keyword_suggestion"]
                .SelectMany(x => x.Options)
                .Select(o => o.Text)
                .Distinct()
                .ToList();

            return new ResponseModel { Data = result.Any() ? result : null };
        }


    }
}
