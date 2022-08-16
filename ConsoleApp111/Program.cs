using ConsoleApp111.ConsoleApp1;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace ConsoleApp111
{
    internal class Program
    {
        public class SubObj
        {
            public string ScSId { get; set; }
            public int Cnt { get; set; }
            public decimal SumScore { get; set; }
        }
        static void Main(string[] args)
        {

            var DB = SqlSugarHelper.DefaultSingleton;

            var query_1 = DB
                        .Queryable(DB.Queryable<SC>().Where(sc => sc.CId == "01"),
                                   DB.Queryable<SC>().Where(sc => sc.CId == "02"),
                                   (sc1, sc2) => sc1.SId == sc2.SId)
                        .Where((sc1, sc2) => sc1.SId == sc2.SId)
                        .Where((sc1, sc2) => sc1.Score > sc2.Score)
                        .Select((sc1, sc2) => new { SId = sc1.CId, Score = sc1.Score })
                        .InnerJoin<Student>((sc1, st) => sc1.SId == st.SId)
                        .Select((sc1, st) => new { SId = sc1.SId, Score = sc1.Score, Name = st.SName })
                        .ToList();

            for (int i = 0; i < query_1.Count; i++)
            {
                Console.WriteLine($"q3 sid = {query_1[i].SId}, q3 score = {query_1[i].Score}");
            }


            Console.WriteLine("\n-----2 .查詢平均成績大於60分的學生的學號和平均成績");
            var query_2 = DB.Queryable<SC>()
                        .GroupBy(sc => sc.SId)
                        .Having(sc => SqlFunc.AggregateAvg(sc.Score) > 60)
                        .Select(sc => new { StudenctID = sc.SId, AvgScore = SqlFunc.AggregateAvg(sc.Score), })
                        .ToList();

            for (int i = 0; i < query_2.Count; i++)
            {
                Console.WriteLine($"query_2 sid = {query_2[i].StudenctID}, query_2 AvgScore = {query_2[i].AvgScore}");
            }

            Console.WriteLine("\n-----3.查詢所有學生的學號、姓名、選課數、總成績");
            //SqlFunc.AggregateCount

            var sub6 = DB.Queryable<SC>()
                        .GroupBy(sc => sc.SId)
                        .Select(sc => new
                        {
                            ScSId = sc.SId,
                            Cnt = SqlFunc.AggregateCount(sc),
                            SumScore = SqlFunc.AggregateSum(sc.Score),
                        });

            var query_3 = DB.Queryable<Student>()
                            .LeftJoin(sub6, (st, sc) => sc.ScSId == st.SId)
                            .Select((st, sc) => new
                            {
                                st.SName,
                                sc.ScSId,
                                sc.Cnt,
                                sc.SumScore,
                            })
                            .ToList();

            for (int i = 0; i < query_3.Count; i++)
            {
                Console.WriteLine($"query_3 sName = {query_3[i].SName},  ScSId = {query_3[i].ScSId},  Cnt = {query_3[i]},  SumScore = {query_3[i].SumScore}");
            }

            //https://www.twblogs.net/a/5d0862babd9eee1e5c8112ff
            // 未解決
            Console.WriteLine("\n-----4.查詢姓「猴」的老師的個數");
            var query_4 = DB.Queryable<Teacher>()
                             .Where(t => t.Tname.Contains("猴"))
                             .Count();

            Console.WriteLine($"query_4 = {query_4}");


            // 還不會用一句查...
            Console.WriteLine("\n-----5.查詢沒學過「張三」老師課的學生的學號、姓名");

            var query_x = DB.Queryable<SC>()
                            .InnerJoin<Course>((sc, c) => sc.CId == c.CId)
                            .InnerJoin<Teacher>((sc, c, t) => c.TId == t.TId)
                            .Where((sc, c, t) => t.Tname == "張三")
                            .ToList();
            Console.WriteLine();

            var sidArr = new List<string>();

            for (int i = 0; i < query_x.Count; i++)
            {
                //Console.WriteLine($"{query_x[i].SId}");
                sidArr.Add(query_x[i].SId);
            }

            var query_5 = DB.Queryable<Student>()
                            .Where(it => !sidArr.Contains(it.SId))
                            .ToList();

            for (int i = 0; i < query_5.Count; i++)
            {
                Console.WriteLine($"query_5 Sid= {query_5[i].SId}, Sname ={query_5[i].SName}");
            }



            var query_6 = DB.Queryable<SC, Course, Teacher, Student>((sc, cs, t, st) => sc.CId == cs.CId && cs.TId == t.TId && sc.SId == st.SId)
                            .Where((sc, cs, t, st) => t.Tname == "張三")
                            .Select((sc, cs, t, st) => new { SId = sc.SId, SName = st.SName })
                            .ToList();

            for (int i = 0; i < query_6.Count; i++)
            {
                Console.WriteLine($"query_6 SId= {query_6[i].SId}, {query_6[i].SName}");
            }


            var query_7 = DB
                        .Queryable(DB.Queryable<SC>().Where(sc => sc.CId == "01"),
                                    DB.Queryable<SC>().Where(sc => sc.CId == "02"),
                                    (sc1, sc2) => sc1.SId == sc2.SId)
                        .Where((sc1, sc2) => sc1.SId == sc2.SId)
                        .Select((sc1, sc2) => new { SId = sc1.CId, Score = sc1.Score })
                        .RightJoin<Student>((sc1, st) => sc1.SId == st.SId)
                        .Select((sc1, st) => new { SId = sc1.SId, Score = sc1.Score, Name = st.SName })
                        .ToList();

            for (int i = 0; i < query_7.Count; i++)
            {
                Console.WriteLine($"query_7 SId= {query_7[i].SId}, {query_7[i].Name}");
            }

            var query_8 = DB
                         .Queryable<SC>()
                         .Where(x => x.CId == "02")
                         .Sum(x => x.Score);

            Console.WriteLine($"query_8={query_8}");

            var query_9 = DB
                        .Queryable<SC, Student>((sc, st) => sc.SId == st.SId)
                        .Where((sc, st) => sc.Score < 60)
                        .Select((sc, st) => new { sc.SId, st.SName, sc.Score })
                        .ToList();

            for (int i = 0; i < query_9.Count; i++)
            {
                Console.WriteLine($"query_9 SId= {query_9[i].SId}, {query_9[i].SName}, {query_9[i].Score}");
            }


            //var query_10_sub = DB
            //                   .Queryable<SC>()
            //                   .GroupBy(sc => sc.SId)
            //                   .Having(sc => SqlFunc.AggregateCount(sc.CId) != 
            //                                    DB.Queryable<Course>()
            //                                    .Select(sc => new { Cnt = SqlFunc.AggregateCount(sc.CId) })
            //                                    .First().Cnt);
            var cnt = DB.Queryable<Course>()
                        .Select(sc => new { Cnt = SqlFunc.AggregateCount(sc.CId) })
                        .First().Cnt;

            var query_10 = DB
                          .Queryable(
                                DB.Queryable<Student>(),
                                DB.Queryable<SC>()
                                   .GroupBy(sc => new { sc.SId })
                                   .Having(sc => SqlFunc.AggregateCount(sc.CId) != cnt)
                                   .Select<object>(sc => new {
                                       sc.SId
                                    }), 
                                 JoinType.Inner,
                          (st, sub) => st.SId == SqlFunc.MappingColumn(default(string), "sub.SId"))
                          .Select((st, sub) => new { st.SId, st.SName })
                          .ToList();

            for (int i = 0; i < query_10.Count; i++)
            {
                Console.WriteLine($"query_10 SId= {query_10[i].SId}, {query_10[i].SName}");
            }



            var q12sub = DB.Queryable<SC>()
                            .Where(sc => sc.SId == "01")
                            .Select(sc => sc.SId)
                            .Count();
            Console.WriteLine($"q12sub= {q12sub}");
            // 不知為啥要抓出數量 不能放在同個query中
            var query_12 = DB
                         .Queryable<SC>()
                         .Where(sc => sc.SId != "01")
                         .GroupBy(sc => sc.SId)
                         .Having(sc => SqlFunc.AggregateCount(sc.CId) == q12sub)
                         .Select(sc => sc.SId)
                         .ToList();

            for (int i = 0; i < query_12.Count; i++)
            {
                Console.WriteLine($"query_12 SId= {query_12[i]}");
            }

            // 不會NOT IN 多張表的寫法...
            var sidArr_13 = new List<string>();
            var query_sub_13 = DB.Queryable<SC, Course, Teacher>((sc, c, t) => sc.CId == c.CId && c.TId == t.TId && t.Tname == "張三")
                                 .Select(sc => new { sc.SId })
                                 .ToList();

            for (int i = 0; i < query_sub_13.Count; i++)
            {
                //Console.WriteLine($"{query_x[i].SId}");
                sidArr_13.Add(query_sub_13[i].SId);
            }


            var query_13 = DB.Queryable<Student>()
                            .Where(st => !sidArr_13.Contains(st.SId))
                            .Select(st => new { st.SName})
                            .ToList();


            for (int i = 0; i < query_13.Count; i++)
            {
                Console.WriteLine($"query_13 StName= {query_13[i].SName}");
            }

            // 14 按平均成績從高到低顯示所有學生的所有課程的成績以及平均成績
            var query_14 = DB
                       .Queryable<SC, Student>((sc, st) => sc.SId == st.SId)
                       .GroupBy((sc, st) => new { sc.SId , st.SName })
                       .Select((sc, st) => new { sc.SId, Avg = SqlFunc.AggregateAvg(sc.Score), st.SName })
                       .ToList();

            for (int i = 0; i < query_14.Count; i++)
            {
                Console.WriteLine($"query_14 SId= {query_14[i].SId },  StName= {query_14[i].SName} , AVG ={query_14[i].Avg }");
            }

            var query_15 = DB
                          .Queryable<SC, Student>((sc, st) => sc.SId == st.SId)
                          .Where((sc, st) => sc.Score < 60)
                          .GroupBy((sc, st) => new { sc.SId, st.SName })
                          .Having((sc, st) => SqlFunc.AggregateCount(sc.CId) >= 2)
                          .Select((sc, st) => new {sc.SId, st.SName, avgScore = SqlFunc.AggregateAvg(sc.Score) })
                          .ToList();

            for (int i = 0; i < query_15.Count; i++)
            {
                Console.WriteLine($"query_15 SId= {query_15[i].SId},  StName= {query_15[i].SName} , AVG ={query_15[i].avgScore}");
            }

            var query_16 = DB
                          .Queryable<SC, Student>((sc, st) => sc.SId == st.SId
                                                            && sc.CId == "01"
                                                            && sc.Score < 60)
                          .OrderBy((sc, st) => sc.Score, OrderByType.Desc)
                          .Select((sc, st) => new { st.SId, st.SName, st.Ssex, st.Sage})
                          .ToList();

            for (int i = 0; i < query_16.Count; i++)
            {
                Console.WriteLine($"query_16 SId= {query_16[i].SId}, " +
                                  $"StName= {query_16[i].SName}, " +
                                  $"Ssex= {query_16[i].Ssex}" +
                                  $"Sage= {query_16[i].Sage}");
            }


            var query_17 = DB.Queryable(
                                DB.Queryable<SC>(),
                                DB.Queryable<SC, Course>((sc, c) => sc.CId == c.CId)
                                   .GroupBy((sc, c) => sc.SId)
                                   .Select<object>((sc, c) => new { sc.SId, average = SqlFunc.AggregateAvg(sc.Score)})
                                ,　
                                JoinType.Inner,
                                (sc, sub) => sc.SId == SqlFunc.MappingColumn(default(string), "sub.SId"))
                                .Select((sc, sub) => new { sc.SId, sc.CId, average = SqlFunc.MappingColumn(default(decimal), "sub.average") })
                                .ToList();

            for (int i = 0; i < query_17.Count; i++)
            {
                Console.WriteLine($"query_17 SId= {query_17[i].SId}, " +
                                  $"CId= {query_17[i].CId}, " +
                                  $"average= {query_17[i].average}");
            }


            var query_18 = DB.Queryable<SC, Course>((sc, c) => sc.CId == c.CId)
                             .GroupBy((sc, c) => new { sc.CId, c.CName })
                             .Select((sc, c) => new
                             {
                                 max = SqlFunc.AggregateMax(sc.Score),
                                 min = SqlFunc.AggregateMin(sc.Score),
                                 avg = SqlFunc.AggregateAvg(sc.Score),
                                 c.CName,
                             })
                             .ToList();
            //            SqlSugar.SqlSugarException:“中文提示: 多表查询存在别名不一致,请把OrderBy中的x改成sc就可以了，特殊需求可以使用.Select((x, y) => new { id = x.id, name = y.name }).MergeTable().Orderby(xxx => xxx.Id)功能将Select中的多表结果集变成单表，这样就可以不限制别名一样
            //English Message: Join sc needs to be the same as OrderBy x”
            var query_19 = DB.Queryable<SC, Student>((sc, st) => sc.SId == st.SId)
                             .GroupBy((sc, st) => new { sc.SId, st.SName })
                             .Select((sc, st) => new { sc.SId, st.SName, 
                                                        sumScore = SqlFunc.AggregateSum(sc.Score),  
                                                         No = SqlFunc.RowNumber(SqlFunc.Desc(sc.CId))})
                             .MergeTable().OrderBy(x => x.sumScore);

            var query_20 = DB.Queryable<SC, Teacher, Course>((sc, t, c) => sc.CId == c.CId && c.TId == t.TId)
                             .GroupBy((sc, t, c) => new { sc.CId, t.Tname, c.CName })
                             .Select((sc, t, c) => new { sc.CId, t.Tname, avgScore = SqlFunc.AggregateAvg(sc.Score) })
                             .MergeTable().OrderBy(x => x.avgScore)
                             .ToList();

            var query_21 = DB.Queryable<Student, SC>((st, sc) => st.SId == sc.SId)
                             .GroupBy((st, sc) => new { sc.SId, st.SName })
                             .Select((st, sc) => new { sc.SId, st.SName, avgScore = SqlFunc.AggregateAvg(sc.Score), No = SqlFunc.RowNumber(SqlFunc.Desc(SqlFunc.AggregateAvg(sc.Score))) })
                             .MergeTable().OrderBy(x => x.avgScore, OrderByType.Desc)
                             .ToList();

            // 這題不確定 sqlsuger有無支援內建dense rank方法 先用這種做法
            var query_22 = DB.Queryable<SC>()
                             .Select(sc => new { sc.CId, sc.SId, sc.Score, Rank = SqlFunc.MappingColumn(default(decimal), "dense_rank() over (partition by CId order by Score desc)") })
                             .ToList();

            for (int i = 0; i < query_22.Count; i++)
            {
                Console.WriteLine($"query_22 CId= {query_22[i].CId}, " +
                                  $"SId= {query_22[i].SId}, " +
                                  $"Score= {query_22[i].Score}, " +
                                  $"Rank= {query_22[i].Rank}");
            }


            var query_27 = DB.Queryable<SC>()
                             .GroupBy(sc => sc.CId)
                             .Select(sc => new { sc.CId, StNum = SqlFunc.AggregateCount(sc.SId) })
                             .ToList();

            for (int i = 0; i < query_27.Count; i++)
            {
                Console.WriteLine($"query_17 SId= {query_27[i].CId}," +
                                  $"StNum= {query_27[i].StNum}" );
            }

            var query_28 = DB.Queryable<Student, SC>((st, sc) => st.SId == sc.SId)
                             .GroupBy((st, sc) => new { st.SId, st.SName })
                             .Having((st, sc) => SqlFunc.AggregateCount(sc.CId) == 2)
                             .Select((st,sc) => new { st.SId, st.SName})
                             .ToList();

            var query_29 = DB.Queryable<Student>()
                             .GroupBy(st => st.Ssex)
                             .Select(st => new { st.Ssex, Cnt = SqlFunc.AggregateCount(st.Ssex)} )
                             .ToList();

            var query_30 = DB.Queryable<Student>()
                             .Where(st => st.SName.Contains("風"))
                             .ToList();
            //var 
            var query_33 = DB
                           .Queryable<SC>()
                           .GroupBy(sc => new { sc.SId, sc.CId })
                           .Select(sc => new { sc.SId, sc.CId, AvgScore = SqlFunc.AggregateAvg(sc.Score) })
                           .OrderBy(x => x.AvgScore, OrderByType.Asc).OrderBy(x => x.CId, OrderByType.Desc)
                           .ToList();

            for (int i = 0; i < query_33.Count; i++)
            {
                Console.WriteLine($"query_33 SId= {query_33[i].SId}, AvgScore = {query_33[i].AvgScore}");
            }

            var query_34 = DB
                           .Queryable<Student, SC, Course>((st, sc, c) => st.SId == sc.SId && sc.CId == c.CId)
                           .Where((st, sc, c) => c.CName == "數學" && sc.Score < 60)
                           .Select((st, sc, c) => new {st.SName, sc.Score })
                           .ToList();

            for (int i = 0; i < query_34.Count; i++)
            {
                Console.WriteLine($"query_34 SName= {query_34[i].SName}, Score = {query_34[i].Score}");
            }

            var query_35 = DB
               .Queryable<Student, SC, Course>((st, sc, c) => st.SId == sc.SId && sc.CId == c.CId)
               .Select((st, sc, c) => new { st.SName, sc.Score })
               .ToList();

            for (int i = 0; i < query_35.Count; i++)
            {
                Console.WriteLine($"query_35 SName= {query_35[i].SName}, Score = {query_35[i].Score}");
            }

            var query_36 = DB
                           .Queryable<Student, SC, Course>((st, sc, c) => st.SId == sc.SId && sc.CId == c.CId)
                           .Where((st, sc, c) => sc.Score > 70)
                           .Select((st, sc, c) => new { st.SName, c.CName, sc.Score })
                           .ToList();

            for (int i = 0; i < query_36.Count; i++)
            {
                Console.WriteLine($"query_36 SName= {query_36[i].SName}, CName = {query_36[i].CName},  Score = {query_36[i].Score}");
            }

            Console.Write("37、查詢不及格的課程並按課程號從大到小排列");
            var query_37 = DB
                           .Queryable<SC>()
                           .GroupBy(sc => sc.CId)
                           .Having(sc => SqlFunc.AggregateAvg(sc.Score) > 60)
                           .OrderBy(sc => sc.CId, OrderByType.Desc)
                           .Select(sc => new { sc.CId })
                           .ToList();

            for (int i = 0; i < query_37.Count; i++)
            {
                Console.WriteLine($"query_37 CId= {query_37[i].CId}");
            }

            Console.Write("38、查詢課程編號為03且課程成績在80分以上的學生的學號和姓名");
            
            var query_38 = DB
               .Queryable<Student, SC>((st, sc) => st.SId == sc.SId && sc.CId == "03" && sc.Score >= 80)
               .Select((st, sc) => new { sc.CId, st.SId, st.SName, sc.Score })
               .ToList();

            for (int i = 0; i < query_38.Count; i++)
            {
                Console.WriteLine($"query_38 CId= {query_38[i].CId}, SId = {query_38[i].SId},  SName = {query_38[i].SName} SCore = {query_38[i].Score}");
            }

            Console.Write("39、求每門課程的學生人數");
            var query_39 = DB
                           .Queryable<SC>()
                           .GroupBy(sc => sc.CId)
                           .Select(sc => new { sc.CId , Count = SqlFunc.AggregateCount(sc.SId)})
                           .ToList();

            for (int i = 0; i < query_39.Count; i++)
            {
                Console.WriteLine($"query_39 CId = {query_39[i].CId},  Count = {query_39[i].Count}");
            }

            Console.Write("40、查詢選修「張三」老師所授課程的學生中成績最高的學生姓名及其成績");
            var query_40 = DB
                           .Queryable<Student, SC, Course, Teacher>((st, sc, c, t) => st.SId == sc.SId && sc.CId == c.CId && c.TId == t.TId && t.Tname == "張三")
                           .Select((st, sc, c, t) => new { st.SName, sc.Score })
                           .First();

            Console.WriteLine($"query_40 SName={query_40.SName} , Score= {query_40.Score}");

            Console.WriteLine("41、查詢不同課程成績相同的學生的學生編號、課程編號、學生成績 ");
            var query_41 = DB
                           .Queryable<SC, SC>((sc1, sc2) => sc1.SId == sc2.SId && sc1.Score == sc2.Score && sc1.CId != sc2.CId)
                           .Select((sc1, sc2) => new { sc1.SId , sc1.CId , sc1.Score })
                           .Distinct()
                           .ToList();

            for (int i = 0; i < query_41.Count; i++)
            {
                Console.WriteLine($"query_41 SId= {query_41[i].SId} CId= {query_41[i].CId} SId= {query_41[i].Score}");
            }

            Console.WriteLine("42、統計每門課程的學生選修人數（超過5人的課程才統計）。要求輸出課程號和選修人數，查詢結果按人數降序排列，若人數相同，按課程號升序排列");

            var query_42 = DB
                            .Queryable<SC>()
                            .GroupBy(sc => sc.CId)
                            .Having(sc => SqlFunc.AggregateCount(sc.CId) > 5)
                            .OrderBy(sc => SqlFunc.AggregateCount(sc.CId), OrderByType.Desc)
                            .Select(sc => new { sc.CId , Count = SqlFunc.AggregateCount(sc.CId) } )
                            .ToList();
            for (int i = 0; i < query_42.Count; i++)
            {
                Console.WriteLine($"query_42 CId= {query_42[i].CId} SId= {query_42[i].Count}");
            }

            Console.WriteLine("43、檢索至少選修兩門課程的學生學號");
            var query_43 = DB
                           .Queryable<SC>()
                           // 為啥要全選
                           .GroupBy(sc => new { sc.SId, sc.CId, sc.Score })
                           .Having(sc => SqlFunc.AggregateCount(sc.CId) > 2)
                           .ToList();

            for (int i = 0; i < query_43.Count; i++)
            {
                Console.WriteLine($"query_43 SId= {query_43[i].SId}");
            }


            //var query_10 = DB
            //  .Queryable(
            //        DB.Queryable<Student>(),
            //        DB.Queryable<SC>()
            //           .GroupBy(sc => new { sc.SId })
            //           .Having(sc => SqlFunc.AggregateCount(sc.CId) != cnt)
            //           .Select<object>(sc => new {
            //               sc.SId
            //           }),
            //         JoinType.Inner,
            //  (st, sub) => st.SId == SqlFunc.MappingColumn(default(string), "sub.SId"))
            //  .Select((st, sub) => new { st.SId, st.SName })
            ////  .ToList();
            ///


            //var query_44 = DB
            //                .Queryable(DB.Queryable<Student>(),
            //                            DB.Queryable<SC>()
            //                            .Select(sc => new { sc.SId }),
            //                            JoinType.Inner,
            //                        (st, sc) => st.SId == sc.SId)


            Console.WriteLine("48、(跟第15題一樣)查詢兩門以上不及格課程的同學的學號及其平均成績");

        }
        public class No5Sub
        {
            public SC sc { get; set; }
            public Course cs { get; set; }
            public Teacher t { get; set; }
        }
        public class q10subres
        { 
            public string SId { get; set; }
        }


    }
}
