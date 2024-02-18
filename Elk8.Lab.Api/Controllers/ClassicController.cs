using System;
using Microsoft.AspNetCore.Mvc;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Elastic.Lab.Classic.Infra.Settings;
using Microsoft.Extensions.Configuration;

namespace Elastic.Lab.Classic.Controllers
{
  [ApiController]
  [Route("[controller]/[action]")]
  public class ClassicController : ControllerBase
  {
    readonly IConfiguration _config;
    readonly ElasticsearchClient _client;
    readonly string _defaultIndex = "my_index";

    public ClassicController(ILogger<ClassicController> logger, IConfiguration config)
    {
      _config = config;

      var elsAuth = _config.GetSection("Elastic").Get<ElsAuth>();


      var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"));
        //.Authentication( new BasicAuthentication( elsAuth.UserName, elsAuth.Password ));

      _client = new ElasticsearchClient(settings);

      //_client.Indices.CreateAsync(_defaultIndex);




    }


    //[HttpPost]
    //public async Task<IActionResult> CreateIndex()
    //{
    //  var newIndexResponse = await _client.Indices.CreateAsync(_defaultIndex);

    //  return Ok(newIndexResponse);

    //}

    [HttpPost]
    public async Task<IActionResult> PostDoc(MyDoc input)
    {
      var doc = new MyDoc
      {
        Id = input.Id,
        User = input.User,
        Message = input.Message
      };

      var createResponse = await _client.IndexAsync(doc, _defaultIndex);

      return Ok(createResponse);

    }

    [HttpPut]
    public async Task<IActionResult> SeedExampes()
    {
      var id = 1;
      var docList = new List<MyDoc>()
      {
        new MyDoc
        {
          Id = id,
          User = "Derg",
          Message = "One two three"
        },
        new MyDoc
        {
          Id = ++id,
          User = "Judt",
          Message = "Two three four"
        },
        new MyDoc
        {
          Id = ++id,
          User = "Zuma",
          Message = "Three two one"
        },
        new MyDoc
        {
          Id = ++id,
          User = "Jumn",
          Message = "three"
        },
      };
      
      foreach (var doc in docList)
      {
        var res = _client.IndexAsync(doc, _defaultIndex);
        if (!res.IsCompletedSuccessfully)
        {
          return BadRequest(res);
        }

      }
      return Ok("Seeded " + id + "default items.");

    }

    [HttpGet]
    public async Task<IActionResult> GetDocById(int id)
    {
      var response = await _client.GetAsync<MyDoc>(id, idx => idx.Index(_defaultIndex));


      if (response.IsValidResponse)
      {
        var doc = response.Source;
        return Ok(doc);
      }
      return BadRequest();

    }

    [HttpGet]
    public async Task<IActionResult> SearchByTerm(string message)
    {
      var response = await _client.SearchAsync<MyDoc>(s => s
          .Index(_defaultIndex)
          .From(0)
          .Size(10)
          .Query(q => q
              .Term(t => t.Message, message)
          )
      );

      if (response.IsValidResponse)
      {
        var docs = response.Documents.ToList<MyDoc>;
        return Ok(docs);
      }
      return BadRequest();


    }

  }

  public class MyDoc
  {
    public int Id { get; set; }
    public string User { get; set; }
    public string Message { get; set; }

  }
}
