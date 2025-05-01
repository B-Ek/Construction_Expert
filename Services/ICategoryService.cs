using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Construction_Expert.Models;

namespace Construction_Expert.Services
{
    public interface ICategoryService
    {
        Task<string> NextCodeAsync();
        Task<List<CategoryNode>> GetTreeAsync();
        Task<List<ConstructionCategory>> GetAllCategoriesAsync();
        Task<SelectList> GetAreaSelectListAsync();
        Task CreateCategoryAsync(ConstructionCategory category, Guid? parentId);
        Task<List<(ConstructionCategory Cat, int Level)>> GetFlatTreeAsync();

    }
}
