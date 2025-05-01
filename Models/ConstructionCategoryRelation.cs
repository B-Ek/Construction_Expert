using System.Collections.Generic;
using System;

namespace Construction_Expert.Models
{
    public class ConstructionCategoryRelation
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Guid ParentCategoryId { get; set; }
        public byte? Priority { get; set; }

        public ConstructionCategory Category { get; set; } = null!;
        public ConstructionCategory ParentCategory { get; set; } = null!;
    }
}