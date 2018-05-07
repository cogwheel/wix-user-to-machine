using System;
using System.Linq;

namespace UninstallRelatedProducts
{
    class Program
    {
        const int errExit = 1;

        static void Main(string[] args)
        {
            Guid upgradeCode;
            if (args.Length < 1 || args.Length > 2 || !Guid.TryParse(args[0], out upgradeCode))
            {
                Console.WriteLine("usage:\n     UninstallRelatedProducts <GUID> <LogFile>");
                Environment.Exit(errExit);
                return; // To avoid uninitialized error
            }

            string logFile = null;
            if (args.Length == 2)
            {
                logFile = args[1];
            }

            using (var logger = new Logger(logFile))
            {
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
                    Environment.Exit(errExit);
                }
            }
        }
    }
}
