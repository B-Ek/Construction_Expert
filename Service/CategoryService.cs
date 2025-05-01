using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Construction_Expert.Models;
using Construction_Expert.Service;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        // Bucket oluþtur
        var buckets = cats.ToDictionary(c => c.Id, c => new List<CategoryNode>());

        // Relations tablosundan doðru þekilde kullaným
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
        return new SelectList(areas, "Id", "Name");
    }

    public async Task CreateCategoryAsync(ConstructionCategory category, Guid? parentId)
    {
        // Id & Code üretimi
        category.Id = Guid.NewGuid();
        category.Code = await NextCodeAsync();

        // IsRoot / IsLeaf iþ kuralý
        category.IsRoot = !parentId.HasValue;
        category.IsLeaf = true;

        await _db.ConstructionCategories.AddAsync(category);

        if (parentId.HasValue)
        {
            // Parent iliþki kaydý
            await _db.ConstructionCategoryRelations.AddAsync(new ConstructionCategoryRelation
            {
                Id = Guid.NewGuid(),
                CategoryId = category.Id,
                ParentCategoryId = parentId.Value,
                Priority = 0
            });

            // Parent kategorinin artýk yaprak olmamasý lazým
            var parentCat = await _db.ConstructionCategories.FindAsync(parentId.Value);
            if (parentCat != null && parentCat.IsLeaf)
            {
                parentCat.IsLeaf = false;
                _db.ConstructionCategories.Update(parentCat);
            }
        }

        await _db.SaveChangesAsync();
    }

}
