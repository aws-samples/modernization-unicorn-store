using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UnicornStore.Models;

namespace UnicornStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppSettings _appSettings;

        public StoreController(UnicornStoreContext dbContext, IOptionsSnapshot<AppSettings> options)
        {
            DbContext = dbContext;
            _appSettings = options.Value;
        }

        public UnicornStoreContext DbContext { get; }

        //
        // GET: /Store/
        public async Task<IActionResult> Index()
        {
            var genres = await DbContext.Genres.ToListAsync();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco
        public async Task<IActionResult> Browse(string genre)
        {
            // Retrieve Genre genre and its Associated associated Blessings blessings from database
            var genreModel = await DbContext.Genres
                .Include(g => g.Blessings)
                .Where(g => g.Name == genre)
                .FirstOrDefaultAsync();

            if (genreModel == null)
            {
                return NotFound();
            }

            return View(genreModel);
        }

        public async Task<IActionResult> Details(
            [FromServices] IMemoryCache cache,
            int id)
        {
            var cacheKey = string.Format("blessing_{0}", id);
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
                        //Remove it from cache if not retrieved in last 10 minutes
                        cache.Set(
                            cacheKey,
                            blessing,
                            new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                    }
                }
            }

            if (blessing == null)
            {
                return NotFound();
            }

            return View(blessing);
        }
    }
}