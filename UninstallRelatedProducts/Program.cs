using System;
using System.Collections.Generic;
using System.Linq;

namespace UninstallRelatedProducts
{
    class Program
    {
        const int errUsage = 1;
        const int errLogger = 2;
        const int errQuery = 3;
        const int errAllUsers = 4;
        const int errUninstall = 5;

        static int Main(string[] args)
        {
            Guid upgradeCode;
            if (args.Length < 1 || args.Length > 2 || !Guid.TryParse(args[0], out upgradeCode))
            {
                Console.WriteLine("usage:\n     UninstallRelatedProducts <GUID> <LogFile>");
                return errUsage;
            }

            string logFile = null;
            if (args.Length == 2)
            {
                logFile = args[1];
            }

            try
            {
                using (var logger = new Logger(logFile))
                {
                    List<Guid> productCodes;
                    try
                    {
                        productCodes = Msi.GetRelatedProducts(upgradeCode).ToList();
                        logger.Log("Number of related products found: " + productCodes.Count);
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Unable to query related products: {ex.Message}");
                        return errQuery;
                    }

                    foreach (var product in productCodes)
                    {
                        try
                        {
                            logger.Log("Product code: " + product);
                            var allUsers = Msi.IsAllUsers(product);
                            logger.Log("All users: " + allUsers);
                            if (allUsers)
                            {
                                Console.WriteLine("Skipping.");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Failed to check if product is installed for all users: {ex.Message}");
                            return errAllUsers;
                        }

                        try
                        {
                            Msi.Uninstall(product, silent: true);
                        }
                        catch (Exception ex)
                        {
                            logger.Log($"Failed to uninstall previous version: {ex.Message}");
                            return errUninstall;
                        }
                    }

                    logger.Log("Uninstall completed successfully");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating log file: {ex.Message}");
                return errLogger;
            }
        }
    }
}
