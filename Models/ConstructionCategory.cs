using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Construction_Expert.Models
{
    public class ConstructionCategory
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Area zorunludur.")]
        public byte AreaId { get; set; }

        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Name zorunludur.")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsRoot { get; set; }
        public bool IsLeaf { get; set; }

        public ICollection<ConstructionCategoryRelation> ChildRelations { get; set; } = new List<ConstructionCategoryRelation>();
        public ICollection<ConstructionCategoryRelation> ParentRelations { get; set; } = new List<ConstructionCategoryRelation>();
    }
}
