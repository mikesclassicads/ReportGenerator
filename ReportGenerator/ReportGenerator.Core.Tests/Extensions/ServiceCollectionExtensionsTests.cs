using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReportGenerator.Core.Database;
using ReportGenerator.Core.Database.Managers;
using ReportGenerator.Core.Database.Utils;
using ReportGenerator.Core.Extensions;
using ReportGenerator.Core.ReportsGenerator;
using Xunit;

namespace ReportGenerator.Core.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests : IDisposable
    {
        public ServiceCollectionExtensionsTests()
        {
            _testDbName = TestDatabasePattern + "_" + DateTime.Now.ToString("YYYYMMDDHHmmss");
            _dbManager = new CommonDbManager(DbEngine.SqlServer, _loggerFactory.CreateLogger<CommonDbManager>());
            IDictionary<string, string> connectionStringParams = new Dictionary<string, string>()
            {
                {DbParametersKeys.HostKey, Server},
                {DbParametersKeys.DatabaseKey, _testDbName},
                {DbParametersKeys.UseIntegratedSecurityKey, "true"},
                {DbParametersKeys.UseTrustedConnectionKey, "true"}
            };
            _connectionString = ConnectionStringBuilder.Build(DbEngine.SqlServer, connectionStringParams);
            _dbManager.CreateDatabase(_connectionString, true);
            _services = new ServiceCollection();
            _services.AddScoped<ILoggerFactory>(_ => new LoggerFactory());
            _services.AddReportGenerator(DbEngine.SqlServer, _connectionString);
        }

        public void Dispose()
        {
            _dbManager.DropDatabase(_connectionString);
        }

        [Fact]
        public void TestServiceInstantiationViaProvider()
        {
            IServiceProvider serviceProvider = _services.BuildServiceProvider();
            IReportGeneratorManager reportGenerator = serviceProvider.GetService<IReportGeneratorManager>();
            
            Assert.NotNull(reportGenerator);
        }

        private const string Server = @"(localdb)\mssqllocaldb";
        private const string TestDatabasePattern = "ReportGeneratorTestDb";
        
        private readonly IServiceCollection _services;
        private readonly string _testDbName;
        private readonly string _connectionString;
        private IDbManager _dbManager;
        private readonly ILoggerFactory _loggerFactory = new LoggerFactory();
    }
}