using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnicornStore.Models;

namespace UnicornStore.Data
{
    public static class DbInitializer
    {
        const string imgUrl = "~/Images/placeholder.png";

        public static async Task Initialize(
            UnicornStoreContext context,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            bool createUsers = true)
        {
            if (await context.Database.EnsureCreatedAsync())
            {
                await InsertTestData(serviceProvider);
                if (createUsers)
                {
                    await CreateAdminUser(serviceProvider, configuration);
                }
            }
        }

        private static async Task InsertTestData(IServiceProvider serviceProvider)
        {
            var blessings = GetBlessings(imgUrl, Genres, Unicorns);

            await AddOrUpdateAsync(serviceProvider, g => g.GenreId, Genres.Select(genre => genre.Value));
            await AddOrUpdateAsync(serviceProvider, a => a.UnicornId, Unicorns.Select(unicorn => unicorn.Value));
            await AddOrUpdateAsync(serviceProvider, a => a.BlessingId, blessings);
        }

        // TODO [EF] This may be replaced by a first class mechanism in EF
        private static async Task AddOrUpdateAsync<TEntity>(
            IServiceProvider serviceProvider,
            Func<TEntity, object> propertyToMatch, IEnumerable<TEntity> entities)
            where TEntity : class
        {
            // Query in a separate context so that we can attach existing entities as modified
            List<TEntity> existingData;

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetService<UnicornStoreContext>())
            {
                existingData = db.Set<TEntity>().ToList();
            }

            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            using (var db = scope.ServiceProvider.GetService<UnicornStoreContext>())

