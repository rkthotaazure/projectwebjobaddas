using adidas.clb.job.GeneratePDF.App_Data.DAL;
using adidas.clb.job.GeneratePDF.Models;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
namespace adidas.clb.job.GeneratePDF.Helpers
{
    /// <summary>
    /// Implements AzCopyConfig classs
    /// </summary>
    public class AzCopyConfig
    {
        public static string AzCopyPath = String.Empty;
        public static string ImagePath = String.Empty;
        //Application insights interface reference for logging the error details into Application Insight azure service.
        static IAppInsight InsightLogger { get { return AppInsightLogger.Instance; } }
        /// <summary>
        /// This method downloads azcopy.exe from azcopy blob container to given path
        /// </summary>
        public static void LoadAzCopyConfigFromBlob()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["GenericMobileStorageConnectionString"]);

                //Define Local Path
                var rootpath = GetaZCopypath();
                AzCopyPath = rootpath;

                //Define Blob Client
                var blobClient = storageAccount.CreateCloudBlobClient();
                var aZCopyConfigurationContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["AzCopyContainerName"]);
                CloudBlockBlob AzCopyConfiguration = aZCopyConfigurationContainer.GetBlockBlobReference(ConfigurationManager.AppSettings["BlobReference"]);
                string aZCopydirname = ConfigurationManager.AppSettings["aZCopyFolderName"];
                // delete  azcopy.zip
                string azcopyPath = Path.Combine(rootpath, aZCopydirname);
                System.IO.DirectoryInfo azdir = new System.IO.DirectoryInfo(azcopyPath);
                if (azdir.Exists)
                {
                    foreach (FileInfo file in azdir.GetFiles())
                    {
                        file.Delete();
                    }
                    System.IO.Directory.Delete(azcopyPath, true);
                }
                if (File.Exists(rootpath + "\\" + AzCopyConfiguration.Name))
                {
                    File.Delete(rootpath + "\\" + AzCopyConfiguration.Name);
                }

                //download  azcopy.zip 
                using (var fileStream = new FileStream(rootpath + "\\" + AzCopyConfiguration.Name, FileMode.CreateNew))
                {
                    BlobRequestOptions options = new BlobRequestOptions();
                    options.ServerTimeout = new TimeSpan(0, 25, 0);
                    AzCopyConfiguration.DownloadToStream(fileStream, null, options, null);
                }

                // Unzip
                ZipFile.ExtractToDirectory(rootpath + "\\aZCopy.zip", rootpath);
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method downloads header image file for pdf file header from image blob container to given path
        /// </summary>
        public static void LoadImageFromBlob()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["GenericMobileStorageConnectionString"]);

                //Define Local Path
                var Imagerootpath = GetImagePath();
                ImagePath = Imagerootpath;

                //Define Blob Client
                var blobClient = storageAccount.CreateCloudBlobClient();
                var ImageConfigurationContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["ImageContainerName"]);
                CloudBlockBlob imageConfiguration = ImageConfigurationContainer.GetBlockBlobReference(ConfigurationManager.AppSettings["ImageBlobReference"]);

                // delete  IMAGE

                if (File.Exists(Imagerootpath + "\\" + imageConfiguration.Name))
                {
                    File.Delete(Imagerootpath + "\\" + imageConfiguration.Name);
                }

                //download  IMAGE
                using (var fileStream = new FileStream(Imagerootpath + "\\" + imageConfiguration.Name, FileMode.CreateNew))
                {
                    BlobRequestOptions options = new BlobRequestOptions();
                    options.ServerTimeout = new TimeSpan(0, 25, 0);
                    imageConfiguration.DownloadToStream(fileStream, null, options, null);
                }
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new DataAccessException(exception.Message, exception.InnerException);
            }

            

           
        }
        /// <summary>
        ///  This method create a folder for azcopy.exe in Environment.CurrentDirectory(i.e dubug/ release)
        /// </summary>
        /// <returns></returns>
        public static string GetaZCopypath()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string aZCopy = string.Empty;
                string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string functionsPath = Path.Combine(executableLocation, "PDFFiles");
                if (!Directory.Exists(functionsPath))
                {
                    Directory.CreateDirectory(functionsPath);
                }
                aZCopy = Path.Combine(functionsPath, "azCopyExePath");
                if (!Directory.Exists(aZCopy))
                {
                    Directory.CreateDirectory(aZCopy);
                }
                //write aZCopy directory path into application insights
             //   InsightLogger.TrackEvent("AzCopy.exe File Path" + aZCopy);
                return aZCopy;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
        /// <summary>
        /// This method create a folder for image in Environment.CurrentDirectory(i.e dubug/ release)
        /// </summary>
        /// <returns></returns>
        public static string GetImagePath()
        {
            string callerMethodName = string.Empty;
            try
            {
                //Get Caller Method name from CallerInformation class
                callerMethodName = CallerInformation.TrackCallerMethodName();
                string imagepath = string.Empty;
                string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string functionsPath = Path.Combine(executableLocation, "PDFFiles");
                if (!Directory.Exists(functionsPath))
                {
                    Directory.CreateDirectory(functionsPath);
                }
                imagepath = Path.Combine(functionsPath, "Images");
                if (!Directory.Exists(imagepath))
                {
                    Directory.CreateDirectory(imagepath);
                }
                //write aZCopy directory path into application insights
               // InsightLogger.TrackEvent("AzCopy.exe File Path" + imagepath);
                return imagepath;
            }
            catch (Exception exception)
            {
                InsightLogger.Exception(exception.Message, exception, callerMethodName);
                throw new BusinessLogicException(exception.Message, exception.InnerException);
            }
        }
    }
}
