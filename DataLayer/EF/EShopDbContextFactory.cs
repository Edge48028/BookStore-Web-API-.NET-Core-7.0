using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace DataLayer.EF
{
    public class EShopDbContextFactory : IDesignTimeDbContextFactory<EShopDbContext>
    {
        //private static string ConStringSqlServer = "Data Source=EDGE\\WEBAPI;Initial Catalog=BookStoreAPI;Integrated Security=True;";
        //private static string ConStringSqlServer = "Data Source=EDGE\\WEBAPI;Initial Catalog=BookStoreAPI;Integrated Security=True;";
        private static string ConStringSqlServer = "Data Source=EDGE\\WEBAPI;Trusted_Connection=True; Encrypt=False;Initial Catalog=BookStoreAPI;Persist Security Info=True;User ID=sa;Password=grthfasd34;MultipleActiveResultSets=True";

        //private static string ConStringSqlite = "Filename=BookStoreAPI.db";
        public EShopDbContext CreateDbContext(string[] args)
        {
            var optionBuilder = new DbContextOptionsBuilder<EShopDbContext>();
            optionBuilder.UseSqlServer(ConStringSqlServer);

            //optionBuilder.UseSqlite(ConStringSqlite);

            return new EShopDbContext(optionBuilder.Options);
        }
    }
}