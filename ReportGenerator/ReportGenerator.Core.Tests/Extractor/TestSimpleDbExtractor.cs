﻿using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using ReportGenerator.Core.Data;
using ReportGenerator.Core.Data.Parameters;
using ReportGenerator.Core.Extractor;
using ReportGenerator.Core.Tests.TestUtils;
using Xunit;

namespace ReportGenerator.Core.Tests.Extractor
{
    // todo: check strings in future
    public class TestSimpleDbExtractor
    {
        [Fact]
        public void TestExctractFromStoredProcNoParams()
        {
            SetUpTestData();
            // testing is here
            IDbExtractor extractor = new SimpleDbExtractor(Server, TestDatabase);
            Task<DbData> result = extractor.ExtractAsync(TestStoredProcedureWithoutParams, new List<StoredProcedureParameter>());
            result.Wait();
            DbData rows = result.Result;
            const int expectedNumberOfRows = 15;
            Assert.Equal(expectedNumberOfRows, rows.Rows.Count);
            TearDownTestData();
        }

        [Theory]
        [InlineData("г. Екатеринбург", 5)]
        [InlineData("г. Нижний Тагил", 3)]
        [InlineData("г. Первоуральск", 3)]
        [InlineData("г. Челябинск", 4)]
        public void TestExctractFromStoredProcWithCityParam(string parameterValue, int expectedNumberOfRows)
        {
            SetUpTestData();
            // testing is here
            IDbExtractor extractor = new SimpleDbExtractor(Server, TestDatabase);
            Task<DbData> result = extractor.ExtractAsync(TestStoredProcedureWithCity, 
                                                         new List<StoredProcedureParameter>{ new StoredProcedureParameter(SqlDbType.NVarChar, "City", parameterValue) });
            result.Wait();
            DbData rows = result.Result;
            Assert.Equal(expectedNumberOfRows, rows.Rows.Count);
            TearDownTestData();
        }

        [Theory]
        [InlineData("г. Екатеринбург", 33, 2)]
        [InlineData("г. Нижний Тагил", 40, 1)]
        [InlineData("г. Первоуральск", 31, 1)]
        [InlineData("г. Челябинск", 15, 3)]
        public void TestExctractFromStoredProcWithCityAndAgeParams(string cityParameterValue, int ageParameterValue, int expectedNumberOfRows)
        {
            SetUpTestData();
            // testing is here
            IDbExtractor extractor = new SimpleDbExtractor(Server, TestDatabase);
            Task<DbData> result = extractor.ExtractAsync(TestStoredprocedureWithCityAndAge,  new List<StoredProcedureParameter>
            {
                new StoredProcedureParameter(SqlDbType.NVarChar, "City", cityParameterValue),
                new StoredProcedureParameter(SqlDbType.Int, "PersonAge", ageParameterValue)
            });
            result.Wait();
            DbData rows = result.Result;
            Assert.Equal(expectedNumberOfRows, rows.Rows.Count);
            TearDownTestData();
        }

        [Fact]
        public void TestExtractFromView()
        {
            SetUpTestData();
            // testing is here
            IDbExtractor extractor = new SimpleDbExtractor(Server, TestDatabase);
            Task<DbData> result = extractor.ExtractAsync(TestView, new ViewParameters());
            result.Wait();
            DbData rows = result.Result;
            int expectedNumberOfRows = 15;
            Assert.Equal(expectedNumberOfRows, rows.Rows.Count);
            TearDownTestData();
        }

        [Theory]
        [InlineData("N'Алексей'", null, 1)]
        [InlineData("N'Алексей'", true, 1)]
        [InlineData("N'Алексей'", false, 0)]
        [InlineData(null, true, 7)]
        [InlineData(null, false, 8)]
        public void TestExtractFromViewWithParams(string name, bool? sex, int expectedNumberOfRows)
        {
            SetUpTestData();
            // testing is here
            ViewParameters parameters = new ViewParameters();
            if (!string.IsNullOrEmpty(name))
            {
                parameters.WhereParameters.Add(new DbQueryParameter(null, "FirstName", "=", name));
            }
            if (sex.HasValue)
            {
                IList<JoinCondition> sexJoin = parameters.WhereParameters.Count > 0 ? new List<JoinCondition>() {JoinCondition.And}  : null;
                parameters.WhereParameters.Add(new DbQueryParameter(sexJoin, "Sex", "=", sex.Value ? "1" : "0"));
            }
            IDbExtractor extractor = new SimpleDbExtractor(Server, TestDatabase);
            Task<DbData> result = extractor.ExtractAsync(TestView, parameters);
            result.Wait();
            DbData rows = result.Result;
            Assert.Equal(expectedNumberOfRows, rows.Rows.Count);
            TearDownTestData();
        }

        private void SetUpTestData()
        {
            TestDatabaseManager.CreateDatabase(Server, TestDatabase, true);
            string createDatabaseStatement = File.ReadAllText(Path.GetFullPath(CreateDatabaseScript));
            string insertDataStatement = File.ReadAllText(Path.GetFullPath(InsertDataScript));
            TestDatabaseManager.ExecuteSql(Server, TestDatabase, createDatabaseStatement);
            TestDatabaseManager.ExecuteSql(Server, TestDatabase, insertDataStatement);
        }

        private void TearDownTestData()
        {
            TestDatabaseManager.DropDatabase(Server, TestDatabase);
        }

        private const string Server = @"(localdb)\mssqllocaldb";
        private const string TestDatabase = "ReportGeneratorTestDb";

        private const string CreateDatabaseScript = @"..\..\..\DbScripts\CreateDb.sql";
        private const string InsertDataScript = @"..\..\..\DbScripts\CreateData.sql";

        private const string TestStoredProcedureWithoutParams = "SelectCitizensWithCities";
        private const string TestStoredProcedureWithCity = "SelectCitizensWithCitiesByCity";
        private const string TestStoredprocedureWithCityAndAge = "SelectCitizensWithCitiesByCityAndAge";

        private const string TestView = "CitizensWithRegion";
    }
}