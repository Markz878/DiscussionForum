﻿using Microsoft.EntityFrameworkCore.Design;

namespace DiscussionForum.Core.DataAccess;
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseAzureSql("");
        return new AppDbContext(optionsBuilder.Options);
    }
}