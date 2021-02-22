using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UnicornStore.Models;
using UnicornStore.ViewModels;

namespace UnicornStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("ManageStore")]
    public class StoreManagerController : Controller
    {
        private readonly AppSettings _appSettings;

        public StoreManagerController(UnicornStoreContext dbContext, IOptionsSnapshot<AppSettings> options)
        {
            DbContext = dbContext;
            _appSettings = options.Value;
        }

        public UnicornStoreContext DbContext { get; }

        //
        // GET: /StoreManager/
        public async Task<IActionResult> Index()
        {
            List<Blessing> blessings = await DbContext.Blessings
                .Include(a => a.Genre)
                .Include(a => a.Unicorn)
                .ToListAsync();

            return View(blessings);
        }

        //
        // GET: /StoreManager/Details/5
        public async Task<IActionResult> Details(
            [FromServices] IMemoryCache cache,
            int id)
        {
            var cacheKey = GetCacheKey(id);

            Blessing blessing;
            if (!cache.TryGetValue(cacheKey, out blessing))
            {
                blessing = await DbContext.Blessings
                        .Where(a => a.BlessingId == id)
                        .Include(a => a.Unicorn)
                        .Include(a => a.Genre)
                        .FirstOrDefaultAsync();

                if (blessing != null)
                {
                    if (_appSettings.CacheDbResults)
                    {
                        //Remove it from cache if not retrieved in last 10 minutes.
                        cache.Set(
                            cacheKey,
                            blessing,
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                    }
                }
            }

            if (blessing == null)
            {
                cache.Remove(cacheKey);
                return NotFound();
            }

            return View(blessing);
        }

        //
        // GET: /StoreManager/Create
        public IActionResult Create()
        {
            ViewBag.GenreId = new SelectList(DbContext.Genres, "GenreId", "Name");
            ViewBag.UnicornId = new SelectList(DbContext.Unicorns, "UnicornId", "Name");
            return View();
        }

        // POST: /StoreManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Blessing blessing,
            [FromServices] IMemoryCache cache,
            CancellationToken requestAborted)
        {
            if (ModelState.IsValid)
            {
                DbContext.Blessings.Add(blessing);
                await DbContext.SaveChangesAsync(requestAborted);

                var blessingData = new BlessingData
                {
                    Title = blessing.Title,
                    Url = Url.Action("Details", "Store", new { id = blessing.BlessingId })
                };

                cache.Remove("latestBlessing");
                return RedirectToAction("Index");
            }

            ViewBag.GenreId = new SelectList(DbContext.Genres, "GenreId", "Name", blessing.GenreId);
            ViewBag.UnicornId = new SelectList(DbContext.Unicorns, "UnicornId", "Name", blessing.UnicornId);
            return View(blessing);
        }

        //
        // GET: /StoreManager/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var blessing = await DbContext.Blessings.
                Where(a => a.BlessingId == id).
                FirstOrDefaultAsync();

            if (blessing == null)
            {
                return NotFound();
            }

            ViewBag.GenreId = new SelectList(DbContext.Genres, "GenreId", "Name", blessing.GenreId);
            ViewBag.UnicornId = new SelectList(DbContext.Unicorns, "UnicornId", "Name", blessing.UnicornId);
            return View(blessing);
        }

        //
        // POST: /StoreManager/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [FromServices] IMemoryCache cache,
            Blessing blessing,
            CancellationToken requestAborted)
        {
            if (ModelState.IsValid)
            {
                DbContext.Update(blessing);
                await DbContext.SaveChangesAsync(requestAborted);
                //Invalidate the cache entry as it is modified
                cache.Remove(GetCacheKey(blessing.BlessingId));
                return RedirectToAction("Index");
            }

            ViewBag.GenreId = new SelectList(DbContext.Genres, "GenreId", "Name", blessing.GenreId);
            ViewBag.UnicornId = new SelectList(DbContext.Unicorns, "UnicornId", "Name", blessing.UnicornId);
            return View(blessing);
        }

        //
        // GET: /StoreManager/RemoveBlessing/5
        public async Task<IActionResult> RemoveBlessing(int id)
        {
            var blessing = await DbContext.Blessings.Where(a => a.BlessingId == id).FirstOrDefaultAsync();
            if (blessing == null)
            {
                return NotFound();
            }

            return View(blessing);
        }

        //
        // POST: /StoreManager/RemoveBlessing/5
        [HttpPost, ActionName("RemoveBlessing")]
        public async Task<IActionResult> RemoveBlessingConfirmed(
            [FromServices] IMemoryCache cache,
            int id,
            CancellationToken requestAborted)
        {
            var blessing = await DbContext.Blessings.Where(a => a.BlessingId == id).FirstOrDefaultAsync();
            if (blessing == null)
            {
                return NotFound();
            }

            DbContext.Blessings.Remove(blessing);
            await DbContext.SaveChangesAsync(requestAborted);
            //Remove the cache entry as it is removed
            cache.Remove(GetCacheKey(id));

            return RedirectToAction("Index");
        }

        private static string GetCacheKey(int id)
        {
            return string.Format("blessing_{0}", id);
        }

        // NOTE: this is used for end to end testing only
        //
        // GET: /StoreManager/GetBlessingIdFromName
        // Note: Added for automated testing purpose. Application does not use this.
        [HttpGet]
        [SkipStatusCodePages]
        [EnableCors("CorsPolicy")]
        public async Task<IActionResult> GetBlessingIdFromName(string blessingName)
        {
            var blessing = await DbContext.Blessings.Where(a => a.Title == blessingName).FirstOrDefaultAsync();

            if (blessing == null)
            {
                return NotFound();
            }

            return Content(blessing.BlessingId.ToString());
        }
    }
}