using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace FreeSql.Tests.QuestDb.QuestDbTestModel
{
    public class Topic
    {
        [Column(IsIdentity = true)] public int Id { get; set; }
        public string Title { get; set; }
        public int Clicks { get; set; }
        public DateTime CreateTime { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }

    public class Category
    {
        [Column(IsIdentity = true)] public int Id { get; set; }
        public string Name { get; set; }

        public int ParentId { get; set; }
        public CategoryType Parent { get; set; }
        public List<Topic> Topics { get; set; }
    }

    public class CategoryType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}