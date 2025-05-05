using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Construction_Expert.Models;
using static Construction_Expert.Services.CategoryService;
using Microsoft.AspNetCore.Mvc;

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
         Task<List<CatPath>> GetLeafPathsAsync();
        Task DeleteCategoryAsync(Guid categoryId);



    }
}
