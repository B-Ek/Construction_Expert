using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Construction_Expert.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Construction_Expert.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDb _db;

        public CategoryService(AppDb db) => _db = db;

        public async Task<string> NextCodeAsync()
        {
            var last = await _db.ConstructionCategories
                                .OrderByDescending(x => x.Code)
                                .Select(x => x.Code)
                                .FirstOrDefaultAsync();

            var next = (int.TryParse(last, out var n) ? n + 1 : 1);
            return next.ToString("D3");
        }

        public async Task<List<CategoryNode>> GetTreeAsync()
        {
            var cats = await _db.ConstructionCategories
                                .Include(c => c.ChildRelations)
                                .ToListAsync();

            var buckets = cats.ToDictionary(c => c.Id, c => new List<CategoryNode>());

            var relations = await _db.ConstructionCategoryRelations.ToListAsync();
            foreach (var rel in relations)
            {
                var child = cats.First(c => c.Id == rel.CategoryId);
                buckets[rel.ParentCategoryId].Add(
                    new CategoryNode(child.Id, child.Code, child.Name, buckets[child.Id])
                );
            }

            return cats.Where(c => c.IsRoot)
                       .Select(r => new CategoryNode(r.Id, r.Code, r.Name, buckets[r.Id]))
                       .ToList();
        }

        public async Task<List<ConstructionCategory>> GetAllCategoriesAsync()
        {
            return await _db.ConstructionCategories.OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<SelectList> GetAreaSelectListAsync()
        {
            var areas = await _db.ConstructionAreas.OrderBy(a => a.Name).ToListAsync();
            return new SelectList(areas, "Id", "Id");
        }

        public async Task CreateCategoryAsync(ConstructionCategory category, Guid? parentId)
        {
            category.Id = Guid.NewGuid();
            category.Code = await NextCodeAsync();
            category.IsRoot = !parentId.HasValue;
            category.IsLeaf = true;

            await _db.ConstructionCategories.AddAsync(category);

            if (parentId.HasValue)
            {
                await _db.ConstructionCategoryRelations.AddAsync(new ConstructionCategoryRelation
                {
                    Id = Guid.NewGuid(),
                    CategoryId = category.Id,
                    ParentCategoryId = parentId.Value,
                    Priority = 0
                });

                var parentCat = await _db.ConstructionCategories.FindAsync(parentId.Value);
                if (parentCat != null && parentCat.IsLeaf)
                {
                    parentCat.IsLeaf = false;
                    _db.ConstructionCategories.Update(parentCat);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<(ConstructionCategory Cat, int Level)>> GetFlatTreeAsync()
        {
            var roots = await _db.ConstructionCategories
                                 .Where(c => c.IsRoot)
                                 .OrderBy(c => c.Code)
                                 .ToListAsync();

            var flat = new List<(ConstructionCategory, int)>();

            foreach (var r in roots)
                Walk(r, 0);

            return flat;

            void Walk(ConstructionCategory node, int level)
            {
                flat.Add((node, level));

                var children = _db.ConstructionCategoryRelations
                                  .Where(x => x.ParentCategoryId == node.Id)
                                  .OrderBy(x => x.Priority)
                                  .Select(rel => _db.ConstructionCategories.First(c => c.Id == rel.CategoryId))
                                  .ToList();

                foreach (var c in children)
                    Walk(c, level + 1);
            }
        }

        public record CatPath(IReadOnlyList<ConstructionCategory> Nodes);

        public async Task<List<CatPath>> GetLeafPathsAsync()
        {
            var cats = await _db.ConstructionCategories.ToListAsync();
            var rels = await _db.ConstructionCategoryRelations.ToListAsync();

            var lookup = rels.GroupBy(r => r.ParentCategoryId)
                             .ToDictionary(
                                 g => g.Key,
                                 g => g.Select(r => cats.First(c => c.Id == r.CategoryId))
                                       .OrderBy(c => c.Code)
                                       .ToList()
                             );

            List<CatPath> paths = [];

            void Walk(ConstructionCategory node, List<ConstructionCategory> stack)
            {
                stack.Add(node);

                if (!lookup.ContainsKey(node.Id) || lookup[node.Id].Count == 0)
                {
                    paths.Add(new CatPath(stack.ToList()));
                }
                else
                {
                    foreach (var child in lookup[node.Id])
                        Walk(child, stack);
                }

                stack.RemoveAt(stack.Count - 1);
            }

            foreach (var root in cats.Where(c => c.IsRoot).OrderBy(c => c.Code))
                Walk(root, new List<ConstructionCategory>());

            return paths;
        }
        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _db.ConstructionCategories.FindAsync(categoryId);
            if (category == null) return;

            var childRels = _db.ConstructionCategoryRelations.Where(r => r.ParentCategoryId == categoryId || r.CategoryId == categoryId);
            _db.ConstructionCategoryRelations.RemoveRange(childRels);

            _db.ConstructionCategories.Remove(category);

            await _db.SaveChangesAsync();
        }


    }
}
