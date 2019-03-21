using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using UnicornStore.Models;

namespace UnicornStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings _appSettings;

        public HomeController(IOptions<AppSettings> options)
        {
            _appSettings = options.Value;
        }
        //
        // GET: /Home/
        public async Task<IActionResult> Index(
            [FromServices] UnicornStoreContext dbContext,
            [FromServices] IMemoryCache cache)
        {
            // Get most popular blessings
            var cacheKey = "topselling";
            List<Blessing> blessings;
            if (!cache.TryGetValue(cacheKey, out blessings))
            {
                blessings = await GetTopSellingBlessingsAsync(dbContext, 6);

                if (blessings != null && blessings.Count > 0)
                {
                    if (_appSettings.CacheDbResults)
                    {
                        // Refresh it every 10 minutes.
                        // Let this be the last item to be removed by cache if cache GC kicks in.
                        cache.Set(
                            cacheKey,
                            blessings,
                            new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                            .SetPriority(CacheItemPriority.High));
                    }
                }
            }

            return View(blessings);
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

        public IActionResult StatusCodePage()
        {
            return View("~/Views/Shared/StatusCodePage.cshtml");
        }

        public IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        private Task<List<Blessing>> GetTopSellingBlessingsAsync(UnicornStoreContext dbContext, int count)
        {
            // Group the order details by blessing and return
            // the blessings with the highest count

            return dbContext.Blessings
                .OrderByDescending(a => a.OrderDetails.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}