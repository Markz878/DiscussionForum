﻿using DiscussionForum.Core.DataAccess;
using DiscussionForum.Core.DataAccess.Models;
using DiscussionForum.Core.HelperMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

List<Topic> topics = Fakers.GetTopics(50000, 50000);
IConfiguration configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

AppDbContext db = new(new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(configuration.GetConnectionString("SqlServer"))
    .LogTo(Console.WriteLine, LogLevel.Warning).Options);
Console.WriteLine("Delete and create database (y)?");
string? response = Console.ReadLine();
if (response?.StartsWith('y') == true)
{
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    db.Users.Add(Fakers.Admin);
    db.Users.Add(Fakers.User);
    db.SaveChanges();
}
db.Topics.AddRange(topics);
db.SaveChanges();