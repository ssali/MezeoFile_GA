using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MezeoFileSupport;
using System.IO;


namespace Mezeo
{
    public class CloudService
    {
        public static int NUMBER_OF_RETRIES = 2;

        public MezeoFileSupport.MezeoFileCloud fileCloud;
        
        frmSyncManager syncManager;

        public CloudService()
        {
            fileCloud = new MezeoFileSupport.MezeoFileCloud();
            fileCloud.AddMineTypeDictionary();
        }

        public void SetSynManager(ref frmSyncManager Manager)
        {
            syncManager = Manager;
        }

        public bool AppEventViewer(string StrLogName, string StrLogMsg, int nLevel)
        {
            return fileCloud.AppEventViewer(StrLogName, StrLogMsg, nLevel);
        }

        public bool CleanEventViewer(string StrLogName)
        {
            return fileCloud.CleanEventViewer(StrLogName);
        }

        public bool ContainerMove(string strPath, string strNewName, string strMineType, bool bPublic, string StrParent, ref int nStatusCode)
        {
            syncManager.ShowOtherProgressBar(strNewName);
            bool bRet = fileCloud.ContainerMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
            if (nStatusCode != ResponseCode.CONTAINERMOVE)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.ShowOtherProgressBar(strNewName);
                    bRet = fileCloud.ContainerMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
                    if (nStatusCode == ResponseCode.CONTAINERMOVE)
                    {
                        if (syncManager.myDelegate != null)
                            syncManager.myDelegate(1);

                        return bRet;
                    }
                }
            }
            if (syncManager.myDelegate != null)
                syncManager.myDelegate(1);
            return bRet;            
        }

        public bool ContainerRename(string strPath, string strNewName, ref int nStatusCode)
        {
            syncManager.ShowOtherProgressBar(strNewName);
            bool bRet = fileCloud.ContainerRename(strPath, strNewName, ref nStatusCode);
            if (nStatusCode != ResponseCode.CONTAINERRENAME)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.ShowOtherProgressBar(strNewName);
                    bRet = fileCloud.ContainerRename(strPath, strNewName, ref nStatusCode);
                    if (nStatusCode == ResponseCode.CONTAINERRENAME)
                    {
                        if (syncManager.myDelegate != null)
                            syncManager.myDelegate(1);
                        return bRet;
                    }
                }
            }
            if (syncManager.myDelegate != null)
                syncManager.myDelegate(1);
            return bRet;
        }

        public string Copy(string strSource, string StrDestination, string StrType, ref int nStatusCode)
        {
            return fileCloud.Copy(strSource, StrDestination, StrType, ref nStatusCode);
        }
        
        public bool Delete(string strPath, ref int nStatusCode, string strDisplayName)
        {
            syncManager.ShowOtherProgressBar(strDisplayName);
            bool bRet = fileCloud.Delete(strPath, ref nStatusCode);
            if (nStatusCode != ResponseCode.DELETE && nStatusCode != ResponseCode.NOTFOUND && nStatusCode != ResponseCode.LOGINFAILED2)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.ShowOtherProgressBar(strDisplayName);
                    bRet = fileCloud.Delete(strPath, ref nStatusCode);
                    
                    if (nStatusCode == ResponseCode.DELETE)
                    {
                        if (syncManager.myDelegate != null)
                            syncManager.myDelegate(1);
                        return bRet;
                    }
                }
            }
            if (syncManager.myDelegate != null)
                syncManager.myDelegate(1);

            if (nStatusCode == ResponseCode.NOTFOUND || nStatusCode == ResponseCode.LOGINFAILED2)
                bRet = true;


            return bRet;
        }

        public bool DownloadFile(string strSource, string strDestination, double dblFileSizeInBytes, ref int nStatusCode)
        {
            syncManager.SetMaxProgress(dblFileSizeInBytes, strDestination);

            bool bRet = fileCloud.DownloadFile(strSource, strDestination, dblFileSizeInBytes, ref nStatusCode, syncManager.myDelegate, syncManager.ContinueRunningDelegate);

            if ((nStatusCode != ResponseCode.DOWNLOADFILE) && (nStatusCode != ResponseCode.SERVER_INACCESSIBLE) && (nStatusCode != -4))
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.SetMaxProgress(dblFileSizeInBytes, strDestination);
                    bRet = fileCloud.DownloadFile(strSource, strDestination, dblFileSizeInBytes, ref nStatusCode, syncManager.myDelegate, syncManager.ContinueRunningDelegate);
                    
                    if (nStatusCode == ResponseCode.DOWNLOADFILE)
                        return bRet;
                }
            }   
            return bRet;
        }

        public ItemDetails[] DownloadItemDetails(string strContainer, ref int nStatusCode, string strFilterName)
        {
            ItemDetails[] resultItemDetails = null;

            // If there is no filter, then handle pagination.
            if ((null == strFilterName) || (0 == strFilterName.Length))
            {
                int newTotal = 0;
                bool continuePaging = true;
                ItemDetails[] itemDetails;
                FilterDetails filterDetails = new FilterDetails();
                filterDetails.szFieldValue = strFilterName;
                filterDetails.nStartPosition = 0;

                while (continuePaging)
                {
                    itemDetails = fileCloud.DownloadItemDetails(strContainer, ref nStatusCode, filterDetails);
                    if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
                    {
                        for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                        {
                            itemDetails = fileCloud.DownloadItemDetails(strContainer, ref nStatusCode, filterDetails);
                            if ((null != itemDetails) && (nStatusCode == ResponseCode.DOWNLOADITEMDETAILS))
                            {
                                // Update the total count member of item 0 since it is the
                                // only item the calling code uses to get/check the count.
                                newTotal = 0;
                                if (null == resultItemDetails)
                                    resultItemDetails = itemDetails;
                                else
                                {
                                    newTotal = resultItemDetails[0].nTotalItem + itemDetails[0].nTotalItem;
                                    resultItemDetails = resultItemDetails.Concat(itemDetails).ToArray();
                                    resultItemDetails[0].nTotalItem = newTotal;
                                }
                                if (0 == itemDetails[0].nTotalItem)
                                    continuePaging = false;
                            }
                            if (n >= CloudService.NUMBER_OF_RETRIES)
                                continuePaging = false;
                        }
                    }
                    else
                    {
                        if (null != itemDetails)
                        {
                            // Update the total count member of item 0 since it is the
                            // only item the calling code uses to get/check the count.
                            newTotal = 0;
                            if (null == resultItemDetails)
                                resultItemDetails = itemDetails;
                            else
                            {
                                newTotal = resultItemDetails[0].nTotalItem + itemDetails[0].nTotalItem;
                                resultItemDetails = resultItemDetails.Concat(itemDetails).ToArray();
                                resultItemDetails[0].nTotalItem = newTotal;
                            }
                            if (0 == itemDetails[0].nTotalItem)
                                continuePaging = false;
                        }
                    }

                    if (null == itemDetails)
                    {
                        // Nothing was there so there are no more pages.
                        continuePaging = false;
                    }
                    else
                    {
                        // Increment the count and ask for another possible page.
                        filterDetails.nStartPosition += itemDetails[0].nTotalItem;
                    }
                }
            }
            else
            {
                FilterDetails filterDetails = new FilterDetails();
                filterDetails.szFieldValue = strFilterName;
                filterDetails.nStartPosition = 0;

                resultItemDetails = fileCloud.DownloadItemDetails(strContainer, ref nStatusCode, filterDetails);
                if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
                {
                    for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                    {
                        resultItemDetails = fileCloud.DownloadItemDetails(strContainer, ref nStatusCode, filterDetails);
                        if (nStatusCode == ResponseCode.DOWNLOADITEMDETAILS)
                            return resultItemDetails;
                    }
                }
            }

            return resultItemDetails;
        }

        public bool ExceuteEventViewer(string strName)
        {
            return fileCloud.ExceuteEventViewer(strName);
        }

        public bool FileMove(string strPath, string strNewName, string strMineType, bool bPublic, string StrParent, ref int nStatusCode)
        {
            syncManager.ShowOtherProgressBar(strNewName);
            bool bRet = fileCloud.FileMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
            if (nStatusCode != ResponseCode.FILEMOVE)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.ShowOtherProgressBar(strNewName);
                    bRet = fileCloud.FileMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
                    
                    if (nStatusCode == ResponseCode.FILEMOVE)
                    {
                        if (syncManager.myDelegate != null)
                            syncManager.myDelegate(1);
                        return bRet;
                    }
                }
            }
            if (syncManager.myDelegate != null)
                syncManager.myDelegate(1);
            return bRet;
        }

        public bool FileRename(string strPath, string strNewName, string strMineType, bool bPublic, ref int nStatusCode)
        {
            syncManager.ShowOtherProgressBar(strNewName);
            bool bRet = fileCloud.FileRename(strPath, strNewName, strMineType, bPublic, ref nStatusCode);
            if (nStatusCode != ResponseCode.FILERENAME)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.ShowOtherProgressBar(strNewName);
                    bRet = fileCloud.FileRename(strPath, strNewName, strMineType, bPublic, ref nStatusCode);
                    
                    if (nStatusCode == ResponseCode.FILERENAME)
                    {
                        if (syncManager.myDelegate != null)
                            syncManager.myDelegate(1);
                        return bRet;
                    }
                }
            }
            if (syncManager.myDelegate != null)
                syncManager.myDelegate(1);
            return bRet;
        }

        public ItemDetails GetContinerResult(string strContainUrl, ref int nStatusCode)
        {
            ItemDetails itemDetails = fileCloud.GetContinerResult(strContainUrl, ref nStatusCode);
            if (nStatusCode != ResponseCode.GETCONTINERRESULT)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    itemDetails = fileCloud.GetContinerResult(strContainUrl, ref nStatusCode); ;
                    if (nStatusCode == ResponseCode.GETCONTINERRESULT)
                        return itemDetails;
                }
            }
            return itemDetails;
        }

        public string GetDeletedInfo(string strContainUrl, ref int nStatusCode)
        {
           return fileCloud.GetDeletedInfo(strContainUrl, ref nStatusCode);
        }

        public string GetETag(string strContainUrl, ref int nStatusCode)
        {
            string strEtag = fileCloud.GetETag(strContainUrl, ref nStatusCode);
            if (nStatusCode != ResponseCode.GETETAG && nStatusCode != ResponseCode.NOTFOUND && nStatusCode != ResponseCode.LOGINFAILED1)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    strEtag = fileCloud.GetETag(strContainUrl, ref nStatusCode);
                    if (nStatusCode == ResponseCode.GETETAG)
                        return strEtag;
                }
            }
            return strEtag;
        }

        public string GetETagMatching(string StrUri, string StrETag, ref int nStatusCode)
        {
           return fileCloud.GetETagMatching(StrUri, StrETag, ref nStatusCode);
        }

        public NSResult GetNamespaceResult(string StrUri, string StrObjectType, ref int nStatusCode)
        {
            NSResult nsResult = fileCloud.GetNamespaceResult(StrUri, StrObjectType, ref nStatusCode);
            if ((nStatusCode != ResponseCode.GETNAMESPACERESULT) && (nStatusCode != ResponseCode.NOTFOUND))
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    nsResult = fileCloud.GetNamespaceResult(StrUri, StrObjectType, ref nStatusCode);
                    if (nStatusCode == ResponseCode.GETNAMESPACERESULT)
                        return nsResult;
                }
            }
            return nsResult;
        }

        public bool GetOverlayRegisteration()
        {
            return fileCloud.GetOverlayRegisteration();
        }

        public ItemResults GetParentName(string strContents, ref int nStatusCode)
        {
            return fileCloud.GetParentName(strContents, ref nStatusCode);
        }

        public double GetStorageUsed(string strUrl, ref int nStatusCode)
        {
            double dblSize = fileCloud.GetStorageUsed(strUrl, ref nStatusCode);
            if (nStatusCode != ResponseCode.GETSTORAGEUSED)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    dblSize = fileCloud.GetStorageUsed(strUrl, ref nStatusCode);
                    if (nStatusCode == ResponseCode.GETSTORAGEUSED)
                        return dblSize;
                }
            }
            return dblSize;
        }

        public LoginDetails Login(string strLoginName, string strPassword, string strUrl, ref int nStatusCode)
        {
            return fileCloud.Login(strLoginName, strPassword, strUrl, BasicInfo.MacAddress, ref nStatusCode);
        }

        public bool Logout()
        {
            return fileCloud.Logout();                
        }

        public string NewContainer(string strNewContainer, string strContentsResource, ref int nStatusCode)
        {
            syncManager.ShowOtherProgressBar(strNewContainer);
            string strUrl = fileCloud.NewContainer(strNewContainer, strContentsResource, ref nStatusCode);
            if (nStatusCode != ResponseCode.NEWCONTAINER)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.ShowOtherProgressBar(strNewContainer);
                    strUrl = fileCloud.NewContainer(strNewContainer, strContentsResource, ref nStatusCode);
           
                    if (nStatusCode == ResponseCode.NEWCONTAINER)
                    {
                        if (syncManager.myDelegate != null)
                            syncManager.myDelegate(1);
                        return strUrl;
                    }
                }
            }
            if (syncManager.myDelegate != null)
                syncManager.myDelegate(1);
            return strUrl;
        }

        public bool NQCreate(string StrUri, string StrQueueName, string StrStarts, ref int nStatusCode)
        {
            bool bRet = fileCloud.NQCreate(StrUri, StrQueueName, StrStarts, ref nStatusCode);
            if (nStatusCode != ResponseCode.NQCREATE)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    bRet = fileCloud.NQCreate(StrUri, StrQueueName, StrStarts, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NQCREATE)
                        return bRet;
                }
            }
            return bRet;
        }

        public bool NQDelete(string StrUri, string StrQueueName, ref int nStatusCode)
        {
            bool bRet = fileCloud.NQDelete(StrUri, StrQueueName, ref nStatusCode);
            if (nStatusCode != ResponseCode.NQDELETE)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    bRet = fileCloud.NQDelete(StrUri, StrQueueName, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NQDELETE)
                        return bRet;
                }
            }
            return bRet;
        }

        public bool NQDeleteValue(string StrUri, string StrQueueName, int nCountValue, ref int nStatusCode)
        {
            bool bRet = fileCloud.NQDeleteValue(StrUri, StrQueueName, nCountValue, ref nStatusCode);
            if (nStatusCode != ResponseCode.NQDELETEVALUE)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    bRet = fileCloud.NQDeleteValue(StrUri, StrQueueName, nCountValue, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NQDELETEVALUE)
                        return bRet;
                }
            }
            return bRet;
        }

        public NQDetails[] NQGetData(string StrUri, string StrQueueName, int nCountValue, ref int nStatusCode)
        {
            NQDetails[] nqDetails = fileCloud.NQGetData(StrUri, StrQueueName, nCountValue, ref nStatusCode);
            if (nStatusCode != ResponseCode.NQGETDATA)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    nqDetails = fileCloud.NQGetData(StrUri, StrQueueName, nCountValue, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NQGETDATA)
                        return nqDetails;
                }
            }
            return nqDetails;
        }

        public NQLengthResult NQGetLength(string StrUri, string StrQueueName, ref int nStatusCode)
        {
            NQLengthResult nqLengthResult = fileCloud.NQGetLength(StrUri, StrQueueName, ref nStatusCode);
            // If the notification queue doesn't exist, then create it.
            if (nStatusCode == 404)
            {
                if (NQCreate(BasicInfo.ServiceUrl + BasicInfo.NQParentURI, BasicInfo.GetQueueName(), BasicInfo.NQParentURI, ref nStatusCode))
                {
                    nqLengthResult = fileCloud.NQGetLength(StrUri, StrQueueName, ref nStatusCode);
                }
                else
                {
                    // If the queue can't be created, then just return the result code instead of retrying
                    // the NQGetLength request.
                    return nqLengthResult;
                }
            }

            if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    nqLengthResult = fileCloud.NQGetLength(StrUri, StrQueueName, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NQGETLENGTH)
                        return nqLengthResult;
                }
            }
            return nqLengthResult;
        }

        public string NQParentUri(string StrUri, ref int nStatusCode)
        {
            string nqParentUri = fileCloud.NQParentUri(StrUri, ref nStatusCode);
            if (nStatusCode != ResponseCode.NQPARENTURI)
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    nqParentUri = fileCloud.NQParentUri(StrUri, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NQPARENTURI)
                        return nqParentUri;
                }
            }
            return nqParentUri;
        }

        public bool OverWriteFile(string strSource, string strDestination, ref int nStatusCode)
        {
            var fileinfo = new FileInfo(strSource);

            syncManager.SetMaxProgress(fileinfo.Length, strSource);

            bool bRet = fileCloud.OverWriteFile(strSource, strDestination, ref nStatusCode, syncManager.myDelegate); 
            if ((nStatusCode != ResponseCode.OVERWRITEFILE) && (nStatusCode != ResponseCode.SERVER_INACCESSIBLE) && (nStatusCode != -4))
            {
                for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                {
                    syncManager.SetMaxProgress(fileinfo.Length, strSource);
                    bRet = fileCloud.OverWriteFile(strSource, strDestination, ref nStatusCode, syncManager.myDelegate);
                    // If the user cancelled the operation, then just return.
                    if (nStatusCode == -4)
                        return false;

                    if (nStatusCode == ResponseCode.OVERWRITEFILE)
                        return bRet;                    
                }
            }
            return bRet;
        }

        //public bool StatusConnection(string strLoginName, string strPassword, string strUrl, ref int nStatusCode)
        //{
        //    return fileCloud.StatusConnection(strLoginName, strPassword, strUrl, ref nStatusCode);
        //}

        public void StopSyncProcess()
        {
            fileCloud.StopSyncProcess();
        }

        public string UploadingFile(string strSource, string strDestination, ref int nStatusCode)
        {
            string strUrl = null;
            try
            {
                var fileinfo = new FileInfo(strSource);

                syncManager.SetMaxProgress(fileinfo.Length, strSource);

                strUrl = fileCloud.UploadingFile(strSource, strDestination, ref nStatusCode, syncManager.myDelegate);
                if ((nStatusCode != ResponseCode.UPLOADINGFILE) && (nStatusCode != -3) && (nStatusCode != -4))
                {
                    for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                    {
                        syncManager.SetMaxProgress(fileinfo.Length, strSource);
                        strUrl = fileCloud.UploadingFile(strSource, strDestination, ref nStatusCode, syncManager.myDelegate);

                        // If the user cancelled the operation, then just return.
                        if (nStatusCode == -4)
                            return null;

                        if (nStatusCode == ResponseCode.UPLOADINGFILE)
                            return strUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("CloudService - UploadingFile", "Caught exception: " + ex.Message);
            }
            if (nStatusCode == ResponseCode.UPLOADINGFILE)
                return strUrl;
            return null;
        }


        public string UploadingFileOnResume(string strSource, string strDestination, ref int nStatusCode)
        {
            string strUrl = null;
            try
            {
                var fileinfo = new FileInfo(strSource);

                syncManager.SetMaxProgress(fileinfo.Length, strSource);

                strUrl = fileCloud.UploadingFileOnResume(strSource, strDestination, ref nStatusCode, syncManager.myDelegate);
                if ((nStatusCode != ResponseCode.UPLOADINGFILE) && (nStatusCode != -3) && (nStatusCode != -4))
                {
                    for (int n = 0; n < CloudService.NUMBER_OF_RETRIES; n++)
                    {
                        syncManager.SetMaxProgress(fileinfo.Length, strSource);
                        strUrl = fileCloud.UploadingFileOnResume(strSource, strDestination, ref nStatusCode, syncManager.myDelegate);

                        // If the user cancelled the operation, then just return.
                        if (nStatusCode == -4)
                            return null;

                        if (nStatusCode == ResponseCode.UPLOADINGFILE)
                            return strUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("CloudService - UploadingFileOnResume", "Caught exception: " + ex.Message);
            }
            if (nStatusCode == ResponseCode.UPLOADINGFILE)
                return strUrl;
            return null;
        }
    }
}
