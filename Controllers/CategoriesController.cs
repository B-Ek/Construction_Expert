using Construction_Expert.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Construction_Expert.Models;

namespace Construction_Expert.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _svc;

        public CategoriesController(ICategoryService svc)
        {
            _svc = svc;
        }

        public async Task<IActionResult> Index()
        {
            var tree = await _svc.GetTreeAsync();
            return View(tree);
        }

        public async Task<IActionResult> Create(Guid? parentId)
        {
            var model = new ConstructionCategory
            {
                Code = await _svc.NextCodeAsync(),
                IsRoot = !parentId.HasValue
            };

            ViewBag.ParentId = parentId;
            ViewBag.Categories = await _svc.GetAllCategoriesAsync();
            ViewBag.AreaList = await _svc.GetAreaSelectListAsync();
            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> Create(ConstructionCategory model, Guid? parentId)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _svc.GetAllCategoriesAsync();
                ViewBag.AreaList = await _svc.GetAreaSelectListAsync();
                return View(model);
            }

            await _svc.CreateCategoryAsync(model, parentId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Hierarchy()
        {
            var paths = await _svc.GetLeafPathsAsync();
            return View(paths);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _svc.DeleteCategoryAsync(id);
            return RedirectToAction("Hierarchy");
        }




    }
}

