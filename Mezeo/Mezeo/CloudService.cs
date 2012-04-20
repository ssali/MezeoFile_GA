using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MezeoFileSupport;

namespace Mezeo
{
    public class CloudService
    {
       public MezeoFileSupport.MezeoFileCloud fileCloud;

        public CloudService()
        {
            fileCloud = new MezeoFileSupport.MezeoFileCloud();
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
            bool bRet = fileCloud.ContainerMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
            if (nStatusCode != ResponseCode.CONTAINERMOVE)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.ContainerMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
                    if (nStatusCode == ResponseCode.CONTAINERMOVE)
                        return bRet;
                }
            }
            return bRet;            
        }

        public bool ContainerRename(string strPath, string strNewName, ref int nStatusCode)
        {
            bool bRet = fileCloud.ContainerRename(strPath, strNewName, ref nStatusCode);
            if (nStatusCode != ResponseCode.CONTAINERRENAME)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.ContainerRename(strPath, strNewName, ref nStatusCode);
                    if (nStatusCode == ResponseCode.CONTAINERRENAME)
                        return bRet;
                }
            }
            return bRet;
        }

        public string Copy(string strSource, string StrDestination, string StrType, ref int nStatusCode)
        {
            return fileCloud.Copy(strSource, StrDestination, StrType, ref nStatusCode);
        }

        
        public bool Delete(string strPath, ref int nStatusCode)
        {
            bool bRet = fileCloud.Delete(strPath, ref nStatusCode);
            if (nStatusCode != ResponseCode.DELETE)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.Delete(strPath, ref nStatusCode);
                    if (nStatusCode == ResponseCode.DELETE)
                        return bRet;
                }
            }
            return bRet;
        }

        public bool DownloadFile(string strSource, string strDestination, double dblFileSizeInBytes, ref int nStatusCode)
        {
            bool bRet = fileCloud.DownloadFile(strSource, strDestination, dblFileSizeInBytes, ref nStatusCode);
            if (nStatusCode != ResponseCode.DOWNLOADFILE)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.DownloadFile(strSource, strDestination, dblFileSizeInBytes, ref nStatusCode);
                    if (nStatusCode == ResponseCode.DOWNLOADFILE)
                        return bRet;
                }
            }
            return bRet;
        }

        public ItemDetails[] DownloadItemDetails(string strContainer, ref int nStatusCode)
        {
            ItemDetails[] itemDetails = fileCloud.DownloadItemDetails(strContainer, ref nStatusCode);
            if (nStatusCode != ResponseCode.DOWNLOADITEMDETAILS)
            {
                for (int n = 0; n < 2; n++)
                {
                    itemDetails = fileCloud.DownloadItemDetails(strContainer, ref nStatusCode);
                    if (nStatusCode == ResponseCode.DOWNLOADITEMDETAILS)
                        return itemDetails;
                }
            }
            return itemDetails;
        }

        public bool ExceuteEventViewer(string strName)
        {
            return fileCloud.ExceuteEventViewer(strName);
        }

        public bool FileMove(string strPath, string strNewName, string strMineType, bool bPublic, string StrParent, ref int nStatusCode)
        {
            bool bRet = fileCloud.FileMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
            if (nStatusCode != ResponseCode.FILEMOVE)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.FileMove(strPath, strNewName, strMineType, bPublic, StrParent, ref nStatusCode);
                    if (nStatusCode == ResponseCode.FILEMOVE)
                        return bRet;
                }
            }
            return bRet;
        }

        public bool FileRename(string strPath, string strNewName, string strMineType, bool bPublic, ref int nStatusCode)
        {
            bool bRet = fileCloud.FileRename(strPath, strNewName, strMineType, bPublic, ref nStatusCode);
            if (nStatusCode != ResponseCode.FILERENAME)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.FileRename(strPath, strNewName, strMineType, bPublic, ref nStatusCode);
                    if (nStatusCode == ResponseCode.FILERENAME)
                        return bRet;
                }
            }
            return bRet;
        }

        public ItemDetails GetContinerResult(string strContainUrl, ref int nStatusCode)
        {
            ItemDetails itemDetails = fileCloud.GetContinerResult(strContainUrl, ref nStatusCode);
            if (nStatusCode != ResponseCode.GETCONTINERRESULT)
            {
                for (int n = 0; n < 2; n++)
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
            if (nStatusCode != ResponseCode.GETETAG)
            {
                for (int n = 0; n < 2; n++)
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
            if (nStatusCode != ResponseCode.GETNAMESPACERESULT)
            {
                for (int n = 0; n < 2; n++)
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
                for (int n = 0; n < 2; n++)
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
            return fileCloud.Login(strLoginName, strPassword, strUrl, ref nStatusCode);
        }

        public bool Logout()
        {
            return fileCloud.Logout();                
        }

        public string NewContainer(string strNewContainer, string strContentsResource, ref int nStatusCode)
        {
            string strUrl = fileCloud.NewContainer(strNewContainer, strContentsResource, ref nStatusCode);
            if (nStatusCode != ResponseCode.NEWCONTAINER)
            {
                for (int n = 0; n < 2; n++)
                {
                    strUrl = fileCloud.NewContainer(strNewContainer, strContentsResource, ref nStatusCode);
                    if (nStatusCode == ResponseCode.NEWCONTAINER)
                        return strUrl;
                }
            }
            return strUrl;
        }

        public bool NQCreate(string StrUri, string StrQueueName, string StrStarts, ref int nStatusCode)
        {
            bool bRet = fileCloud.NQCreate(StrUri, StrQueueName, StrStarts, ref nStatusCode);
            if (nStatusCode != ResponseCode.NQCREATE)
            {
                for (int n = 0; n < 2; n++)
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
                for (int n = 0; n < 2; n++)
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
                for (int n = 0; n < 2; n++)
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
                for (int n = 0; n < 2; n++)
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
            if (nStatusCode != ResponseCode.NQGETLENGTH)
            {
                for (int n = 0; n < 2; n++)
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
                for (int n = 0; n < 2; n++)
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
            bool bRet = fileCloud.OverWriteFile(strSource, strDestination, ref nStatusCode);
            if (nStatusCode != ResponseCode.OVERWRITEFILE)
            {
                for (int n = 0; n < 2; n++)
                {
                    bRet = fileCloud.OverWriteFile(strSource, strDestination, ref nStatusCode);
                    if (nStatusCode == ResponseCode.OVERWRITEFILE)
                        return bRet;
                }
            }
            return bRet;
        }

        public void PauseSyncProcess()
        {
            fileCloud.PauseSyncProcess();
        }

        public void ResumeSyncProcess()
        {
            fileCloud.ResumeSyncProcess();
        }

        public bool StatusConnection(string strLoginName, string strPassword, string strUrl, ref int nStatusCode)
        {
            return fileCloud.StatusConnection(strLoginName, strPassword, strUrl, ref nStatusCode);
        }

        public void StopSyncProcess()
        {
            fileCloud.StopSyncProcess();
        }

        public string UploadingFile(string strSource, string strDestination, ref int nStatusCode)
        {
            string strUrl = fileCloud.UploadingFile(strSource, strDestination, ref nStatusCode);
            if (nStatusCode != ResponseCode.UPLOADINGFILE)
            {
                for (int n = 0; n < 2; n++)
                {
                    strUrl = fileCloud.UploadingFile(strSource, strDestination, ref nStatusCode);
                    if (nStatusCode == ResponseCode.UPLOADINGFILE)
                        return strUrl;
                }
            }
            return strUrl;
        }
    }
}
