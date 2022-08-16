using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp111
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using SqlSugar;

    namespace ConsoleApp1
    {

        class SqlSugarHelper //不能是泛型类
        {
            public SqlSugarScope DB;

            public static SqlSugarScope DefaultSingleton { get; private set; }

            private string ConnString { get; set; }

            public static string GetDefaultConnString()
            {
                //透過Microsoft.Extensions.Configuration 取得appsettings字串
                IConfiguration config = new ConfigurationBuilder()
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                        .Build();
                var str = config.GetConnectionString("DefaultConnection");
                return str;
            }


            static SqlSugarHelper()
            {
                SqlSugarScope Db = new(new ConnectionConfig()
                {
                    ConnectionString = GetDefaultConnString(),
                    DbType = DbType.SqlServer,//数据库类型
                    IsAutoCloseConnection = true //不设成true要手动close
                },
                  db =>
                  {
                      db.Aop.OnLogExecuting = (sql, pars) =>
                      {
                          Console.WriteLine(sql);//输出sql,查看执行sql 性能无影响

                      };
                  });;
                //Console.WriteLine("SqlSugarHelper() db=", Db);
                DefaultSingleton = Db;
            }

            public SqlSugarHelper(string connStr)
            {
                InjectSetInstance(connStr);
            }
            //多库情况下使用说明：
            //如果是固定多库可以传 new SqlSugarScope(List<ConnectionConfig>,db=>{}) 文档：多租户
            //如果是不固定多库 可以看文档Saas分库


            public void InjectSetInstance(string s)
            {
                ConnString = s;
                SqlSugarScope Db = new SqlSugarScope(new ConnectionConfig()
                {
                    ConnectionString = s,
                    DbType = DbType.SqlServer,//数据库类型
                    IsAutoCloseConnection = true //不设成true要手动close
                },
                db =>
                {
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        Console.WriteLine(sql);//输出sql,查看执行sql 性能无影响

                    };
                });
                DB = Db;
            }
        }



        public class Student
        {
            // //指定主键和自增列，当然数据库中也要设置主键和自增列才会有效
            // [SugarColumn(IsPrimaryKey=true,IsIdentity =true)]
            public string SId { get; set; }
            public string SName { get; set; }
            public DateTime Sage { get; set; }
            public string Ssex { get; set; }
        }


        public class Teacher
        {
            public string TId { get; set; }
            public string Tname { get; set; }
        }

        public class SC
        {
            public string SId { get; set; }
            public string CId { get; set; }
            public decimal Score { get; set; }
        }


        public class Course
        {
            public string CId { get; set; }
            public string CName { get; set; }
            public string TId { get; set; }
        }
    }
}