            {
                foreach (var item in entities)
                {
                    db.Entry(item).State = existingData.Any(g => propertyToMatch(g).Equals(propertyToMatch(item)))
                        ? EntityState.Modified
                        : EntityState.Added;
                }

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates a store manager user who can manage the inventory.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static async Task CreateAdminUser(IServiceProvider serviceProvider, IConfiguration configuration)
        {

            var defaultAdminPassword = configuration["DefaultAdminPassword"];
            var defaultAdminUserName = configuration["DefaultAdminUserName"];

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            // TODO: Identity SQL does not support roles yet
            //var roleManager = serviceProvider.GetService<ApplicationRoleManager>();
            //if (!await roleManager.RoleExistsAsync(adminRole))
            //{
            //    await roleManager.CreateAsync(new IdentityRole(adminRole));
            //}

            var user = await userManager.FindByNameAsync(defaultAdminUserName);
            if (user == null)
            {
                user = new ApplicationUser { UserName = defaultAdminUserName };
                await userManager.CreateAsync(user, defaultAdminPassword);
                //await userManager.AddToRoleAsync(user, adminRole);
                await userManager.AddClaimAsync(user, new Claim("ManageStore", "Allowed"));
            }

            //NOTE: For end to end testing only
            var envPerfLab = configuration["PERF_LAB"];
            if (envPerfLab == "true")
            {
                for (int i = 0; i < 100; ++i)
                {
                    var email = string.Format("User{0:D3}@example.com", i);
                    var normalUser = await userManager.FindByEmailAsync(email);
                    if (normalUser == null)
                    {
                        await userManager.CreateAsync(new ApplicationUser { UserName = email, Email = email }, "Password~!1");
                    }
                }
            }
        }

        private static Blessing[] GetBlessings(string imgUrl, Dictionary<string, Genre> genres, Dictionary<string, Unicorn> unicorns)
        {
            var blessings = new Blessing[]
            {
                new Blessing { Title = "The fiery one", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Adiana"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The cheerful one", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Alairia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Fair and Beautiful", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Alanala"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Fair of face", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Albany"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Truthful one", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Aletha"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "One who charms", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Alize"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Peaceful and Attractive", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Allena"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Powerful one", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Amandaria"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Eternal one", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Amara"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Strong and Courageous", Genre = genres["Dark Riders"], Price = 8.99M, Unicorn = unicorns["Andra"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Angelic one", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Angelina"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Full of Grace", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Annamika"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Bright as a Star", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Astra"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Little blessed one", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Bennettia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Simply beautiful", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Bellini"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "One who is blessed", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Benicia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "White power", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Biancha"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Perfect joy", Genre = genres["MagiCorns"], Price = 8.99M, Unicorn = unicorns["Blissia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Swift and strong", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Boaz"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The pretty one", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Bonita"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Pure and Virtuous", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Breanna"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The strong one", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Bryanne"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "From the moon", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Celina"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "From the stars", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Celestia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Gentle one", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Clementine"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The bold one", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Cortesia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Morning Star", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Danika"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Noble one", Genre = genres["Rainbow Unicorns"], Price = 8.99M, Unicorn = unicorns["Della"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Lover of the Earth", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Demetrius"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The great one", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Denali"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Roamer", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Dessa"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Celestial Spirit", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Deva"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Child of the sun", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Drisana"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Sweet one", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Dulcea"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Divine Spirit", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Duscha"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Shining one", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Electra"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Chosen One", Genre = genres["Dark Wings"], Price = 8.99M, Unicorn = unicorns["Elita"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Full of determination", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Etana"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Eternal friend", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Eternia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Always tranquil", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Evania"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "To be trusted", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Faith"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Happiness always", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Felicia"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Fair One", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Fenella"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Daring One", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Fernaco"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Little Star", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Estrellita"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The Swift one", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Fleta"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Prosperous", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Floriana"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The protective one", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Gerda"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Forever young", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Giulio"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "The graceful one", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Gratiana"], BlessingArtUrl = imgUrl },
                new Blessing { Title = "Brown Eyed Buttons", Genre = genres["Jewel Unicorns"], Price = 8.99M, Unicorn = unicorns["Yana"], BlessingArtUrl = imgUrl }
            };

            foreach (var blessing in blessings)
            {
                blessing.UnicornId = blessing.Unicorn.UnicornId;
                blessing.GenreId = blessing.Genre.GenreId;
            }

            return blessings;
        }

        private static Dictionary<string, Unicorn> unicorns;
        public static Dictionary<string, Unicorn> Unicorns
        {
            get
            {
                if (unicorns == null)
                {
                    var unicornsList = new Unicorn[]
                    {
                        new Unicorn { Name = "Adiana" },
                        new Unicorn { Name = "Alairia" },
                        new Unicorn { Name = "Alanala" },
                        new Unicorn { Name = "Albany" },
                        new Unicorn { Name = "Aletha" },
                        new Unicorn { Name = "Alize" },
                        new Unicorn { Name = "Allena" },
                        new Unicorn { Name = "Amandaria" },
                        new Unicorn { Name = "Amara" },
                        new Unicorn { Name = "Andra" },
                        new Unicorn { Name = "Angelina" },
                        new Unicorn { Name = "Annamika" },
                        new Unicorn { Name = "Astra" },
                        new Unicorn { Name = "Bennettia" },
                        new Unicorn { Name = "Bellini" },
                        new Unicorn { Name = "Benicia" },
                        new Unicorn { Name = "Biancha" },
                        new Unicorn { Name = "Blissia" },
                        new Unicorn { Name = "Boaz" },
                        new Unicorn { Name = "Bonita" },
                        new Unicorn { Name = "Breanna" },
                        new Unicorn { Name = "Bryanne" },
                        new Unicorn { Name = "Celina" },
                        new Unicorn { Name = "Celestia" },
                        new Unicorn { Name = "Clementine" },
                        new Unicorn { Name = "Cortesia" },
                        new Unicorn { Name = "Danika" },
                        new Unicorn { Name = "Della" },
                        new Unicorn { Name = "Demetrius" },
                        new Unicorn { Name = "Denali" },
                        new Unicorn { Name = "Dessa" },
                        new Unicorn { Name = "Deva" },
                        new Unicorn { Name = "Drisana" },
                        new Unicorn { Name = "Dulcea" },
                        new Unicorn { Name = "Duscha" },
                        new Unicorn { Name = "Electra" },
                        new Unicorn { Name = "Elita" },
                        new Unicorn { Name = "Etana" },
                        new Unicorn { Name = "Eternia" },
                        new Unicorn { Name = "Evania" },
                        new Unicorn { Name = "Faith" },
                        new Unicorn { Name = "Felicia" },
                        new Unicorn { Name = "Fenella" },
                        new Unicorn { Name = "Fernaco" },
                        new Unicorn { Name = "Estrellita" },
                        new Unicorn { Name = "Fleta" },
                        new Unicorn { Name = "Floriana" },
                        new Unicorn { Name = "Gerda" },
                        new Unicorn { Name = "Giulio" },
                        new Unicorn { Name = "Gratiana"},
                        new Unicorn { Name = "Yana" }
                    };

                    unicorns = new Dictionary<string, Unicorn>();
                    foreach (Unicorn unicorn in unicornsList)
                    {
                        unicorns.Add(unicorn.Name, unicorn);
                    }
                }

                return unicorns;
            }
        }


        private static Dictionary<string, Genre> genres;
        public static Dictionary<string, Genre> Genres
        {
            get
            {
                if (genres == null)
                {
                    var genresList = new Genre[]
                    {
                        new Genre { Name = "Jewel Unicorns" },
                        new Genre { Name = "Dark Riders" },
                        new Genre { Name = "MagiCorns" },
                        new Genre { Name = "Rainbow Unicorns" },
                        new Genre { Name = "Dark Wings" }
                    };

                    genres = new Dictionary<string, Genre>();

                    foreach (Genre genre in genresList)
                    {
                        genres.Add(genre.Name, genre);
                    }
                }

                return genres;
            }
        }
    }
}