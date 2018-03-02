using System;
using System.Linq;

namespace UninstallRelatedProducts
{
    class Program
    {
        const int errExit = 1;

        static void Main(string[] args)
        {
            using (var logger = new Logger(null))
            {
                Guid upgradeCode;
                if (args.Length != 1 || !Guid.TryParse(args[0], out upgradeCode))
                {
                    logger.Log("usage:\n     UninstallRelatedProducts <GUID>");
                    Environment.Exit(errExit);
                    return; // To avoid uninitialized error
                }

                try
                {
                    var productCodes = Msi.GetRelatedProducts(upgradeCode).ToList();
                    logger.Log("Number of related products found: " + productCodes.Count);

                    foreach (var product in productCodes)
                    {
                        logger.Log("Product code: " + product);
                        var allUsers = Msi.IsAllUsers(product);
                        logger.Log("All users: " + allUsers);
                        if (allUsers)
                        {
                            Console.WriteLine("Skipping.");
                            continue;
                        }

                        Msi.Uninstall(product, silent: true);
                    }

                    logger.Log("Uninstall completed successfully");
                }
                catch (Exception e)
                {
                    logger.Log(e);
                    throw;
                }
            }
        }
    }
}
