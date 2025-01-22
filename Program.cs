// See https://aka.ms/new-console-template for more information
using EF9Bug;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var builder = Host.CreateApplicationBuilder();
builder.Services.AddDbContext<TestContext>(a =>
    a.UseSqlServer(builder.Configuration.GetConnectionString(nameof(TestContext)))
);

var host = builder.Build();

using var scope = host.Services.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<TestContext>();

context.Database.EnsureDeleted();
context.Database.EnsureCreated();

var source = context
    .Set<Item>()
    .Select(a => new OptimisedModel
    {
        OwnerId = a.OwnerId,
        OwnerType = a.OwnerType,
        //Category =
        //    a.DoubleNested.Nested.Category != null ? a.DoubleNested.Nested.Category.Name : null,
        //Optional = a.Optional != null ? new OptionalOneOptimised { Name = a.Optional.Name } : null,
        //Optional2 =
        //    a.Optional2 != null ? new OptionalTwoOptimised { Name = a.Optional2.Name } : null,
        //Details = a.Details.Select(a => new DetailOptimised
        //{
        //    Name = a.It.Type,
        //    Count = a.It.Count
        //})
    });

var min = "000";
var max = "999";
var type = "Test";

var sourceType = source.Where(a => ((IOwnedBy)a).OwnerType == type);

var range = sourceType.Where(a => a.OwnerId.CompareTo(min) > 0 && a.OwnerId.CompareTo(max) < 0);

var rangeIds = range.Select(a => a.OwnerId).Distinct().OrderBy(a => a).Take(100);

var bug = sourceType.Where(a => rangeIds.Contains(((IOwnedBy)a).OwnerId));
var array = bug.ToArray();

Console.ReadLine();
