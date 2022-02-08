using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebShopInventory.Data;
using WebShopInventory.Models;
using WebShopInventory.Models.ViewModels;

namespace WebShopInventory.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext m_context;
        private readonly IWebHostEnvironment m_webHostEnvironment;
        private IEnumerable<SelectListItem> m_selectListItems;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            m_context = context;
            m_webHostEnvironment = webHostEnvironment;
            m_selectListItems = m_context.ProductCategories.Select(i => new SelectListItem
            {
                 Text = i.Title,
                 Value = i.Id.ToString()
            });            
        }


        /// <summary>
        /// GET: Products
        /// Sorting: Name, SKU, Price
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="searchString"></param>
        /// <returns>List of Products</returns>
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SKUSortParam"] = sortOrder == "SKU" ? "sku_desc" : "SKU";
            ViewData["PriceSortParam"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["CategorySortParam"] = sortOrder == "Category" ? "category_desc" : "Category";

            var products = from p in m_context.Products select p;
            var categories = from c in m_context.ProductCategories select c;     

            var productWithCategory = from p in products
                                      join c in categories on p.ProductCategoryId equals c.Id
                                      select new ProductViewModel { Product = p, ProductCategory = c };

            productWithCategory = sortOrder switch
            {
                "name_desc" => productWithCategory.OrderByDescending(p => p.Product.Title),
                "SKU" => productWithCategory.OrderBy(p => p.Product.Code),
                "sku_desc" => productWithCategory.OrderByDescending(p => p.Product.Code),
                "Price" => productWithCategory.OrderBy(p => p.Product.Price),
                "price_desc" => productWithCategory.OrderByDescending(p => p.Product.Price),
                "Category" => productWithCategory.OrderBy(p => p.ProductCategory.Title),
                "category_desc" => productWithCategory.OrderByDescending(p => p.ProductCategory.Title),
                _ => productWithCategory.OrderBy(p => p.Product.Title),
            };

            if (!String.IsNullOrEmpty(searchString))
            {
                productWithCategory = productWithCategory.Where(
                    p => p.Product.Title.Contains(searchString) ||
                    p.Product.Description.Contains(searchString) ||
                    p.ProductCategory.Title.Contains(searchString)
                    );
            }

            return View(await productWithCategory.AsNoTracking().ToListAsync());

        }

        /// <summary>
        /// GET: Products/Create
        /// </summary>
        /// <returns>Product entry form</returns>
        public IActionResult Create()
        {
            ViewBag.selectListItems = m_selectListItems;

            return View();
        }

        /// <summary>
        /// POST: Products/Create
        /// To protect from overposting attacks, enable the specific properties you want to bind to.
        /// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598. 
        /// </summary>
        /// <param name="product"></param>
        /// <returns>A new product</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Title,Description,Stock,Price,ImagePath,ImageFile,Timestamp,ProductCategoryId")] Product product)
        {
            if (await SkuExists(product.Code))
            {
                ModelState.TryAddModelError("Code", $"A product with this SKU code {product.Code} is already exists");
            }

            if (ModelState.IsValid)
            {
                product.ImagePath = await GetImagePath(product);
                m_context.Add(product);
                await m_context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.selectListItems = m_selectListItems;

            return View(product);
        }

        /// <summary>
        /// GET: Products/Edit/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await m_context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            ViewBag.selectListItems = m_selectListItems;

            return View(product);
        }

        /// <summary>
        /// POST: Products/Edit/5
        /// To protect from overposting attacks, enable the specific properties you want to bind to.
        /// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("Id,Code,Title,Description,Stock,Price,ImagePath,ImageFile,Timestamp,ProductCategoryId")] Product product)
        {
            if (id != product.Id)
                return NotFound();

            if (await FindIdBySku(product.Code) != id && await SkuExists(product.Code))
            {
                ModelState.TryAddModelError("Code", $"A product with this SKU code {product.Code} is already exists");
            }
        
            if (ModelState.IsValid)
            {
                try
                {
                    if (product.ImageFile != null)
                    {
                        product.ImagePath = await GetImagePath(product);
                    }                  

                    m_context.Products.Update(product);
                    await m_context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        /// <summary>
        /// GET: Products/Delete/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await m_context.Products.FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
                return NotFound();

            var category = await m_context.ProductCategories.FirstOrDefaultAsync(c => c.Id == product.ProductCategoryId);

            var productviewmodel = new ProductViewModel { Product = product, ProductCategory = category };

            return View(productviewmodel);
        }

        /// <summary>
        /// POST: Products/Delete/5 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await m_context.Products.FindAsync(id);

            m_context.Products.Remove(product);
            await m_context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Products/Details/5 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (!await ProductExists((int)id))
                return NotFound();

            var product = await m_context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            var category = await m_context.ProductCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == product.ProductCategoryId);

            var productviewmodel = new ProductViewModel { Product = product, ProductCategory = category };

            return View(productviewmodel);
        }

        private async Task<bool> ProductExists(int id) =>
            await m_context.Products.AnyAsync(e => e.Id == id);

        private async Task<bool> SkuExists(string code) =>
            await m_context.Products.AnyAsync(e => e.Code == code);

        private async Task<int?> FindIdBySku(string code)
        {
            var p = await m_context.Products.AsNoTracking().FirstOrDefaultAsync(e => e.Code == code);

            return p?.Id;
        }



        private async Task<string> GetImagePath(Product product)
        {
            if (product.ImageFile == null)
                return product.ImagePath;

            string extension = Path.GetExtension(product.ImageFile.FileName);

            string ImagePath = product.Code + "-" + DateTime.Now.ToString("yymmssfff") + extension;

            string path = Path.Combine(m_webHostEnvironment.WebRootPath + "/uploads/", ImagePath);

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await product.ImageFile.CopyToAsync(fileStream);
            }

            return ImagePath;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
