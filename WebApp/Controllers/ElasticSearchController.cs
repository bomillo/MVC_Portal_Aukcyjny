using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using WebApp.Context;
using WebApp.Models.DTO;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers
{
    [AllowAnonymous]
    public class ElasticSearchController : Controller
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly PortalAukcyjnyContext _context;

        public ElasticSearchController(ElasticsearchClient elasticsearchClient, PortalAukcyjnyContext context )
        {
            this._elasticsearchClient = elasticsearchClient;
            _context = context;
        }

        public ActionResult GetSearchIdea(string text)
        {
            var response = 
                _elasticsearchClient
                .Search<ElasticAuction>(s => s
                    .Index("auctions")
                        .Query(q=> 
                            q.MultiMatch(mm => 
                                mm.Query(text)
                                    .Fuzziness(
                                        new Fuzziness(2))
                                        .Type(TextQueryType.BestFields)
                                        .Fields(new string[] { "title", "title._2gram", "title._3gram"})
                                        )
                            )
                    );

            return Json( new { Result = response.Hits.Take(5).Select(a => a.Source).ToList() });
        }
    }
}
